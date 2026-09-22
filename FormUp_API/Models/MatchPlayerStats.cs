using Google.Cloud.Firestore;

namespace FormUp_API.Models
{
    [FirestoreData]
    public class MatchPlayerStats
    {
        [FirestoreProperty("goals")]
        public int Goals { get; set; }

        [FirestoreProperty("assists")]
        public int Assists { get; set; }

        [FirestoreProperty("cleansheet")]
        public bool Cleansheet { get; set; }

        [FirestoreProperty("minutesPlayed")]
        public int MinutesPlayed { get; set; }

        [FirestoreProperty("rating")]
        public double Rating { get; set; }
        public MatchPlayerStats()
        {
            
        }
    }
}
