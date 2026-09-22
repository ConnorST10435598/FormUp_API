namespace FormUp_API.Models.Requests
{
    public class UpdateTrainingSessionRequest
    {
        public DateTimeOffset? SessionDate { get; set; }
        public string? Notes { get; set; }
    }
}
