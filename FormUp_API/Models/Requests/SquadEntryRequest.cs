namespace FormUp_API.Models.Requests
{
    public class SquadEntryRequest
    {
        public string PlayerId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Selection { get; set; } = string.Empty;
        public string AssignedPosition { get; set; } = string.Empty;
        public double? PitchX { get; set; }
        public double? PitchY { get; set; }
    }
}
