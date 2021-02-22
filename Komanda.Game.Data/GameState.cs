using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Game.Data
{
    public class GameState
    {                                                       // класс описывающий состояние игры

        public int CurrentTurn { get; set; }

        public int[,] Map { get; set; }

        public int SpawnsRemain { get; set; }

        public List<Player> Players { get; set; }

        public List<Enemy> Enemies { get; set; }
                                                            // конструктор
        public GameState(
            int currentTurn, 
            int[,] map, 
            int spawnsRemain, 
            IReadOnlyList<Player> players, 
            IReadOnlyList<Enemy> enemies)
        {
            CurrentTurn = currentTurn;
            Map = map;
            SpawnsRemain = spawnsRemain;
            Players = new List<Player>(players);
            Enemies = new List<Enemy>(enemies);
        }

    }

}
