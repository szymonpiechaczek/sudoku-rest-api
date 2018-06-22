using SudokuSolverService.Services;
using SudokuSolverService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace SudokuSolverService.Controllers
{
    public class SolverController : ApiController
    {
        private readonly ISudokuSolver sudokuSolver = new SudokuSolver();

        [ResponseType(typeof(int[,]))]
        public IHttpActionResult Post([FromBody]int[,] sudoku)
        {
            return Ok(sudokuSolver.SolveSudokuAndSaveToDBAsync(sudoku, 0, 0) ? sudoku : new int[0, 0]);
        }
    }
}
