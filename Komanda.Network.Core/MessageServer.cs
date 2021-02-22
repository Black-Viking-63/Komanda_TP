using Komanda.Network.Core.Serialization;
using Komanda.Network.Data;
using System;
using System.Collections.Generic;

namespace Komanda.Network.Core
{
    public class MessageServer
    {
        private Server Server { get; set; }

        private ISerializer Serializer { get; set; }

        private Dictionary<Client, MessageClient> Clients { get; set; } = new Dictionary<Client, MessageClient>();

        public event EventHandler<EventArgs> ClientConnected = delegate { };
        public event EventHandler<EventArgs> ClientDisconnected = delegate { };

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public MessageServer(int port, ISerializer serializer)
        {

            Server = new Server(port);

            Server.ClientConnected += OnClientConnected;
            Server.ClientDisconnected += OnClientDisconnected;
            Server.DataReceived += OnMessageReceived;

            Serializer = serializer;
        }

        public void Start()                     // запуск сервера
        {
            Server.Start();
        }

        public void Process()
        {
            Server.Process(); // прием новых соедниенй

            foreach (var client in Clients.Values)
            {
                client.Process();  // прием новых сообщений
            }
        }

        protected virtual void OnClientConnected(object sender, EventArgs e)            // подключение клиента
        {
            var client = sender as Client;

            MessageClient messageClient = new MessageClient(Serializer);

            messageClient.Initialize(client);

            Clients.Add(client, messageClient);

            ClientConnected(messageClient, EventArgs.Empty);
        }

        protected virtual void OnClientDisconnected(object sender, EventArgs e)     // обработка отклбчения клиента
        {
            var client = sender as Client;
            var messageClient = Clients[client];

            Clients.Remove(client);

            ClientDisconnected(messageClient, EventArgs.Empty);
        }

        protected virtual void OnMessageReceived(object sender, DataReceivedEventArgs e)  // десериализеут сообщения и передает месседжи в события в gameserver
        {
            var client = sender as Client;
            var messageClient = Clients[client];

            var message = Serializer.Deserialize<Message>(e.Data);

            MessageReceived(messageClient, new MessageReceivedEventArgs(message));

        }

        public void Dispose()                   // зачистка памяти по окончании работы
        {
            Server?.Dispose();

            foreach (var client in Clients.Values)
            {
                client.Dispose();
            }
        }
    }
}
