using System;
using System.Collections.Generic;
using System.Text;

namespace Komanda.Game.Data
{
    public class SaveData
    {
        //класс описывает те данные которые будем сохранять
        public GameOptions Options { get; set; }

        public GameState State { get; set; }

    }
}
