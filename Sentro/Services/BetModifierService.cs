using System;
using System.Configuration;
using System.Linq;
using Sentro.Enums;
using Sentro.Models;

namespace Sentro.Services
{
    public class BetModifierService
    {
        /// <summary>
        /// Modify wager depending on balance below tourney/mines thresholds, or due to exhibs, all according to user settings
        /// </summary>
        /// <param name="bet"></param>
        /// <param name="mode"></param>
        /// <param name="balance"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public Bet ApplySituation(Bet bet, Mode mode, int? balance, Match match)
        {
            int TOURNEY_ALL_IN_UNTIL;
            if (!Int32.TryParse(ConfigurationManager.AppSettings["tourneyAllInUntil"], out TOURNEY_ALL_IN_UNTIL))
            {
                TOURNEY_ALL_IN_UNTIL = 30000;
            }

            int MINES_ALL_IN_UNTIL;
            if (!Int32.TryParse(ConfigurationManager.AppSettings["minesAllInUntil"], out MINES_ALL_IN_UNTIL))
            {
                MINES_ALL_IN_UNTIL = 50000;
            }

            int EXHIBS_BASE_WAGER;
            if (!Int32.TryParse(ConfigurationManager.AppSettings["exhibsBaseWager"], out EXHIBS_BASE_WAGER))
            {
                EXHIBS_BASE_WAGER = 1;
            }

            if (mode == Mode.Exhibitions)
            {
                bet.Wager = EXHIBS_BASE_WAGER;
            }

            if (mode == Mode.Tournament && balance.HasValue && balance.Value < TOURNEY_ALL_IN_UNTIL)
            {
                Console.WriteLine("Pump it up! It's tourney time! {0} Bison Dollars!", balance.Value);
                bet.Wager = balance.Value;
            }
            else if (balance.HasValue && balance.Value < MINES_ALL_IN_UNTIL)
            {
                bet.Team = match.Blue.Players.First().Winrate <= match.Red.Players.First().Winrate ? match.Blue : match.Red; // bet underdog
                Console.WriteLine("We in the jungle baby, you gonna {1}! ${0}", balance.Value, bet.Team.Players.First().Name);
                bet.Wager = balance.Value;
            }

            return bet;
        }
    }
}
