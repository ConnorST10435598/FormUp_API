using Google.Cloud.Firestore;

namespace FormUp_API.Models
{
    [FirestoreData]
    public class SavedFormation
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("name")]
        public string Name { get; set; } = string.Empty;

        [FirestoreProperty("code")]
        public string Code { get; set; } = string.Empty;

        [FirestoreProperty("positions")]
        public List<FormationSlot> Positions { get; set; } = new();

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public SavedFormation()
        {
        }
    }
}