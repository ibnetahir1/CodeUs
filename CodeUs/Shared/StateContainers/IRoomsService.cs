using CodeUs.Shared.Models;

namespace CodeUs.Shared.StateContainers
{
    public interface IRoomsService
    {
        public List<Room> Rooms { get; set; }

        public bool ContainsRoom(string roomCode);

        public Player? AddPlayerToRoom(string playerName, string connectionId, string roomCode);

        public Player RemovePlayerFromRoom(string playerName, string roomCode);

        public (Player? player, string? roomCode) RemovePlayerWithId(string connectionId);

        public Room? GetRoom(string roomCode);

        public List<Player> GetPlayers(string roomCode);

        public Player GetPlayer(string playerName, string roomCode);

        public void RandomlyAssignRoles(string roomCode);

        public void SetTotalTurns(int turns, string roomCode);

        public void SetNextTurn(string roomCode);

        public Player GetCurrentTurnPlayer(string roomCode);

        public Player GetGuesser(string roomCode);

        public void SetClue(Clue clue, string roomCode);

        public void DecrementGuessesLeft(string roomCode);

        public void PlayerVoted(string voter, string votee, string roomCode);

        public Player? CheckIfPlayerVotedOut(string roomCode);

        public void AddGameLog(GameLog log, string roomCode);
    }
}
