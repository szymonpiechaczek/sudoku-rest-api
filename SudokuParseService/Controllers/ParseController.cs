using SudokuParseService.Services;
using SudokuParseService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace SudokuParseService.Controllers
{
    public class ParseController : ApiController
    {
        private readonly ISudokuParser sudokuParser = new SudokuParser();

        [ResponseType(typeof(int[,]))]
        public IHttpActionResult Get(String imagePath, Boolean isGoodShape)
        {
            return Ok(sudokuParser.ParseSudokuImage(imagePath, isGoodShape));
        }
    }
}
