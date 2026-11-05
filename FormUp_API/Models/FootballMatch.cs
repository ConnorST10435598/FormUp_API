using Google.Cloud.Firestore;

namespace FormUp_API.Models
{
    [FirestoreData]
    public class FootballMatch
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("opponent")]
        public string Opponent { get; set; } = string.Empty;

        [FirestoreProperty("matchDate")]
        public DateTime MatchDate { get; set; }

        [FirestoreProperty("venue")]
        public string Venue { get; set; } = string.Empty;

        [FirestoreProperty("teamScore")]
        public int? TeamScore { get; set; }

        [FirestoreProperty("opponentScore")]
        public int? OpponentScore { get; set; }

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty("squad")]
        public Dictionary<string, SquadEntry> Squad { get; set; } = new();

        [FirestoreProperty("matchStats")]
        public Dictionary<string, MatchPlayerStats> Stats { get; set; } = new();

        [FirestoreProperty("attendance")]
        public Dictionary<string, AttendanceEntry> Attendance { get; set; } = new();

        [FirestoreProperty("formationName")]
        public string FormationName { get; set; } = string.Empty;

        [FirestoreProperty("formationCode")]
        public string FormationCode { get; set; } = string.Empty;



        public FootballMatch()
        {
            
        }
    }
}
