using CodeUs.Shared.Models;

namespace CodeUs.Shared.StateContainers
{
    public interface IWordsService
    {
        List<Word> Words { get; set; }

        List<Word> GetWordListFromPack(GameSettings gameSettings);

        List<Word> GetWordListFromCustomList(List<string> customList);

        void WordGuessed(Word word);
    }
}