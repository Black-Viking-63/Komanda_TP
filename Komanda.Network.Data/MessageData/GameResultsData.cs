using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Network.Data.MessageData
{
    public class GameResultsData
    {

        public GameEndReason Reason { get; set; }

        public GameResultsData(GameEndReason reason)
        {
            Reason = reason;
        }
    }
}
