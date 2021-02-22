using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Game.Data
{
    public class PlayerAction
    {  
                                                                        //описание хода игрока
        public ActionType Type { get; private set; }

        public Direction Direction { get; private set; }

        public PlayerAction(ActionType type, Direction direction)
        {
            Type = type;                                        // тип стрельба или движение
            Direction = direction;                              // направление
        }

    }
}
