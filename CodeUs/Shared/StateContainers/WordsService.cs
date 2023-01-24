using CodeUs.Extensions;
using CodeUs.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace CodeUs.Shared.StateContainers
{
    public class WordsService : IWordsService
    {
        public List<Word> Words { get; set; } = new();

        /// <summary>
        /// Gets 30 random words from the specified pack
        /// </summary>
        /// <returns></returns>
        public List<Word> GetWordListFromPack(string wordPack)
        {
            Random rng = new();

            string filePath = "";
            switch (wordPack)
            {
                case "Default":
                    filePath = "Packs/Default.txt";
                    break;
            }

            List<Word> newWordList = new();

            // get all lines
            var lines = File.ReadLines(filePath);

            int totalWordCount = 30;
            int totalSpyCount = 2;
            int totalAgentCount = (totalWordCount - totalSpyCount + 2 - 1) / 2; // rounds up

            //populate only 30 words
            for (int i = 0; i < totalWordCount; i++)
            {
                //must be a unique word
                bool isUnique = false;
                while (!isUnique)
                {
                    string randomWord = lines.ElementAt(rng.Next(lines.Count()));
                    Word newWord = new() { Value = randomWord };

                    if (!newWordList.Exists(x => x.Value == newWord.Value))
                    {
                        // assign faction
                        if(totalSpyCount > 0)
                        {
                            newWord.Faction = Faction.Spy;
                            totalSpyCount--;
                        }
                        else if(totalAgentCount > 0)
                        {
                            newWord.Faction = Faction.Agent;
                            totalAgentCount--;
                        }
                        else
                        {
                            newWord.Faction = Faction.Neutral;
                        }

                        isUnique = true;
                        newWordList.Add(newWord);
                    }
                }
            }

            newWordList.Shuffle();

            return newWordList;
        }

        public List<Word> GetWordListFromCustomList(List<string> customList)
        {
            throw new NotImplementedException();
        }

        public void WordGuessed(Word word)
        {
            Words.FirstOrDefault(x => x.Value == word.Value)!.IsGuessed = true;
        }
    }
}
