namespace FormUp_API.Models.Requests
{
    public class CreateMatchRequest
    {
        public string Opponent { get; set; } = string.Empty;
        public DateTimeOffset MatchDate { get; set; }
        public string Venue { get; set; } = string.Empty;
        public int? TeamScore { get; set; }
        public int? OpponentScore { get; set; }
    }
}
