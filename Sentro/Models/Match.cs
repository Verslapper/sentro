using System;
using System.Linq;

namespace Sentro.Models
{
    public class Match : IComparable
    {
        public Team Red { get; set; }
        public Team Blue { get; set; }
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            var match = obj as Match;
            if (match != null)
            {
                if (this.Red.Players.First().Name == match.Red.Players.First().Name &&
                    this.Blue.Players.First().Name == match.Blue.Players.First().Name &&
                    this.Red.Players.First().Tier == match.Red.Players.First().Tier &&
                    this.Blue.Players.First().Tier == match.Blue.Players.First().Tier) // assume unique names across tiers
                {
                    return 0;
                }
            }

            return 1;
        }
    }
}
