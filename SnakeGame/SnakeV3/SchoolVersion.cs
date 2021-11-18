using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame.SnakeV3
{
    public class SchoolVersion
    {
        private readonly int _height;
        private readonly int _width;
        private readonly bool _isHumanPlaying;

        public SchoolVersion(int height, int width, bool isHumanPlaying)
        {
            _height = height;
            _width = width;
            _isHumanPlaying = isHumanPlaying;
        }

        public void Play()
        {
            int goal = int.MaxValue;
            List<(int score, BigInteger fitness, int generation, int generationId)> results = new List<(int score, BigInteger fitness, int generation, int generationId)>();

            for (int i = 0; i < 30 * 2; i++)
                results.Add((0, 0, 0, 0));

            const int populationSize = 5000;
            for (int i = 1; i <= 30 * 2; i++)
            {
                List<Board> _boards = new List<Board>();
                NeuralNetwork _bestBrain = null;
                int _bestScore = 0;
                BigInteger _bestFitness = 0;
                int _generation = 1;
                
                for (int j = 0; j < populationSize; j++)
                    _boards.Add(new Board(_height, _width, _isHumanPlaying));

                bool isMatch = false;
                while (_generation * populationSize <= 150000)
                {
                    for (int j = 0; j < populationSize; j++)
                    {
                        _boards[j].GenerationId = (j + 1) + (_generation * populationSize);
                        _boards[j].Play();
                    }

                    int bestScoreThisGeneration = -1;
                    int bestScoreIndexThisGeneration = -1;
                    BigInteger bestFitnessThisGeneration = -1;
                    for (int j = 0; j < populationSize; j++)
                    {
                        if (_boards[j].Fitness > bestFitnessThisGeneration)
                        {
                            bestScoreIndexThisGeneration = j;
                            bestScoreThisGeneration = _boards[j].Score;
                            bestFitnessThisGeneration = _boards[j].Fitness;

                            if (_boards[j].Score >= goal)
                                break;
                        }
                    }

                    if (_bestBrain == null || bestFitnessThisGeneration > _bestFitness)
                    {
                        _bestBrain = _boards[bestScoreIndexThisGeneration].Brain.Clone();
                        _bestScore = Math.Max(_bestScore, bestScoreThisGeneration);
                        _bestFitness = bestFitnessThisGeneration;
                    }

                    if (_bestScore >= goal)
                    {
                        isMatch = true;
                        _bestBrain = _boards[bestScoreIndexThisGeneration].Brain.Clone();
                        _bestScore = Math.Max(_bestScore, bestScoreThisGeneration);
                        _bestFitness = BigInteger.Max(_bestFitness, bestFitnessThisGeneration);
                        _boards[bestScoreIndexThisGeneration].PlayReplay(true);
                        Console.WriteLine("FOUND WINNER!");
                        Console.WriteLine(_generation);
                        Console.WriteLine(_boards[bestScoreIndexThisGeneration].GenerationId);
                        results[i - 1] = (bestScoreThisGeneration, _bestFitness, _generation, _boards[bestScoreIndexThisGeneration].GenerationId);
                        break;
                    }

                    Console.WriteLine($"Currently using neurons: {Constants.NEURONS}");
                    Console.WriteLine($"Currently running game {i} out of {30 * 2}");
                    Console.WriteLine($"Current best fitness-score: {_bestFitness}");
                    Console.WriteLine($"Current best score: {_bestScore}");
                    Console.WriteLine($"Generation: {_generation}");
                    Console.WriteLine($"Population size: {populationSize}");
                    Console.WriteLine($"Total games played this session: {_generation * populationSize}");
                    Console.WriteLine($"Best score this generation: {bestScoreThisGeneration}");
                    Console.WriteLine($"Best fitness-score this generation: {bestFitnessThisGeneration}");
                    double averageScore = _boards.Average(b => b.Score);
                    Console.WriteLine($"Average score this generation: {averageScore}");
                    Console.WriteLine();

                    for (int j = 0; j < populationSize; j++)
                    {
                        NeuralNetwork child = _bestBrain.Breed(_boards[j].Brain);
                        child.Mutate();
                        _boards[j] = new Board(_height, _width, child);
                    }
                    _generation++;
                }

                if (!isMatch)
                {
                    results[i - 1] = (_bestScore, _bestFitness, _generation, populationSize * _generation);
                }

                switch (i)
                {
                    case 10 * 2:
                        Constants.NEURONS = 100;
                        break;
                    case 20 * 2:
                        Constants.NEURONS = 200;
                        break;
                    default:
                        break;
                }
            }

            Console.Clear();
            int index = 1;
            foreach (var (score, fitness, generation, generationId) in results)
            {
                Console.WriteLine($"Index: {index} -- {score}  {fitness}  {generation}  {generationId}");
                index++;
            }
        }
    }
}
