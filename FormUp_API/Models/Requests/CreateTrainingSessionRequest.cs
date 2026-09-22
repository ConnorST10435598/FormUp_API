namespace FormUp_API.Models.Requests
{
    public class CreateTrainingSessionRequest
    {
        public DateTimeOffset SessionDate { get; set; }
        public string? Notes { get; set; }
    }
}
