namespace FormUp_API.Models.Requests
{
    public class SaveTeamRequest
    {
        public string TeamName { get; set; } = string.Empty;
        public int AgeGroup { get; set; }
    }
}
