using System;

namespace Gonzales.Models
{
    public class Match
    {
        public string Red { get; set; }
        public string Blue { get; set; }
        public int RedTotalBets { get; set; }
        public int BlueTotalBets { get; set; }
        public string Winner { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
