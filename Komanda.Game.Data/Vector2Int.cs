using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Game.Data
{
    public struct Vector2Int
    {
                                                                    // класс для работы с координатами
        public int x, y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2Int Direction(Direction direction)
        {
            switch (direction)
            {
                case Data.Direction.Up:
                    return Up;
                case Data.Direction.Right:
                    return Right;
                case Data.Direction.Down:
                    return Down;
                case Data.Direction.Left:
                    return Left;
                default:
                    throw new ArgumentException($"Unknown direction {direction}");
            }
        }
                                                                                                        // описание ходов
        public static Vector2Int Left => new Vector2Int(-1, 0);

        public static Vector2Int Right => new Vector2Int(1, 0);

        public static Vector2Int Up => new Vector2Int(0, -1);

        public static Vector2Int Down => new Vector2Int(0, 1);
        

        public static Vector2Int operator +(Vector2Int a)
        {
            return a;
        }

        public static Vector2Int operator -(Vector2Int a)
        {
            return new Vector2Int(-a.x, -a.y);
        }

        public static Vector2Int operator *(Vector2Int a, int length)
        {
            return new Vector2Int(a.x * length, a.y * length);
        }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x + b.x, a.y + b.y);
        }

        public static Vector2Int operator -(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x - b.x, a.y - b.y);
        }


    }
}
