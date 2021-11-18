using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        private BigInteger _bestFitness = 0;
        private int _generation = 1;
        private const int POPULATION_SIZE = 5000;

        public Game(int height, int width, bool isHumanPlaying)
        {
            _boards = new List<Board>();
            _height = height;
            _width = width;
            _isHumanPlaying = isHumanPlaying;

            if (isHumanPlaying)
                _boards.Add(new Board(height, width, isHumanPlaying));
            else
            {
                for (int j = 0; j < POPULATION_SIZE; j++)
                    _boards.Add(new Board(height, width, isHumanPlaying));

                bool shouldLoadPresavedNetworkFile = false;
                NeuralNetwork load = NeuralNetwork.LoadNetwork();
                for (int i = 0; i < POPULATION_SIZE; i++)
                {
                    if (shouldLoadPresavedNetworkFile)
                        _boards.Add(new Board(height, width, load.Clone()));
                    else
                        _boards.Add(new Board(height, width, isHumanPlaying));
                }
            }
        }

        public void Play()
        {
            //_boards.First().LoadReplay("Score 74 21-09-2021");
            //return;

            if (_isHumanPlaying)
            {
                Board board = _boards.First();
                board.Play();
            }
            else
            {
                while (true)
                {
                    for (int i = 0; i < POPULATION_SIZE; i++)
                    {
                        _boards[i].GenerationId = (i + 1) + (_generation * POPULATION_SIZE);
                        _boards[i].Play();
                    }

                    BigInteger totalFitnessScoreThisGeneration = BigInteger.Zero;
                    BigInteger bestFitnessThisGeneration = -1;
                    int bestScoreThisGeneration = -1;
                    int index = -1;
                    for (int i = 0; i < POPULATION_SIZE; i++)
                    {
                        totalFitnessScoreThisGeneration += _boards[i].Fitness;
                        if (_boards[i].Fitness > bestFitnessThisGeneration)
                        {
                            index = i;                            
                            bestFitnessThisGeneration = _boards[i].Fitness;
                        }

                        if (_boards[i].Score > bestScoreThisGeneration)
                            bestScoreThisGeneration = _boards[i].Score;
                    }

                    if (_bestBrain == null || bestFitnessThisGeneration > _bestFitness)
                    {
                        _bestBrain = _boards[index].Brain.Clone();
                        _bestScore = Math.Max(_bestScore, bestScoreThisGeneration);
                        _bestFitness = bestFitnessThisGeneration;
                        _bestBrain.SaveNetwork(_bestScore);
                        _boards[index].PlayReplay(true);
                    }

                    Console.WriteLine($"Currently using neurons: {Constants.NEURONS}");
                    Console.WriteLine($"Current best fitness-score: {_bestFitness}");
                    Console.WriteLine($"Current best score: {_bestScore}");
                    Console.WriteLine($"Generation: {_generation}");
                    Console.WriteLine($"Population size: {POPULATION_SIZE}");
                    Console.WriteLine($"Total games played this session: {_generation * POPULATION_SIZE}");
                    Console.WriteLine($"Best score this generation: {bestScoreThisGeneration}");
                    Console.WriteLine($"Best fitness-score this generation: {bestFitnessThisGeneration}");
                    Console.WriteLine($"Average score this generation: {_boards.Average(b => b.Score)}");
                    Console.WriteLine($"Average fitness score this generation: {totalFitnessScoreThisGeneration / POPULATION_SIZE}");
                    Console.WriteLine();

                    for (int i = 0; i < POPULATION_SIZE; i++)
                    {
                        NeuralNetwork child = _bestBrain.Breed(_boards[i].Brain);
                        child.Mutate();
                        _boards[i] = new Board(_height, _width, child);
                    }
                    _generation++;
                }
            }
        }
    }
}
