using Google.Cloud.Firestore;

namespace FormUp_API.Models
{
    [FirestoreData]
    public class Coach
    {
        [FirestoreDocumentId]
        public string Uid { get; set; } = string.Empty;

        [FirestoreProperty("email")]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        [FirestoreProperty("profilePhotoUrl")]
        public string? ProfilePhotoUrl { get; set; } 

        [FirestoreProperty("preferredLanguage")]
        public string PreferredLanguage { get; set; } = string.Empty;

        [FirestoreProperty("teamName")]
        public string? TeamName { get; set; }

        [FirestoreProperty("ageGroup")]
        public int? AgeGroup { get; set; }

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Coach()
        {
            
        }
    }
}
