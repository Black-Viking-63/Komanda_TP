using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Network.Data
{
    public enum MessageType                             // типы передаваемых сообщений
    {
        Initialization,                         // инициализация
        
        GameStart,                          // начало игры

        PlayerAction,                           // ход игрока
        StateUpdate,                                // обновление состояния

        Heartbeat,

        GameResult                          // результат игры
    }
}
