using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Game
{
    public enum TileType            // перечисление типов даных находящихся на карте
    {
        Empty,                  // пустая клетка
        Wall,                   // стена
        Enemy,                  // враг
        Player                  // игрок
    }   
}
