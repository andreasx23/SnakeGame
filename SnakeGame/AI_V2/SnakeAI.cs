using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame.AI_V2
{
    public class SnakeAI
    {
        public static int SIZE = 20;
        public const int HIDDEN_NODES = 16;
        public const int HIDDEN_LAYERS = 2;
        public const int FPS = 100;

        public int Highscore = 0;

        public static float MUTATION_RATE = 0.05f;
        public static float defaultMutationRate = 0.05f;

        public const bool IS_HUMAN_PLAYING = false;
        public const bool REPLAY_BEST = true;
        public const bool SEE_VISION = false;
        public const bool MODEL_LOADED = false;

        public static List<int> Evolution;

        public static char[][] Grid;
        public static int Height;
        public static int Width;

        private Snake _snake;
        private Snake _model;
        private Population _population;

        public SnakeAI(int populationSize)
        {
            InitGrid();
            //Print();
            Setup(populationSize);
        }

        public static void Print()
        {
            Console.WriteLine();
            //Console.Clear();
            //Console.WriteLine($"Highscore: {Highscore}");
            foreach (var row in Grid)
            {
                Console.WriteLine(string.Join("", row));
            }
        }

        public void Setup(int populationSize)
        {
            Evolution = new List<int>();
            if (IS_HUMAN_PLAYING)
                _snake = new Snake(HIDDEN_LAYERS);
            else
                _population = new Population(populationSize);
        }

        public void Draw()
        {
            if (IS_HUMAN_PLAYING)
            {
                _snake.Move();
                _snake.Show();
                Print();
                if (_snake.Dead) _snake = new Snake(HIDDEN_LAYERS);
            }
            else
            {
                if (!MODEL_LOADED)
                {
                    if (_population.Done())
                    {
                        Highscore = _population.BestSnake.Score;
                        _population.CalculateFitness();
                        _population.NaturalSelection();
                        InitGrid();
                    }
                    else
                    {
                        _population.Update();
                        _population.Show();
                    }

                    Console.WriteLine($"Generation: {_population.Generation}");
                    Console.WriteLine($"Best fitness: {_population.BestSnake.Fitness}");
                    Console.WriteLine($"Moves left: {_population.BestSnake.LifeLeft}");
                    Console.WriteLine($"Mutation rate: {MUTATION_RATE * 100}%");
                    Console.WriteLine($"Score: {_population.BestSnake.Score}");
                    Console.WriteLine($"Highscore: {Highscore}");
                }
                else
                {
                    _model.Look();
                    _model.Think();
                    _model.Move();
                    _model.Show();
                    _model.Brain.Show(0, 0, 360, 790, _model.Vision, _model.Decisions);
                    if (_model.Dead)
                    {
                        Snake newModel = new Snake(HIDDEN_LAYERS) { Brain = _model.Brain.Clone() };
                        _model = newModel;
                    }
                    Console.WriteLine($"Score: {_model.Score}");
                }
            }
        }

        public void FileSelectedIn()
        {

        }

        public void FileSelectedOut()
        {

        }

        public void KeyPressed()
        {
            if (IS_HUMAN_PLAYING)
            {
                Stopwatch watch = Stopwatch.StartNew();
                int sleepTimeInMs = 375;

                do
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKey consoleKey = Console.ReadKey(true).Key;
                        switch (consoleKey)
                        {
                            case ConsoleKey.A:
                            case ConsoleKey.LeftArrow:
                                _snake.MoveLeft();
                                break;
                            case ConsoleKey.D:
                            case ConsoleKey.RightArrow:
                                _snake.MoveRight();
                                break;
                            case ConsoleKey.W:
                            case ConsoleKey.UpArrow:
                                _snake.MoveUp();
                                break;
                            case ConsoleKey.S:
                            case ConsoleKey.DownArrow:
                                _snake.MoveDown();
                                break;
                            case ConsoleKey.Escape:
                                Environment.Exit(1);
                                break;
                            default:
                                Console.WriteLine("ERROR!");
                                break;
                        }
                    }
                } while (watch.ElapsedMilliseconds < sleepTimeInMs);
            }
        }

        public static char GetObject(Objects value)
        {
            return (char)value;
        }

        public enum Objects
        {
            WALL = '#',
            FLOOR = ' ',
            FOOD = '@',
            HEAD = 'x',
            BODY = '+'
        }

        #region Private helper methods
        private void InitGrid()
        {
            Grid = new char[SIZE + 2][];
            Height = Grid.Length;
            for (int i = 0; i < Height; i++)
            {
                Grid[i] = new char[SIZE + 2];
            }

            Width = Grid.First().Length;
            for (int i = 0; i < Height; i++)
            {
                Grid[i][0] = GetObject(Objects.WALL);
                Grid[i][Width - 1] = GetObject(Objects.WALL);
            }

            for (int i = 0; i < Width; i++)
            {
                Grid[0][i] = GetObject(Objects.WALL);
                Grid[Height - 1][i] = GetObject(Objects.WALL);
            }

            for (int i = 1; i < Height - 1; i++)
            {
                for (int j = 1; j < Width - 1; j++)
                {
                    Grid[i][j] = GetObject(Objects.FLOOR);
                }
            }
        }
        #endregion
    }
}
