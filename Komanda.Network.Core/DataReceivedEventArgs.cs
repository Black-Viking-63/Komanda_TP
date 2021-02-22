using System;

namespace Komanda.Network.Core
{

    public class DataReceivedEventArgs : EventArgs
    {
        // необходим нам обработки событий получения данных на Сlient(tcp-client)
        public string Data { get; set; }

        public DataReceivedEventArgs(string data)
        {
            Data = data;
        }
    }
}
