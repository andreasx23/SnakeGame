using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeGame.SnakeV3
{
    public class Game
    {
        private readonly List<Board> _boards;
        private readonly int _height;
        private readonly int _width;
        private readonly bool _isHumanPlaying;

        private NeuralNetwork _bestBrain;
        private int _bestScore = 0;
        private int _bestFitness = 0;
        private int _generation = 1;

        public Game(int height, int width, bool isHumanPlaying)
        {
            _boards = new List<Board>();
            _height = height;
            _width = width;
            _isHumanPlaying = isHumanPlaying;

            if (isHumanPlaying)
                _boards.Add(new Board(_height, _width, isHumanPlaying));
            else
            {
                bool shouldLoadPresavedNetworkFile = true;
                int populationSize = 5000;
                for (int i = 0; i < populationSize; i++)
                {
                    if (shouldLoadPresavedNetworkFile)
                    {
                        var load = NeuralNetwork.LoadNetwork();
                        _boards.Add(new Board(_height, _width, load));
                    }
                    else
                        _boards.Add(new Board(_height, _width, isHumanPlaying));
                }
            }
        }

        public void Play()
        {
            if (_isHumanPlaying)
            {
                var board = _boards.First();
                board.Play();
            }
            else
            {
                while (true)
                {
                    foreach (var board in _boards)
                    {
                        board.Play();
                    }

                    int n = _boards.Count;
                    var bestScoreThisGeneration = -1;
                    var bestScoreIndexThisGeneration = -1;
                    for (int i = 0; i < n; i++)
                    {
                        if (_boards[i].Score > bestScoreThisGeneration)
                        {
                            bestScoreThisGeneration = _boards[i].Score;
                            bestScoreIndexThisGeneration = i;
                        }
                    }

                    if (_bestBrain == null || bestScoreThisGeneration > _bestScore)
                    {
                        _bestBrain = _boards[bestScoreIndexThisGeneration].Brain;
                        _bestScore = bestScoreThisGeneration;
                        _bestBrain.SaveNetwork(_bestScore);
                        _boards[bestScoreIndexThisGeneration].PlayReplay();
                    }

                    Console.WriteLine($"Best score: {_bestScore}");
                    Console.WriteLine($"Generation: {_generation}");
                    Console.WriteLine($"Population size: {n}");
                    Console.WriteLine($"Total games played this session: {_generation * n}");
                    Console.WriteLine($"Best score this generation: {bestScoreThisGeneration}");
                    var averageScore = _boards.Average(b => b.Score);
                    Console.WriteLine($"Average score this generation: {averageScore}");
                    Console.WriteLine();
                    for (int i = 0; i < n; i++)
                    {
                        var child = _bestBrain.Breed(_boards[i].Brain);
                        child.Mutate();
                        _boards[i] = new Board(_height, _width, child);
                    }
                    _generation++;
                }
            }
        }
    }
}
