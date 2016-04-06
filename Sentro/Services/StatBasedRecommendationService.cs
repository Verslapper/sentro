using System;
using System.Configuration;
using System.Linq;
using Sentro.Models;

namespace Sentro.Services
{
    public class StatBasedRecommendationService
    {
        public Bet GetRecommendedBet(Match latestMatch, int baseWager, int? balance)
        {
            var red = latestMatch.Red.Players.First();
            var blue = latestMatch.Blue.Players.First();

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

            if (red.Winrate - blue.Winrate > UPSET_POTENTIAL_DIFFERENCE)
            {
                betOn = latestMatch.Red;
                if (red.Winrate - blue.Winrate > CLEAR_FAVOURITE_DIFFERENCE)
                {
                    multiplier = 10;
                }
                else if (red.Winrate - blue.Winrate > SOLID_FAVOURITE_DIFFERENCE)
                {
                    multiplier = 3;
                }
                wager = Math.Min(baseWager * multiplier, balance.HasValue ? balance.Value : baseWager * multiplier);
            }
            else if (blue.Winrate - red.Winrate > UPSET_POTENTIAL_DIFFERENCE)
            {
                betOn = latestMatch.Blue;
                if (blue.Winrate - red.Winrate > CLEAR_FAVOURITE_DIFFERENCE)
                {
                    multiplier = 10;
                }
                else if (blue.Winrate - red.Winrate > SOLID_FAVOURITE_DIFFERENCE)
                {
                    multiplier = 3;
                }
                wager = Math.Min(baseWager * multiplier, balance.HasValue ? balance.Value : baseWager * multiplier);
            }
            else if (red.Meter - blue.Meter >= 500)
            {
                betOn = latestMatch.Red;
            }
            else if (blue.Meter - red.Meter >= 500)
            {
                betOn = latestMatch.Blue;
            }
            else if (red.Life - blue.Life >= 800)
            {
                betOn = latestMatch.Red;
            }
            else if (blue.Life - red.Life >= 800)
            {
                betOn = latestMatch.Blue;
            }
            else // this bets upset by default in close matches
            {
                betOn = blue.Winrate <= red.Winrate ? latestMatch.Blue : latestMatch.Red;
                wager = Math.Min(baseWager * 3, balance.HasValue ? balance.Value : baseWager * 3);
            }

            return new Bet {Team = betOn, Wager = wager};
        }
    }
}
