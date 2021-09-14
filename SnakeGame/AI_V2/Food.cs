using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame.AI_V2
{
    public class Food
    {
        public (int x, int y) Position;
        private readonly Random _rand;

        public Food(char[][] grid)
        {
            _rand = new Random();
            GenerateFood(grid);
        }

        public Food()
        {

        }

        public void Show(char[][] grid)
        {
            grid[Position.x][Position.y] = Snake.GetGameObject(Snake.GameObjects.FOOD);
        }

        public Food Clone()
        {
            Food clone = new Food() { Position = Position };
            return clone;
        }

        private void GenerateFood(char[][] grid)
        {
            int x = _rand.Next(1, Snake.Height);
            int y = _rand.Next(1, Snake.Width);

            while (grid[x][y] != Snake.GetGameObject(Snake.GameObjects.FLOOR))
            {
                x = _rand.Next(1, Snake.Height);
                y = _rand.Next(1, Snake.Width);
            }

            Position = (x, y);
            grid[x][y] = Snake.GetGameObject(Snake.GameObjects.FOOD);
        }
    }
}
