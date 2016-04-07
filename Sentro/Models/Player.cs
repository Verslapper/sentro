using Sentro.Enums;

namespace Sentro.Models
{
    public class Player
    {
        public string Name { get; set; }
        public int TotalMatches { get; set; }
        public int Winrate { get; set; }
        public Tier Tier { get; set; }
        public int Life { get; set; }
        public int Meter { get; set; }
        public string Author { get; set; }
        public int Palette { get; set; }
    }
}
