using Komanda.Game.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Game
{
    public class LevelMap
    {

        public int[,] Map { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public LevelMap(int width, int height)           //конструктор
        {
            int[,] map = new int[width, height];

            UpdateMap(map);
        }

        public LevelMap(int[,] map)                 // конструктор
        {
            UpdateMap(map);
        }

        public void UpdateMap(int[,] map)                   // обновление карты
        {
            Height = map.GetLength(0);
            Width = map.GetLength(1);

            Map = map;
        }

        private TileType GetTileType(int tileId)            // проверяем что находится в клетке
        {
            if (tileId > 1) return TileType.Enemy;                //  2 3 4 5 и тд это враги
            if (tileId == 1) return TileType.Wall;                //  1 стена
            if (tileId == 0) return TileType.Empty;               //  -1 и -2 это игроки

            return TileType.Player;
        }

        public TileType GetTileType(int x, int y)               // значение клетки по координатам
        {
            return GetTileType(this[x, y]);
        }

        public TileType GetTileType(Vector2Int pos)            //  значение клетки по позиции
        {
            return GetTileType(this[pos]);
        }

        public int this[int x, int y]                               // получение и расчет координат для движение по карте
        {
            get
            {
                x = (x % Width + Width) % Width;
                y = (y % Height + Height) % Height;

                return Map[y, x];
            }

            set
            {
                x = (x % Width + Width) % Width;
                y = (y % Height + Height) % Height;

                Map[y, x] = value;
            }
        }

        public int this[Vector2Int pos]                 // работа с координанатами
        {
            get { return this[pos.x, pos.y]; }
            set { this[pos.x, pos.y] = value; }
        }

    }
}
