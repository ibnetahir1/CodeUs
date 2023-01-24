﻿using CodeUs.Shared.Models;

namespace CodeUs.Shared.StateContainers
{
    public class RoomsService : IRoomsService
    {
        public List<Room> Rooms { get; set; } = new();

        public bool ContainsRoom(string roomCode)
        {
            foreach(var room in Rooms)
            {
                if(room.RoomCode == roomCode)
                {
                    return true;
                }
            }
            return false;
        }

        public Player AddPlayerToRoom(string roomCode, string playerName)
        {
            Room? room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode);

            Player player = new();
            player.Name = playerName;

            if (room == null)
            {
                room = new();
                room.RoomCode = roomCode;
                player.IsHost = true;
            }

            room.AddPlayer(player);
            Rooms.Add(room);

            return player;
        }

        public Room? GetRoom(string roomCode)
        {
            return Rooms.FirstOrDefault(x => x.RoomCode == roomCode);
        }

        public List<Player> GetPlayers(string roomCode)
        {
            return Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!.Players;
        }

        public Player GetPlayer(string playerName, string roomCode)
        {
            return Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!.GetPlayer(playerName)!;
        }

        public void RandomlyAssignRoles(string roomCode)
        {
            Room? room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode);
            List<Player> newPlayerList = new();
            Random rng = new();
            int totalPlayers = room!.Players.Count;
            int randomIndex = 0;

            // set guesser
            randomIndex = rng.Next(totalPlayers);
            Player guesser = room.Players[randomIndex];
            guesser.IsGuesser = true;
            guesser.Faction = Faction.Agent;
            newPlayerList.Add(guesser);
            room.Players.RemoveAt(randomIndex);
            totalPlayers--;

            // set spy
            randomIndex = rng.Next(totalPlayers);
            Player spy = room.Players[randomIndex];
            spy.Faction = Faction.Spy;
            newPlayerList.Add(spy);
            room.Players.RemoveAt(randomIndex);
            totalPlayers--;

            // set agents
            for (int i = 0; i < totalPlayers; i++)
            {
                Player agent = room.Players[i];
                agent.Faction= Faction.Agent;
                newPlayerList.Add(agent);
            }

            room.Players = newPlayerList.OrderBy(x => x.Name).ToList();
        }

        public void SetNextTurn(string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            room.SetNextTurn();
        }

        public Player GetCurrentTurnPlayer(string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            return room.GetCurrentTurnPlayer();
        }

        public Player GetGuesser(string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            return room.GetGuesser();
        }

        public void SetClue(Clue clue, string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            room.Clue = clue;
            room.Clue.GuessesLeft = room.Clue.NumberOfWords;
        }

        public void DecrementGuessesLeft(string roomCode)
        {
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomCode)!;

            room.Clue.GuessesLeft--;
        }
    }
}
