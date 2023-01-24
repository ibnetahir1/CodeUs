namespace CodeUs.Shared.Models
{
    public class Room
    {
        public string RoomCode { get; set; } = "";
        public List<Player> Players { get; set; } = new();
        public Clue Clue { get; set; } = new();

        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }

        public Player? GetPlayer(string name)
        {
            return Players.FirstOrDefault(x => x.Name == name);
        }

        public Player GetCurrentTurnPlayer()
        {
            return Players.FirstOrDefault(x => x.IsTurn == true)!;
        }

        public Player GetGuesser()
        {
            return Players.FirstOrDefault(x => x.IsGuesser == true)!;
        }

        public void SetNextTurn()
        {
            int currentTurnIndex = Players.FindIndex(x => x.IsTurn == true);
            int guesserIndex = Players.FindIndex(x => x.IsGuesser == true);
            int lastTurnIndex = Players.FindIndex(x => x.WasLastTurn == true);

            //first turn
            if(currentTurnIndex == -1)
            {
                //first in the list is guesser
                if (guesserIndex == 0)
                {
                    Players[1].IsTurn = true;
                    return;
                }
                //set the turn to be the first player's in the list
                else
                {
                    Players[0].IsTurn = true;
                    return;
                }
            }

            //current turn is not the guesser, so set the turn to be the guesser's
            if(currentTurnIndex != guesserIndex)
            {
                Players[currentTurnIndex].IsTurn = false;
                Players[currentTurnIndex].WasLastTurn = true;
                Players[guesserIndex].IsTurn = true;
                return;
            }

            //current turn is the guesser's, so set current turn to be last + 1.
            currentTurnIndex = lastTurnIndex;
            bool turnSet = false;
            while (!turnSet)
            {
                currentTurnIndex++;

                //end of the list
                if(currentTurnIndex == Players.Count)
                {
                    currentTurnIndex = 0;
                }

                // current index is the guesser, so we skip it
                if(currentTurnIndex == guesserIndex)
                {
                    continue;
                }

                Players[guesserIndex].IsTurn = false;
                Players[lastTurnIndex].WasLastTurn = false;
                Players[currentTurnIndex].IsTurn = true;
                turnSet = true;
            }
        }
    }
}
