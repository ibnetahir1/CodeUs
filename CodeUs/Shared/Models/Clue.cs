namespace CodeUs.Shared.Models
{
    public class Clue
    {
        public string Hint { get; set; } = "";
        public int NumberOfWords { get; set; }
        public int GuessesLeft { get; set; }
    }
}
