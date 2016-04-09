using Sentro.Models;
using System.Collections.Generic;

namespace Sentro.Interfaces
{
    public interface IStreakRepository
    {
        void Save(PlayerStreak streak);

        PlayerStreak Get(Player player);

        Dictionary<string, List<PlayerStreak>> GetStreakData();
    }
}
