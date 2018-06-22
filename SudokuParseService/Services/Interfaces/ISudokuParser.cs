using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuParseService.Services.Interfaces
{
    interface ISudokuParser
    {
        int[,] ParseSudokuImage(String imagePath, bool isGoodShape);
    }
}
