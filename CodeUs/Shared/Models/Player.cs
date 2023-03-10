namespace CodeUs.Shared.Models
{
    public class Player
    {
        public string ConnectionId { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsHost { get; set; } = false;
        public bool IsReady { get; set; } = false;
        public bool IsGuesser { get; set; } = false;
        public bool IsTurn { get; set; } = false;
        public bool WasLastTurn { get; set; } = false;
        public bool HasMeeting { get; set; } = true;
        public bool HasVoted { get; set; } = false;
        public Faction Faction { get; set; }
    }
}
