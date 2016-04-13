using System;
using System.Configuration;
using Sentro.Interfaces;
using Sentro.Models;
using System.IO;
using System.Collections.Generic;
using Sentro.Enums;

namespace Sentro.Repositories
{
    internal class FileStreakRepository : IStreakRepository
    {
        private readonly string STREAK_FILE_NAME = "streaks.csv";
        public FileStreakRepository()
        {
            if (ConfigurationManager.AppSettings["saveStreaks"] != "false" && ConfigurationManager.AppSettings["baseFilePath"] != null)
            {
                STREAK_FILE_NAME = ConfigurationManager.AppSettings["baseFilePath"] + STREAK_FILE_NAME;
            }
        }
        
        public Dictionary<string, List<PlayerStreak>> GetStreakData()
        {
            var streakData = new Dictionary<string, List<PlayerStreak>>();

            using (var reader = new StreamReader(STREAK_FILE_NAME))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(',');
                    if (parts.Length > 1)
                    {
                        var player = new Player { Name = parts[0] };
                        var streak = new PlayerStreak { Streak = int.Parse(parts[1]) };
                        if (parts.Length > 2)
                        {
                            player.Tier = (Tier)Enum.Parse(typeof(Tier), parts[2]);
                        }
                        if (parts.Length > 3)
                        {
                            player.Winrate = int.Parse(parts[3]);
                        }
                        DateTime streakDate;
                        if (parts.Length > 4 && DateTime.TryParse(parts[4], out streakDate))
                        {
                            streak.Date = streakDate;
                        }
                        streak.Player = player;

                        List<PlayerStreak> existingStreak;
                        if (streakData.TryGetValue(player.Name, out existingStreak))
                        {
                            existingStreak.Add(streak);
                            streakData[player.Name] = existingStreak;
                        }
                        else
                        {
                            streakData.Add(player.Name, new List<PlayerStreak> { streak });
                        }
                    }
                }
            }

            return streakData;
        }

        public void Save(PlayerStreak streak)
        {
            // Remove , from player so CSV is maintained (thanks The manticore, the queen, and the dragon)
            using (var writetext = new StreamWriter(STREAK_FILE_NAME, true))
            {
                writetext.WriteLine(streak.Player.Name.Replace(",","") + "," + streak.Streak + "," + streak.Player.Tier + "," + streak.Player.Winrate + "," + DateTime.Now);
            }

            Console.WriteLine("Saved {3}% {0} streak {1} in {2} tier to file", streak.Player.Name, streak.Streak, streak.Player.Tier, streak.Player.Winrate);
        }
    }
}
