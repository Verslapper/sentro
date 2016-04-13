using System.Collections.Generic;
using Sentro.Interfaces;
using Sentro.Repositories;

namespace Sentro.Services
{
    public class RankService
    {
        private readonly Dictionary<string,int> _ranks = new Dictionary<string,int>();
        private readonly IRankRepository _rankRepository = new RankRepository();

        public RankService()
        {
            _ranks = GetAllRanks();
        }

        public int? GetRank(string playerName)
        {
            int rank;
            if (_ranks.TryGetValue(playerName, out rank))
            {
                return rank;
            }
            return null;
        }

        private Dictionary<string, int> GetAllRanks()
        {
            return _rankRepository.GetAllRanks();
        }
    }
}
