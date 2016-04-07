using System;
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

        public void Save(PlayerStreak streak)
        {
            Console.WriteLine("Fake saved {0} streak {1} in {2} tier", streak.Player.Name, streak.Streak, streak.Player.Tier);
        }
    }
}
