using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentro.DTOs
{
    internal class PlayerStreakDTO
    {
        public string PlayerName { get; set; }
        public int Streak { get; set; }
        public string Tier { get; set; }
        public int Winrate { get; set; }
        public DateTime Date { get; set; }
    }
}
