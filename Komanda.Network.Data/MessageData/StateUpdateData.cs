using Komanda.Game.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Network.Data.MessageData
{
    public class StateUpdateData
    {

        public GameState State { get; set; }

        public bool TurnAllowed { get; set; }

        public StateUpdateData(GameState state, bool turnAllowed)
        {
            State = state;                                  // состояние
            TurnAllowed = turnAllowed;                              // разрешение хода
        }
    }
}
