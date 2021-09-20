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
        private List<(int x, int y)> _replayBody;
        private List<(int x, int y)> _replayFood;

        private GameObject[][] _grid;
        private readonly int _height;
        private readonly int _width;
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
                _replayBody = new List<(int x, int y)>();
                _replayFood = new List<(int x, int y)>();
                _replayBody.Add((_head.x, _head.y));
                _replayFood.Add((_food.Position.x, _food.Position.y));
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
            _replayBody = new List<(int x, int y)>();
            _replayFood = new List<(int x, int y)>();
            _replayBody.Add((_head.x, _head.y));
            _replayFood.Add((_food.Position.x, _food.Position.y));
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
                {
                    HumanMove();
                }

                if (FoodCollide(_food.Position.x, _food.Position.y))
                    Eat();

                if (!ShiftBody(_head.x, _head.y) || !_isHumanPlaying && _movesLeft <= 0)
                {
                    _gameState = GameState.DONE;

                    if (!_isHumanPlaying)
                        _replayFood.Add((_food.Position.x, _food.Position.y));
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
            _grid[_replayFood[foodIndex].x][_replayFood[foodIndex].y] = GameObject.FOOD;
            foreach (var (x, y) in _replayBody)
            {
                if (FoodCollide(_replayFood[foodIndex].x, _replayFood[foodIndex].y))
                {
                    Eat();
                    foodIndex++;
                    _grid[_replayFood[foodIndex].x][_replayFood[foodIndex].y] = GameObject.FOOD;
                }

                ShiftBody(x, y);
                PrintGrid();
            }

            _gameState = GameState.DONE;

            if (saveReplay)
                SaveReplay();
        }

        public void LoadReplay()
        {
            var file = "";
            List<List<(int x, int y)>> replay = JsonConvert.DeserializeObject<List<List<(int x, int y)>>>(file);
            _replayBody = replay.First();
            _replayFood = replay.Last();
            PlayReplay();
        }

        private void SaveReplay()
        {
            List<List<(int x, int y)>> replay = new List<List<(int x, int y)>>() { _replayBody, _replayFood };
            var replayBody = JsonConvert.SerializeObject(replay);
            string path = Utility.GetCurrentDirectoryPath();
            var directoryName = "SavedReplays";
            var directoryCombine = Path.Combine(path, directoryName);
            if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryCombine);
            var fileName = $"{Score}-{DateTime.Now}.txt";
            var fileCombine = Path.Combine(directoryCombine, fileName);
            var stream = File.Create(fileCombine);
            stream.Close();
            File.WriteAllText(fileCombine, replayBody);
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
            Console.Clear();
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
                    _replayFood.Add((_food.Position.x, _food.Position.y));

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
            {
                _head = (xPos, yPos);
            }

            bool isAlive = true;
            if (WallCollide() || BodyCollide()) isAlive = false;
            if (!_isHumanPlaying && _gameState != GameState.REPLAY) _replayBody.Add((_head.x, _head.y));

            _grid[_head.x][_head.y] = GameObject.HEAD;
            for (int i = 0; i < _body.Count; i++)
            {
                _grid[prev.x][prev.y] = GameObject.BODY;
                var temp = _body[i];
                _body[i] = prev;
                prev = temp;
            }
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
                var (x, y) = dirs[i];
                var dx = _head.x + x;
                var dy = _head.y + y;
                double value = Convert.ToDouble(_grid[dx][dy] == GameObject.FLOOR || _grid[dx][dy] == GameObject.FOOD);
                input[i] = value * 2 - 1;
            }

            input[4] = Convert.ToDouble(_food.Position.y < _head.y) * 2 - 1;
            input[5] = Convert.ToDouble(_food.Position.x > _head.x) * 2 - 1;
            input[6] = Convert.ToDouble(_food.Position.y > _head.y) * 2 - 1;
            input[7] = Convert.ToDouble(_food.Position.x < _head.x) * 2 - 1;

            var output = Brain.Compute(input).ToList();
            var max = output.Max();
            var maxIndex = output.IndexOf(max);

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
