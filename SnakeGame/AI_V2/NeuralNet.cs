using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame.AI_V2
{
    public class NeuralNet
    {
        //Using natural selection
        private readonly int _inputNodes;
        private readonly int _hiddenNodes;
        private readonly int _outputNodes;
        private readonly int _hiddenLayers;
        private readonly Matrix[] _weights;

        public NeuralNet(int input, int hidden, int output, int hiddenLayers)
        {
            _inputNodes = input;
            _hiddenNodes = hidden;
            _outputNodes = output;
            _hiddenLayers = hiddenLayers;

            _weights = new Matrix[_hiddenLayers + 1];
            _weights[0] = new Matrix(_hiddenNodes, _inputNodes + 1);
            for (int i = 1; i < _hiddenLayers; i++)
            {
                _weights[i] = new Matrix(_hiddenNodes, _hiddenNodes + 1);
            }
            _weights[_weights.Length - 1] = new Matrix(_outputNodes, _hiddenNodes + 1);

            foreach (var weight in _weights)
            {
                weight.Randomize();
            }
        }

        public void Mutate(float mutationRate)
        {
            foreach (var weight in _weights)
            {
                weight.Mutate(mutationRate);
            }
        }

        public float[] Output(float[] inputArray)
        {
            Matrix inputs = _weights.First().SingleColumnMatrixFromArray(inputArray);
            Matrix currentBias = inputs.AddBias();

            for (int i = 0; i < _hiddenLayers; i++)
            {
                Matrix hiddenIP = _weights[i].Dot(currentBias);
                Matrix hiddenOP = hiddenIP.Activate();
                currentBias = hiddenOP.AddBias();
            }

            Matrix outputIP = _weights[_weights.Length - 1].Dot(currentBias);
            Matrix output = outputIP.Activate();

            return output.ToArray();
        }

        public NeuralNet Crossover(NeuralNet partner)
        {
            NeuralNet child = new NeuralNet(_inputNodes, _hiddenNodes, _outputNodes, _hiddenLayers);

            for (int i = 0; i < _weights.Length; i++)
            {
                child._weights[i] = _weights[i].Crossover(partner._weights[i]);
            }

            return child;
        }

        public NeuralNet Clone()
        {
            NeuralNet clone = new NeuralNet(_inputNodes, _hiddenNodes, _outputNodes, _hiddenLayers);

            for (int i = 0; i < _weights.Length; i++)
            {
                clone._weights[i] = _weights[i].Clone();
            }

            return clone;
        }

        public void Load(Matrix[] weights)
        {
            for (int i = 0; i < _weights.Length; i++)
            {
                weights[i] = _weights[i];
            }
        }

        public Matrix[] Pull()
        {
            Matrix[] model = (Matrix[])_weights.Clone(); //Clone -- might need fixing
            return model;
        }

        public void Show(int x, int y, int width, int height, float[] vision, float[] decisions)
        {
            float space = 5;
            float nSize = (height - (space * (_inputNodes - 2))) / _inputNodes;
            float nSpace = (width - (_weights.Length * nSize)) / _weights.Length;
            float hBuff = (height - (space * (_hiddenNodes - 1)) - (nSize * _hiddenNodes)) / 2;
            float oBuff = (height - (space * (_outputNodes-1)) - (nSize * _outputNodes)) / 2;

            int maxIndex = 0;
            for (int i = 1; i < decisions.Length; i++)
            {
                if (decisions[i] > decisions[maxIndex])
                {
                    maxIndex = i;
                }
            }
        }
    }
}
