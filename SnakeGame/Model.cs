using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tensorflow.NumPy;
using Torch;

namespace SnakeGame
{
    public class Model
    {
        public class LinearQNet : torch.nn.Module
        {
            private readonly torch.nn.Linear _linear1;
            private readonly torch.nn.Linear _linear2;

            public LinearQNet(int inputSize, int hiddenSize, int outputSize) : base()
            {
                _linear1 = new torch.nn.Linear(inputSize, hiddenSize);
                _linear2 = new torch.nn.Linear(hiddenSize, outputSize);
            }

            public int[][] Forward(Tensor tensor)
            {
                base.forward(new int[][] { });

                var x = torch.nn.functional.relu(tensor);

                //var x = torch.nn.functional.relu(new Tensor(d));
                throw new NotImplementedException();
            }

            public void Save(string fileName = "model.pth")
            {
                string modelFolderPath = Environment.CurrentDirectory;
                var modelFolderDirectory = Path.Combine(modelFolderPath, "model");
                if (!Directory.Exists(modelFolderDirectory)) Directory.CreateDirectory(modelFolderDirectory);
                var combined = Path.Combine(modelFolderPath, fileName);
                if (!File.Exists(combined)) File.Create(combined);
                File.WriteAllText(combined, ""); //TODO WHAT TO WRITE!!
            }
        }

        public class QTrainer
        {
            private readonly LinearQNet _model;
            private readonly double _learningRate;
            private readonly double _gamma;
            private readonly torch.nn.MSELoss _criterion;

            public QTrainer(LinearQNet model, double learningRate, double gamma)
            {
                _model = model;
                _learningRate = learningRate;
                _gamma = gamma;
                //Optimizer?? optim.adam
                _criterion = new torch.nn.MSELoss();
            }

            public void TrainStep(NDArray state, int[] action, int reward, NDArray nextState, bool isGameOver)
            {
                var npStateArray = Numpy.np.array(state.ToArray());
                var tensorState = torch.tensor(npStateArray);

                var npNextStateArray = Numpy.np.array(nextState.ToArray());
                var tensorNextState = torch.tensor(npNextStateArray);

                var tensorAction = torch.tensor(action).values();

                var npRewardArray = new int[] { reward };
                var tensorReward = torch.tensor(npRewardArray).values();

                var isGameOverTuple = (isGameOver, -1);
                if (tensorState.Shape.Dimensions.First() == 1)
                {
                    tensorState = torch.unsqueeze(tensorState, 0);
                    tensorNextState = torch.unsqueeze(tensorNextState, 0);
                    tensorAction = torch.unsqueeze(tensorAction, 0);
                    tensorReward = torch.unsqueeze(tensorReward, 0);
                }

                var prediction = _model.Forward(tensorState);
                var target = prediction.ToArray();
                if (!isGameOver)
                {
                    var newTest = _model.Forward(tensorNextState);
                    var newNpNextStateArray = Numpy.np.array(newTest.ToArray());
                    var newTensorNextState = torch.tensor(newNpNextStateArray);

                    var newQ = reward + _gamma * torch.max(newTensorNextState);
                    target[0][torch.argmax(action).item<int>()] = newQ.item<int>();
                }

                //TODO NEED TO OD LOSS????
                //var loss = _criterion.
            }
        }
    }
}
