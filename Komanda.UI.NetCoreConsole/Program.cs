using Komanda.Game;
using Komanda.Game.Data;
using Komanda.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Komanda.UI.NetCoreConsole
{
    class Program
    {

        private static JsonSerializer Serializer { get; set; } = new JsonSerializer();

        private static JSchema SaveDataJSchema { get; set; } = JSchema.Parse(Resources.SaveDataJSchema);

        static void Main(string[] args)
        {
            do
            {
                int mode = SelectionPrompt("KOMANDA", "Host Game", "Join Game", "Exit");            // просим выбрать режим работы

                switch (mode)
                {
                    case 1: 
                        HostGame();                                 // запускаем игру
                        break;
                    case 2:
                        string host = AddressPrompt();                           // ip adress
                        int port = PortPrompt();                                 // номер порта

                        try
                        {
                            JoinGame(host, port);                       // присоединение к игре
                        }   
                        catch (Exception)
                        {
                            Console.WriteLine("Couldn't connect to specified server");              // если адрес неверный говрим об этом
                        }
                        break;
                    case 3:
                        CloseGame();                                    // закрываем игру
                        return;
                }
            } while (true);
        }

        static void HostGame()
        {

            do
            {
                int mode = SelectionPrompt("HOST GAME MODE", "New Game", "Load Save", "Back");  // предлагаем выбрать режим запуска игры 

                GameSession session;

                Thread thread;
                int port;
                switch (mode)
                {
                    case 1:                                                 // запуск новой игры
                        port = PortPrompt();                                    // спрашиваем номер порта

                        session = new GameSession(PrepareGameOptions());                // инициализируем игровую сессию с настройками по умолчанию

                        thread = new Thread(() => StartServer(session, port));              // выделяем поток под запуск сервера на определенный порт
                        thread.IsBackground = true;          //устанавливается для того, чтобы при закрытии программы фоновые потоки были закрыты
                        thread.Start();                                                         // запускаем поток => сервер

                        JoinGame("127.0.0.1", port);                                            // присоединяемся к игре на свой пк так как создаем игру
                        break;
                    case 2:
                        port = PortPrompt();                                    // спрашиваем номер порта

                        session = LoadGame();                                       // запускаем сохранение
                        if(session != null)
                        {
                            thread = new Thread(() => StartServer(session, port));      // выделяем поток под запуск сервера на определенный порт
                            thread.IsBackground = true;                 //устанавливается для того, чтобы при закрытии программы фоновые потоки были закрыты
                            thread.Start();                                             // запускаем поток => сервер

                            JoinGame("127.0.0.1", port);            // присоединяемся к игре на свой пк так как создаем игру
                        }
                        break;
                    case 3:
                        return;                             // возврат назад
                }
            } while (true);
        }

        static int PortPrompt()                                 // считывание номера порта
        {
            int port = -1;
            do
            {
                Console.WriteLine("Please enter port number:");
                int.TryParse(Console.ReadLine(), out port);
            } while (port < 0);

            return port;
        }

        static string AddressPrompt()                       // считываем ip адресс на который подключаемся
        {
            Console.WriteLine("Please provide address");
            return Console.ReadLine();
        }
        static void StartServer(GameSession session, int port)
        {
            var serializer = new Network.Core.Serialization.JsonSerializer();                       // инициализация сериализатора
            GameServer server = new GameServer(port, serializer, session);                      // инициализация игрового сервера

            try
            {
                
                server.Start();                                         // запуск сервера
                do
                {
                    server.Process();                                           // проверяем есть ли данные для получения + проверка на соединение
                    Thread.Sleep(1);
                } while (server.State != GameServerState.GameEnded);                // работает пока игра не окончится
            }
            catch (Exception)
            {
                //Something went wrong
            }

            server.Dispose();                                           // зачистка памяти по окончании работы
        }

        static GameSession LoadGame()
        {
            string[] paths = Directory.GetFiles("saves/", "*.json");                    //получаем список всех файлов сохранений
                
            if(paths.Length == 0)                                                       // если длина списка 0 значит соранений нет
            {
                Console.WriteLine("No saved games found.");
                return null;
            }

            var filenames = paths.Select(path => Path.GetFileName(path))                    // получаем имя файла  
                .ToArray();

            int fileIndex = SelectionPrompt("Select file to load", filenames) - 1;              // получаем индекс файла

            SaveData saveData = LoadSaveFile("saves/"+filenames[fileIndex]);                //загружаем выбранный файл

            if(saveData == null)                                            // если что-то пошло не так (данные в файле повреждены)
            {
                Console.WriteLine("File validation failed.");
                return null;
            }

            return new GameSession(saveData.Options, saveData.State);           // из файла восстанавливаем опции и состояния
        }

        static SaveData LoadSaveFile(string filename)
        {
            using (FileStream fileStream = File.OpenRead(filename))
            using (StreamReader fileReader = new StreamReader(fileStream))
            using (JsonTextReader jsonTextReader = new JsonTextReader(fileReader))
            using (JSchemaValidatingReader validatingReader = new JSchemaValidatingReader(jsonTextReader))
            {
                validatingReader.Schema = SaveDataJSchema;
                
                IList<string> messages = new List<string>();
                validatingReader.ValidationEventHandler += (o, a) => messages.Add(a.Message);

                SaveData saveData = Serializer.Deserialize<SaveData>(validatingReader);

                if (messages.Count != 0)
                {
                    return null;
                }
                else
                {
                    return saveData;
                }
            }
        }

        static void JoinGame(string addr, int port)
        {
            var serializer = new Network.Core.Serialization.JsonSerializer();      // сериализатор (приведение данных к унифицированному формату) 
            GameClient client = new GameClient(serializer);                         // инициализация игрового клиента
            GameView gameView = new GameView(client);                               // инициализация игрового представления

            try
            {
                client.Connect(addr, port);                                                     // подключаем клиент
                gameView.Process();                                                             // запсукаем игровой процесс
            }
            catch (Exception e)
            {
                Console.WriteLine(e);                                           // если не получилось подключиться то говорим об этом
                Console.WriteLine("Couldn't connect to specified server");
            }
            
        }


        static void CloseGame()         // закрываем игру
        {
            Environment.Exit(0);
        }

        static GameOptions PrepareGameOptions()     // настройки по умолчанию
        {
            return new GameOptions()
            {
                Seed = DateTime.Now.Millisecond
            };
        }

        public static int SelectionPrompt(string header, params string[] options)    // считываение номеров для выбора условий
        {
            int selection = -1;
            do
            {
                Console.WriteLine(header);
                for (int i = 0; i < options.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {options[i]}");
                }

                string input = Console.ReadLine();

                int.TryParse(input, out selection);

            } while (selection < 1 && selection > options.Length);

            return selection;
        }

    }
}
