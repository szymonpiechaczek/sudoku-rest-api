using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolverService.Services.Interfaces
{
    interface ISudokuSolver
    {
        bool SolveSudoku(int[,] puzzle, int row, int col);
        bool SolveSudokuAndSaveToDBAsync(int[,] puzzle, int row, int col);
    }
}
