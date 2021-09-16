using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeGame.AI_V2
{
    public class SnakeAI
    {
        public const int SIZE = 20;
        public const int HIDDEN_NODES = 16;
        public const int HIDDEN_LAYERS = 2;
        public const int FPS = 100;

        public int Highscore = 0;

        public static float MUTATION_RATE = 0.05f;
        public static float defaultMutationRate = 0.05f;

        public const bool IS_HUMAN_PLAYING = false;
        public const bool REPLAY_BEST = true;
        public const bool SEE_VISION = false;
        public static bool MODEL_LOADED = false;

        public static List<int> Evolution;

        private Snake _snake;
        private Snake _model;
        private Population _population;

        public SnakeAI(int populationSize)
        {
            Setup(populationSize);
        }

        public void Play()
        {
            while (true)
            {
                Draw();
            }
        }

        private void Draw()
        {
            KeyPressed();

            if (IS_HUMAN_PLAYING)
            {
                _snake.Move();
                _snake.Show();
                if (_snake.Dead)
                {
                    Console.WriteLine("Game over!");
                    Console.WriteLine("Press escape to close or any other key to play again");
                    ConsoleKey consoleKey = Console.ReadKey(true).Key;
                    switch (consoleKey)
                    {
                        case ConsoleKey.Escape:
                            Environment.Exit(1);
                            break;
                        default:
                            _snake = new Snake(HIDDEN_LAYERS);
                            break;
                    }
                }
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
                    
                    if (!_population.BestSnake.Dead)
                    {
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    _model.Look();
                    _model.Think();
                    _model.Move();
                    _model.Show();
                    //_model.Brain.Show(0, 0, 360, 790, _model.Vision, _model.Decisions);
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
            var data = JsonConvert.SerializeObject(_population.BestSnake);


            //File.WriteAllText("", data);
        }

        #region Private helper methods
        private void Setup(int populationSize)
        {
            Evolution = new List<int>();
            if (IS_HUMAN_PLAYING)
                _snake = new Snake(HIDDEN_LAYERS);
            else
                _population = new Population(populationSize);
        }

        private void KeyPressed()
        {
            if (IS_HUMAN_PLAYING)
            {
                Stopwatch watch = Stopwatch.StartNew();
                int sleepTimeInMs = 125;

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
                                return;
                            case ConsoleKey.D:
                            case ConsoleKey.RightArrow:
                                _snake.MoveRight();
                                return;
                            case ConsoleKey.W:
                            case ConsoleKey.UpArrow:
                                _snake.MoveUp();
                                return;
                            case ConsoleKey.S:
                            case ConsoleKey.DownArrow:
                                _snake.MoveDown();
                                return;
                            case ConsoleKey.Escape:
                                Environment.Exit(1);
                                return;
                        }
                    }
                } while (watch.ElapsedMilliseconds < sleepTimeInMs);
            }
            else
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKey consoleKey = Console.ReadKey(true).Key;
                    switch (consoleKey)
                    {
                        case ConsoleKey.Escape:
                            Environment.Exit(1);
                            break;
                    }
                }
            }
        }
        #endregion
    }
}
