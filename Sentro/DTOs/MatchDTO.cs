﻿namespace Sentro.DTOs
{
    internal class MatchDTO
    {
        // {"p1name":"Gowcaizer","p1totalmatches":"175","p1winrate":"53","p1tier":"A","p1life":"1000","p1meter":"0","p1author":"Shimon","p1palette":"2",
        // "p2name":"Big volfogg","p2totalmatches":"122","p2winrate":"47","p2tier":"A","p2life":"800","p2meter":"0","p2author":"Nani-","p2palette":"4"}
        public string p1name { get; set; }
        public string p1totalmatches { get; set; }
        public string p1winrate { get; set; }
        public string p1tier { get; set; }
        public string p1life { get; set; }
        public string p1meter { get; set; }
        public string p1author { get; set; }
        public string p1palette { get; set; }

        public string p2name { get; set; }
        public string p2totalmatches { get; set; }
        public string p2winrate { get; set; }
        public string p2tier { get; set; }
        public string p2life { get; set; }
        public string p2meter { get; set; }
        public string p2author { get; set; }
        public string p2palette { get; set; }
    }
}
