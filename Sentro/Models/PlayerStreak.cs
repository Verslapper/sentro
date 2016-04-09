using System;

namespace Sentro.Models
{
    public class PlayerStreak
    {
        public Player Player { get; set; }
        public int Streak { get; set; }
        public DateTime Date { get; set; }
    }
}
