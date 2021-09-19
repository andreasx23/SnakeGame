using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame.SnakeV3
{
    public class Food
    {
        private readonly Random _rand;
        public (int x, int y) Position;

        public Food()
        {
            _rand = new Random();
        }

        public void GenerateFood(GameObject[][] grid)
        {
            int height = grid.Length, width = grid.First().Length;
            int x = _rand.Next(1, height);
            int y = _rand.Next(1, width);

            while (grid[x][y] != GameObject.FLOOR)
            {
                x = _rand.Next(1, height);
                y = _rand.Next(1, width);
            }

            Position = (x, y);
            grid[x][y] = GameObject.FOOD;
        }
    }
}
