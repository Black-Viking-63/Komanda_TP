using Komanda.Game;
using Komanda.Game.Data;
using Komanda.Network;
using Komanda.Network.Core;
using Komanda.Network.Core.Serialization;
using Komanda.Network.Data;
using Komanda.Network.Data.MessageData;
using System;
using System.Linq;

namespace Komanda.UI.NetCoreConsole
{
    public class GameClient
    {
        public GameClientState State { get; private set; } = GameClientState.NotStarted;

        public Player PlayerInfo { get; set; }
        private int PlayerId { get; set; }
        public GameSession GameSession { get; set; }

        private DateTime LastHeartbeat { get; set; }

        private MessageClient Client { get; set; }

        public event EventHandler<EventArgs> Connected = delegate { };
        public event EventHandler<EventArgs> Disconnected = delegate { };

        public event EventHandler<EventArgs> GameStarted = delegate { };
        public event EventHandler<Network.GameEndedEventArgs> GameEnded = delegate { };

        public event EventHandler<EventArgs> TurnReceived = delegate { };
        public event EventHandler<MapUpdatedEventArgs> MapUpdated = delegate { };
        

        public GameClient(ISerializer serializer)       // конструктор
        {
            Client = new MessageClient(serializer);

            Client.Connected += Client_Connected;
            Client.Disconnected += Client_Disconnected;
            Client.MessageReceived += OnMessageReceived;
        }

        private void Client_Disconnected(object sender, EventArgs e) // обработка события отключения клиента  
        {
            State = GameClientState.Disconnected;
            Disconnected(sender, e);
        }

        private void Client_Connected(object sender, EventArgs e) // обработка события подключения клиента
        {
            State = GameClientState.WaitingForGameInfo;
            Connected(sender, e);
        }

        public void Connect(string host, int port)  // само подключение
        {
            Client.Connect(host, port);
        }

        public void Process()  // опрос клинта на получение сообщений 
        {
            Client.Process();

            if ((DateTime.Now - LastHeartbeat).TotalMilliseconds > 1000)  // проверка на соединение
            {
                Client.SendMessage(new Message(MessageType.Heartbeat));
                LastHeartbeat = DateTime.Now;
            }
        }

        public bool ProcessTurn(PlayerAction action)  // обработка действий игрока ходьба стрельба
        {
            if (GameSession.ValidateAction(action))
            {
                var message = new Message(MessageType.PlayerAction, playerActionData: new PlayerActionData(action));
                Client.SendMessage(message);
                State = GameClientState.WaitingForTurn;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual void OnMessageReceived(object sender, MessageReceivedEventArgs e)  // метод обработки событие на получение сообщения
        {
            switch (e.Message.Type)
            {
                case MessageType.Initialization:
                    ProcessInitData(e.Message.InitializationData);                          // инициализации
                    break;
                case MessageType.StateUpdate:
                    ProcessStateUpdateData(e.Message.StateUpdateData);                      // обновления состояния (ход игрока/бомжа, слом стены)
                    break;
                case MessageType.GameResult:
                    ProcessGameResultsData(e.Message.GameResultsData);                      // окончание игры
                    break;
            }
        }

        private void ProcessInitData(InitializationData data)   // метод обаботки сообщений с данными инициализации
        {
            PlayerId = data.PlayerId;
            PlayerInfo = data.GameState.Players.First(player => player.Id == PlayerId);
            GameSession = new GameSession(data.GameOptions, data.GameState);
            State = GameClientState.WaitingForGameStart;
            MapUpdated(this, new MapUpdatedEventArgs(data.GameState.Map)); 
        }

        private void ProcessStateUpdateData(StateUpdateData data)    // метод обработки сообщений с данными состояния игры
        {
            if(data.State.CurrentTurn == 0)
            {
                State = GameClientState.WaitingForTurn;
                GameStarted(this, EventArgs.Empty);
            }

            PlayerInfo = data.State.Players.First(player => player.Id == PlayerId);
            if(PlayerInfo.Alive == false)                                                       // если игрок мерт он просто наблюдает
            {
                State = GameClientState.Spectating;
            }

            GameSession.UpdateState(data.State);            // обновляем состояния
                
            

            if (data.TurnAllowed)   // проверка  нашего хода можем идти или нет
            {
                State = GameClientState.WaitingForInput;
                MapUpdated(this, new MapUpdatedEventArgs(data.State.Map));
                TurnReceived(this, EventArgs.Empty);  // получили ход
            }
            else
            {
                MapUpdated(this, new MapUpdatedEventArgs(data.State.Map));      // если нет то ходит другой а карту обновляем
            }
        }

        private void ProcessGameResultsData(GameResultsData data)  // метод обработки соощения с данными о конце игры
        {
            State = GameClientState.GameEnded;
            GameEnded(this, new Network.GameEndedEventArgs(data.Reason));
        }

    }

    public enum GameClientState
    {                               // перечеилсения флагов которые необходимы и могут быть использованы
        NotStarted,
        WaitingForGameInfo,
        WaitingForTurn,
        WaitingForInput,
        WaitingForGameStart,
        Spectating,
        GameEnded,
        Disconnected
    }
}
