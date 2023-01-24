namespace CodeUs.Shared.Models
{
    public class Word
    {
        public string Value { get; set; } = "";
        public Faction Faction { get; set; }
        public bool IsGuessed { get; set; } = false;
    }
}
