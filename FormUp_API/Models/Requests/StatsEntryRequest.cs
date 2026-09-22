namespace FormUp_API.Models.Requests
{
    public class StatsEntryRequest
    {
        public string PlayerId { get; set; } = string.Empty;
        public int Goals { get; set; }
        public int Assists { get; set; }
        public bool Cleansheet { get; set; }
        public int MinutesPlayed { get; set; }
        public double Rating { get; set; }
    }
}
