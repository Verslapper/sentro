using System;
using System.Collections.Generic;
using Sentro.Interfaces;
using Sentro.Models;

namespace Sentro.Repositories
{
    internal class ConsoleStreakRepository : IStreakRepository
    {
        public PlayerStreak Get(Player player)
        {
            Console.WriteLine("I can't get streak stats, I'm just a woxy!");
            return null;
        }

        public Dictionary<string, List<PlayerStreak>> GetStreakData()
        {
            Console.WriteLine("I can't get streak data, I'm just a woxy!");
            return new Dictionary<string, List<PlayerStreak>>();
        }

        public void Save(PlayerStreak streak)
        {
            Console.WriteLine("Fake saved {0} streak {1} in {2} tier", streak.Player.Name, streak.Streak, streak.Player.Tier);
        }
    }
}
