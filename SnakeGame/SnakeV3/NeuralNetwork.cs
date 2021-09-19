using AForge.Neuro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame.SnakeV3
{
    [Serializable]
    public class NeuralNetwork : ActivationNetwork
    {
        private const string DIRECTORY_NAME = "NeuralNetwork";
        private const string SAVE_LOAD_NETWORK_SUFFIX = "-NeuralNetworkSaveFile.txt";

        private readonly Random _rand;

        public NeuralNetwork(IActivationFunction function, int inputsCount, params int[] neuronsCount) : base(function, inputsCount, neuronsCount)
        {
            _rand = new Random();
        }

        public void SaveNetwork(int score)
        {
            var path = GetCurrentDirectoryPath();
            var directoryCombine = Path.Combine(path, DIRECTORY_NAME);
            if (!Directory.Exists(directoryCombine)) Directory.CreateDirectory(directoryCombine);

            var fileName = $"{score}{SAVE_LOAD_NETWORK_SUFFIX}";
            var fileCombine = Path.Combine(directoryCombine, fileName);
            if (!File.Exists(fileCombine))
            {
                var stream = File.Create(fileCombine);
                stream.Close();
            }
            Save(fileCombine);
        }

        public static NeuralNetwork LoadNetwork(int score = -1)
        {
            var path = GetCurrentDirectoryPath();
            var directoryCombine = Path.Combine(path, DIRECTORY_NAME);
            if (!Directory.Exists(directoryCombine)) return null;

            string fileCombine = string.Empty;
            if (score == -1)
            {
                var files = Directory.GetFiles(directoryCombine);
                int maxScore = -1;
                foreach (var filePathFromDirectory in files)
                {
                    var fileName = Path.GetFileName(filePathFromDirectory);
                    if (!fileName.Contains(SAVE_LOAD_NETWORK_SUFFIX)) continue;
                    var split = fileName.Split('-');
                    var currentScore = int.Parse(split.First());
                    if (currentScore > maxScore)
                    {
                        maxScore = currentScore;
                        fileCombine = filePathFromDirectory;
                    }
                }
            }
            else
            {
                string fileName = $"{score}{SAVE_LOAD_NETWORK_SUFFIX}";
                fileCombine = Path.Combine(directoryCombine, fileName);
            }

            if (!File.Exists(fileCombine)) return null;
            NeuralNetwork neuralNetwork = (NeuralNetwork)Load(fileCombine);
            return neuralNetwork;
        }

        private static string GetCurrentDirectoryPath()
        {
            var path = $@"{Environment.CurrentDirectory}";
            if (path.Contains("bin\\Debug")) path = path.Replace("bin\\Debug", "");
            else if (path.Contains("bin\\Release")) path = path.Replace("bin\\Release", "");
            return path;
        }

        public void Mutate()
        {
            if (_rand.NextDouble() < Constants.MUTATION_CHANCE)
            {
                int count = _rand.Next(1, Constants.MAX_MUTATION_COUNT);
                for (int i = 0; i < count; i++)
                {
                    int layerIndex = _rand.Next(layers.Length);
                    Layer layer = layers[layerIndex];
                    int neuronIndex = _rand.Next(layer.Neurons.Length);
                    Neuron neurons = layer.Neurons[neuronIndex];
                    int weightIndex = _rand.Next(neurons.Weights.Length);
                    neurons.Weights[weightIndex] = _rand.NextDouble();
                }
            }
        }

        public NeuralNetwork Breed(NeuralNetwork other)
        {
            double alpha = ChooseAlpha(other);
            NeuralNetwork child = new NeuralNetwork(new BipolarSigmoidFunction(alpha), Constants.INPUTS_COUNT, Constants.NEURONS, Constants.OUTPUT_COUNT);
            ChooseWeights(other, child);
            return child;
        }

        private void ChooseWeights(NeuralNetwork other, NeuralNetwork child)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                Layer myLayer = layers[i];
                Layer otherLayer = other.layers[i];
                for (int j = 0; j < myLayer.Neurons.Length; j++)
                {
                    ActivationNeuron myNeurons = (ActivationNeuron)myLayer.Neurons[j];
                    ActivationNeuron otherNeurons = (ActivationNeuron)otherLayer.Neurons[j];
                    for (int k = 0; k < myNeurons.Weights.Length; k++)
                    {
                        ActivationNeuron neuronToApply = (ActivationNeuron)child.layers[i].Neurons[j];
                        double rand = _rand.NextDouble();
                        double range = Math.Abs(myNeurons.Weights[k] - otherNeurons.Weights[k]);
                        double min = Math.Min(myNeurons.Weights[k], otherNeurons.Weights[k]);
                        double weight = rand * range + min;
                        neuronToApply.Weights[k] = weight;
                        neuronToApply.Threshold = 0;
                    }
                }
            }
        }

        private double ChooseAlpha(NeuralNetwork other)
        {
            int rand = _rand.Next(0, 3);
            double ourselfAlpha = GetNetworkAlpha(this);
            double otherAlpha = GetNetworkAlpha(other);

            switch (rand)
            {
                case 0: return ourselfAlpha;
                case 1: return otherAlpha;
                case 2: return _rand.NextDouble();
                default: throw new Exception("Invalid value exception");
            }
        }

        private double GetNetworkAlpha(NeuralNetwork network)
        {
            var activationFunction = (BipolarSigmoidFunction)((ActivationNeuron)network.Layers[0].Neurons[0]).ActivationFunction;
            return activationFunction.Alpha;
        }
    }
}
