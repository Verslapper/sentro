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

            int CLEAR_FAVOURITE_DIFFERENCE;
            if (!Int32.TryParse(ConfigurationManager.AppSettings["clearFavouriteDifference"], out CLEAR_FAVOURITE_DIFFERENCE))
            {
                CLEAR_FAVOURITE_DIFFERENCE = 40;
            }

            Team betOn;
            var wager = baseWager;

            if (red.Winrate - blue.Winrate > UPSET_POTENTIAL_DIFFERENCE)
            {
                betOn = latestMatch.Red;
                wager = red.Winrate - blue.Winrate > CLEAR_FAVOURITE_DIFFERENCE ? Math.Min(baseWager * 10, balance.HasValue ? balance.Value : baseWager * 10) : baseWager;
            }
            else if (blue.Winrate - red.Winrate > UPSET_POTENTIAL_DIFFERENCE)
            {
                betOn = latestMatch.Blue;
                wager = blue.Winrate - red.Winrate > CLEAR_FAVOURITE_DIFFERENCE ? Math.Min(baseWager * 10, balance.HasValue ? balance.Value : baseWager * 10) : baseWager;
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
