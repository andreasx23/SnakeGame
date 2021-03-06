using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeGame.AI_V2
{
    public class Matrix
    {
        private readonly int _rows;
        private readonly int _columns;
        private readonly float[][] _matrix;

        public Matrix(int row, int column)
        {
            _rows = row;
            _columns = column;
            _matrix = new float[_rows][];
            for (int i = 0; i < _rows; i++)
            {
                _matrix[i] = new float[_columns];
            }
        }

        public Matrix(float[][] matrix)
        {
            _rows = matrix.Length;
            _columns = matrix.First().Length;
            _matrix = matrix;
        }

        public void Output()
        {
            foreach (var row in _matrix)
            {
                Console.WriteLine(string.Join(" ", row));
            }
        }

        public Matrix Dot(Matrix n)
        {
            Matrix result = new Matrix(_rows, n._columns);

            if (_columns == n._rows)
            {
                for (int i = 0; i < _rows; i++)
                {
                    for (int j = 0; j < n._columns; j++)
                    {
                        float sum = 0;
                        for (int k = 0; k < _columns; k++)
                        {
                            sum += _matrix[i][k] * n._matrix[k][j];
                        }
                        result._matrix[i][j] = sum;
                    }
                }
            }

            return result;
        }

        public void Randomize()
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _matrix[i][j] = Utility.NextFloat(-1, 1);
                }
            }
        }

        public Matrix SingleColumnMatrixFromArray(float[] array)
        {
            int n = array.Length;
            Matrix matrix = new Matrix(n, 1);

            for (int i = 0; i < n; i++)
            {
                matrix._matrix[i][0] = array[i];
            }

            return matrix;
        }

        public float[] ToArray()
        {
            float[] array = new float[_rows * _columns];

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    array[j + i * _columns] = _matrix[i][j];
                }
            }

            return array;
        }

        public Matrix AddBias()
        {
            Matrix n = new Matrix(_rows + 1, 1);

            for (int i = 0; i < _rows; i++)
            {
                n._matrix[i][0] = _matrix[i][0];
            }
            n._matrix[_rows][0] = 1;

            return n;
        }

        public Matrix Activate()
        {
            Matrix n = new Matrix(_rows, _columns);

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    n._matrix[i][j] = Relu(_matrix[i][j]);
                }
            }

            return n;
        }

        public void Mutate(float mutationRate)
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    float randValue = (float)Utility.NextFloat(0, 1);
                    if (randValue < mutationRate)
                    {
                        _matrix[i][j] += RandomGaussian() / 5;

                        if (_matrix[i][j] > 1) _matrix[i][j] = 1;
                        if (_matrix[i][j] < -1) _matrix[i][j] = -1;
                    }
                }
            }
        }

        public Matrix Crossover(Matrix partner)
        {
            Matrix child = new Matrix(_rows, _columns);

            int randomRow = (int)Math.Floor(Utility.NextFloat(0, _rows));
            int randomColumn = (int)Math.Floor(Utility.NextFloat(0, _columns));
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    if (i < randomRow || i == randomRow && j <= randomColumn)
                        child._matrix[i][j] = _matrix[i][j];
                    else
                        child._matrix[i][j] = partner._matrix[i][j];
                }
            }

            return child;
        }

        public Matrix Clone()
        {
            Matrix clone = new Matrix(_rows, _columns);

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    clone._matrix[i][j] = _matrix[i][j];
                }
            }

            return clone;
        }

        #region Private helper functions
        private float RandomGaussian()
        {
            //https://stackoverflow.com/questions/218060/random-gaussian-variables
            double mean = 100;
            double stdDev = 10;
            Normal normalDist = new Normal(mean, stdDev);
            double randomGaussianValue = normalDist.Sample();
            return float.Parse(randomGaussianValue.ToString());
        }

        private float Relu(float x)
        {
            return Math.Max(0, x);
        }
        #endregion
    }
}
