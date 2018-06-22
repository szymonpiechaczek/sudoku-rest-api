using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Drawing;
using System.Text.RegularExpressions;
using SudokuParseService.Services.Interfaces;
using System.Net;

namespace SudokuParseService.Services
{
    public class SudokuParser : ISudokuParser
    {

        const String destFile = @"W:\workspace\sudoku-recognition\SudokuServices\SudokuParseService\Resources\dest.jpg";
        const String outDir = @"W:\workspace\sudoku-recognition\SudokuServices\SudokuParseService\Resources\out\";
        const String tessData = @"W:\workspace\sudoku-recognition\SudokuServices\SudokuParseService\Resources\tessdata";
        const String imageLocalPath = @"W:\workspace\sudoku-recognition\SudokuServices\SudokuParseService\Resources\sudoku.jpg";

        const int CONFIDENCE = 180;
        
        public int[,] ParseSudokuImage(String imagePath, bool isGoodShape)
        {
            WebRequest requestPic = WebRequest.Create(imagePath);
            WebResponse responsePic = requestPic.GetResponse();
            Image webImage = Image.FromStream(responsePic.GetResponseStream()); // Error
            webImage.Save(imageLocalPath);

            //isPerfectShape = isGoodShape;
            //Image<Bgr, Byte> tmpImage = new Image<Bgr, byte>(image);
            Mat sudoku = CvInvoke.Imread(imageLocalPath, 0);
           bool isPerfectShape = isGoodShape;

            Mat transformed;
            Mat outerBox = Preprocess(sudoku, isGoodShape);

            if (!isPerfectShape)
            {
                VectorOfVectorOfPoint biggestContour = FindBiggestContour(outerBox);
                List<Point> points = FindCornerPoints(biggestContour);
                transformed = Transform(outerBox, points);
            }
            else
                transformed = outerBox;
            transformed = ExtractTables(transformed);
            Rectangle border = new Rectangle(new Point(0, 0), transformed.Size);
            CvInvoke.Rectangle(transformed, border, new MCvScalar(0), transformed.Height / 20);

            CvInvoke.BitwiseNot(transformed, transformed);
            
            transformed.Save(destFile);

            Bitmap[,] array = SplitSudokuImage(Image.FromFile(destFile));

            for (int i = 0; i < 9; ++i)
                for (int j = 0; j < 9; ++j)
                    array[i, j].Save(outDir + i.ToString() + j.ToString() + ".png");

            return RecognizeDigits();
        }

        private static Mat Preprocess(Mat image, bool isPerfectShape)
        {
            Mat outerBox = new Mat(image.Size, DepthType.Cv8U, 3);
            CvInvoke.GaussianBlur(image, image, new Size(7, 7), 0);
            CvInvoke.AdaptiveThreshold(image, outerBox, 255, AdaptiveThresholdType.MeanC, ThresholdType.Binary, 5, 2);
            CvInvoke.BitwiseNot(outerBox, outerBox);

            if (!isPerfectShape || true)
            {
                var element = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(3, 3), new Point(-1, -1));
                CvInvoke.Dilate(outerBox, outerBox, element, new Point(-1, -1), 1, BorderType.Default, default(MCvScalar));
                CvInvoke.Erode(outerBox, outerBox, element, new Point(-1, -1), 1, BorderType.Default, default(MCvScalar));

            }
            return outerBox;
        }

        private static VectorOfVectorOfPoint FindBiggestContour(Mat image)
        {
            var biggest = new VectorOfPoint();
            double max_area = 0;
            var contours = new VectorOfVectorOfPoint();
            var biggestContour = new VectorOfVectorOfPoint();
            var hierarchy = new Mat();
            CvInvoke.FindContours(image, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            for (int i = 0; i < contours.Size; ++i)
            {
                var area = CvInvoke.ContourArea(contours[i]);
                if (area > 100)
                {
                    var peri = CvInvoke.ArcLength(contours[i], true);
                    CvInvoke.ApproxPolyDP(contours[i], contours[i], 0.02 * peri, true); //Aproximate to ideal lines
                    if (area > max_area)
                    {
                        biggest = contours[i];
                        max_area = area;
                    }
                }
            }
            biggestContour.Push(biggest);
            return biggestContour;
        }

        private static List<Point> FindCornerPoints(VectorOfVectorOfPoint contour)
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < contour.Size; ++i)
                points.AddRange(contour[i].ToArray());

            return points;
        }

        private static Mat Transform(Mat image, List<Point> points)
        {
            points = Rectify(points);

            var srcs = new PointF[4];
            for (int i = 0; i < 4; i++)
                srcs[i] = new PointF(points[i].X, points[i].Y);

            var dsts = new PointF[4];
            dsts[0] = new PointF(0, 0);
            dsts[1] = new PointF(449, 0);
            dsts[2] = new PointF(449, 449);
            dsts[3] = new PointF(0, 449);

            var retval = CvInvoke.GetPerspectiveTransform(srcs, dsts);

            CvInvoke.WarpPerspective(image, image, retval, new Size(450, 450));

            return image;
        }

        private static List<Point> Rectify(List<Point> points)
        {
            var newList = new List<Point>();
            var sumList = new List<int>();
            var diffList = new List<int>();

            var finalSumList = new List<Point>();
            var finalDiffList = new List<Point>(points);

            foreach (Point point in points)
                sumList.Add(point.X + point.Y);

            finalSumList.Add(points[sumList.IndexOf(sumList.Min())]);
            finalSumList.Add(points[sumList.IndexOf(sumList.Max())]);
            finalDiffList.Remove(finalSumList[0]);
            finalDiffList.Remove(finalSumList[1]);

            foreach (Point point in finalDiffList)
                diffList.Add(Math.Abs(point.X - point.Y));

            newList.Add(finalSumList[0]);
            newList.Add(finalDiffList[diffList.IndexOf(diffList.Min())]);
            newList.Add(finalSumList[1]);
            newList.Add(finalDiffList[diffList.IndexOf(diffList.Max())]);

            return newList;
        }
        
        private static Bitmap[,] SplitSudokuImage(Image img)
        {
            int widthThird = (int)((double)img.Width / 9.0);
            int heightThird = (int)((double)img.Height / 9.0);
            Bitmap[,] bmps = new Bitmap[9, 9];
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    bmps[i, j] = new Bitmap(widthThird, heightThird);

                    Image<Bgr, Byte> tmpImage = new Image<Bgr, byte>(bmps[i, j]);
                    var tmpMat = tmpImage.Mat;
                    Rectangle border = new Rectangle(new Point(0, 0), bmps[i, j].Size);
                    CvInvoke.Rectangle(tmpMat, border, new MCvScalar(0), bmps[i, j].Height / 15);
                    bmps[i, j] = tmpMat.Bitmap;

                    Graphics g = Graphics.FromImage(bmps[i, j]);
                    g.DrawImage(img, new Rectangle(0, 0, widthThird, heightThird), new Rectangle(j * widthThird, i * heightThird, widthThird, heightThird), GraphicsUnit.Pixel);
                    g.Dispose();
                }

            return bmps;
        }

        private static int[,] RecognizeDigits()
        {
            int[,] sudoku = new int[9, 9];
            for (int i = 0; i < 9; ++i)
            {
                for (int j = 0; j < 9; ++j)
                {
                    Bitmap image = new Bitmap(outDir + i.ToString() + j.ToString() + ".png");
                    tessnet2.Tesseract ocr = new tessnet2.Tesseract();
                    ocr.Init(tessData, "eng", false); // To use correct tessdata
                    List<tessnet2.Word> result = ocr.DoOCR(image, Rectangle.Empty);

                    foreach (tessnet2.Word word in result)
                    {
                        Regex regex = new Regex(@"\d+");
                        Match match = regex.Match(word.Confidence < CONFIDENCE ? word.Text : "");
                        sudoku[i, j] = match.Value == "" ? 0 : Int32.Parse(match.Value);
                        Console.WriteLine("{0} : {1}", word.Confidence, word.Text);
                    }
                }
            }
            return sudoku;
        }

        private static Mat ExtractTables(Mat image)
        {
            Mat horizontal = image.Clone();
            Mat vertical = image.Clone();

            int scale = 10;

            int horizontalSize = horizontal.Cols / scale;
            Mat horizontalStructure = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(horizontalSize, 1), new Point(-1, -1));

            CvInvoke.Erode(horizontal, horizontal, horizontalStructure, new Point(-1, -1), 1, BorderType.Default, default(MCvScalar));
            CvInvoke.Dilate(horizontal, horizontal, horizontalStructure, new Point(-1, -1), 1, BorderType.Default, default(MCvScalar));

            int verticalSize = vertical.Rows / scale;
            Mat verticalStructure = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(1, verticalSize), new Point(-1, -1));

            CvInvoke.Erode(vertical, vertical, verticalStructure, new Point(-1, -1), 1, BorderType.Default, default(MCvScalar));
            CvInvoke.Dilate(vertical, vertical, verticalStructure, new Point(-1, -1), 1, BorderType.Default, default(MCvScalar));

            var mask = new Mat();

            CvInvoke.Add(horizontal, vertical, mask);

            var joints = new Mat();
            CvInvoke.BitwiseAnd(horizontal, vertical, joints);

            var res = new Mat();
            CvInvoke.AbsDiff(image, mask, res);
            CvInvoke.AbsDiff(res, joints, res);

            var element = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(3, 3), new Point(-1, -1));
            CvInvoke.Erode(res, res, element, new Point(-1, -1), 1, BorderType.Default, default(MCvScalar));
            CvInvoke.Dilate(res, res, element, new Point(-1, -1), 1, BorderType.Default, default(MCvScalar));
          
            return res;
        }
    }
}
