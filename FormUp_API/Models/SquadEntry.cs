using Google.Cloud.Firestore;

namespace FormUp_API.Models
{
    [FirestoreData]
    public class SquadEntry
    {
        [FirestoreProperty("status")]
        public string Status { get; set; } = string.Empty;

        [FirestoreProperty("selection")]
        public string Selection { get; set; } = string.Empty;
        public SquadEntry()
        {
            
        }
    }
}
