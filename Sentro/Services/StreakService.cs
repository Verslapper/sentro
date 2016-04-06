using Sentro.Interfaces;
using Sentro.Models;
using Sentro.Repositories;

namespace Sentro.Services
{
    public class StreakService
    {
        private readonly IStreakRepository _streakRepository = new StreakRepository();

        public void Save(PlayerStreak streak)
        {
            _streakRepository.Save(streak);
        }
    }
}
