using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CocosSharpSudoku
{
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    public class Square
    {
        private Random _randomGenerator;
        private List<int> _availableNumbers;
        private List<Tuple<int,int>> _regionNeighbors;
        private int _value;
        private int _column, _row;

        public int Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }

        public List<Tuple<int,int>> RegionNeighbors
        {
            get
            {
                return _regionNeighbors;
            }

            set
            {
                _regionNeighbors = value;
            }
        }

        public int Row
        {
            get
            {
                return _row;
            }

            set
            {
                _row = value;
            }
        }

        public int Column
        {
            get
            {
                return _column;
            }

            set
            {
                _column = value;
            }
        }

        public Square(int column, int row)
        {
            _randomGenerator = new Random(Guid.NewGuid().GetHashCode());
            _availableNumbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            _value = 0;
            _column = column;
            _row = row;
            GenerateListOfRegionNeighbors();
        }

        private void GenerateListOfRegionNeighbors()
        {
            _regionNeighbors = new List<Tuple<int, int>>();
            if(_column < 3)
            {
                // First region
                if (_row < 3)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (_row == j && _column == i) continue;
                            RegionNeighbors.Add(new Tuple<int, int>(i,j));
                        }
                    }
                }
                // Second region
                if(_row >= 3 && _row < 6)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 3; j < 6; j++)
                        {
                            if (_row == j && _column == i) continue;
                            RegionNeighbors.Add(new Tuple<int, int>(i, j));

                        }
                    }
                }
                // Third region
                if(_row >= 6 && _row < 9)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 6; j < 9; j++)
                        {
                            if (_row == j && _column == i) continue;
                            RegionNeighbors.Add(new Tuple<int, int>(i, j));
                        }
                    }
                }
            }

            if (_column >= 3 && _column < 6)
            {
                // Fourth region
                if (_row < 3)
                {
                    for (int i = 3; i < 6; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (_row == j && _column == i) continue;
                            RegionNeighbors.Add(new Tuple<int, int>(i, j));
                        }
                    }
                }
                // Fifth region
                if (_row >= 3 && _row < 6)
                {
                    for (int i = 3; i < 6; i++)
                    {
                        for (int j = 3; j < 6; j++)
                        {
                            if (_row == j && _column == i) continue;
                            RegionNeighbors.Add(new Tuple<int, int>(i, j));
                        }
                    }
                }
                // Sixth region
                if (_row >= 6 && _row < 9)
                {
                    for (int i = 3; i < 6; i++)
                    {
                        for (int j = 6; j < 9; j++)
                        {
                            if (_row == j && _column == i) continue;
                            RegionNeighbors.Add(new Tuple<int, int>(i, j));
                        }
                    }
                }
            }
            if (_column >= 6 && _column < 9)
            {
                // First region
                if (_row < 3)
                {
                    for (int i = 6; i < 9; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (_row == j && _column == i) continue;
                            RegionNeighbors.Add(new Tuple<int, int>(i, j));
                        }
                    }
                }
                // Second region
                if (_row >= 3 && _row < 6)
                {
                    for (int i = 6; i < 9; i++)
                    {
                        for (int j = 3; j < 6; j++)
                        {
                            if (_row == j && _column == i) continue;
                            RegionNeighbors.Add(new Tuple<int, int>(i, j));
                        }
                    }
                }
                // Third region
                if (_row >= 6 && _row < 9)
                {
                    for (int i = 6; i < 9; i++)
                    {
                        for (int j = 6; j < 9; j++)
                        {
                            if (_row == j && _column == i) continue;
                            RegionNeighbors.Add(new Tuple<int, int>(i, j));
                        }
                    }
                }
            }
        }

        internal int GetAvailableNumber()
        {
            int index = _randomGenerator.Next(0, _availableNumbers.Count);
            return _availableNumbers[index];

        }

        internal bool HasAvailableNumbers()
        {
            return _availableNumbers.Count != 0;
        }

        internal void RemoveAvailableNumber(int potentialNumber)
        {
            _availableNumbers.Remove(potentialNumber);
        }

        internal void ResetAvailableNumbers()
        {
            for (int i = 1; i <= 9; i++)
            {
                _availableNumbers.Add(i);
            }
        }
    }

    public class Sudoku
    {
        private Square[,] _sudoku;
        private Random _randomGenerator;

        public Sudoku()
        {
            _randomGenerator = new Random(Guid.NewGuid().GetHashCode());
            _sudoku = new Square[9, 9];
            InitializeSquares();
            GenerateSudoku();
        }

        public void PrintSudoku()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Console.Write(_sudoku[i, j].Value + " ");
                }
                Console.Write("\n");
            }     
        }

        public static void ClearConsole()
        {
            int currentLine = Console.CursorTop;
            if(currentLine != 0) // just started
            {
                for(int i = 9; i > 0; i--)
                {
                    Console.SetCursorPosition(0, currentLine - i);
                    Console.Write(new string(' ', Console.WindowWidth));
                }
                Console.SetCursorPosition(0, currentLine - 9);
            }
            
        }
        private void InitializeSquares()
        {
            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
                {
                    _sudoku[i, j] = new Square(i, j);
                }
            }
        }

        // Backtracking sudoku generating algorithm
        private void GenerateSudoku()
        {
            bool continueToNextSquare = true;

            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
                {
                    //PrintSudoku();
                    //if(!continueToNextSquare)
                    //{
                    //    if (j > 0) j--;
                    //    else if (j == 0 && i > 0) i--;
                    //    continueToNextSquare = true;
                    //}
                    // Check if we are out of available numbers for the square
                    if (_sudoku[i,j].HasAvailableNumbers())
                    {
                        // If not: Get a number from the available numbers for the square
                        int potentialNumber = _sudoku[i, j].GetAvailableNumber();
                        // Check if that number conflicts 
                        if(!NumberIsConflicting(potentialNumber, i, j))
                        {
                            // If not: Use the number and remove it from the list of available numbers
                            _sudoku[i, j].Value = potentialNumber;
                            _sudoku[i, j].RemoveAvailableNumber(potentialNumber);
                        }
                        else
                        {
                            // If yes: Remove the number from available numbers
                            _sudoku[i, j].RemoveAvailableNumber(potentialNumber);

                            // Continue try to find a number for this square
                            j--;
                        }
                    }
                    else
                    {
                        // Refill this square's available numbers and go back 1 square
                        _sudoku[i, j].ResetAvailableNumbers();
                        _sudoku[i, j].Value = 0;
                        if (j > 0)
                        {
                            j = j - 2;
                        }
                        else
                        {
                            j--;
                            if (i > 0) i--;
                        }
                       
                    }
                }
            }

            // Start with the first square

            // Check if we are out of available numbers for the square
            // If yes: 
            //      Go back 1 square
            // If no:
            //      Get a number from the available numbers for the square
            //      Check if that number conflicts 
            //      If yes:
            //          Remove that number from available numbers for the square
            //      If no:
            //          Use the number
            // Go forward 1 square
        }

        private bool NumberIsConflicting(int potentialNumber, int i, int j)
        {
            if(!NumberConflictsInColumn(potentialNumber, i) &&
                !NumberConflictsInRow(potentialNumber, j) &&
                !NumberConflictsInRegion(potentialNumber, i, j))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool NumberConflictsInRegion(int potentialNumber, int column, int row)
        {
            foreach (var square in _sudoku[column, row].RegionNeighbors)
            {
                if (_sudoku[square.Item1, square.Item2].Value == potentialNumber)
                    return true;
            }
            return false;
        }

        private bool NumberConflictsInRow(int potentialNumber, int row)
        {
            for (int column = 0; column < 9; column++)
            {
                if (_sudoku[column, row].Value == potentialNumber)
                    return true;
            }
            return false;
        }

        private bool NumberConflictsInColumn(int potentialNumber, int column)
        {
            for (int row = 0; row < 9; row++)
            {
                if (_sudoku[column, row].Value == potentialNumber)
                    return true;
            }
            return false;
        }

        internal List<Square> GetInitialNumbers(Difficulty difficulty)
        {
            List<Square> squares = new List<Square>();

            switch(difficulty)
            {
                case Difficulty.Easy:
                    squares = PickRandomSquares(60);
                    break;
                case Difficulty.Normal:
                    squares = PickRandomSquares(40);
                    break;
                case Difficulty.Hard:
                    squares = PickRandomSquares(20);
                    break;
            }
            return squares;
        }

        private List<Square> PickRandomSquares(int numbersToPick)
        {
            List<Square> squares = new List<Square>();

            for (int i = 0; i < numbersToPick; i++)
            {
                int column = _randomGenerator.Next(0, 9);
                int row = _randomGenerator.Next(0, 9);

                Square square = _sudoku[column, row];
                if (!squares.Any(sq => sq.Column == column && sq.Row == row))
                {
                    squares.Add(square);
                }
                else
                {
                    i--;
                }
            }
            return squares;
        }

        internal Square GetSquare(int i, int j)
        {
            return _sudoku[i, j];
        }
    }
}