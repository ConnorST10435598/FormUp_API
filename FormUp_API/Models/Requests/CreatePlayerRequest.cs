namespace FormUp_API.Models.Requests
{
    public class CreatePlayerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
    }
}
