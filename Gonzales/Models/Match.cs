using System;

namespace Gonzales.Models
{
    public class Match
    {
        public string Red { get; set; }
        public string Blue { get; set; }
        public int RedTotalBetted { get; set; }
        public int BlueTotalBetted { get; set; }
        public string Winner { get; set; }
        public TimeSpan Duration { get; set; }
        public int Bettors { get; set; }
    }
}
