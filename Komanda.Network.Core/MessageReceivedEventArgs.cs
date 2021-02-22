using Komanda.Network.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Network.Core
{
    public class MessageReceivedEventArgs : EventArgs
    {
        // он необходим нам обработки событий получения сообщений(Message) на MessageСlient
        public Message Message { get; set; }

        public MessageReceivedEventArgs(Message message)
        {
            Message = message;
        }
    }
}
