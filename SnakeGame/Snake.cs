using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    public class Snake
    {
        public int HeadX;
        public int HeadY;
        public List<(int x, int y)> Body;
        public Direction Direction;
        
        public Snake(int headX, int headY)
        {
            HeadX = headX;
            HeadY = headY;
            Body = new List<(int x, int y)>();

            for (int i = 1; i <= 2; i++)
            {
                IncreaseSize(headX, headY + i);
            }

            Direction = Direction.LEFT;
        }

        public void IncreaseSize(int x, int y)
        {
            Body.Add((x, y));
        }

        public void UpdateSnakePosition()
        {
            var prev = (HeadX, HeadY);
            var (x, y) = GetDirection();
            HeadX += x;
            HeadY += y;
            int n = Body.Count;
            for (int i = 0; i < n; i++)
            {
                var current = Body[i];
                Body[i] = prev;
                prev = current;
            }
        }

        public List<(int x, int y)> GetSnakeCoordinates()
        {
            List<(int x, int y)> coords = new List<(int x, int y)> { (HeadX, HeadY) };
            coords.AddRange(Body);
            return coords;
        }

        private (int x, int y) GetDirection()
        {
            switch (Direction)
            {
                case Direction.LEFT:
                    return (0, -1);
                case Direction.RIGHT:
                    return (0, 1);
                case Direction.UP:
                    return (-1, 0);
                case Direction.DOWN:
                    return (1, 0);
                default:
                    return (0, 0);
            }
        }
    }

    public enum Direction
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }
}
