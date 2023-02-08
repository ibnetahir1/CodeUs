namespace CodeUs.Shared.Models
{
    public class GameLog
    {
        public string PlayerName { get; set; } = "";
        public string Info { get; set; } = "";
        public LogType LogType { get; set; }
        public Faction GuessType { get; set; }
    }

    public enum LogType
    {
        Clue,
        Guess
    }
}
