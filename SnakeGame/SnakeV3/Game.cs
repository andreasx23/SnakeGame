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

        private NeuralNetwork _bestBoardBrain;
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
                for (int i = 0; i < 20000; i++)
                {
                    var load = NeuralNetwork.LoadNetwork();
                    _boards.Add(new Board(_height, _width, load));
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

                    var score = -1;
                    var bestScoreIndex = -1;
                    for (int i = 0; i < _boards.Count; i++)
                    {
                        if (_boards[i].Score > score)
                        {
                            score = _boards[i].Score;
                            bestScoreIndex = i;
                        }
                    }

                    if (_bestBoardBrain == null || score > _bestScore)
                    {
                        _bestBoardBrain = _boards[bestScoreIndex].Brain;
                        _bestScore = score;
                        _bestBoardBrain.SaveNetwork(_bestScore);
                        _boards[bestScoreIndex].PlayReplay();
                    }

                    Console.WriteLine($"Best score: {_bestScore}");
                    Console.WriteLine($"Generation: {_generation}");
                    for (int i = 0; i < _boards.Count; i++)
                    {
                        var child = _bestBoardBrain.Breed(_boards[i].Brain);
                        child.Mutate();
                        _boards[i] = new Board(_height, _width, child);
                    }
                    _generation++;
                }
            }
        }
    }
}
