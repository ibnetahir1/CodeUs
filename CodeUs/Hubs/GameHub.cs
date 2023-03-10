using CodeUs.Shared.Models;
using CodeUs.Shared.StateContainers;
using Microsoft.AspNetCore.SignalR;
using System.Numerics;


namespace CodeUs.Hubs
{
    public class GameHub : Hub
    {
        public const string HubUrl = "/gamehub";

        private IRoomsService _roomsService;
        private IWordsService _wordsService;

        public GameHub(IRoomsService roomsService, IWordsService wordsService)
        {
            _roomsService = roomsService;
            _wordsService = wordsService;
        }

        public async Task PlayerJoined(string playerName, string roomCode)
        {
            Player player = _roomsService.GetPlayer(playerName, roomCode);
            if (player.ConnectionId == "")
            {
                _roomsService.UpdateConnectionId(playerName, Context.ConnectionId, roomCode);
                await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
                await Clients.Group(roomCode).SendAsync("Update", player);
            }
            //Player? player = _roomsService.AddPlayerToRoom(playerName, Context.ConnectionId, roomCode);
            //if (player != null)
            //{
            //    await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            //}
            //await Clients.Group(roomCode).SendAsync("Update", player);
        }

        public async Task RemovePlayer(string playerName, string roomCode)
        {
            Player player = _roomsService.RemovePlayerFromRoom(playerName, roomCode);
            await Clients.Group(roomCode).SendAsync("PlayerRemoved", player);
            await Groups.RemoveFromGroupAsync(player.ConnectionId, roomCode);
        }

        public async Task ToggleReady(string playerName, string roomCode)
        {
            Player player = _roomsService.GetRoom(roomCode)!.GetPlayer(playerName)!;
            player.IsReady = !player.IsReady;
            await Clients.Group(roomCode).SendAsync("Update", player);
        }

        public async Task GameStart(GameSettings gameSettings, string roomCode, List<string>? customList)
        {
            // get the random words to play with
            List<Word> wordList = new();
            switch (gameSettings.Pack)
            {
                case Packs.Default:
                    wordList = _wordsService.GetWordListFromPack(gameSettings);
                    break;
                case Packs.Custom:
                    wordList = _wordsService.GetWordListFromCustomList(customList!);
                    break;
            }

            // randomly assign guesser and factions
            _roomsService.RandomlyAssignRoles(roomCode);

            // set total turns
            _roomsService.SetTotalTurns(gameSettings.NumberOfTurns, roomCode);

            // send all the information to everyone in the room
            await Clients.Group(roomCode).SendAsync("GameStart", wordList);
        }

        public async Task ClueGiven(Clue? clue, string playerName, string roomCode)
        {
            if (clue != null)
            {
                _roomsService.SetClue(clue, roomCode);
                GameLog log = new GameLog();
                log.PlayerName = playerName;
                log.Info = clue.Hint + " " + clue.NumberOfWords;
                log.LogType = LogType.Clue;
                _roomsService.AddGameLog(log, roomCode);
            }
            _roomsService.SetNextTurn(roomCode);
            await Clients.Group(roomCode).SendAsync("NextTurnAfterClue");
        }

        public async Task WordGuessed(Word word, string playerName, string roomCode)
        {
            _roomsService.DecrementGuessesLeft(roomCode);

            GameLog log = new GameLog();
            log.Info = word.Value;
            log.PlayerName = playerName;
            log.LogType = LogType.Guess;
            log.GuessType = word.Faction;
            _roomsService.AddGameLog(log, roomCode);

            await Clients.Group(roomCode).SendAsync("NextStepAfterGuess", word);
        }

        public async Task GuesserTurnDone(string roomCode)
        {
            if (_roomsService.GetRoom(roomCode)!.TurnsLeft == 0)
            {
                await Clients.Group(roomCode).SendAsync("TurnLimitReached");
            }
            else
            {
                _roomsService.SetNextTurn(roomCode);
                await Clients.Group(roomCode).SendAsync("NextTurnAfterGuesser");
            }
        }

        public async Task AllAgentsGuessed(string roomCode)
        {
            await Clients.Group(roomCode).SendAsync("GameOverAgentsWin");
        }

        public async Task AllSpiesGuessed(string roomCode)
        {
            await Clients.Group(roomCode).SendAsync("GameOverSpyWins");
        }

        public async Task CallMeeting(string playerName, string roomCode)
        {
            Player player = _roomsService.GetPlayer(playerName, roomCode);
            if (player.HasMeeting)
            {
                player.HasMeeting = false;

                await Clients.Group(roomCode).SendAsync("MeetingCalled", player);
            }
        }

        public async Task PlayerVoted(string voter, string votee, string roomCode)
        {
            _roomsService.GetRoom(roomCode)!.PlayerVoted(voter, votee);
            await Clients.Group(roomCode).SendAsync("PlayerVoted");
        }

        public async Task AllPlayersVoted(string roomCode)
        {
            Player? mostVoted = _roomsService.CheckIfPlayerVotedOut(roomCode);

            if (mostVoted != null)
            {
                await Clients.Group(roomCode).SendAsync("PlayerVotedOut", mostVoted);
            }
            else
            {
                await Clients.Group(roomCode).SendAsync("NoPlayerVotedOut");
            }
        }

        public async Task ContinueGameAfterVoting(string roomCode)
        {
            Room room = _roomsService.GetRoom(roomCode)!;

            room.Votes = new();
            foreach (var player in room.Players)
            {
                player.HasVoted = false;
            }

            await Clients.Group(roomCode).SendAsync("ContinueGameAfterVoting");
        }

        public async Task ReturnToLobby(string roomCode)
        {
            //Room newRoom = new();
            //List<Player> newPlayers = new();

            //List<Player> players = _roomsService.GetPlayers(roomCode);
            //foreach (Player player in players)
            //{
            //    Player newPlayer = new();
            //    newPlayer.Name = player.Name;
            //    //newPlayer.ConnectionId = "";
            //    newPlayer.IsHost= player.IsHost;

            //    newPlayers.Add(newPlayer);
            //    //player.IsGuesser = false;
            //    //player.IsTurn = false;
            //    //player.WasLastTurn = false;
            //    //player.IsReady = false;
            //    //player.HasMeeting = true;
            //    //player.HasVoted = false;
            //    //player.Faction = Faction.Neutral;
            //}

            //Room room = _roomsService.GetRoom(roomCode)!;

            //newRoom.RoomCode = room.RoomCode;
            //newRoom.Players = newPlayers;
            //newRoom.TurnsLeft = 8;

            _roomsService.RemoveRoom(roomCode);
            //_roomsService.AddRoom(newRoom);

            //room.Clue = new();
            //room.Votes = new();
            //room.GameLogs = new();
            //room.TurnsLeft = 8;

            await Clients.Group(roomCode).SendAsync("NavigateToLobby");
        }

        public async Task RemovePlayerFromGroup(string roomCode)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode!);
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"{Context.ConnectionId} connected");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? e)
        {
            (Player? player, string? roomCode) = _roomsService.RemovePlayerWithId(Context.ConnectionId);

            if (player != null && roomCode != null)
            {
                await Clients.Group(roomCode).SendAsync("PlayerRemoved", player);
                await Groups.RemoveFromGroupAsync(player!.ConnectionId, roomCode!);
            }
            
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
            await base.OnDisconnectedAsync(e);
        }
    }
}
