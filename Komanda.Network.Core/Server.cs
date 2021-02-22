using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Komanda.Network.Core
{
    public class Server : IDisposable
    {

        private TcpListener TcpListener { get; set; }

        private HashSet<Client> Clients { get; set; } = new HashSet<Client>();

        public IPAddress IPAddress { get; private set; }

        public int Port { get; private set; }

        public event EventHandler<EventArgs> ClientConnected = delegate { };
        public event EventHandler<EventArgs> ClientDisconnected = delegate { };

        public event EventHandler<DataReceivedEventArgs> DataReceived = delegate { };

        public Server(IPAddress ipAddress, int port)
        {
            TcpListener = new TcpListener(ipAddress, port);

            IPAddress = ipAddress;
            Port = port;
        }

        public Server(int port) : this(IPAddress.Any, port) { }

        public void Start()                     // старт сервера
        {
            TcpListener.Start();
        }

        public void Process() // 
        {
            while (TcpListener.Pending())  // ожидающие обработки подключения
            {
                AcceptClient();
            }

            foreach(var client in Clients)  // опрос клиентов на обработку данных
            {
                client.Process();
            }
        }

        public void AcceptClient()  // прием клиента
        {
            TcpClient tcpClient = null;

            try
            {
                tcpClient = TcpListener.AcceptTcpClient();
            }
            catch (SocketException)
            {
                //просто игнорим этого клиента
                tcpClient?.Close();
                return;
            }

            Client client = new Client();

            client.Connected += OnClientConnected;
            client.Disconnected += OnClientDisconnected;
            client.MessageReceived += OnDataReceived;

            client.Initialize(tcpClient);

            Clients.Add(client);
        }

        protected virtual void OnClientConnected(object sender, EventArgs e)                // подключение клиента
        {
            ClientConnected(sender, e);
        }

        protected virtual void OnClientDisconnected(object sender, EventArgs e)                     // отключение клиента
        {
            ClientDisconnected(sender, e);
        }

        protected virtual void OnDataReceived(object sender, DataReceivedEventArgs e)               // обработка события получения данных
        {
            DataReceived(sender, e);
        }

        public void Dispose()                                                           // зачистка памяти по окончании работы
        {
            foreach (var client in Clients)
            {
                client.Dispose();
            }

            TcpListener?.Stop();
        }
    }
}
