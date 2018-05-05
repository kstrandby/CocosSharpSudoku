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
        public bool ShouldRedraw;

        public BoardSquare(CCPoint location, CCLabel label) : this()
        {
            this.Location = location;
            this.Label = label;
            this.ShouldRedraw = false;
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

        public GameLayer() : base(Common.color1)
        {
            _fullSudoku = new Sudoku();
            _currentSudoku = new Square[9, 9];
            _board = new BoardSquare[9, 9];
            _numbers = new CCSprite[9];
            _currentChosenNumber = _previouslyChosenNumber = 0;
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

        private void RunGameLogic(float frameTimeInSeconds)
        {
            UpdateChosenNumber();
            UpdateChosenField();
            UpdateBoard();
        }

        private void UpdateBoard()
        {
            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
                {
                    if (_board[i,j].ShouldRedraw)
                    {
                        _board[i, j].Label.Text = _currentSudoku[i, j].Value.ToString();
                        _board[i, j].ShouldRedraw = false;
                    }
                }
            }
            
        }

        private void UpdateChosenField()
        {
            //throw new NotImplementedException();
        }

        private void UpdateChosenNumber()
        {
            if(_currentChosenNumber != _previouslyChosenNumber)
            {

                string spriteFrame = _currentChosenNumber + "_selected";

                CCPoint position = _numbers[_currentChosenNumber - 1].Position;
                _numbers[_currentChosenNumber - 1].Dispose();
                _numbers[_currentChosenNumber - 1] = new CCSprite(spriteFrame);
                _numbers[_currentChosenNumber - 1].Scale = 0.15f;
                _numbers[_currentChosenNumber - 1].Position = position;
                AddChild(_numbers[_currentChosenNumber - 1]);

                if (_previouslyChosenNumber != 0)
                {
                    spriteFrame = _previouslyChosenNumber.ToString();

                    position = _numbers[_previouslyChosenNumber - 1].Position;
                    _numbers[_previouslyChosenNumber - 1].Dispose();
                    _numbers[_previouslyChosenNumber - 1] = new CCSprite(spriteFrame);
                    _numbers[_previouslyChosenNumber - 1].Scale = 0.15f;
                    _numbers[_previouslyChosenNumber - 1].Position = position;
                    AddChild(_numbers[_previouslyChosenNumber - 1]);

                    _previouslyChosenNumber = _currentChosenNumber;
                }
                else
                {
                    _previouslyChosenNumber = _currentChosenNumber;
                }
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

                Tuple<int,int> clickedSquare = GetTouchedSquare(touches[0].Location);
                if (clickedSquare != null)
                {
                    if(_currentChosenNumber != 0)
                    {
                        _currentSudoku[clickedSquare.Item1, clickedSquare.Item2].Value = _currentChosenNumber;
                        _board[clickedSquare.Item1, clickedSquare.Item2].ShouldRedraw = true;
                    }
                    Console.WriteLine("Square touched: " + clickedSquare.Item1 + "," + clickedSquare.Item2+ " Value: " + _currentChosenNumber);
                }

                for (int i = 0; i < 9; i++)
                {
                    if(_numbers[i].BoundingBoxTransformedToWorld.ContainsPoint(touches[0].Location))
                    {
                        _currentChosenNumber = i + 1;
                        Console.WriteLine("Number touched: " + (i + 1));
                    }
                }
            }
        }

        private Tuple<int, int> GetTouchedSquare(CCPoint location)
        {
            Tuple<int, int> indices = null;
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
                        indices = new Tuple<int, int>(i, j);
                    }
                }
            }
            return indices;
        }

        private void DrawSudokuBoard()
        {
            var drawNode = new CCDrawNode();

            this.AddChild(drawNode);

            // Origin is bottom-left of the screen. This moves
            // the drawNode 100 pixels to the right and 100 pixels up
            float xBorder = (_bounds.Size.Width - (9 * _fieldSize)) / 2;
            float yBorder = (_bounds.Size.Height - (9 * _fieldSize)) / 2;
            drawNode.PositionX = xBorder;
            drawNode.PositionY = yBorder;


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

                    label.Position = point;
                    label.Color = new CCColor3B(Common.color5);
                    AddChild(label);

                    _board[indexI, indexJ] = new BoardSquare(point, label);
                    drawNode.DrawRect(new CCRect(i, j, _fieldSize, _fieldSize),
                        fillColor: CCColor4B.White,
                        borderWidth: 1,
                        borderColor: Common.color5);
                }
            }

            // Draw the 9 big fields with thicker border
            int bigFieldSize = 3 * _fieldSize;
            for (var i = 0; i < (3 * bigFieldSize); i += bigFieldSize)
            {
                for (var j = 0; j < (3 * bigFieldSize); j += bigFieldSize)
                {
                    drawNode.DrawRect(new CCRect(i, j, bigFieldSize, bigFieldSize),
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

