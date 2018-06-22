using SudokuDBModel;
using System.Collections.Generic;

namespace DBService.DAL.Interfaces
{
    interface ISudokuDAL
    {
        Sudoku Get(int id);
        IEnumerable<Sudoku> GetAll();
        int Add(Sudoku sudoku);
    }
}
