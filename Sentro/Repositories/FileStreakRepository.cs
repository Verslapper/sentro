using System;
using Sentro.Interfaces;
using Sentro.Models;
using System.IO;

namespace Sentro.Repositories
{
    internal class FileStreakRepository : IStreakRepository
    {
        const string STREAK_FILE_NAME = "streaks.csv"; // REFACTOR: Get from config so we can link to Dropbox. If it's not being run in two places at once.
        public PlayerStreak Get(Player player)
        {
            PlayerStreak streak = null;
            var streakText = string.Empty;
            using (StreamReader readtext = new StreamReader(STREAK_FILE_NAME))
            {
                //string readMeText = readtext.ReadLine();
                streakText = readtext.ReadToEnd();
            }

            if (streakText.Contains(player.Name + ","))
            {
                Console.WriteLine("{0} is in our streak data file!", player.Name);
            }

            return streak;
        }

        public void Save(PlayerStreak streak)
        {
            // Remove , from player so CSV is maintained (thanks The manticore, the queen, and the dragon)
            using (StreamWriter writetext = new StreamWriter(STREAK_FILE_NAME, true))
            {
                writetext.WriteLine(streak.Player.Name.Replace(",","") + "," + streak.Streak + "," + streak.Player.Tier + "," + streak.Player.Winrate + "," + DateTime.Now.Date);
            }

            Console.WriteLine("Saved {3}% {0} streak {1} in {2} tier to file", streak.Player.Name, streak.Streak, streak.Player.Tier, streak.Player.Winrate);
        }
    }
}
