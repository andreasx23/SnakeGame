using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tensorflow.NumPy;

namespace SnakeGame
{
    public class GameAI
    {
        private readonly int _height;
        private readonly int _width;
        private char[][] _grid;
        public Snake Snake;
        public Food Food;
        private int _score;
        private int _steps;
        private int _frameInteration;
        private const int SLEEP_TIME_IN_MS = 375;

        public GameAI(int height, int width)
        {
            _height = height + 2;
            _width = width + 2;
            ResetGame();
        }

        public void ResetGame()
        {
            _grid = new char[_height][];

            for (int i = 0; i < _height; i++)
            {
                _grid[i] = new char[_width];
            }

            for (int i = 0; i < _height; i++)
            {
                _grid.First()[i] = (char)Entity.WALL;
                _grid.Last()[i] = (char)Entity.WALL;
            }

            for (int i = 0; i < _width; i++)
            {
                _grid[i][0] = (char)Entity.WALL;
                _grid[i][_width - 1] = (char)Entity.WALL;
            }

            for (int i = 1; i < _height - 1; i++)
            {
                for (int j = 1; j < _width - 1; j++)
                {
                    _grid[i][j] = (char)Entity.FLOOR;
                }
            }

            Snake = new Snake(_height / 2, _width / 2);
            UpdateSnakePositionOnGrid();
            GenerateFood();
            _score = 0;
            _steps = 0;
            _frameInteration = 0;
        }

        public (int reward, int score, bool isGameOver) PlayStep(NDArray action)
        {
            var (lastBodyX, lastBodyY) = Snake.Body.Last();
            Snake.UpdateSnakePosition();

            int reward = 0;
            if (IsCollision((Snake.HeadX, Snake.HeadY)) || _frameInteration > 100 * (Snake.Body.Count + 1))
            {
                UpdateSnakePositionOnGrid(lastBodyX, lastBodyY);
                Print();
                Console.WriteLine("GAME OVER!");
                reward = -10;
                return (reward, _score, true);
            }
            else if (!_grid.Any(row => row.Any(c => c == (char)Entity.FLOOR)))
            {
                UpdateSnakePositionOnGrid(lastBodyX, lastBodyY);
                Print();
                Console.WriteLine("WINNER!");
                reward = 10;
            }
            else if (_grid[Snake.HeadX][Snake.HeadY] == (char)Entity.FOOD)
            {
                Snake.IncreaseSize(Snake.HeadX, Snake.HeadY);
                GenerateFood();
                _score++;
                reward = 10;
            }

            UpdateSnakePositionOnGrid(lastBodyX, lastBodyY);
            Print();

            Snake.Direction = NextDirection(action);
            _steps++;
            _frameInteration++;

            return (reward, _score, false);
        }

        public bool IsCollision((int x, int y) point)
        {
            return _grid[point.x][point.y] == (char)Entity.WALL || Snake.Body.Contains(point);
        }

        #region Private helper functions
        private void UpdateSnakePositionOnGrid(int x = -1, int y = -1)
        {
            if (x != -1 && y != -1)
            {
                _grid[x][y] = (char)Entity.FLOOR;
            }

            var snakeCoordinates = Snake.GetSnakeCoordinates();
            for (int i = 1; i < snakeCoordinates.Count; i++)
            {
                var (bodyX, bodyY) = snakeCoordinates[i];
                _grid[bodyX][bodyY] = (char)Entity.BODY;
            }
            var headX = snakeCoordinates.First().x;
            var headY = snakeCoordinates.First().y;
            _grid[headX][headY] = (char)Entity.HEAD;
        }

        private Direction NextDirection(NDArray action)
        {
            Stopwatch watch = Stopwatch.StartNew();
            
            //STRAIGHT - RIGHT - LEFT
            Direction newDirection;
            List<Direction> clockWise = new List<Direction>() { Direction.RIGHT, Direction.DOWN, Direction.LEFT, Direction.UP };
            var currentDirectionIndex = clockWise.IndexOf(Snake.Direction);
            int n = clockWise.Count;
            if (np.array_equal(action, new NDArray(new int[] { 1, 0, 0 })))
            {
                newDirection = clockWise[currentDirectionIndex];
            }
            else if (np.array_equal(action, new NDArray(new int[] { 0, 1, 0 })))
            {
                newDirection = clockWise[(currentDirectionIndex + 1) % n];
            }
            else if (np.array_equal(action, new NDArray(new int[] { 0, 0, 1 })))
            {
                newDirection = clockWise[(currentDirectionIndex - 1) % n];
            }
            else
            {
                Console.WriteLine("ERROR!");
                throw new Exception();
            }

            while (watch.ElapsedMilliseconds < SLEEP_TIME_IN_MS) { }

            return newDirection;
        }

        private void GenerateFood()
        {
            Random rand = new Random();

            var x = rand.Next(1, _height - 1);
            var y = rand.Next(1, _width - 1);
            while (_grid[x][y] != (char)Entity.FLOOR)
            {
                x = rand.Next(1, _height - 1);
                y = rand.Next(1, _width - 1);
            }

            Food = new Food(x, y);
            _grid[Food.X][Food.Y] = (char)Entity.FOOD;
        }

        private void Print()
        {
            Console.Clear();
            Console.WriteLine($"# STEPS: {_steps} #");
            Console.WriteLine($"# SCORE: {_score} #");
            Console.WriteLine();

            foreach (var row in _grid)
            {
                Console.WriteLine(string.Join("", row));
            }
        }
        #endregion
    }
}
