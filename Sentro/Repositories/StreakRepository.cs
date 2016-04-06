using System;
using Sentro.Interfaces;
using Sentro.Models;

namespace Sentro.Repositories
{
    internal class StreakRepository : IStreakRepository
    {
        public void Save(PlayerStreak streak)
        {
            Console.WriteLine("Fake saved {0} streak {1} in {2}", streak.Player.Name, streak.Streak, streak.Player.Tier);
        }
    }
}
