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

        public Food()
        {
            _rand = new Random();
            GenerateFood();
        }

        public void Show()
        {
            SnakeAI.Grid[Position.x][Position.y] = SnakeAI.GetObject(SnakeAI.Objects.FOOD);
        }

        public Food Clone()
        {
            Food clone = new Food() { Position = Position };
            return clone;
        }

        private void GenerateFood()
        {
            int x = _rand.Next(1, SnakeAI.Height);
            int y = _rand.Next(1, SnakeAI.Width);

            while (SnakeAI.Grid[x][y] != SnakeAI.GetObject(SnakeAI.Objects.FLOOR))
            {
                x = _rand.Next(1, SnakeAI.Height);
                y = _rand.Next(1, SnakeAI.Width);
            }

            Position = (x, y);
            SnakeAI.Grid[x][y] = SnakeAI.GetObject(SnakeAI.Objects.FOOD);
        }
    }
}
