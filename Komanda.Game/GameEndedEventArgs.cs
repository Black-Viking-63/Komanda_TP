using System;

namespace Komanda.Game
{
    public class GameEndedEventArgs : EventArgs
    {
        public bool Win { get; set; }
        
        // метод обрабатывающий информаци об окончании игры каким образо она закончилась победой или нет
        public GameEndedEventArgs(bool win)
        {
            Win = win;
        }
    }
}