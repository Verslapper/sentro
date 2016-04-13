using System;
using System.Linq;
using Sentro.Models;

namespace Sentro.Services
{
    public class NameBasedRecommendationService
    {
        private readonly RankService _rankService = new RankService();

        public Team GetTextOnlyRecommendedBet(Match match)
        {
            Team betOn = new Random().Next(100) % 2 == 0 ? match.Red : match.Blue;

            var redRank = _rankService.GetRank(match.Red.Players.First().Name);
            var blueRank = _rankService.GetRank(match.Blue.Players.First().Name);

            if (redRank.HasValue && blueRank.HasValue)
            {
                // Prefer low rank
                if (redRank.Value < blueRank.Value)
                {
                    Console.WriteLine("{0}'s #{1} more impressive than {2}'s #{3}", match.Red.Players.First().Name, redRank.Value, match.Blue.Players.First().Name, blueRank.Value);
                    betOn = match.Red;
                }
                else if (redRank.Value > blueRank.Value)
                {
                    Console.WriteLine("{0}'s #{1} oughta smash {2}'s #{3}!", match.Blue.Players.First().Name, blueRank.Value, match.Red.Players.First().Name, redRank.Value);
                    betOn = match.Blue;
                }
            }

            return betOn;
        }
    }
}
