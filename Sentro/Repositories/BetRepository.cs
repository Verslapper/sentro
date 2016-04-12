using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Sentro.Interfaces;
using Sentro.Models;

namespace Sentro.Repositories
{
    internal class BetRepository : IBetRepository
    {
        private readonly string BET_FILE_NAME = "bets.csv";

        public BetRepository()
        {
            if (ConfigurationManager.AppSettings["baseFilePath"] != null)
            {
                BET_FILE_NAME = ConfigurationManager.AppSettings["baseFilePath"] + BET_FILE_NAME;
            }
        }

        public void Save(Bet bet, int? balance)
        {
            // Remove , from player so CSV is maintained (thanks The manticore, the queen, and the dragon)
            using (var writetext = new StreamWriter(BET_FILE_NAME, true))
            {
                writetext.WriteLine(bet.Team.Players.First().Name.Replace(",", "") + "," + bet.Wager + "," + DateTime.Now + "," + (balance.HasValue ? balance.Value + "," : ""));
            }
        }
    }
}
