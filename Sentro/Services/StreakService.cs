using Sentro.Interfaces;
using Sentro.Models;
using Sentro.Repositories;
using System.Collections.Generic;

namespace Sentro.Services
{
    public class StreakService
    {
        private readonly IStreakRepository _streakRepository = new FileStreakRepository();
        private Dictionary<string, List<PlayerStreak>> _streakData = new Dictionary<string, List<PlayerStreak>>();

        public StreakService()
        {
            _streakData = _streakRepository.GetStreakData();
        }

        public void Save(PlayerStreak streak)
        {
            _streakRepository.Save(streak);
        }

        public List<PlayerStreak> GetStreaksFor(string playerName)
        {
            List<PlayerStreak> playerStreaks;
            if (_streakData.TryGetValue(playerName, out playerStreaks))
            {
                return playerStreaks;
            }
            return new List<PlayerStreak>();
        }
    }
}
