using System;
using System.Configuration;
using System.Linq;
using Sentro.Models;

namespace Sentro.Services
{
    public class StatBasedRecommendationService
    {
        private readonly StreakService _streakService;
        private int UPSET_POTENTIAL_DIFFERENCE;
        private int SOLID_FAVOURITE_DIFFERENCE;
        private int CLEAR_FAVOURITE_DIFFERENCE;

        public StatBasedRecommendationService(StreakService streakService)
        {
            _streakService = streakService;

            if (!int.TryParse(ConfigurationManager.AppSettings["upsetPotentialDifference"], out UPSET_POTENTIAL_DIFFERENCE))
            {
                UPSET_POTENTIAL_DIFFERENCE = 5;
            }

            if (!int.TryParse(ConfigurationManager.AppSettings["solidFavouriteDifference"], out SOLID_FAVOURITE_DIFFERENCE))
            {
                SOLID_FAVOURITE_DIFFERENCE = 40;
            }

            if (!int.TryParse(ConfigurationManager.AppSettings["clearFavouriteDifference"], out CLEAR_FAVOURITE_DIFFERENCE))
            {
                CLEAR_FAVOURITE_DIFFERENCE = 40;
            }
        }

        public Bet GetRecommendedBet(Match match, int baseWager, int? balance)
        {
            Team betOn;
            var wager = baseWager;
            var multiplier = 1;
            var red = match.Red.Players.First();
            var blue = match.Blue.Players.First();

            int redWinrate, blueWinrate;
            GetPlayerWinrates(match, out redWinrate, out blueWinrate);

            var initialFave = redWinrate > blueWinrate ? match.Red : match.Blue;
            
            var redStreak = _streakService.GetStreaksFor(red.Name);
            var blueStreak = _streakService.GetStreaksFor(blue.Name);
            int? redRecentWinrate = null;
            int? blueRecentWinrate = null;
            if (redStreak.Count > 0)
            {
                var currentStreak = redStreak.LastOrDefault().Streak;
                var bestStreak = redStreak.Max(s => s == null ? 0 : s.Streak);
                var worstStreak = redStreak.Min(s => s == null ? 0 : s.Streak);
                var streakDiff = bestStreak + worstStreak;

                var streakToUse = streakDiff;
                if (streakToUse > 0)
                {
                    redRecentWinrate = (int)((double)streakToUse / (streakToUse + 1) * 100);
                }
                else if (streakToUse < 0)
                {
                    redRecentWinrate = (int)((double)1 / (-streakToUse + 1) * 100);
                }

                Console.WriteLine("{0} is currently on a {1} streak", red.Name, currentStreak);
            }

            if (blueStreak.Count > 0)
            {
                var currentStreak = blueStreak.LastOrDefault().Streak;
                var bestStreak = blueStreak.Where(s => s.Player.Tier == match.Blue.Players.First().Tier).Max(s => s == null ? 0 : s.Streak);
                var worstStreak = blueStreak.Min(s => s == null ? 0 : s.Streak);
                var streakDiff = bestStreak + worstStreak;

                var streakToUse = streakDiff;
                if (streakToUse > 0)
                {
                    blueRecentWinrate = (int)((double)streakToUse / (streakToUse + 1) * 100);
                }
                else if (streakToUse < 0)
                {
                    blueRecentWinrate = (int)((double)1 / (-streakToUse + 1) * 100);
                }
                Console.WriteLine("{0} is currently on a {1} streak", blue.Name, currentStreak);
            }

            if (redRecentWinrate.HasValue)
            {
                redWinrate = (redWinrate + redRecentWinrate.Value) / 2;
                Console.WriteLine("Adjusted {0} winrate to {1}+{2}/2={3}%", red.Name, red.Winrate, redRecentWinrate, redWinrate);
            }

            if (blueRecentWinrate.HasValue)
            {
                blueWinrate = (blueWinrate + blueRecentWinrate.Value) / 2;
                Console.WriteLine("Adjusted {0} winrate to {1}+{2}/2={3}%", blue.Name, blue.Winrate, blueRecentWinrate, blueWinrate);
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
                    multiplier = 5;
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
                    multiplier = 5;
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
            else if (initialFave == match.Blue && blueWinrate < redWinrate) // if my stats pick a different winner to winrates
            {
                betOn = match.Red;
                wager = Math.Min(baseWager * 4, balance.HasValue ? balance.Value : baseWager * 4);
            }
            else if (initialFave == match.Red && redWinrate < blueWinrate) // if my stats pick a different winner to winrates
            {
                betOn = match.Blue;
                wager = Math.Min(baseWager * 4, balance.HasValue ? balance.Value : baseWager * 4);
            }
            else // this bets upset by default in close matches
            {
                betOn = blueWinrate <= redWinrate ? match.Blue : match.Red;
                wager = Math.Min(baseWager * 3, balance.HasValue ? balance.Value : baseWager * 3);
            }

            return new Bet { Team = betOn, Wager = wager };
        }

        private static void GetPlayerWinrates(Match match, out int redWinrate, out int blueWinrate)
        {
            redWinrate = match.Red.Players.First().Winrate;
            if (match.Red.Players.Count == 2)
            {
                redWinrate += match.Red.Players.Last().Winrate;
                Console.WriteLine("Second red player has {0}%", match.Red.Players.Last().Winrate);
                redWinrate = redWinrate / 2;
                Console.WriteLine("Averaged red to {0}%", redWinrate);
            }
            blueWinrate = match.Blue.Players.First().Winrate;
            if (match.Blue.Players.Count == 2)
            {
                blueWinrate += match.Blue.Players.Last().Winrate;
                blueWinrate = blueWinrate / 2;
            }
        }
    }
}
