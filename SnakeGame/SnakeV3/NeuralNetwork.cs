using AForge.Neuro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public NeuralNetwork Clone()
        {
            double alpha = GetNetworkAlpha(this);
            NeuralNetwork clone = NewNeuralNetwork(alpha);

            Layer[] myLayers = layers;
            for (int i = 0; i < myLayers.Length; i++)
            {
                ActivationLayer myLayer = (ActivationLayer)myLayers[i];
                ActivationLayer cloneLayer = (ActivationLayer)clone.layers[i];
                for (int j = 0; j < myLayer.Neurons.Length; j++)
                {
                    ActivationNeuron myNeuron = (ActivationNeuron)myLayer.Neurons[j];
                    ActivationNeuron cloneNeuron = (ActivationNeuron)cloneLayer.Neurons[j];
                    double threshold = myNeuron.Threshold;
                    cloneNeuron.Threshold = threshold;
                    for (int k = 0; k < myNeuron.Weights.Length; k++)
                    {
                        double weight = myNeuron.Weights[k];
                        cloneNeuron.Weights[k] = weight;
                    }
                }
            }

            return clone;
        }

        public void SaveNetwork(int score)
        {
            string currentDirectoryPath = Utility.GetCurrentDirectoryPath();
            string directoryCombine = Path.Combine(currentDirectoryPath, DIRECTORY_NAME);
            directoryCombine = Path.Combine(directoryCombine, "Result");
            if (!Directory.Exists(directoryCombine)) Directory.CreateDirectory(directoryCombine);

            string fileName = $"{score}{SAVE_LOAD_NETWORK_SUFFIX}";
            string fileCombine = Path.Combine(directoryCombine, fileName);
            if (!File.Exists(fileCombine))
            {
                FileStream stream = File.Create(fileCombine);
                stream.Close();
            }
            Save(fileCombine);
        }

        /// <summary>
        /// Load a presaved network file
        /// </summary>
        /// <param name="score">Score of the presaved network file. Leave blank to select file with highest score</param>
        /// <returns>Returns a presaved network file if one exists else null</returns>
        public static NeuralNetwork LoadNetwork(int score = -1)
        {
            string currentDirectoryPath = Utility.GetCurrentDirectoryPath();
            string directoryCombine = Path.Combine(currentDirectoryPath, DIRECTORY_NAME);
            if (!Directory.Exists(directoryCombine)) return null;

            string fileCombine = string.Empty;
            if (score == -1)
            {
                IEnumerable<string> files = Directory.GetFiles(directoryCombine).Where(file => file.EndsWith(SAVE_LOAD_NETWORK_SUFFIX));
                int maxScore = -1;
                foreach (var filePathFromDirectory in files)
                {
                    string fileName = Path.GetFileName(filePathFromDirectory);
                    string[] split = fileName.Split('-');
                    if (int.TryParse(split.First(), out int currentScore) && currentScore > maxScore)
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

            if (string.IsNullOrEmpty(fileCombine) || !File.Exists(fileCombine)) return null;

            NeuralNetwork neuralNetwork = (NeuralNetwork)Load(fileCombine);
            return neuralNetwork;
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
                    Neuron neuron = layer.Neurons[neuronIndex];
                    int weightIndex = _rand.Next(neuron.Weights.Length);
                    neuron.Weights[weightIndex] = _rand.NextDouble();
                }
            }
        }

        public NeuralNetwork Breed(NeuralNetwork other)
        {
            double alpha = ChooseAlpha(other);
            NeuralNetwork child = NewNeuralNetwork(alpha);
            ChooseWeights(other, child);
            return child;
        }

        #region Private helper methods
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
            double myAlpha = GetNetworkAlpha(this);
            double otherAlpha = GetNetworkAlpha(other);

            switch (rand)
            {
                case 0: return myAlpha;
                case 1: return otherAlpha;
                case 2: return _rand.NextDouble();
                default: throw new Exception("Invalid value");
            }
        }

        private double GetNetworkAlpha(NeuralNetwork network)
        {
            var activationFunction = (BipolarSigmoidFunction)((ActivationNeuron)network.Layers[0].Neurons[0]).ActivationFunction;
            return activationFunction.Alpha;
        }

        private NeuralNetwork NewNeuralNetwork(double alpha = -1d)
        {
            if (alpha != -1d)
                return new NeuralNetwork(new BipolarSigmoidFunction(alpha), Constants.INPUTS_COUNT, Constants.NEURONS, Constants.OUTPUT_COUNT);
            else
                return new NeuralNetwork(new BipolarSigmoidFunction(), Constants.INPUTS_COUNT, Constants.NEURONS, Constants.OUTPUT_COUNT);
        }
        #endregion
    }
}
