using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame.AI_V2
{
    public class Population
    {
        private Snake[] _snakes;
        public Snake BestSnake;

        private int _bestSnakeScore = 0;
        public int Generation = 0;
        private int _sameBest = 0;

        private float _bestFitness = 0;
        private float _fitnessSum = 0;

        public Population(int size)
        {
            _snakes = new Snake[size];
            for (int i = 0; i < size; i++)
            {
                _snakes[i] = new Snake(SnakeAI.HIDDEN_LAYERS);
            }
            BestSnake = _snakes.First().Clone();
            BestSnake.Replay = true;
        }

        public bool Done()
        {
            if (!BestSnake.Dead || _snakes.Any(s => !s.Dead))
                return false;
            else
                return true;
        }

        public void Update()
        {
            if (!BestSnake.Dead)
            {
                BestSnake.Look();
                BestSnake.Think();
                BestSnake.Move();
            }

            foreach (var snake in _snakes)
            {
                if (!snake.Dead)
                {
                    snake.Look();
                    snake.Think();
                    snake.Move();
                }
            }
        }

        public void Show()
        {
            if (SnakeAI.REPLAY_BEST)
            {
                BestSnake.Show();
                //BestSnake.Brain.Show(0, 0, 360, 790, BestSnake.Vision, BestSnake.Decisions);
            }
            else
            {
                foreach (var snake in _snakes)
                    snake.Show();
            }
        }

        public void SetBestSnake()
        {
            float bestFitnessScore = 0;
            int bestFitnessScoreIndex = 0;


            float currFitness = 0;
            int index = 0;
            int sum = 0;
            for (int i = 0; i < _snakes.Length; i++)
            {
                if (_snakes[i].Fitness > bestFitnessScore)
                {
                    bestFitnessScore = _snakes[i].Fitness;
                    bestFitnessScoreIndex = i;
                }

                if (_snakes[i].sum > sum)
                {
                    sum = _snakes[i].sum;
                    index = i;
                    currFitness = _snakes[i].Fitness;
                }
                else if (_snakes[i].sum == sum && _snakes[i].Fitness > bestFitnessScore)
                {
                    sum = _snakes[i].sum;
                    index = i;
                    currFitness = _snakes[i].Fitness;
                }
            }

            if (bestFitnessScore > _bestFitness)
            {
                _bestFitness = bestFitnessScore;
                BestSnake = _snakes[bestFitnessScoreIndex].CloneForReplay();
                _bestSnakeScore = _snakes[bestFitnessScoreIndex].Score;
                _sameBest = 0;
                SnakeAI.MUTATION_RATE = SnakeAI.defaultMutationRate;
            }
            else if (sum > BestSnake.sum)
            {
                _bestFitness = currFitness;
                BestSnake = _snakes[index].CloneForReplay();
                _bestSnakeScore = _snakes[index].Score;
                _sameBest = 0;
                SnakeAI.MUTATION_RATE = SnakeAI.defaultMutationRate;
            }
            else if (sum == BestSnake.sum && currFitness > BestSnake.Fitness)
            {
                _bestFitness = currFitness;
                BestSnake = _snakes[index].CloneForReplay();
                _bestSnakeScore = _snakes[index].Score;
                _sameBest = 0;
                SnakeAI.MUTATION_RATE = SnakeAI.defaultMutationRate;
            }
            else
            {
                BestSnake = BestSnake.CloneForReplay();
                if (_sameBest > 2)
                {
                    _sameBest = 0;
                    SnakeAI.MUTATION_RATE *= 2f;
                }
                else
                    _sameBest++;
            }
        }

        public Snake SelectParent()
        {
            float randValue = Utility.NextFloat(0, _fitnessSum);
            float summation = 0;
            for (int i = 0; i < _snakes.Length; i++)
            {
                summation += _snakes[i].Fitness;
                if (summation > randValue)
                    return _snakes[i];
            }

            return _snakes.First();
        }

        public void NaturalSelection()
        {
            int n = _snakes.Length;
            Snake[] newSnakes = new Snake[n];
            SetBestSnake();
            CalculateFitnessSum();
            newSnakes[0] = BestSnake.Clone();
            for (int i = 1; i < n; i++)
            {
                Snake child = SelectParent().Crossover(SelectParent());
                child.Mutate();
                newSnakes[i] = child;
            }
            _snakes = newSnakes; //(Snake[])newSnakes.Clone(); //Might need fixing
            SnakeAI.Evolution.Add(_bestSnakeScore);
            Generation++;
        }

        public void Mutate()
        {
            for (int i = 1; i < _snakes.Length; i++)
                _snakes[i].Mutate();
        }

        public void CalculateFitness()
        {
            foreach (var snake in _snakes)
                snake.CalculateFitness();
        }

        private void CalculateFitnessSum()
        {
            _fitnessSum = _snakes.Sum(s => s.Fitness);
        }
    }
}
