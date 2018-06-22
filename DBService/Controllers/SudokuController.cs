using DBService.DAL;
using DBService.DAL.Interfaces;
using SudokuDBModel;
using System.Collections.Generic;
using System.Web.Http;

namespace DBService.Controllers
{
    public class SudokuController : ApiController
    {
        private static readonly ISudokuDAL sudokuDAL = new SudokuDAL();
        // GET: api/Sudoku
        public IEnumerable<Sudoku> Get()
        {
            return sudokuDAL.GetAll();
        }

        // GET: api/Sudoku/5
        public Sudoku Get(int id)
        {
            return sudokuDAL.Get(id);
        }

        // POST: api/Sudoku
        public int Post([FromBody]Sudoku sudoku)
        {
            return sudokuDAL.Add(sudoku);
        }
    }
}
