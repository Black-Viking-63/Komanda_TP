using Komanda.Game.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Network.Data.MessageData
{
    public class PlayerActionData
    {

        public PlayerAction Action { get; set; }

        public PlayerActionData(PlayerAction action)
        {
            Action = action;
        }

    }

}
