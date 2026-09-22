using Google.Cloud.Firestore;

namespace FormUp_API.Models
{
    [FirestoreData]
    public class Player
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("name")]
        public string Name { get; set; } = string.Empty;

        [FirestoreProperty("position")]
        public string Position { get; set; } = string.Empty;

        [FirestoreProperty("dateOfBirth")]
        public string? DateOfBirth { get; set; }

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Player()
        {
            
        }
    }
}
