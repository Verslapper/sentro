using System;
using System.Configuration;
using System.Linq;
using Sentro.Models;

namespace Sentro.Services
{
    public class StatBasedRecommendationService
    {
        public Bet GetRecommendedBet(Match match, int baseWager, int? balance)
        {
            int UPSET_POTENTIAL_DIFFERENCE;
            if (!Int32.TryParse(ConfigurationManager.AppSettings["upsetPotentialDifference"], out UPSET_POTENTIAL_DIFFERENCE))
            {
                UPSET_POTENTIAL_DIFFERENCE = 5;
            }

            int SOLID_FAVOURITE_DIFFERENCE;
            if (!Int32.TryParse(ConfigurationManager.AppSettings["solidFavouriteDifference"], out SOLID_FAVOURITE_DIFFERENCE))
            {
                SOLID_FAVOURITE_DIFFERENCE = 40;
            }

            int CLEAR_FAVOURITE_DIFFERENCE;
            if (!Int32.TryParse(ConfigurationManager.AppSettings["clearFavouriteDifference"], out CLEAR_FAVOURITE_DIFFERENCE))
            {
                CLEAR_FAVOURITE_DIFFERENCE = 40;
            }

            Team betOn;
            var wager = baseWager;
            var multiplier = 1;
            var red = match.Red.Players.First();
            var blue = match.Blue.Players.First();
            var redWinrate = match.Red.Players.First().Winrate;
            if (match.Red.Players.Count == 2)
            {
                redWinrate += match.Red.Players.Last().Winrate;
                Console.WriteLine("Second red player has {0}%", match.Red.Players.Last().Winrate);
                redWinrate = redWinrate / 2;
                Console.WriteLine("Averaged red to {0}%", redWinrate);
            }
            var blueWinrate = match.Blue.Players.First().Winrate;
            if (match.Blue.Players.Count == 2)
            {
                blueWinrate += match.Blue.Players.Last().Winrate;
                blueWinrate = blueWinrate / 2;
            }

            if (redWinrate - blueWinrate > UPSET_POTENTIAL_DIFFERENCE)
            {
                betOn = match.Red;
                if (redWinrate - blueWinrate > CLEAR_FAVOURITE_DIFFERENCE)
                {
                    multiplier = 10;
                }
                else if (redWinrate - blueWinrate > SOLID_FAVOURITE_DIFFERENCE)
                {
                    multiplier = 3;
                }
                wager = Math.Min(baseWager * multiplier, balance.HasValue ? balance.Value : baseWager * multiplier);
            }
            else if (blueWinrate - redWinrate > UPSET_POTENTIAL_DIFFERENCE)
            {
                betOn = match.Blue;
                if (blueWinrate - redWinrate > CLEAR_FAVOURITE_DIFFERENCE)
                {
                    multiplier = 10;
                }
                else if (blueWinrate - redWinrate > SOLID_FAVOURITE_DIFFERENCE)
                {
                    multiplier = 3;
                }
                wager = Math.Min(baseWager * multiplier, balance.HasValue ? balance.Value : baseWager * multiplier);
            }
            else if (red.Meter - blue.Meter >= 500)
            {
                betOn = match.Red;
            }
            else if (blue.Meter - red.Meter >= 500)
            {
                betOn = match.Blue;
            }
            else if (red.Life - blue.Life >= 800)
            {
                betOn = match.Red;
            }
            else if (blue.Life - red.Life >= 800)
            {
                betOn = match.Blue;
            }
            else // this bets upset by default in close matches
            {
                betOn = blueWinrate <= redWinrate ? match.Blue : match.Red;
                wager = Math.Min(baseWager * 3, balance.HasValue ? balance.Value : baseWager * 3);
            }

            return new Bet {Team = betOn, Wager = wager};
        }
    }
}
