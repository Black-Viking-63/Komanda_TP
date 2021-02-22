using Komanda.Game.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Komanda.Game
{
    public class GameSession
    {

        // опции (настройки игры)
        public GameOptions Options { get; private set; }

        // рандомайзер игры
        private Random Random { get; set; }

        // текущее состояние
        public int CurrentTurn { get; private set; }

        // карта
        public LevelMap Map { get; private set; }

        // игроки
        public List<Player> Players { get; private set; }

        // враги

        public List<Enemy> Enemies { get; private set; }

        // оставшиеся спавны

        public int SpawnsRemain { get; private set; }

        // обработка события окончания игры 

        public event EventHandler<GameEndedEventArgs> GameEnded = delegate { };

        //New game
        public GameSession(GameOptions options)                                  // создание новой игры
        {
            Options = options;                                                  // задаем опции (размер поля, число игроков врагов стенок и т.д.)       

            Random = new Random(options.Seed);                                  // запускаем рандомайзер

            SpawnsRemain = options.TotalEnemies;                                // оставшиеся враги которые надо создать

            Players = new List<Player>();                                          // запускаем игроков, врагов
            Enemies = new List<Enemy>();

            Map = new LevelMap(options.MapWidth, options.MapHeight);            // запускаем поле карту...

            GenerateWalls();                            // генерация стен
            GeneratePlayers();                          // генерация игроков
            SpawnEnemies(1.0); // спавн противников со 100% вероятностью
        }


        // конструктор
        public GameSession() : this(new GameOptions()) { }

        //Loaded game
        public GameSession(GameOptions options, GameState state)   // загрузка параметров игры
        {
            Options = options;
            Random = new Random(options.Seed);
            Map = new LevelMap(options.MapWidth, options.MapHeight);
            
            UpdateState(state);
        }

        public GameState RetrieveState()                                // получение состояния
        {
            return new GameState(
                CurrentTurn,
                Map.Map,
                SpawnsRemain,
                Players,
                Enemies);
        }

        public void UpdateState(GameState state)                        // обновление состояния
        {
            CurrentTurn = state.CurrentTurn;
            Map.UpdateMap(state.Map);
            SpawnsRemain = state.SpawnsRemain;
            Players = state.Players;
            Enemies = state.Enemies;
        }

        //Check if attempted move is correct for current player
        public bool ValidateAction(PlayerAction action)                      // проверка действия
        {
            var player = GetPlayerForCurrentTurn();                         // получение объекта игрока

            if (!player.Alive) return false;                                 // проверка на живучесть игрока

            if(action.Type == ActionType.Move)
            {                                                                                               // проверка возможности выполнения хода 
                Vector2Int newPos = player.Position + Vector2Int.Direction(action.Direction);      // подсчет новой позиции
                switch (Map.GetTileType(newPos))
                {
                    case TileType.Player:
                        return false;
                    case TileType.Wall:
                        return false;
                }
            }

            return true;
        }


        public void ProcessAction(PlayerAction action)                      // обработка действия ходьбы или стрельбы
        {

            switch (action.Type)
            {
                case ActionType.Move:                                       // если игрок выполнил ход
                    ProcessPlayerMove(action.Direction);
                    break;
                case ActionType.Shoot:                                      // если игрок выполнил выстрел
                    ProcessPlayerShoot(action.Direction);
                    break;
            }
        }

        private void ProcessPlayerMove(Direction direction)                         // ход игрока или бомжа
        {
            var player = GetPlayerForCurrentTurn();

            Vector2Int newPos = player.Position + Vector2Int.Direction(direction);     // расчитываем координату новой точки в которую мы наступим

            switch (Map.GetTileType(newPos))
            {
                case TileType.Empty:                                                        // если клетка пуста то игрок /бомж может туда наступить
                    MoveEntity(player, newPos);
                    break;
                case TileType.Enemy:                                                        // если в клетке враг и игрок наступает в нее то бомж убивает игрока
                    player.Alive = false;
                    Map[player.Position] = 0;
                    break;
            }
        }

        private void ProcessPlayerShoot(Direction direction)                    // стрельба игрока
        {
            var player = GetPlayerForCurrentTurn();

            int distance = 0;
            switch (direction)                             // определяем направление стрельбы
            {
                case Direction.Down:
                case Direction.Up:
                    distance = Options.MapHeight - 1;
                    break;
                case Direction.Left:
                case Direction.Right:
                    distance = Options.MapWidth - 1;
                    break;
            }

            Vector2Int pos = player.Position;
            for(int i = 0; i < distance; i++)                                       // проверка остановки пули
            {
                pos = pos + Vector2Int.Direction(direction);
                switch (Map.GetTileType(pos))
                {
                    case TileType.Enemy:                                                        // если пуля попала в бомжа
                        var enemy = Enemies.First(enemy => enemy.Id == Map[pos]);
                        Enemies.Remove(enemy);                                              // удаляем бомжа
                        Map[pos] = 0;                                                       // затираем его координату
                        return;
                    case TileType.Wall:                                                         // если пуля попала в стену
                        Map[pos] = 0;
                        return;
                    case TileType.Player:                                                       //если пуля попала во второго игрока
                        var targetPlayer = Players.First(player => player.Id == Map[pos]);
                        targetPlayer.Alive = false;                                                          // удаляем игрока
                        Map[pos] = 0;                                                               // затираем его координату
                        return;
                }
            }
        }

        public void NextTurn()           // следующий ход (игроку или бомжам)                            
        {
            do
            {
                CurrentTurn++;

                if (CurrentTurn % Players.Count == 0)  // ход бомжей после хода второго игрока если он жив
                {
                    ProcessEnemies();
                    SpawnEnemies();
                }
                // порверки живучести игроков/бомжей
                // если все игроки зарезаны бомжами то игра окончена поражением
                if (Players.All(player => !player.Alive))
                {
                    GameEnded(this, new GameEndedEventArgs(false));
                    return;
                }

                // если игроки убили всех бомжей то игра окончена победой
                if (SpawnsRemain == 0 && Enemies.Count == 0)
                {
                    GameEnded(this, new GameEndedEventArgs(true));
                    return;
                }


            } while (!GetPlayerForCurrentTurn().Alive);  // убираем возможность для мертвого игрока делать действия
        }

        public Player GetPlayerForCurrentTurn()  // проверяем может ходить
        {
            return Players[CurrentTurn % Players.Count];
        }

        private void GenerateWalls()                            // генерация стенки
        {
            for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {
                    var roll = Random.NextDouble();                         // ставим стенку в любую рандомную клетку
                    if (roll < Options.WallSpawnChance)
                    {
                        Map[x, y] = 1;
                    }
                }
            }
        }

        private void GeneratePlayers()                  // генерация позиций игроков
        {
            for(int i = -1; i >= -2; i--)
            {
                Vector2Int pos;
                do
                {
                    pos = GetRandomPos();
                } while (Map.GetTileType(pos) == TileType.Player);  // любая клетка без игрока

                var player = new Player(i, pos, true);
                Players.Add(player);
                Map[pos] = player.Id;
            }
        }

        private void SpawnEnemies(double bonusChance = 0.0)                             // спавн бомжей
        {
            if (SpawnsRemain == 0) return;

            for (int i = 0; i < Options.MaxSpawnPerTurn; i++)
            {
                var roll = Random.NextDouble();                                        
                if (roll < Options.SpawnChance + bonusChance)
                {
                    Vector2Int pos;
                    do
                    {
                        pos = GetRandomPos();                                            // генерируем рандомную позицию для бомжа
                    } while (Map.GetTileType(pos) != TileType.Empty);

                    var enemy = new Enemy(SpawnsRemain + 1, pos);
                    Enemies.Add(enemy);                                          // ставим бомжа в найденную клетку

                    Map[pos] = enemy.Id;

                    SpawnsRemain--;
                    if (SpawnsRemain == 0) return;
                }
            }
        }

        private void ProcessEnemies()
        {
            for(int i = 0; i < Enemies.Count; i++)
            {
                Direction moveDirection = (Direction)Random.Next(0, 4);                         // движение игрока в ранддомном направлении

                Vector2Int newPos = Enemies[i].Position + Vector2Int.Direction(moveDirection);      // расчет новых координат

                switch (Map.GetTileType(newPos))
                {
                    case TileType.Empty:                                    // если клетка пустая просто в нее наступает он
                        MoveEntity(Enemies[i], newPos);
                        //Map[Enemies[i].Position] = 0;
                        //Map[newPos] = Enemies[i].Id;
                        //Enemies[i].Position = newPos;
                        break;
                    case TileType.Player:
                        var player = Players.First(player => player.Id == Map[newPos]);         // если в клетке игрок он его режет и встает на его позицию
                        player.Alive = false;
                        MoveEntity(Enemies[i], newPos);
                        break;
                }
            }
        }

        private void MoveEntity(Entity entity, Vector2Int newPosition)                                  // движение персонажей по карте
        {
            Map[entity.Position] = 0;
            Map[newPosition] = entity.Id;                                                           

            newPosition.x = (newPosition.x % Map.Width + Map.Width) % Map.Width;                    // расчет новых позиций
            newPosition.y = (newPosition.y % Map.Height + Map.Height) % Map.Height;

            entity.Position = newPosition;                                                              // присвоение персонажу новой позиции
        }

        private Vector2Int GetRandomPos()                                               // генерация новой позиции
        {
            int x = Random.Next(0, Options.MapWidth);
            int y = Random.Next(0, Options.MapHeight);

            return new Vector2Int(x, y);
        }
    }
}
