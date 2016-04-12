using Sentro.Models;

namespace Sentro.Interfaces
{
    public interface IBetRepository
    {
        void Save(Bet bet, int? balance);
    }
}
