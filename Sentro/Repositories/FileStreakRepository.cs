using System;
using Sentro.Interfaces;
using Sentro.Models;
using System.IO;
using System.Collections.Generic;
using Sentro.Enums;

namespace Sentro.Repositories
{
    internal class FileStreakRepository : IStreakRepository
    {
        const string STREAK_FILE_NAME = "streaks.csv"; // REFACTOR: Get from config so we can link to Dropbox. If it's not being run in two places at once.
        
        public Dictionary<string, List<PlayerStreak>> GetStreakData()
        {
            var streakData = new Dictionary<string, List<PlayerStreak>>();

            using (StreamReader reader = new StreamReader(STREAK_FILE_NAME))
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
                        if (parts.Length > 4)
                        {
                            streak.Date = DateTime.Parse(parts[4]);
                        }

                        List<PlayerStreak> existingStreak;
                        if (streakData.TryGetValue(player.Name, out existingStreak))
                        {
                            existingStreak.Add(streak);
                            streakData[player.Name] = existingStreak;
                        }
                        else
                        {
                            streakData.Add(player.Name, new List<PlayerStreak> { streak });
                        };
                    }
                }
            }

            return streakData;
        }

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
