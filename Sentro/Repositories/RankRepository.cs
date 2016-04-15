using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Sentro.Interfaces;

namespace Sentro.Repositories
{
    public class RankRepository : IRankRepository
    {
        private readonly string RANK_FILE_NAME = "ranks.csv";

        public RankRepository()
        {
            if (ConfigurationManager.AppSettings["baseFilePath"] != null)
            {
                RANK_FILE_NAME = ConfigurationManager.AppSettings["baseFilePath"] + RANK_FILE_NAME;
            }
        }

        public Dictionary<string, int> GetAllRanks()
        {
            var ranks = new Dictionary<string, int>();
            using (var reader = new StreamReader(RANK_FILE_NAME))
            {
                string line;
                var rank = 1; // assumes sorted list, best first
                while ((line = reader.ReadLine()) != null)
                {
                    //X,Kaonasi,8,79,12:30.0
                    var parts = line.Split(',');
                    if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                    {
                        try
                        {
                            ranks.Add(parts[1], rank++);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Bad rank data is {0}", parts[1]);
                        }
                    }
                }
            }

            return ranks;
        }
    }
}
