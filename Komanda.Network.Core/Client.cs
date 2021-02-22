using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Komanda.Network.Core
{
    public class Client : IDisposable
    {

        private TcpClient TcpClient { get; set; }

        private NetworkStream NetworkStream { get; set; }
        private BinaryReader BinaryReader { get; set; }
        private BinaryWriter BinaryWriter { get; set; }

        public event EventHandler<EventArgs> Connected = delegate { };
        public event EventHandler<EventArgs> Disconnected = delegate { };

        public event EventHandler<DataReceivedEventArgs> MessageReceived = delegate { };

        //Только для подключенных клиентов
        internal void Initialize(TcpClient client)                                  // инициализируем клиент на основе tcp клиента
        {
            TcpClient = client;
            TcpClient.NoDelay = true;

            NetworkStream = TcpClient.GetStream();

            BinaryReader = new BinaryReader(NetworkStream, Encoding.UTF8, true);            // создадим средства записи и чтения данных в бинарные потоки и обратно
            BinaryWriter = new BinaryWriter(NetworkStream, Encoding.UTF8, true);
            
            Connected(this, EventArgs.Empty);
        }

        public void Connect(string host, int port)          // подключение tcp клиента
        {
            var tcpClient = TcpClient ?? new TcpClient();

            tcpClient.Connect(host, port);              // подключеаем по порту 

            Initialize(tcpClient);
        }

        public void Process() // проверка наличия данных 
        {
            // строка в событие
            if (NetworkStream.DataAvailable)
            {
                string message = BinaryReader.ReadString();

                MessageReceived(this, new DataReceivedEventArgs(message));
            }
        }

        public void Send(string data)                       // отправка данных в бинарном потоке
        {
            BinaryWriter.Write(data);
        }

        public void Dispose()                                   // очистка данных при отключении
        {
            Disconnected(this, EventArgs.Empty);

            TcpClient?.Close();                             // закрываем tcp клиент

            BinaryReader?.Dispose();  
            BinaryWriter?.Dispose();  

            NetworkStream?.Dispose();  
        }
    }
}
