using System;
using System.Collections.Generic;
using CocosDenshion;
using CocosSharp;
using System.Linq;

namespace CocosSharpSudoku
{
    public struct BoardSquare
    {
        public CCPoint Location; 
        public CCLabel Label;
        public CCRect Rect;
        public CCColor4B Color;
        public bool ShouldRedraw;
        public bool IsChosen;
        public bool IsClashing;

        public BoardSquare(CCPoint location, CCLabel label, CCRect rect) : this()
        {
            this.Location = location;
            this.Label = label;
            this.Rect = rect;
            this.ShouldRedraw = false;
            this.IsChosen = false;
            this.IsClashing = false;
            this.Color = CCColor4B.White;
        }
    }

    public struct Field
    {
        public int I, J;

        public Field(int i, int j)
        {
            this.I = i;
            this.J = j;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Field)) return false;

            Field other = (Field) obj;
            if (I == other.I && J == other.J) return true;
            else return false;
        }

        internal bool IsValid()
        {
            return (I > -1 && J > -1);
        }
    }

    public class GameLayer : CCLayerColor
    {
        CCSprite menuOption, newGame;
        CCLabel scoreLabel, scoreValue, timeValue;

        private Sudoku _fullSudoku;
        private Square[,] _currentSudoku;
        private BoardSquare[,] _board;
        private CCRect _bounds;
        private int _fieldSize;
        private CCSprite[] _numbers;
        private int _currentChosenNumber;
        private int _previouslyChosenNumber;
        private Field _previouslyChosenField;
        private Field _currentlyChosenField;
        private CCDrawNode _drawNode;

        private float _secondsSinceLastUpdate = 0.0f;
        private const float TIME_TO_UPDATE = 0.2f;

        public GameLayer() : base(Common.color1)
        {
            _fullSudoku = new Sudoku();
            _currentSudoku = new Square[9, 9];
            _board = new BoardSquare[9, 9];
            _numbers = new CCSprite[9];
            _currentChosenNumber = _previouslyChosenNumber = 0;
            _currentlyChosenField = new Field(-1, -1);
            _previouslyChosenField = new Field(-1, -1);

        }

        protected override void AddedToScene()
        {
            base.AddedToScene();

            // Use the bounds to layout the positioning of our drawable assets
            _bounds = VisibleBoundsWorldspace;
            _fieldSize = Convert.ToInt32((_bounds.Size.Width * 0.95f)/9);

            // Register for touch events
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesEnded = OnTouchesEnded;
            AddEventListener(touchListener, this);

            menuOption = new CCSprite("menu");
            menuOption.Scale = 0.05f;
            menuOption.Position = new CCPoint(_bounds.Size.Width * 0.8f, _bounds.Size.Height * 0.9f);

            newGame = new CCSprite("NewGamePlus");
            newGame.Scale = 0.05f;
            newGame.Position = new CCPoint(_bounds.Size.Width * 0.2f, _bounds.Size.Height * 0.9f);

            scoreLabel = new CCLabel("Score:", "Arial", 48, CCLabelFormat.SystemFont);
            scoreLabel.Position = new CCPoint(_bounds.Size.Width * 0.5f, _bounds.Size.Height * 0.95f);
            scoreLabel.Color = new CCColor3B(Common.color4);

            scoreValue = new CCLabel("0", "Arial", 48, CCLabelFormat.SystemFont);
            scoreValue.Position = new CCPoint(_bounds.Size.Width * 0.5f, _bounds.Size.Height * 0.90f);
            scoreValue.Color = new CCColor3B(Common.color4);

            timeValue = new CCLabel("00:00", "Arial", 48, CCLabelFormat.SystemFont);
            timeValue.Position = new CCPoint(_bounds.Size.Width * 0.5f, _bounds.Size.Height * 0.85f);
            timeValue.Color = new CCColor3B(Common.color4);

            AddChild(menuOption);
            AddChild(newGame);
            AddChild(scoreLabel);
            AddChild(scoreValue);
            AddChild(timeValue);

            DrawNumbers();
            DrawSudokuBoard();
            DrawSudokuGame();

            Schedule(RunGameLogic);
        }

        // frameTimeInSeconds is the number of seconds that have passed since the last time RunGameLogic was called
        // Is called 60 times a second
        private void RunGameLogic(float frameTimeInSeconds)
        {
            _secondsSinceLastUpdate += frameTimeInSeconds;
            if (_secondsSinceLastUpdate >= TIME_TO_UPDATE)
            {
                UpdateChosenField();
                CheckForClashingNumbers();
                UpdateBoard();
                DrawRegionBorders();
                _secondsSinceLastUpdate = 0.0f;
            }
        }

        private void CheckForClashingNumbers()
        {
            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
                {
                    if (NumberIsClashing(i, j))
                    {
                        _board[i, j].ShouldRedraw = true;
                        _board[i, j].IsClashing = true;
                        _board[i, j].Color = CCColor4B.Red;
                    }
                    else
                    {
                        if(_board[i,j].IsClashing) // number was clashing but is not any longer
                        {
                            _board[i, j].ShouldRedraw = true;
                            _board[i, j].Color = CCColor4B.White;
                        }
                        _board[i, j].IsClashing = false;
                    }
                }
            }
        }

        private void UpdateBoard()
        {
            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
                {
                    if (_board[i,j].ShouldRedraw)
                    {
                        if (_currentSudoku[i, j].Value != 0) // Find which number to draw
                        {
                            _board[i, j].Label.Text = _currentSudoku[i, j].Value.ToString();
                        }

                        if (_board[i, j].IsChosen && !_board[i,j].IsClashing) // Check for chosen number
                        {
                            _board[i, j].Color = Common.color1;
                        }
                        else if(!_board[i,j].IsChosen && !_board[i,j].IsClashing)
                        {
                            _board[i, j].Color = CCColor4B.White;
                        }
                        _drawNode.DrawRect(_board[i, j].Rect, fillColor: _board[i,j].Color /*colorToFill*/, borderWidth: 1, borderColor: Common.color5);
                        _board[i, j].ShouldRedraw = false;
                    }
                }
            }
        }

        private bool NumberIsClashing(int i, int j)
        {
            int value = _currentSudoku[i, j].Value;

            if (value == 0) return false; // Empty field

            if (ColumnContainsNumber(i, j, value)) return true;
            else if (RowContainsNumber(i, j, value)) return true;
            else if (RegionContainsNumber(i, j, value)) return true;
            else return false;
        }

        private bool RowContainsNumber(int i, int j, int value)
        {
            for (int r = 0; r < j; r++)
            {
                if (_currentSudoku[i, r].Value == value) return true;
            }
            for (int r = j + 1; r < 9; r++)
            {
                if (_currentSudoku[i, r].Value == value) return true;
            }
            return false;
        }

        private bool ColumnContainsNumber(int i, int j, int value)
        {
            for (int c = 0; c < i; c++)
            {
                if (_currentSudoku[c, j].Value == value) return true;
            }
            for (int c = i + 1; c < 9; c++)
            {
                if (_currentSudoku[c, j].Value == value) return true;
            }
            return false;
        }

        private bool RegionContainsNumber(int i, int j, int value)
        {
            if(i < 3) // top regions
            {
                if(j < 3) // left
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (c == i) continue;
                        for(int r = 0; r < 3; r++)
                        {
                            if (r == j) continue;

                            if (_currentSudoku[c, r].Value == value) return true;
                        }
                    }
                }
                else if (j < 6) // middle
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (c == i) continue;
                        for (int r = 3; r < 6; r++)
                        {
                            if (r == j) continue;

                            if (_currentSudoku[c, r].Value == value) return true;
                        }
                    }
                }
                else // right
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (c == i) continue;
                        for (int r = 6; r < 9; r++)
                        {
                            if (r == j) continue;

                            if (_currentSudoku[c, r].Value == value) return true;
                        }
                    }
                }
            }
            else if(i < 6) // middle regions
            {
                if (j < 3) // left
                {
                    for (int c = 3; c < 6; c++)
                    {
                        if (c == i) continue;
                        for (int r = 0; r < 3; r++)
                        {
                            if (r == j) continue;

                            if (_currentSudoku[c, r].Value == value) return true;
                        }
                    }
                }
                else if (j < 6) // middle
                {
                    for (int c = 3; c < 6; c++)
                    {
                        if (c == i) continue;
                        for (int r = 3; r < 6; r++)
                        {
                            if (r == j) continue;

                            if (_currentSudoku[c, r].Value == value) return true;
                        }
                    }
                }
                else // right
                {
                    for (int c = 3; c < 6; c++)
                    {
                        if (c == i) continue;
                        for (int r = 6; r < 9; r++)
                        {
                            if (r == j) continue;

                            if (_currentSudoku[c, r].Value == value) return true;
                        }
                    }
                }
            }
            else // right regions
            {
                if (j < 3) // left
                {
                    for (int c = 6; c < 9; c++)
                    {
                        if (c == i) continue;
                        for (int r = 0; r < 3; r++)
                        {
                            if (r == j) continue;

                            if (_currentSudoku[c, r].Value == value) return true;
                        }
                    }
                }
                else if (j < 6) // middle
                {
                    for (int c = 6; c < 9; c++)
                    {
                        if (c == i) continue;
                        for (int r = 3; r < 6; r++)
                        {
                            if (r == j) continue;

                            if (_currentSudoku[c, r].Value == value) return true;
                        }
                    }
                }
                else // right
                {
                    for (int c = 6; c < 9; c++)
                    {
                        if (c == i) continue;
                        for (int r = 6; r < 9; r++)
                        {
                            if (r == j) continue;

                            if (_currentSudoku[c, r].Value == value) return true;
                        }
                    }
                }
            }
            return false;
        }

        private void UpdateChosenField()
        {
            if (!_currentlyChosenField.Equals(_previouslyChosenField))
            {   
                if (_previouslyChosenField.IsValid()) // Not the first time any field is chosen
                {
                    _board[_previouslyChosenField.I, _previouslyChosenField.J].ShouldRedraw = true;
                    _board[_previouslyChosenField.I, _previouslyChosenField.J].IsChosen = false;
                }

                _board[_currentlyChosenField.I, _currentlyChosenField.J].ShouldRedraw = true;
                _board[_currentlyChosenField.I, _currentlyChosenField.J].IsChosen = true;

                _previouslyChosenField = _currentlyChosenField;
            }
        }

        private void DrawNumbers()
        {
            float percentageOfWidth = 0.1f;

            for(int i = 0; i < 9; i++)
            {
                _numbers[i] = new CCSprite((i+1).ToString());
                float percentageOfHeight = (i % 2 == 0) ? 0.1f : 0.15f;
                _numbers[i].Position = new CCPoint(_bounds.Size.Width * percentageOfWidth, _bounds.Size.Height * percentageOfHeight);
                _numbers[i].Scale = 0.15f;
                percentageOfWidth += 0.1f;
                AddChild(_numbers[i]);
            }
        }


        void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (touches.Count > 0)
            {
                Console.WriteLine(touches[0].Location.X + "," + touches[0].Location.Y);

                // Check for board fields touched
                Field clickedField = GetTouchedField(touches[0].Location);
                if (clickedField.IsValid())
                {
                    _board[clickedField.I, clickedField.J].ShouldRedraw = true;
                    _board[clickedField.I, clickedField.J].IsChosen = true;
                    _currentlyChosenField = clickedField;
                    Console.WriteLine("Square touched: " + clickedField.I + "," + clickedField.J+ " Value: " + _currentChosenNumber);
                }

                // Check for numbers touched
                for (int i = 0; i < 9; i++)
                {
                    if(_numbers[i].BoundingBoxTransformedToWorld.ContainsPoint(touches[0].Location))
                    {

                        _currentChosenNumber = i + 1;
                        Console.WriteLine("Number touched: " + (i + 1));
                        if(_currentlyChosenField.IsValid())
                        {
                            _currentSudoku[_currentlyChosenField.I, _currentlyChosenField.J].Value = _currentChosenNumber;
                            _board[_currentlyChosenField.I, _currentlyChosenField.J].ShouldRedraw = true;
                        }
                    }
                }
            }
        }

        private Field GetTouchedField(CCPoint location)
        {
            Field field = new Field(-1, -1);
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    float leftBorder = _board[i, j].Location.X - (_fieldSize / 2);
                    float rightBorder = _board[i, j].Location.X + (_fieldSize / 2);
                    float bottomBorder = _board[i, j].Location.Y - (_fieldSize / 2);
                    float topBorder = _board[i, j].Location.Y + (_fieldSize / 2);

                    if (location.X > leftBorder && location.X < rightBorder &&
                        location.Y > bottomBorder && location.Y < topBorder)
                    {
                        field.I = i;
                        field.J = j;
                    }
                }
            }
            return field;
        }

        private void DrawSudokuBoard()
        {
            _drawNode = new CCDrawNode();

            this.AddChild(_drawNode);

            // Origin is bottom-left of the screen. This moves
            // the _drawNode 100 pixels to the right and 100 pixels up
            float xBorder = (_bounds.Size.Width - (9 * _fieldSize)) / 2;
            float yBorder = (_bounds.Size.Height - (9 * _fieldSize)) / 2;
            _drawNode.PositionX = xBorder;
            _drawNode.PositionY = yBorder;


            // Draw all the 9 x 9 fields
            for (var i = 0; i < (9 * _fieldSize); i += _fieldSize)
            {
                for (var j = 0; j < (9 * _fieldSize); j += _fieldSize)
                {
                    int indexI = (i == 0 ? 0 : i / _fieldSize);
                    int indexJ = (j == 0 ? 0 : j / _fieldSize);
                    float x = i + xBorder + (_fieldSize / 2);
                    float y = j + yBorder + (_fieldSize / 2);
                    CCLabel label = new CCLabel(" ", "Arial", 50, CCLabelFormat.SystemFont);
                    CCPoint point = new CCPoint(x, y);
                    CCRect rect = new CCRect(i, j, _fieldSize, _fieldSize);
                    label.Position = point;
                    label.Color = new CCColor3B(Common.color5);
                    AddChild(label);

                    _board[indexI, indexJ] = new BoardSquare(point, label, rect);
                    _drawNode.DrawRect(rect,
                        fillColor: CCColor4B.White,
                        borderWidth: 1,
                        borderColor: Common.color5);
                }
            }

            DrawRegionBorders();

        }


        private void DrawRegionBorders()
        {
            // Draw the 9 big fields with thicker border
            int bigFieldSize = 3 * _fieldSize;
            for (var i = 0; i < (3 * bigFieldSize); i += bigFieldSize)
            {
                for (var j = 0; j < (3 * bigFieldSize); j += bigFieldSize)
                {
                    _drawNode.DrawRect(new CCRect(i, j, bigFieldSize, bigFieldSize),
                        fillColor: CCColor4B.Transparent,
                        borderWidth: 3,
                        borderColor: Common.color5);
                }
            }
        }

        private void DrawSudokuGame()
        {
            FillCurrentSudokuWithZeroes();
            List<Square> initialSquares = _fullSudoku.GetInitialNumbers(Difficulty.Normal);

            foreach(var square in initialSquares)
            {
                _currentSudoku[square.Column, square.Row].Value = square.Value;
                _board[square.Column, square.Row].Label.Text = square.Value.ToString();

            }
        }

        private void FillCurrentSudokuWithZeroes()
        {
            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
                {
                    _currentSudoku[i, j] = new Square(i, j);
                    _currentSudoku[i,j].Value = 0;
                }
            }
        }


        private void ShowMenu(object stuff = null)
        {
            CCLayerColor menuLayer = new GameMenuLayer();
            AddChild(menuLayer, 1000);
        }

        public static CCScene GameScene(CCGameView mainWindow)
        {
            var scene = new CCScene(mainWindow);
            var layer = new GameLayer();

            scene.AddChild(layer);

            return scene;
        }

        void EndGame()
        {
            // Stop scheduled events as we transition to game over scene
            UnscheduleAll();
        }
    }
}

