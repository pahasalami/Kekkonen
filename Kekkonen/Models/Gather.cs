using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kekkonen.Models
{

    public class GatherGame
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<GatherTeam> Teams { get; set; }
        public GatherTeam WinningTeam { get; set; }
        public int MaxTeams { get; set; }

        public static List<string> FallbackTeamNames = new List<string>()
        {
            "Red", "Blue", "Yellow", "Green", "Cyan", "Pink"
        };

        public override string ToString()
        {
            return $"Name: {Name}, Teams: {string.Join(", ", Teams)}/{MaxTeams}";
        }
    }

    public class GatherTeam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<GatherPlayer> Players { get; set; }
        public int MaxPlayers { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}, Players: {string.Join(", ", Players)}/{MaxPlayers}";
        }

        public bool IsFull
        {
            get
            {
                return Players.Count >= MaxPlayers;
            }
        }
    }

    public class GatherPlayer
    {
        public IGuildUser Player { get; set; }
    }
}
