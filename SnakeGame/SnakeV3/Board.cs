using AForge.Neuro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeGame.SnakeV3
{
    public class Board
    {
        private readonly bool _isHumanPlaying;

        private int _movesLeft = 200;
        private int _totalMoves = 0;
        private ReplayDTO _replay;

        private GameObject[][] _grid;
        private int _height;
        private int _width;
        private GameState _gameState;
        private readonly Food _food;
        public int Score;

        private (int x, int y) _head;
        private List<(int x, int y)> _body;
        private Direction _direction;

        public NeuralNetwork Brain;
        public int Fitness = 0;

        public Board(int height, int width, bool isHumanPlaying)
        {
            _height = height + 2;
            _width = width + 2;
            InitBoardAndSnake();
            _food = new Food();
            _food.GenerateFood(_grid);
            _gameState = GameState.PLAYING;
            _isHumanPlaying = isHumanPlaying;

            if (!_isHumanPlaying)
            {
                Brain = new NeuralNetwork(new BipolarSigmoidFunction(Constants.ALPHA), Constants.INPUTS_COUNT, Constants.NEURONS, Constants.OUTPUT_COUNT);
                _replay = new ReplayDTO(_height, _width);
                _replay.ReplayBody.Add((_head.x, _head.y));
                _replay.ReplayFood.Add((_food.Position.x, _food.Position.y));
            }
        }

        public Board(int height, int width, NeuralNetwork brain)
        {
            _height = height + 2;
            _width = width + 2;
            InitBoardAndSnake();
            _food = new Food();
            _food.GenerateFood(_grid);
            _gameState = GameState.PLAYING;
            _isHumanPlaying = false;
            Brain = brain;
            _replay = new ReplayDTO(_height, _width);
            _replay.ReplayBody.Add((_head.x, _head.y));
            _replay.ReplayFood.Add((_food.Position.x, _food.Position.y));
        }

        public void Play()
        {
            while (_gameState == GameState.PLAYING)
            {
                if (!_isHumanPlaying)
                {
                    _movesLeft--;
                    _totalMoves++;
                    AIMove();
                }
                else
                    HumanMove();

                if (FoodCollide(_food.Position.x, _food.Position.y))
                    Eat();

                if (!ShiftBody(_head.x, _head.y) || !_isHumanPlaying && _movesLeft <= 0)
                {
                    _gameState = GameState.DONE;

                    if (!_isHumanPlaying)
                        _replay.ReplayFood.Add((_food.Position.x, _food.Position.y));
                }

                if (_isHumanPlaying)
                    PrintGrid();
            }

            if (_isHumanPlaying)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("GAME OVER!");
            }
            else
                CalculateFitness();
        }

        public void PlayReplay(bool saveReplay = false)
        {
            _gameState = GameState.REPLAY;
            InitBoardAndSnake();

            Console.Clear();
            Console.WriteLine("Replay");

            int foodIndex = 0;
            _grid[_replay.ReplayFood[foodIndex].x][_replay.ReplayFood[foodIndex].y] = GameObject.FOOD;
            foreach (var (x, y) in _replay.ReplayBody)
            {
                if (FoodCollide(_replay.ReplayFood[foodIndex].x, _replay.ReplayFood[foodIndex].y))
                {
                    Eat();
                    foodIndex++;
                    _grid[_replay.ReplayFood[foodIndex].x][_replay.ReplayFood[foodIndex].y] = GameObject.FOOD;
                }

                ShiftBody(x, y);
                PrintGrid();
            }

            if (saveReplay)
                _replay.SaveReplay(Score);

            _gameState = GameState.DONE;
        }

        public void LoadReplay(string replayFileName)
        {
            var reply = ReplayDTO.LoadReplay(replayFileName);
            _height = reply.Height;
            _width = reply.Width;
            _replay = reply;
            PlayReplay();
        }

        public void MoveLeft()
        {
            if (_direction != Direction.RIGHT)
                _direction = Direction.LEFT;
        }

        public void MoveRight()
        {
            if (_direction != Direction.LEFT)
                _direction = Direction.RIGHT;
        }

        public void MoveUp()
        {
            if (_direction != Direction.DOWN)
                _direction = Direction.UP;
        }

        public void MoveDown()
        {
            if (_direction != Direction.UP)
                _direction = Direction.DOWN;
        }

        private void CalculateFitness()
        {
            if (Score < 10)
            {
                Fitness = _totalMoves * _totalMoves + (int)Math.Pow(2, Score);
            }
            else
            {
                Fitness = _totalMoves * _totalMoves;
                Fitness *= (int)Math.Pow(2, 10);
                Fitness *= Score - 9;
            }
        }

        private void PrintGrid()
        {
            Utility.ClearConsole();
            Console.WriteLine($"Score: {Score}");

            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    Console.Write(MapObject(_grid[i][j]));
                }
                Console.WriteLine();
            }

            if (!_isHumanPlaying && _gameState == GameState.REPLAY)
                Thread.Sleep(Constants.FPS);
        }

        private char MapObject(GameObject obj)
        {
            switch (obj)
            {
                case GameObject.WALL: return '#';
                case GameObject.FLOOR: return ' ';
                case GameObject.HEAD: return 'x';
                case GameObject.FOOD: return '@';
                case GameObject.BODY: return '+';
                default: throw new Exception("Invalid GameObject exception");
            }
        }

        private void Eat()
        {
            Score++;
            var (x, y) = _body.Last();
            _body.Add((x, y));

            if (_gameState != GameState.REPLAY)
            {
                _food.GenerateFood(_grid);
                if (!_isHumanPlaying)
                {
                    _replay.ReplayFood.Add((_food.Position.x, _food.Position.y));

                    if (_movesLeft < 500)
                    {
                        if (_movesLeft > 400)
                            _movesLeft = 500;
                        else
                            _movesLeft += 100;
                    }
                }
            }
        }

        private bool ShiftBody(int xPos, int yPos)
        {
            var prev = _head;
            if (_gameState != GameState.REPLAY)
            {
                var (x, y) = NextDirection();
                _head = (xPos + x, yPos + y);
            }
            else
                _head = (xPos, yPos);

            bool isAlive = true;
            if (WallCollide() || BodyCollide()) isAlive = false;
            if (!_isHumanPlaying && _gameState != GameState.REPLAY) _replay.ReplayBody.Add((_head.x, _head.y));

            for (int i = 0; i < _body.Count; i++)
            {
                _grid[prev.x][prev.y] = GameObject.BODY;
                var temp = _body[i];
                _body[i] = prev;
                prev = temp;
            }
            _grid[_head.x][_head.y] = GameObject.HEAD;
            _grid[prev.x][prev.y] = GameObject.FLOOR;

            return isAlive;
        }

        private bool FoodCollide(int x, int y)
        {
            return _head.x == x && _head.y == y;
        }

        private bool WallCollide()
        {
            return _grid[_head.x][_head.y] == GameObject.WALL;
        }

        private bool BodyCollide()
        {
            return _body.Contains(_head);
        }

        private (int x, int y) NextDirection()
        {
            switch (_direction)
            {
                case Direction.UP: return (-1, 0);
                case Direction.LEFT: return (0, -1);
                case Direction.DOWN: return (1, 0);
                case Direction.RIGHT: return (0, 1);
                default: throw new Exception("Invalid direction exception");
            }
        }

        private void AIMove()
        {
            List<(int x, int y)> dirs = new List<(int x, int y)>()
            {
                (0, -1), //Left
                (1, 0), //Down
                (0, 1), //Right
                (-1, 0) //Up
            };

            double[] input = new double[Constants.INPUTS_COUNT];
            for (int i = 0; i < dirs.Count; i++)
            {
                (int x, int y) = dirs[i];
                int dx = _head.x + x;
                int dy = _head.y + y;
                double value = Convert.ToDouble(_grid[dx][dy] == GameObject.FLOOR || _grid[dx][dy] == GameObject.FOOD);
                input[i] = value;
            }

            input[4] = Convert.ToDouble(_food.Position.y < _head.y);
            input[5] = Convert.ToDouble(_food.Position.x > _head.x);
            input[6] = Convert.ToDouble(_food.Position.y > _head.y);
            input[7] = Convert.ToDouble(_food.Position.x < _head.x);

            for (int i = 0; i < input.Length; i++)
            {
                input[i] = input[i] * 2 - 1;
            }

            List<double> output = Brain.Compute(input).ToList();
            double max = output.Max();
            int maxIndex = output.IndexOf(max);
            switch (maxIndex)
            {
                case 0:
                    MoveUp();
                    break;
                case 1:
                    MoveDown();
                    break;
                case 2:
                    MoveLeft();
                    break;
                case 3:
                    MoveRight();
                    break;
                default:
                    throw new Exception("Invald index exception");
            }
        }

        private void HumanMove()
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
                            if (_direction == Direction.LEFT) break;
                            MoveLeft();
                            return;
                        case ConsoleKey.D:
                        case ConsoleKey.RightArrow:
                            if (_direction == Direction.RIGHT) break;
                            MoveRight();
                            return;
                        case ConsoleKey.W:
                        case ConsoleKey.UpArrow:
                            if (_direction == Direction.UP) break;
                            MoveUp();
                            return;
                        case ConsoleKey.S:
                        case ConsoleKey.DownArrow:
                            if (_direction == Direction.DOWN) break;
                            MoveDown();
                            return;
                        case ConsoleKey.Escape:
                            Environment.Exit(1);
                            return;
                    }
                }
            } while (watch.ElapsedMilliseconds < Constants.FPS);
        }

        private void InitBoardAndSnake()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Score = 0;
            _grid = new GameObject[_height][];
            for (int i = 0; i < _height; i++)
            {
                _grid[i] = new GameObject[_width];
            }

            for (int i = 0; i < _height; i++)
            {
                _grid[i][0] = GameObject.WALL;
                _grid[i][_height - 1] = GameObject.WALL;
            }

            for (int i = 0; i < _width; i++)
            {
                _grid[0][i] = GameObject.WALL;
                _grid[_height - 1][i] = GameObject.WALL;
            }

            for (int i = 1; i < _height - 1; i++)
            {
                for (int j = 1; j < _width - 1; j++)
                {
                    _grid[i][j] = GameObject.FLOOR;
                }
            }

            _head = (_height / 2, _width / 2);
            _grid[_head.x][_head.y] = GameObject.HEAD;
            _body = new List<(int x, int y)>();
            for (int i = 1; i <= 2; i++)
            {
                var coord = (_head.x, _head.y + i);
                _body.Add(coord);
                _grid[coord.x][coord.Item2] = GameObject.BODY;
            }
            _direction = Direction.LEFT;
        }
    }
}
