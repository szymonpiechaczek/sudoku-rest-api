using System;
using System.Collections.Generic;
using System.Text;

namespace SudokuDBModel
{
    public class Sudoku
    {
        public int Id { get; set; }
        public int[][] OriginalValues { get; set; }
        public int[][] SolvedValues { get; set; }
    }
}
