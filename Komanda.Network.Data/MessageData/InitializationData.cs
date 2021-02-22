using Komanda.Game.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Network.Data.MessageData
{
    public class InitializationData 
    {

        public int PlayerId { get; set; }

        public GameOptions GameOptions { get; set; }

        public GameState GameState { get; set; }

        public InitializationData(int playerId, GameOptions gameOptions, GameState gameState)
        {
            PlayerId = playerId;
            GameOptions = gameOptions;
            GameState = gameState;
        }

    }
}
