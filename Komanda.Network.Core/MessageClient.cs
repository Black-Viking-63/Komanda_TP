using Komanda.Game;
using Komanda.Network.Core.Serialization;
using Komanda.Network.Data;
using Komanda.Network.Data.MessageData;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Komanda.Network.Core
{
    public class MessageClient : IDisposable
    {
        private Client Client { get; set; }

        private ISerializer Serializer { get; set; }

        public event EventHandler<EventArgs> Connected = delegate { };
        public event EventHandler<EventArgs> Disconnected = delegate { };

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public MessageClient(ISerializer serializer)
        {
            Serializer = serializer;
        }

        internal void Initialize(Client client)             // констрктор
        {
            Client = client;

            Client.Connected += OnConnected;                        // подпись на события
            Client.Disconnected += OnDisconnected;
            Client.MessageReceived += OnMessageReceived;
        }

        public void Connect(string host, int port)              // подключение MessageClient
        {
            var client = new Client();

            Initialize(client);

            Client.Connect(host, port);
        }

        public void Process()
        {
            Client.Process();
        }

        public void SendMessage(Message message)                // отправка Message
        {
            var data = Serializer.Serialize(message);               // сериализация
            Client.Send(data);
        }

        protected virtual void OnConnected(object sender, EventArgs e)              // подключение
        {
            Connected(this, e);
        }

        protected virtual void OnDisconnected(object sender, EventArgs e)                   // отключение
        {
            Disconnected(this, e);
        }

        protected virtual void OnMessageReceived(object sender, DataReceivedEventArgs e)        // десериализация полученных сообщений
        {
            var message = Serializer.Deserialize<Message>(e.Data);
            MessageReceived(this, new MessageReceivedEventArgs(message));
        }

        public void Dispose()                                           // зачистка паямти по окончании
        {
            Client?.Dispose();
        }

    }
}
