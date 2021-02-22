using Komanda.Game;
using Komanda.Game.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Komanda.UI.NetCoreConsole
{
    public class GameView
    {

        private GameClient GameClient { get; set; }
        private int PlayerId { get; set; }
        private GameSession GameSession { get; set; }

        private int LastSave { get; set; } = -1;

        private bool GameEnded { get; set; }
        private bool TurnAllowed { get; set; }

        public GameView(GameClient gameClient)
        {
            GameClient = gameClient;

            gameClient.GameStarted += GameClient_GameStarted;
            gameClient.GameEnded += GameClient_GameEnded;

            gameClient.MapUpdated += GameClient_MapUpdated;
            gameClient.TurnReceived += GameClient_TurnReceived;
        }

        public void Process()
        {
            Console.CursorVisible = false;
            try
            {
                do
                {
                    ProcessInput();                     // ожидаем ввода с клавиатуры (ожидаем окончания процесса игры)

                    GameClient.Process();
                    Thread.Sleep(1);

                } while (!GameEnded);
            }
            catch (Exception)
            {
                if (!GameEnded)     //если игра не окончилась
                {
                    Console.SetCursorPosition(0, 9);                //далее говорим что соединение потеряно и предлагаем сохраниться
                    int mode = Program.SelectionPrompt("Connection lost. Do you want to save the game?", "Yes", "No");   
                    switch (mode)
                    {
                        case 1:
                            SaveGame();
                            break;
                        case 2:
                            break;
                    }
                }
            }
            Console.SetCursorPosition(0, 0);
            Console.Clear();
            Console.CursorVisible = true;
        }

        private void ProcessInput()                                  // обработка нажатий клавиш
        {
            if (Console.KeyAvailable)                           // ввод с консоли доступен
            {
                var keyInfo = Console.ReadKey(true);
                                                                    //считываем нажатую клавишу если была нажата v значит нам надо сохранпиться
                switch (keyInfo.Key)
                {
                    case ConsoleKey.V:  
                        SaveGame();                                     // сохраняем игру
                        break;
                }

                if (TurnAllowed)                        // обработка движени
                {
                    Direction direction;                    // направление
                    ActionType actionType;                      // тип действия
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.W:                  // движение вверх
                            actionType = ActionType.Move;
                            direction = Direction.Up;
                            break;
                        case ConsoleKey.A:                  // движение влево
                            actionType = ActionType.Move;
                            direction = Direction.Left;
                            break;
                        case ConsoleKey.S                       :// движение вниз
                            actionType = ActionType.Move;
                            direction = Direction.Down;
                            break;
                        case ConsoleKey.D:                  // движение вправо
                            actionType = ActionType.Move;
                            direction = Direction.Right;
                            break;
                        case ConsoleKey.UpArrow:                    // стрельба вверх
                            actionType = ActionType.Shoot;
                            direction = Direction.Up;
                            break;
                        case ConsoleKey.LeftArrow:                    // стрельба влево
                            actionType = ActionType.Shoot;
                            direction = Direction.Left;
                            break;
                        case ConsoleKey.DownArrow:                    // стрельба вниз
                            actionType = ActionType.Shoot;
                            direction = Direction.Down;
                            break;
                        case ConsoleKey.RightArrow:                    // стрельба вправо
                            actionType = ActionType.Shoot;
                            direction = Direction.Right;
                            break;
                        default:
                            return;
                    }

                    var playerAction = new PlayerAction(actionType, direction);     // выполняем действие выбранное игроком
                    if (GameClient.ProcessTurn(playerAction))  
                    {
                        TurnAllowed = false;                    // после чего закрываем ему доступ к нажатию кнопок на движение
                    }
                }
            }
        }

        private void SaveGame()  //сохраняет игру
        {
            GameOptions options = GameClient.GameSession.Options;                       // для сохранения нам необходимо знать на каком состоянии мы сохраняемся
            GameState state = GameClient.GameSession.RetrieveState();               // и какие опции на данный момент в игре

            SaveData saveData = new SaveData()
            {
                Options = options,
                State = state
            };

            Directory.CreateDirectory("saves");                        

            string json = JsonConvert.SerializeObject(saveData);                            // сериализуем данные в строку для франения в формате json

            string[] paths = Directory.GetFiles("saves/", "*.json");                        // вызываем список файлов всех сохранений которые есть в папке с данным расширением

            File.WriteAllText($"saves/{paths.Length}.json", json);                          // записываем файл указанную папку с номером = числу файлов

            LastSave = GameClient.GameSession.CurrentTurn;                              //обновим переменную с номером хода, на котором мы сохранились, 

            DrawSidebar();                                                      //перерисуем сайдбар, в котором это значение отображается
        }

        private void GameClient_TurnReceived(object sender, EventArgs e)                    //обработчик событий возможности движения
        {
            TurnAllowed = true;
        }

        private void GameClient_MapUpdated(object sender, Network.MapUpdatedEventArgs e)                    // обновление карты
        {
            Redraw();
        }

        private void GameClient_GameEnded(object sender, Network.GameEndedEventArgs e)          // обработка события окончания игры
        {
            Redraw();
            GameEnded = true;                                       // флаг окончания игры
            Console.SetCursorPosition(0, 9);
            Console.Write("Game ended. You ");
            switch (e.Reason)
            {   
                case Network.Data.GameEndReason.Win:            // игра окончилась победой
                    Console.WriteLine("won!");
                    break;
                case Network.Data.GameEndReason.Lose:           // игра окончилась поражением
                    Console.WriteLine("lost!");
                    break;
            }
            Console.WriteLine("Press ENTER to return to main menu");

            Console.ReadLine();
        }

        private void GameClient_GameStarted(object sender, EventArgs e)
        {
            GameEnded = false;                                      //смена флага при начале игры
        }

        public void Redraw()        // перерисовка карты
        {
            Console.Clear();

            DrawHeader();
            DrawGameField();
            DrawFooter();
            DrawSidebar();
        }

        private void DrawHeader()                           // нарисовка заголовка-подсказки
        {
            Console.SetCursorPosition(0, 0);
            Console.Write("[WASD] Move [Arrow Keys] Shoot [V] Save Game");
        }

        private void DrawGameField()                                // отрисовка игровых полей
        {
            Console.SetCursorPosition(0, 1);
            for(int y = 0; y < 8; y++)
            {
                for(int x = 0; x < 8; x++)
                {
                    Console.SetCursorPosition(x, y + 1);
                    TileType tileType = GameClient.GameSession.Map.GetTileType(x, y);
                    switch (tileType)
                    {
                        case TileType.Empty:
                            Console.Write('.');                                 // точки это пустые клетки
                            break;
                        case TileType.Enemy:                                    // & - бомжи
                            Console.Write('&');
                            break;
                        case TileType.Wall:                                     // # - стена 
                            Console.Write('#');
                            break;
                        case TileType.Player:                                   // @ - игрок
                            Console.Write('@');
                            break;
                    }
                }
            }

            //Highlight player
            if (GameClient.PlayerInfo.Alive)                    // выделение живого игрока
            {
                var pos = GameClient.PlayerInfo.Position;
                Console.SetCursorPosition(pos.x, pos.y+1);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write('@');
                Console.ResetColor();
            }
        }

        private void DrawFooter()                           // отрисовка сообщений
        {
            Console.SetCursorPosition(0, 9);
            switch (GameClient.State)
            {
                case GameClientState.WaitingForGameInfo:
                    Console.Write("Waiting for game info");         // посмотри информацию об игре                 
                    break;
                case GameClientState.WaitingForGameStart:
                    Console.Write("Waiting for second player");         //ожидание второго игрока
                    break;
                case GameClientState.WaitingForTurn:
                    Console.Write("Waiting for turn");                  // ожидание действия
                    break;
                case GameClientState.WaitingForInput:
                    Console.Write("Waiting for input");                 // ождиание хода
                    break;  
                case GameClientState.Spectating:
                    Console.Write("YOU ARE DEAD. Spectating");          // вы убиты наблюдайте
                    break;
            }
        }

        private void DrawSidebar()                      // обновляющееся меню
        {
            Console.SetCursorPosition(9, 1);
            Console.Write($"Turn {GameClient.GameSession.CurrentTurn}");                // количество выполненых ходов
            Console.SetCursorPosition(9, 3);
            Console.Write($"Enemies {GameClient.GameSession.SpawnsRemain + GameClient.GameSession.Enemies.Count}");        // количество оставшихся игроков
            Console.SetCursorPosition(9, 5);
            Console.Write("Saved on turn ");                                            // сохранено на таком то ходу
            if(LastSave >= 0)
            {
                Console.Write(LastSave);
            }
            else
            {
                Console.Write('-');
            }
        }

    }
}
