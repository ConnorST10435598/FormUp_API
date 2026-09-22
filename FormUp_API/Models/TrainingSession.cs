using Google.Cloud.Firestore;

namespace FormUp_API.Models
{
    [FirestoreData]
    public class TrainingSession
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("sessionDate")]
        public DateTime SessionDate { get; set; }

        [FirestoreProperty("notes")]
        public string? Notes { get; set; }

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty("attendance")]
        public Dictionary<string, AttendanceEntry> Attendance { get; set; } = new();

        public TrainingSession()
        {
            
        }
    }
}
