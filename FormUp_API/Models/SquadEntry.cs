
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

        // The player's tactical position for this particular match.
        // This does not change the player's registered position.
        [FirestoreProperty("assignedPosition")]
        public string AssignedPosition { get; set; } = string.Empty;

        // Normalised pitch coordinates.
        [FirestoreProperty("pitchX")]
        public double? PitchX { get; set; }

        [FirestoreProperty("pitchY")]
        public double? PitchY { get; set; }

        public SquadEntry()
        {
        }
    }
}
