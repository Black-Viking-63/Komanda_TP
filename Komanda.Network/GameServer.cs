using Komanda.Game;
using Komanda.Network;
using Komanda.Network.Core;
using Komanda.Network.Core.Serialization;
using Komanda.Network.Data;
using Komanda.Network.Data.MessageData;
using System;
using System.Collections.Generic;

namespace Komanda.UI.NetCoreConsole
{
    public class GameServer
    {

        public GameServerState State { get; private set; } = GameServerState.NotStarted;

        private GameSession GameSession { get; set; }

        private MessageServer Server { get; set; }

        private DateTime LastHeartbeat { get; set; }

        private Dictionary<int, MessageClient> Clients { get; set; } = new Dictionary<int, MessageClient>();
        private Dictionary<MessageClient, int> Players { get; set; } = new Dictionary<MessageClient, int>();

        public event EventHandler<EventArgs> PlayerConnected = delegate { };
        public event EventHandler<EventArgs> PlayerDisconnected = delegate { };

        public event EventHandler<EventArgs> TurnPassed = delegate { };
        public event EventHandler<Network.GameEndedEventArgs> GameEnded = delegate { };

        public GameServer(int port, ISerializer serializer, GameSession session)
        {

            Server = new MessageServer(port, serializer);

            Server.ClientConnected += OnPlayerConnected;
            Server.ClientDisconnected += OnPlayerDisconnected;

            Server.MessageReceived += OnMessageReceived;

            GameSession = session;

            GameSession.GameEnded += GameSession_GameEnded;
        }

        private void GameSession_GameEnded(object sender, Game.GameEndedEventArgs e)  // метод обработки события о звершении игры
        {
            State = GameServerState.GameEnded;

            foreach (var messageClient in Clients.Values) // новое сосотояние карты игры
            {
                var message = new Message(
                    MessageType.StateUpdate,
                    stateUpdateData: new StateUpdateData(GameSession.RetrieveState(),
                    false));

                messageClient.SendMessage(message);
            }

            GameEndReason reason = e.Win ? GameEndReason.Win : GameEndReason.Lose;  // определяем причину окончания игры
            foreach (var messageClient in Clients.Values)                           // игре конец
            {
                var message = new Message(                              
                    MessageType.GameResult,
                    gameResultsData: new GameResultsData(reason));                  // отправляем сообщение об окончании игры

                messageClient.SendMessage(message);
            }
        }

        public void Start()                                                                 // запуск сервера
        {
            Server.Start();

            State = GameServerState.WaitingForPlayers;
        }

        public void Process()
        {
            if ((DateTime.Now - LastHeartbeat).TotalMilliseconds > 1000)                     // проверка на соединение
            {
                foreach (var client in Clients.Values)
                {
                    client.SendMessage(new Message(MessageType.Heartbeat));
                }

                LastHeartbeat = DateTime.Now;
            }

            Server.Process();                                                               // проверка на непрочитанные сообщения
        }

        protected virtual void OnPlayerConnected(object sender, EventArgs args)             // подключение нового игрока
        {
            var client = sender as MessageClient; 

            if(State != GameServerState.WaitingForPlayers)                                  // если подключается лишний мы его не подключаем
            {
                client.Dispose();
                return;
            }

            Players.Add(client, -(Players.Count + 1));
            Clients.Add(Players[client], client);
                
            PlayerConnected(client, args);                                                      // подключаем игрока к клиенту

            client.SendMessage(
                new Message(MessageType.Initialization, 
                new InitializationData(-Players.Count, GameSession.Options, GameSession.RetrieveState())));     // сообщение о подключении первого игрока

            if(Players.Count == 2)                                                          // подключение 2
            {
                State = GameServerState.WaitingForPlayerTurn;

                int activePlayer = GameSession.GetPlayerForCurrentTurn().Id;
                foreach(var messageClient in Clients.Values)
                {
                    var playerId = Players[messageClient];
                    bool turnAllowed = playerId == activePlayer;
                    var message = new Message(
                        MessageType.StateUpdate, 
                        stateUpdateData: new StateUpdateData(GameSession.RetrieveState(), 
                        turnAllowed));

                    messageClient.SendMessage(message);
                }
            }
        }

        protected virtual void OnPlayerDisconnected(object sender, EventArgs e)             // отключение игрока
        {
            var client = sender as MessageClient;

            var player = Players[client];

            Players.Remove(client);                                     // удаляаем игрока и его клиент
            Clients.Remove(player);

            PlayerDisconnected(player, e);

            if(State != GameServerState.WaitingForPlayers)
            {
                GameEnded(this, new Network.GameEndedEventArgs(GameEndReason.PlayerLeft));      // оотправляем сообщение об окочании игры с причиной отключении одного игрока
                State = GameServerState.GameEnded;
            }
        }

        protected virtual void OnMessageReceived(object sender, MessageReceivedEventArgs e)        // обработка приема сообщений (о ходе игрока который может ходить) 
        {
            var client = sender as MessageClient;

            var player = Players[client];

            switch (State)              
            {
                case GameServerState.WaitingForPlayerTurn when GameSession.GetPlayerForCurrentTurn().Id == player: // ожидаем хода игрока и id совпадает с id того игрока который прислал этот ход
                    var data = e.Message;
                    if (data.Type == MessageType.PlayerAction)
                    {
                        var action = data.PlayerActionData;             
                        // отправка события в игровую логику
                        GameSession.ProcessAction(action.Action);
                        GameSession.NextTurn();                             //ход следующего игрока или компьютера
                        foreach(var messageClient in Clients.Values)        // отправка обновленного состояния
                        {
                            int id = Players[messageClient];
                            bool turnAllowed = (id == GameSession.GetPlayerForCurrentTurn().Id);
                            messageClient.SendMessage(new Message(MessageType.StateUpdate, stateUpdateData: new StateUpdateData(GameSession.RetrieveState(), turnAllowed)));
                            
                        }
                        
                    }
                    break;
            }

        }

        public void Dispose()                   // очистка памяти
        {
            Server.Dispose();
        }

    }

    public enum GameServerState                 // флаги которые необходимы
    {
        NotStarted,
        WaitingForPlayers,
        WaitingForPlayerTurn,
        GameEnded,
        Closed
    }
}
