using System;
using System.Linq;
using Sentro.Models;

namespace Sentro.Services
{
    public class NameBasedRecommendationService
    {
        public Team GetTextOnlyRecommendedBet(Match match)
        {
            Team betOn = new Random().Next(100) % 2 == 0 ? match.Red : match.Blue;

            var redRank = GetRank(match.Red.Players.First());
            var blueRank = GetRank(match.Blue.Players.First());

            // Prefer low rank
            if (redRank < blueRank)
            {
                betOn = match.Red;
            }
            else if (redRank > blueRank)
            {
                betOn = match.Blue;
            }

            return betOn;
        }

        private int? GetRank(Player player)
        {
            // Wanna do a salty-wide ranking system from scrapes of tiers and results? Me neither! Maybe later eh!
            if (player.Name.ToLower().Contains("sentro"))
            {
                return 5;
            }
            if (player.Name.ToLower().Contains("rugal"))
            {
                return 10;
            }
            if (player.Name.ToLower().Contains("geese"))
            {
                return 20;
            }
            if (player.Name.ToLower().Contains("god"))
            {
                return 21;
            }
            if (player.Name.ToLower().Contains("akuma"))
            {
                return 22;
            }
            if (player.Name.ToLower().Contains("orochi"))
            {
                return 25;
            }
            if (player.Name.ToLower().Contains("ken"))
            {
                return 26;
            }
            if (player.Name.ToLower().Contains("ryu"))
            {
                return 27;
            }
            if (player.Name.ToLower().Contains("kula"))
            {
                return 28;
            }
            if (player.Name.ToLower().Contains("k'"))
            {
                return 29;
            }
            if (player.Name.ToLower().Contains("ex5"))
            {
                return 30;
            }
            if (player.Name.ToLower().Contains("ex4"))
            {
                return 40;
            }
            if (player.Name.ToLower().Contains("ex3"))
            {
                return 50;
            }
            if (player.Name.ToLower().Contains("ex2"))
            {
                return 60;
            }
            if (player.Name.ToLower().Contains("ex"))
            {
                return 70;
            }
            return 100000;
        }
    }
}
