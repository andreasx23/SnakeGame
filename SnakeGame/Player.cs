using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tensorflow.Binding;
using Tensorflow;
using Tensorflow.NumPy;
using static Tensorflow.KerasApi;
using Torch;
//using Keras.Datasets;
//using Keras;
//using Keras.Utils;
//using Numpy;
//using Keras.Models;
//using Keras.Layers;
//using Keras.Optimizers;

namespace SnakeGame
{
    public class Player
    {
        //https://towardsdatascience.com/today-im-going-to-talk-about-a-small-practical-example-of-using-neural-networks-training-one-to-6b2cbd6efdb3
        //https://github.com/SciSharp/Keras.NET

        private const int MAX_MEMORY = 100_000;
        private const int BATCH_SIZE = 1_000;
        private const double LEARNING_RATE = 0.001;

        private int _numberOfGames;
        private int _episolon;
        private readonly double _gamma;
        private readonly List<(NDArray state, int[] action, int reward, NDArray newState, bool isGameOver)> _memory;
        private readonly Model.LinearQNet _model;
        private readonly Model.QTrainer _trainer;

        public Player()
        {
            _numberOfGames = 0;
            _episolon = 0; //Randomness
            _gamma = 0.9; //Discount rate
            _memory = new List<(NDArray state, int[] action, int reward, NDArray newState, bool isGameOver)>();
            _model = new Model.LinearQNet(11, 256, 3);
            _trainer = new Model.QTrainer(_model, LEARNING_RATE, _gamma);
        }

        //https://www.youtube.com/watch?v=VGkcmBaeAGM
        public void Train(GameAI game)
        {
            int totalScore = 0;
            int bestScore = 0;

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKey consoleKey = Console.ReadKey(true).Key;
                    if (consoleKey == ConsoleKey.Escape)
                    {
                        Console.WriteLine("Quitting...");
                        Environment.Exit(1);
                    }
                }

                var oldState = GetState(game);
                var finalMove = GetAction(oldState);
                var (reward, score, isGameOver) = game.PlayStep(finalMove);
                var newState = GetState(game);
                TrainShortMemory(oldState, finalMove, reward, newState, isGameOver);
                Remember(oldState, finalMove, reward, newState, isGameOver);

                if (isGameOver)
                {
                    game.ResetGame();
                    _numberOfGames++;
                    TrainLongMemory();
                    if (score > bestScore)
                    {
                        bestScore = score;
                        _model.Save();
                    }
                    totalScore += score;
                }

                var mean = totalScore / _numberOfGames;
                Console.WriteLine($"Game: {_numberOfGames} -- Score: {score} -- Best score: {bestScore} -- Total score: {totalScore} -- Mean: {mean}");
            }
        }

        private NDArray GetState(GameAI game)
        {
            var snake = game.Snake;
            var headX = snake.HeadX;
            var headY = snake.HeadY;

            var pointLeft = (headX - 1, headY);
            var pointRight = (headX + 1, headY);
            var pointUp = (headX, headY - 1);
            var pointDown = (headX, headY + 1);

            var isDirectionLeft = game.Snake.Direction == Direction.LEFT;
            var isDirectionRight = game.Snake.Direction == Direction.RIGHT;
            var isDirectionUp = game.Snake.Direction == Direction.UP;
            var isDirectionDown = game.Snake.Direction == Direction.DOWN;

            int dangerStraight = Convert.ToInt32(isDirectionRight && game.IsCollision(pointRight) ||
                                                 isDirectionLeft && game.IsCollision(pointLeft) ||
                                                 isDirectionUp && game.IsCollision(pointUp) ||
                                                 isDirectionDown && game.IsCollision(pointDown));

            int dangerRight = Convert.ToInt32(isDirectionUp && game.IsCollision(pointUp) ||
                                                 isDirectionDown && game.IsCollision(pointDown) ||
                                                 isDirectionLeft && game.IsCollision(pointLeft) ||
                                                 isDirectionRight && game.IsCollision(pointRight));

            int dangerLeft = Convert.ToInt32(isDirectionDown && game.IsCollision(pointDown) ||
                                                 isDirectionUp && game.IsCollision(pointUp) ||
                                                 isDirectionRight && game.IsCollision(pointRight) ||
                                                 isDirectionLeft && game.IsCollision(pointLeft));

            int[][] state = new int[][]
            {
                new int[] { dangerStraight, dangerRight, dangerLeft },
                new int[] { Convert.ToInt32(isDirectionLeft), Convert.ToInt32(isDirectionRight), Convert.ToInt32(isDirectionUp), Convert.ToInt32(isDirectionDown) },
                new int[] { Convert.ToInt32(game.Food.X < headX), //Food left
                            Convert.ToInt32(game.Food.X > headX), //Food right
                            Convert.ToInt32(game.Food.Y < headY), //Food up
                            Convert.ToInt32(game.Food.Y > headY) } //Food down
            };

            return np.array(state, dtype: TF_DataType.TF_INT32);
        }

        private void Remember(NDArray state, int[] action, int reward, NDArray newState, bool isGameOver)
        {
            if (_memory.Count >= MAX_MEMORY)
                _memory.RemoveAt(0);

            _memory.add((state, action, reward, newState, isGameOver));
        }

        private void TrainLongMemory()
        {
            List<(NDArray state, int[] action, int reward, NDArray newState, bool isGameOver)> samples = new List<(NDArray state, int[] action, int reward, NDArray newState, bool isGameOver)>();

            if (_memory.Count > BATCH_SIZE)
            {
                Random rand = new Random();
                var next = rand.Next(0, BATCH_SIZE);
                HashSet<int> isVisited = new HashSet<int>();
                while (samples.Count < BATCH_SIZE)
                {
                    if (isVisited.Add(next))
                    {
                        var current = _memory[next];
                        samples.Add(current);
                    }
                    next = rand.Next(0, BATCH_SIZE);
                }
            }
            else
                samples = _memory;

            foreach (var (state, action, reward, nextState, isGameOver) in samples)
            {
                TrainShortMemory(state, action, reward, nextState, isGameOver);
            }
        }

        private void TrainShortMemory(NDArray state, int[] action, int reward, NDArray newState, bool isGameOver)
        {
            _trainer.TrainStep(state, action, reward, newState, isGameOver);
        }

        private int[] GetAction(NDArray state)
        {
            _episolon = 80 - _numberOfGames;
            int[] finalMove = new int[] { 0, 0, 0 };

            Random rand = new Random();
            int index;
            if (rand.Next(0, 201) < _episolon)
                index = rand.Next(0, 3);
            else
            {
                var npArray = Numpy.np.array(state.ToArray());
                var stateZero = torch.tensor(npArray);
                List<int[]> prediction = _model.Forward(stateZero).ToList();
                var max = prediction.Max();
                index = prediction.IndexOf(max);
            }

            finalMove[index] = 1;

            return finalMove;
        }
    }
}
