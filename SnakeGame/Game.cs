using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeGame
{
    public class Game
    {
        private readonly int _height;
        private readonly int _width;
        private readonly char[][] _grid;
        private readonly Snake _snake;
        private Food _food;
        private int _score;
        private int _steps;

        public Game(int height, int width)
        {
            _height = height + 2;
            _width = width + 2;
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

            _snake = new Snake(_height / 2, _width / 2);
            UpdateSnakePositionOnGrid();
            GenerateFood();            
        }

        public void Play()
        {
            int sleepTimeInMs = 375;

            while (true)
            {
                var (lastBodyX, lastBodyY) = _snake.Body.Last();
                _snake.UpdateSnakePosition();

                var snakePos = _grid[_snake.HeadX][_snake.HeadY];
                if (snakePos == (char)Entity.WALL || _snake.Body.Contains((_snake.HeadX, _snake.HeadY)))
                {
                    UpdateSnakePositionOnGrid(lastBodyX, lastBodyY);
                    Print();
                    Console.WriteLine("GAME OVER!");
                    break;
                }
                else if (!_grid.Any(row => row.Any(c => c == (char)Entity.FLOOR)))
                {
                    UpdateSnakePositionOnGrid(lastBodyX, lastBodyY);
                    Print();
                    Console.WriteLine("WINNER!");
                    break;
                }
                else if (snakePos == (char)Entity.FOOD)
                {
                    _snake.IncreaseSize(_snake.HeadX, _snake.HeadY);
                    GenerateFood();
                    _score++;
                }

                UpdateSnakePositionOnGrid(lastBodyX, lastBodyY);
                Print();

                _snake.Direction = NextDirection(sleepTimeInMs);
                _steps++;
            }
        }

        #region Private helper functions
        private void UpdateSnakePositionOnGrid(int x = -1, int y = -1)
        {
            if (x != -1 && y != -1)
            {
                _grid[x][y] = (char)Entity.FLOOR;
            }

            var snakeCoordinates = _snake.GetSnakeCoordinates();            
            for (int i = 1; i < snakeCoordinates.Count; i++)
            {
                var (bodyX, bodyY) = snakeCoordinates[i];
                _grid[bodyX][bodyY] = (char)Entity.BODY;
            }
            var headX = snakeCoordinates.First().x;
            var headY = snakeCoordinates.First().y;
            _grid[headX][headY] = (char)Entity.HEAD;
        }

        private Direction NextDirection(int sleepTimeInMs)
        {
            Stopwatch watch = Stopwatch.StartNew();

            do
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKey consoleKey = Console.ReadKey(true).Key;
                    switch (consoleKey)
                    {
                        case ConsoleKey.A:
                        case ConsoleKey.LeftArrow:
                            if (_snake.Direction == Direction.LEFT || _snake.Direction == Direction.RIGHT && consoleKey == ConsoleKey.LeftArrow) break;
                            return Direction.LEFT;
                        case ConsoleKey.D:
                        case ConsoleKey.RightArrow:
                            if (_snake.Direction == Direction.RIGHT || _snake.Direction == Direction.LEFT && consoleKey == ConsoleKey.RightArrow) break;
                            return Direction.RIGHT;
                        case ConsoleKey.W:
                        case ConsoleKey.UpArrow:
                            if (_snake.Direction == Direction.UP || _snake.Direction == Direction.DOWN && consoleKey == ConsoleKey.UpArrow) break;
                            return Direction.UP;
                        case ConsoleKey.S:
                        case ConsoleKey.DownArrow:
                            if (_snake.Direction == Direction.DOWN || _snake.Direction == Direction.UP && consoleKey == ConsoleKey.DownArrow) break;
                            return Direction.DOWN;
                        case ConsoleKey.Escape:
                            Environment.Exit(1);
                            break;
                        default:
                            break;
                    }
                }
            } while (watch.ElapsedMilliseconds < sleepTimeInMs);

            return _snake.Direction;
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

            _food = new Food(x, y);
            _grid[_food.X][_food.Y] = (char)Entity.FOOD;
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

    public enum Entity
    {
        WALL = '#',
        FOOD = '@',
        HEAD = 'x',
        BODY = '+',
        FLOOR = ' '
    }
}
