using Sentro.Interfaces;
using Sentro.Models;
using Sentro.Repositories;

namespace Sentro.Services
{
    public class BetService
    {
        public readonly IBetRepository _betRepository = new BetRepository();

        public void Save(Bet bet, int? balance)
        {
            _betRepository.Save(bet, balance);
        }
    }
}
