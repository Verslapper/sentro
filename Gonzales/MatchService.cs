using Gonzales.Models;
using Gonzales.Interfaces;
using Gonzales.Repositories;

namespace Gonzales
{
    public class MatchService
    {
        private static readonly IMatchRepository _matchRepository = new FileMatchRepository();

        internal void Save(Match match)
        {
            _matchRepository.Save(match);
        }
    }
}
