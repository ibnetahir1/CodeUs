using CodeUs.Shared.Models;

namespace CodeUs.Shared.StateContainers
{
    public class RoomsService : IRoomsService
    {
        ILogger<RoomsService> _logger;

        public List<Room> Rooms { get; set; } = new();

        public RoomsService(ILogger<RoomsService> logger)
        {
            _logger = logger;
        }

        public bool ContainsRoom(string roomCode)
        {
            foreach(var room in Rooms)
            {
                if(room.RoomCode == roomCode)
                {
                    return true;
                }
            }
            return false;
        }

        public Player HostRoom(string playerName, string roomCode)
        {
            _logger.LogInformation("hosting room");

            Player player = new();
            player.Name = playerName;

            Room room = new();
            room.RoomCode = roomCode;
            player.IsHost = true;
            room.AddPlayer(player);
            Rooms.Add(room);

            return player;
        }

        public Player JoinRoom(string playerName, string roomCode)
        {
            _logger.LogInformation("joining room");

            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            Player player = new();
            player.Name = playerName;

            room.AddPlayer(player);

            return player;
        }

        public void UpdateConnectionId(string playerName, string connectionId, string roomCode)
        {
            _logger.LogInformation("updating conId");

            Player player = GetPlayer(playerName, roomCode);

            player.ConnectionId = connectionId;
        }

        public void AddRoom(Room room)
        {
            Rooms.Add(room);
        }

        public void RemoveRoom(string roomCode)
        {
            Rooms.RemoveAll(x => x.RoomCode == roomCode);
        }

        //public Player? AddPlayerToRoom(string playerName, string connectionId, string roomCode)
        //{
        //    Room? room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode);

        //    // room exists and player with this name exists
        //    if(room != null && room.GetPlayer(playerName) != null)
        //    {
        //        _logger.LogInformation($"room with code: {roomCode} exists and player with name: {playerName} exists.");
        //        return null;
        //    }

        //    _logger.LogInformation($"room code is: {room?.RoomCode}. adding player: {playerName}.");

        //    Player player = new();
        //    player.Name = playerName;
        //    player.ConnectionId = connectionId;

        //    if (room == null)
        //    {
        //        room = new();
        //        room.RoomCode = roomCode;
        //        player.IsHost = true;
        //        Rooms.Add(room);
        //    }

        //    room.AddPlayer(player);

        //    return player;
        //}

        public Player RemovePlayerFromRoom(string playerName, string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            return room.RemovePlayer(playerName);
        }

        public (Player? player, string? roomCode) RemovePlayerWithId(string connectionId)
        {
            Room? room = Rooms.FirstOrDefault(x => x.Players.Any(p => p.ConnectionId == connectionId));
            Player? retPlayer = room?.RemovePlayerWithId(connectionId);

            return (retPlayer, room?.RoomCode);
        }

        public Room? GetRoom(string roomCode)
        {
            return Rooms.FirstOrDefault(x => x.RoomCode == roomCode);
        }

        public List<Player> GetPlayers(string roomCode)
        {
            return Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!.Players;
        }

        public Player GetPlayer(string playerName, string roomCode)
        {
            return Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!.GetPlayer(playerName)!;
        }

        public bool ShouldPlayerBeAdded(string playerName, string roomCode)
        {
            Room? room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode);
            if (room == null)
            {
                return true;
            }
            else
            {
                return !room.Players.Exists(x => x.Name == playerName);
            }
        }

        public void RandomlyAssignRoles(string roomCode)
        {
            Room? room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode);
            List<Player> newPlayerList = new();
            Random rng = new();
            int totalPlayers = room!.Players.Count;
            int randomIndex = 0;

            // set guesser
            randomIndex = rng.Next(totalPlayers);
            Player guesser = room.Players[randomIndex];
            guesser.IsGuesser = true;
            guesser.Faction = Faction.Agent;
            newPlayerList.Add(guesser);
            room.Players.RemoveAt(randomIndex);
            totalPlayers--;

            // set spy
            randomIndex = rng.Next(totalPlayers);
            Player spy = room.Players[randomIndex];
            spy.Faction = Faction.Spy;
            newPlayerList.Add(spy);
            room.Players.RemoveAt(randomIndex);
            totalPlayers--;

            // set agents
            for (int i = 0; i < totalPlayers; i++)
            {
                Player agent = room.Players[i];
                agent.Faction= Faction.Agent;
                newPlayerList.Add(agent);
            }

            room.Players = newPlayerList.OrderBy(x => x.Name).ToList();
        }

        public void SetTotalTurns(int turns, string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            room.TurnsLeft = turns;
        }

        public void SetNextTurn(string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            room.SetNextTurn();
        }

        public Player GetCurrentTurnPlayer(string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            return room.GetCurrentTurnPlayer();
        }

        public Player GetGuesser(string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            return room.GetGuesser();
        }

        public void SetClue(Clue clue, string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            room.Clue = clue;
            room.Clue.GuessesLeft = room.Clue.NumberOfWords + 1; // give extra guess
        }

        public void DecrementGuessesLeft(string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            room.Clue.GuessesLeft--;
        }

        public void PlayerVoted(string voter, string votee, string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            room.PlayerVoted(voter, votee);
        }

        public Player? CheckIfPlayerVotedOut(string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            return room.CheckIfPlayerVotedOut();
        }

        public void AddGameLog(GameLog log, string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            room.GameLogs.Add(log);
        }
    }
}
