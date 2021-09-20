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
            bool isHumanPlaying = false;
            int size = 20;
            Game game = new Game(size, size, isHumanPlaying);
            game.Play();
        }
    }
}
