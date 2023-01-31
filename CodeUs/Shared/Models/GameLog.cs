namespace CodeUs.Shared.Models
{
    public class GameLog
    {
        public string Log { get; set; } = "";
        public LogType LogType { get; set; }
    }

    public enum LogType
    {
        Clue,
        Guess
    }
}
