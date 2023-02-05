namespace CodeUs.Shared.Models
{
    public class GameSettings
    {
        public Packs Pack { get; set; }
        public int NumberOfWords { get; set; } = 30;
        public int NumberOfTurns { get; set; } = 8;
    }
}
