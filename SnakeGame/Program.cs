using SnakeGame.AI_V2;
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

            SnakeAI ai = new SnakeAI(2000);
            while (ai.Highscore < 5)
            {
                ai.Draw();
            }
        }
    }
}
