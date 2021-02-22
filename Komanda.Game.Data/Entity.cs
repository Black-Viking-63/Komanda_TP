using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Game.Data
{
    public class Entity
    {       
                                                                                            // класс описывающий персонажей игры игроков и бомжей
        public int Id { get; set; }
        
        public Vector2Int Position { get; set; }

        public Entity(int id, Vector2Int position)                  // конструктор
        {
            Id = id;
            Position = position;
        }

    }

    public class Enemy : Entity
    {
        public Enemy(int id, Vector2Int position) : base(id, position)
        {
        }
    }

    public class Player : Entity
    {

        public bool Alive { get; set; }
        
        public Player(int id, Vector2Int position, bool alive) : base(id, position)                         // метод определющий живучесеть персонажа: жив он или мертв
        {
            Alive = alive;
        }
    }

}
