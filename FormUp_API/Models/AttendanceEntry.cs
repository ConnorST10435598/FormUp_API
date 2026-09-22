using Google.Cloud.Firestore;
using System.Globalization;

namespace FormUp_API.Models
{
    [FirestoreData]
    public class AttendanceEntry
    {
        [FirestoreProperty("status")]
        public string Status { get; set; } = string.Empty;

        [FirestoreProperty("reason")]
        public string? Reason { get; set; }

        public AttendanceEntry()
        {
            
        }
    }
}
