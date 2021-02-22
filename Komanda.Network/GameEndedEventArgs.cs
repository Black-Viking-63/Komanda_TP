using Komanda.Network.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Network
{
    public class GameEndedEventArgs : EventArgs
    {
                                                        // класс описывающий причину окончания игры
        public GameEndReason Reason { get; set; }

        public GameEndedEventArgs(GameEndReason reason)
        {
            Reason = reason;
        }

    }
}
