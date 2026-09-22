namespace FormUp_API.Models.Requests
{
    public class UpdateMatchRequest
    {
        //allnullable incase that field wasnt updated
        public string? Opponent { get; set; }
        public DateTimeOffset? MatchDate { get; set; }
        public string? Venue { get; set; }
        public int? TeamScore { get; set; }
        public int? OpponentScore { get; set; }
    }
}
