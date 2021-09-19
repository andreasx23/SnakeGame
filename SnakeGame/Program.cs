using SnakeGame.SnakeV3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeGame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //https://pythonawesome.com/train-a-neural-network-to-play-snake-using-a-genetic-algorithm/
            //https://github.com/greerviau/SnakeAI/tree/master/SnakeAI

            //Random rand = new Random();

            //for (int i = 0; i < 100; i++)
            //{
            //    Console.WriteLine(rand.NextDouble());
            //}

            //SnakeAI ai = new SnakeAI(2000);
            //ai.Play();

            int size = 20;
            SnakeV3.Game game = new SnakeV3.Game(size, size, false);
            game.Play();
        }
    }
}
