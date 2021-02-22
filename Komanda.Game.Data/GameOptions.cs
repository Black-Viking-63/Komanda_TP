using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Game.Data
{
    public class GameOptions
    {
        // настройки по умолчанию
        // количество игроков
        public int PlayerCount { get; set; } = 2;

        // размер поля (карты)
        public int MapWidth { get; set; } = 8;

        public int MapHeight { get; set; } = 8;

        //шанс спавна стены
        public double WallSpawnChance { get; set; } = 0.1;

        // количество игроков
        public int TotalEnemies { get; set; } = 2;

        // максимальный спавн бомжей за ход если их много
        public int MaxSpawnPerTurn { get; set; } = 3;

        //шанс спавна бомжей
        public double SpawnChance { get; set; } = 0.7;

        public int Seed { get; set; }
        // для генератора рандома
    }
}
