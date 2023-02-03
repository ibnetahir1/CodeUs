using CodeUs.Shared.Models;
using CodeUs.Shared.StateContainers;
using Microsoft.AspNetCore.SignalR;


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
            Player player = _roomsService.AddPlayerToRoom(playerName, roomCode);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Group(roomCode).SendAsync("Update", player);
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
                    wordList = _wordsService.GetWordListFromPack("Default");
                    break;
                case Packs.Custom:
                    wordList = _wordsService.GetWordListFromCustomList(customList!);
                    break;
            }

            // randomly assign guesser and factions
            _roomsService.RandomlyAssignRoles(roomCode);

            // send all the information to everyone in the room
            await Clients.Group(roomCode).SendAsync("GameStart", wordList);
        }

        public async Task ClueGiven(Clue? clue, string playerName, string roomCode)
        {
            //Player? player = null;
            if (clue != null)
            {
                _roomsService.SetClue(clue, roomCode);
                GameLog log = new GameLog();
                log.Log = $"{playerName} has given the clue: {clue.Hint} {clue.NumberOfWords}";
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
            log.Log = $"{playerName} guessed: {word.Value}";
            log.LogType = LogType.Guess;
            _roomsService.AddGameLog(log, roomCode);

            await Clients.Group(roomCode).SendAsync("NextStepAfterGuess", word);
        }

        public async Task GuesserTurnDone(string roomCode)
        {
            if (_roomsService.GetRoom(roomCode)!.TurnsLeft == 0)
            {
                ResetGameState(roomCode);
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
            ResetGameState(roomCode);

            await Clients.Group(roomCode).SendAsync("GameOverAgentsWin");
        }

        public async Task AllSpiesGuessed(string roomCode)
        {
            ResetGameState(roomCode);

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

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"{Context.ConnectionId} connected");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? e)
        {
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
            await base.OnDisconnectedAsync(e);
        }

        private void ResetGameState(string roomCode)
        {
            List<Player> players = _roomsService.GetPlayers(roomCode);
            foreach (Player player in players)
            {
                player.IsGuesser = false;
                player.IsTurn = false;
                player.WasLastTurn = false;
                player.IsReady = false;
                player.HasMeeting = true;
                player.HasVoted = false;
            }

            Room room = _roomsService.GetRoom(roomCode)!;
            room.Clue = new();
            room.Votes = new();
            room.TurnsLeft = 8;
        }
    }
}
