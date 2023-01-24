using CodeUs.Shared.Models;

namespace CodeUs.Shared.StateContainers
{
    public interface IRoomsService
    {
        public List<Room> Rooms { get; set; }

        public bool ContainsRoom(string roomCode);

        public Player AddPlayerToRoom(string roomCode, string playerName);

        public Room? GetRoom(string roomCode);

        public List<Player> GetPlayers(string roomCode);

        public Player GetPlayer(string playerName, string roomCode);

        public void RandomlyAssignRoles(string roomCode);

        public void SetNextTurn(string roomCode);

        public Player GetCurrentTurnPlayer(string roomCode);

        public Player GetGuesser(string roomCode);

        public void SetClue(Clue clue, string roomCode);

        public void DecrementGuessesLeft(string roomCode);
    }
}
