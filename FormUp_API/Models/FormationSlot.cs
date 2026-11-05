using Google.Cloud.Firestore;

namespace FormUp_API.Models
{
    [FirestoreData]
    public class FormationSlot
    {
        [FirestoreProperty("slotId")]
        public string SlotId { get; set; } = string.Empty;

        [FirestoreProperty("label")]
        public string Label { get; set; } = string.Empty;

        [FirestoreProperty("x")]
        public double X { get; set; }

        [FirestoreProperty("y")]
        public double Y { get; set; }

        public FormationSlot()
        {
        }
    }
}