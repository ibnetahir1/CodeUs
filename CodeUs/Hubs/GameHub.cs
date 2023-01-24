using CodeUs.Shared.Models;
using CodeUs.Shared.StateContainers;
using Microsoft.AspNetCore.SignalR;


namespace CodeUs.Hubs
{
    public class GameHub : Hub
    {
        public const string HubUrl = "/game";

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

        public async Task ClueGiven(Clue? clue, string roomCode)
        {
            if (clue != null)
            {
                _roomsService.SetClue(clue, roomCode);
            }
            _roomsService.SetNextTurn(roomCode);
            await Clients.Group(roomCode).SendAsync("NextTurnAfterClue");
        }

        public async Task WordGuessed(Word word, string roomCode)
        {
            _roomsService.DecrementGuessesLeft(roomCode);
            await Clients.Group(roomCode).SendAsync("NextStepAfterGuess", word);
        }

        public async Task GuesserTurnDone(string roomCode)
        {
            _roomsService.SetNextTurn(roomCode);
            await Clients.Group(roomCode).SendAsync("NextTurnAfterGuesser");
        }

        public async Task AllAgentsGuessed(string roomCode)
        {
            List<Player> players = _roomsService.GetPlayers(roomCode);
            foreach (Player player in players)
            {
                player.IsGuesser = false;
                player.IsTurn = false;
                player.WasLastTurn = false;
                player.IsReady = false;
            }

            _roomsService.GetRoom(roomCode)!.Clue = new();

            await Clients.Group(roomCode).SendAsync("GameOverAgentsWin");
        }

        public async Task AllSpiesGuessed(string roomCode)
        {
            List<Player> players = _roomsService.GetPlayers(roomCode);
            foreach (Player player in players)
            {
                player.IsGuesser = false;
                player.IsTurn = false;
                player.WasLastTurn = false;
                player.IsReady = false;
            }

            _roomsService.GetRoom(roomCode)!.Clue = new();

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
    }
}
