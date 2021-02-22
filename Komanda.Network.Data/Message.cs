using Komanda.Network.Data.MessageData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Network.Data
{
    public class Message
    {
        public MessageType Type { get; set; }

        public InitializationData InitializationData { get; set; }

        public PlayerActionData PlayerActionData { get;  set; }

        public StateUpdateData StateUpdateData { get; set; }

        public GameResultsData GameResultsData { get; set; }


            // конструктор сообщения в котором содерждатся данные о типе сообщения, данных инициализации, действиях игрока, обновлениях состояния, результатх игры       
        public Message( 
            MessageType type, 
            InitializationData initializationData = null, 
            PlayerActionData playerActionData = null, 
            StateUpdateData stateUpdateData = null, 
            GameResultsData gameResultsData = null)
        {
            Type = type;
            InitializationData = initializationData;
            PlayerActionData = playerActionData;
            StateUpdateData = stateUpdateData;
            GameResultsData = gameResultsData;
        }
    }
}
