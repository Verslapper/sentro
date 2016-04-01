namespace Sentro.DTOs
{
    internal class ModeDTO
    {
        // {"p1name":"Drake","p2name":"Pac-man delta","p1total":"0","p2total":"0","status":"open","alert":"","x":0,"remaining":"45 more matches until the next tournament!"}
        public string remaining { get; set; }
        public string p1name { get; set; }
        public string p2name { get; set; }
    }
}
