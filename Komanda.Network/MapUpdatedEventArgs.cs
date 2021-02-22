using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Network
{
    public class MapUpdatedEventArgs : EventArgs
    {
        // метод описывающий каким образом изменилась карта
        public int[,] Map { get; set; }

        public MapUpdatedEventArgs(int[,] map)
        {
            Map = map;
        }

    }
}
