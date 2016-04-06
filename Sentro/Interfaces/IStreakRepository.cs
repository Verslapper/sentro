using Sentro.Models;

namespace Sentro.Interfaces
{
    public interface IStreakRepository
    {
        void Save(PlayerStreak streak);
    }
}
