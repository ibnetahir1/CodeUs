namespace CodeUs.Shared.Models
{
    public class Room
    {
        public string RoomCode { get; set; } = "";
        public List<Player> Players { get; set; } = new();
        public Clue Clue { get; set; } = new();
        public int TurnsLeft { get; set; } = 8;

        /// <summary>
        /// first string is the player's name, and the list is the names of the players voting this player.
        /// </summary>
        public Dictionary<string, List<string>> Votes { get; set; } = new();

        public List<GameLog> GameLogs { get; set; } = new();

        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }

        public Player RemovePlayer(string playerName)
        {
            Player player = Players.FirstOrDefault(p => p.Name == playerName)!;
            Players.RemoveAll(p => p.Name == playerName);

            return player;
        }

        public Player? RemovePlayerWithId(string connectionId)
        {
            Player? player = Players.FirstOrDefault(p => p.ConnectionId == connectionId);
            Players.RemoveAll(p => p.ConnectionId == connectionId);

            return player;
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
                TurnsLeft--;
                turnSet = true;
            }
        }

        public void PlayerVoted(string voter, string votee)
        {
            Player voterPlayer = Players.FirstOrDefault(x => x.Name == voter)!;
            voterPlayer.HasVoted = true;

            if(Votes.ContainsKey(votee))
            {
                Votes[votee].Add(voter);
            }
            else
            {
                Votes.Add(votee, new List<string> { voter });
            }
        }

        public Player? CheckIfPlayerVotedOut()
        {
            Player? playerVoted = null;
            int mostVotes = 0;
            foreach(var votee in Votes)
            {
                if (votee.Key == "SkipVote" && votee.Value.Count >= mostVotes)
                {
                    playerVoted = null;
                    mostVotes = votee.Value.Count;
                }
                if (votee.Value.Count > mostVotes)
                {
                    playerVoted = Players.FirstOrDefault(x => x.Name == votee.Key);
                    mostVotes = votee.Value.Count;
                }
                else if (votee.Value.Count == mostVotes)
                {
                    playerVoted = null;
                }
            }

            return playerVoted;
        }
    }
}
