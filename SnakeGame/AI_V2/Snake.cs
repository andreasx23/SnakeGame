using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeGame.AI_V2
{
    public class Snake
    {
        private const int INPUT_LAYERS = 24;
        private const int OUTPUT_LAYERS = 4;

        public int Score = 0;
        public int LifeLeft = 200;
        private int _lifetime = 0;
        private int _x = 0;
        private int _y = 0;
        private int _foodItterator = 0;

        public float Fitness = 0;

        public bool Dead = false;
        public bool Replay = false;

        public float[] Vision;
        public float[] Decisions;

        public (int x, int y) Head;
        public List<(int x, int y)> Body;
        private readonly List<Food> _foods;

        private Food _food;
        public NeuralNet Brain;

        private char[][] _grid;
        public static int Height;
        public static int Width;

        public enum GameObjects
        {
            WALL = '#',
            FLOOR = ' ',
            FOOD = '@',
            HEAD = 'x',
            BODY = '+'
        }

        public Snake(int layers)
        {
            InitGrid();
            InitSnake();
            _food = new Food(_grid);
            _distanceTowardsFood = CalculateManhatten();

            if (!SnakeAI.IS_HUMAN_PLAYING)
            {
                Vision = new float[INPUT_LAYERS];
                Decisions = new float[OUTPUT_LAYERS];
                _foods = new List<Food> { _food.Clone() };
                Brain = new NeuralNet(INPUT_LAYERS, SnakeAI.HIDDEN_NODES, OUTPUT_LAYERS, layers);
                Score += 3;
            }
        }

        public Snake(List<Food> foods)
        {
            InitGrid();
            InitSnake();
            Replay = true;
            Vision = new float[INPUT_LAYERS];
            Decisions = new float[OUTPUT_LAYERS];
            _foods = new List<Food>(foods.Count);
            foreach (var food in foods)
                _foods.Add(food.Clone());
            _food = _foods[_foodItterator];
            _food.Show(_grid);
            _foodItterator++;
            Score += 3;
            _distanceTowardsFood = CalculateManhatten();
        }

        public bool BodyCollide(int x, int y)
        {
            return Body.Contains((x, y));
        }

        public bool FoodCollide(int x, int y)
        {
            return _food.Position.x == x && _food.Position.y == y;
        }

        public bool WallCollide(int x, int y)
        {
            return _grid[x][y] == GetGameObject(GameObjects.WALL);
        }

        public void Show()
        {
            Console.Clear();
            foreach (var (x, y) in Body)
            {
                _grid[x][y] = GetGameObject(GameObjects.BODY);
            }
            _grid[Head.x][Head.y] = GetGameObject(GameObjects.HEAD);
            _food.Show(_grid);

            PrintGrid();
            if (Dead)
            {
                //InitGrid();
                //InitSnake();
                //_food.Show(_grid);
            }
        }

        private int _distanceTowardsFood = -1;
        public int sum = 0;

        private int CalculateManhatten()
        {
            return Math.Abs(Head.x - _food.Position.x) + Math.Abs(Head.y - _food.Position.y);
        }

        public void Move()
        {
            if (!Dead)
            {
                if (!SnakeAI.IS_HUMAN_PLAYING && !SnakeAI.MODEL_LOADED)
                {
                    _lifetime++;
                    LifeLeft--;

                    var newDistance = CalculateManhatten();
                    if (newDistance > _distanceTowardsFood)
                        sum--;
                    else
                        sum++;
                    _distanceTowardsFood = newDistance;
                }

                if (FoodCollide(Head.x, Head.y))
                    Eat();

                ShiftBody();
                if (WallCollide(Head.x, Head.y) || BodyCollide(Head.x, Head.y) || !SnakeAI.IS_HUMAN_PLAYING && LifeLeft <= 0)
                    Dead = true;
            }
        }

        public void Eat()
        {
            _grid[_food.Position.x][_food.Position.y] = GetGameObject(GameObjects.FLOOR);
            Score++;

            int n = Body.Count - 1;
            if (n >= 0)
                Body.Add((Body[n].x, Body[n].y));
            else
                Body.Add((Head.x, Head.y));

            if (!SnakeAI.IS_HUMAN_PLAYING && !SnakeAI.MODEL_LOADED)
            {
                if (LifeLeft < 500)
                {
                    if (LifeLeft > 400)
                        LifeLeft = 500;
                    else
                        LifeLeft += 100;
                }
            }

            if (!Replay)
            {
                _food = new Food(_grid);
                if (!SnakeAI.IS_HUMAN_PLAYING)
                    _foods.Add(_food);
            }
            else
            {
                _food = _foods[_foodItterator];
                _food.Show(_grid);
                _foodItterator++;
            }

            _distanceTowardsFood = CalculateManhatten();
        }

        public void ShiftBody()
        {
            (int x, int y) = Body.Last();
            (int x, int y) prev = Head;
            Head = (prev.x + _x, prev.y + _y);
            for (int i = 0; i < Body.Count; i++)
            {
                (int x, int y) temp = Body[i];
                Body[i] = prev;
                prev = temp;
            }
            _grid[x][y] = GetGameObject(GameObjects.FLOOR);
        }

        public Snake CloneForReplay()
        {
            Snake clone = new Snake(_foods) { Brain = Brain.Clone() };
            return clone;
        }

        public Snake Clone()
        {
            Snake clone = new Snake(SnakeAI.HIDDEN_LAYERS) { Brain = Brain.Clone() };
            return clone;
        }

        public Snake Crossover(Snake parent)
        {
            Snake child = new Snake(SnakeAI.HIDDEN_LAYERS) { Brain = Brain.Crossover(parent.Brain) };
            return child;
        }

        public void Mutate()
        {
            Brain.Mutate(SnakeAI.MUTATION_RATE);
        }

        public void CalculateFitness()
        {
            if (Score < 10)
            {
                if (sum > 0)
                {
                    Fitness = (float)(Math.Pow(sum, 2) * Math.Pow(2, Score));
                }
                else
                {
                    Fitness = (float)Math.Pow(2, Score);
                }
            }
            else
            {
                Fitness = (float)Math.Pow(_lifetime, 2);
                Fitness *= sum;
                Fitness *= (float)Math.Pow(2, 10);
                Fitness *= (Score - 9);
            }
        }

        public void Look() //Might be wrong
        {
            Vision = new float[INPUT_LAYERS];

            List<(int x, int y)> directions = new List<(int x, int y)>()
            {
                (-1, 0), //Up
                (-1, -1), //Upper left
                (0, -1), //Left
                (1, -1), //Lower left
                (1, 0), //Down
                (1, 1), //Lower right
                (0, 1), //Right
                (-1, 1), //Upper right
            };

            int index = 0;
            foreach (var (x, y) in directions)
            {
                float[] temp = LookInDirection(x, y);
                foreach (var value in temp)
                {
                    Vision[index] = value;
                    index++;
                }
            }
        }

        public void Think() //think about what direction to move
        {
            Decisions = Brain.Output(Vision);

            if (true)
            {
                int maxIndex = 0;
                float max = 0;
                for (int i = 0; i < Decisions.Length; i++)
                {
                    if (Decisions[i] > max)
                    {
                        max = Decisions[i];
                        maxIndex = i;
                    }
                }

                switch (maxIndex)
                {
                    case 0:
                        MoveUp();
                        break;
                    case 1:
                        //MoveLeft();
                        MoveDown();
                        break;
                    case 2:
                        //MoveDown();
                        MoveLeft();
                        break;
                    case 3:
                        MoveRight();
                        break;
                    default:
                        throw new Exception();
                }
            }            
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    int maxIndex = 0;
                    float max = 0;
                    for (int j = 0; j < Decisions.Length; j++)
                    {
                        if (Decisions[j] > max)
                        {
                            max = Decisions[j];
                            maxIndex = j;
                        }
                    }

                    switch (maxIndex)
                    {
                        case 0:
                            {
                                if (!MoveUp())
                                    Decisions[maxIndex] = 0;
                                else
                                    return;
                                break;
                            }
                        case 1:
                            {
                                //if (!MoveDown())
                                //    Decisions[maxIndex] = 0;
                                //else
                                //    return;
                                //break;

                                if (!MoveLeft())
                                    Decisions[maxIndex] = 0;
                                else
                                    return;
                                break;
                            }
                        case 2:
                            {
                                //if (!MoveLeft())
                                //    Decisions[maxIndex] = 0;
                                //else
                                //    return;
                                //break;

                                if (!MoveDown())
                                    Decisions[maxIndex] = 0;
                                else
                                    return;
                                break;
                            }
                        case 3:
                            {
                                if (!MoveRight())
                                    Decisions[maxIndex] = 0;
                                else
                                    return;
                                break;
                            }
                        default:
                            throw new Exception();
                    }
                }
            }
            
        }

        public bool MoveUp()
        {
            if (_x != 1)
            {
                _x = -1;
                _y = 0;
                return true;
            }

            return false;
        }

        public bool MoveDown()
        {
            if (_x != -1)
            {
                _x = 1;
                _y = 0;
                return true;
            }

            return false;
        }

        public bool MoveLeft()
        {
            if (_y != 1)
            {
                _x = 0;
                _y = -1;
                return true;
            }

            return false;
        }

        public bool MoveRight()
        {
            if (_y != -1)
            {
                _x = 0;
                _y = 1;
                return true;
            }

            return false;
        }

        public static char GetGameObject(GameObjects value)
        {
            return (char)value;
        }

        #region Private helper methods
        private float[] LookInDirection(int x, int y)
        {
            float[] look = new float[3];
            float distance = 1;
            bool foundFood = false;
            bool foundBody = false;

            int dx = Head.x + x;
            int dy = Head.y + y;
            while (!WallCollide(dx, dy))
            {
                if (!foundFood && FoodCollide(dx, dy))
                {
                    foundFood = true;
                    look[0] = 1;
                }

                if (!foundBody && BodyCollide(dx, dy))
                {
                    foundBody = true;
                    look[1] = 1;
                }

                dx += x;
                dy += y;
                distance++;
            }

            look[2] = 1 / distance;
            return look;
        }

        private void InitGrid()
        {
            _grid = new char[SnakeAI.SIZE + 2][];
            Height = _grid.Length;
            for (int i = 0; i < Height; i++)
            {
                _grid[i] = new char[SnakeAI.SIZE + 2];
            }

            Width = _grid.First().Length;
            for (int i = 0; i < Height; i++)
            {
                _grid[i][0] = GetGameObject(GameObjects.WALL);
                _grid[i][Width - 1] = GetGameObject(GameObjects.WALL);
            }

            for (int i = 0; i < Width; i++)
            {
                _grid[0][i] = GetGameObject(GameObjects.WALL);
                _grid[Height - 1][i] = GetGameObject(GameObjects.WALL);
            }

            for (int i = 1; i < Height - 1; i++)
            {
                for (int j = 1; j < Width - 1; j++)
                {
                    _grid[i][j] = GetGameObject(GameObjects.FLOOR);
                }
            }
        }

        private void InitSnake()
        {
            MoveLeft();
            //if (SnakeAI.IS_HUMAN_PLAYING)
            //    MoveLeft();

            Head = (SnakeAI.SIZE / 2, SnakeAI.SIZE / 2);
            Body = new List<(int x, int y)>();
            for (int i = 1; i <= 2; i++)
            {
                Body.Add((Head.x, Head.y + i));
            }
        }

        private void PrintGrid()
        {
            if (SnakeAI.IS_HUMAN_PLAYING)
                Console.WriteLine($"Score: {Score}");

            foreach (var row in _grid)
            {
                Console.WriteLine(string.Join("", row));
            }
        }
        #endregion
    }
}
