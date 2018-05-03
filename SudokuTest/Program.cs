using CocosSharpSudoku;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TimeSpan total = new TimeSpan();
            for(int i = 0; i < 100; i++)
            {
                DateTime before = DateTime.Now;
                Sudoku sudoku = new Sudoku();
                DateTime after = DateTime.Now;
                sudoku.PrintSudoku();
                TimeSpan dif = after.Subtract(before);
                Console.WriteLine("Generated in: " + dif.Milliseconds + " ms");

                total += dif;
            }

            Console.WriteLine("Average time pr. sudoku: " + total.Milliseconds / 100 + " ms");
            
        }
    }
}
