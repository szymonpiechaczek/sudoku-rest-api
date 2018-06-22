using DBService.DAL.Interfaces;
using LiteDB;
using SudokuDBModel;
using System.Collections.Generic;

namespace DBService.DAL
{
    public class SudokuDAL : ISudokuDAL
    {
        public Sudoku Get(int id)
        {
            using (var db = new LiteDatabase(@"SudokuDB.db"))
            {
                var col = db.GetCollection<Sudoku>("sudoku");

                return col.FindById(id);
            }
        }
        public IEnumerable<Sudoku> GetAll()
        {
            using (var db = new LiteDatabase(@"SudokuDB.db"))
            {
                var col = db.GetCollection<Sudoku>("sudoku");
                return col.FindAll();
            }
        }
        public int Add(Sudoku sudoku)
        {
            using (var db = new LiteDatabase(@"SudokuDB.db"))
            {
                var col = db.GetCollection<Sudoku>("sudoku");
                return col.Insert(sudoku);
            }
        }
    }
}