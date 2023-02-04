using System.ComponentModel.DataAnnotations;

namespace CodeUs.Shared.Models
{
    public class Clue
    {
        [RegularExpression(@"^\S+", ErrorMessage = "Clue cannot contain spaces")]
        public string Hint { get; set; } = "";
        public int NumberOfWords { get; set; }
        public int GuessesLeft { get; set; }
    }
}
