using Google.Cloud.Firestore;

namespace FormUp_API.Services
{
    public class FirestoreService
    {
        public FirestoreDb FireDatabase { get; }

        public FirestoreService(IConfiguration config)
        {
            string projectId = config["Firebase:ProjectId"] ?? throw new InvalidOperationException("missing configuration");
            string? credentialsJson = config["Firebase:CredentialsJson"];

            if (!string.IsNullOrWhiteSpace(credentialsJson))
            {
                FireDatabase = new FirestoreDbBuilder
                { ProjectId = projectId, JsonCredentials = credentialsJson }.Build();
            }
            else
            {
                FireDatabase = FirestoreDb.Create(projectId);
            }

        }

        public DocumentReference CoachDoc(string uid)
        {
            CollectionReference coachesCollection = FireDatabase.Collection("coaches");
            DocumentReference coachDocument = coachesCollection.Document(uid);
            return coachDocument;
        }

        public CollectionReference PlayersCollection(string uid)
        {
            DocumentReference coachDocument = CoachDoc(uid);
            CollectionReference playersSubcollection = coachDocument.Collection("players");
            return playersSubcollection;
        }

        public DocumentReference PlayerDoc(string uid, string playerId)
        {
            CollectionReference playersSubcollection = PlayersCollection(uid);
            DocumentReference playerDocument = playersSubcollection.Document(playerId);
            return playerDocument;
        }

        public CollectionReference MatchesCollection(string uid)
        {
            DocumentReference coachDocument = CoachDoc(uid);
            CollectionReference matchesSubCollection = coachDocument.Collection("matches");
            return matchesSubCollection;
        }

        public DocumentReference MatchDoc(string uid, string matchId)
        {
            CollectionReference matchesSubCollection = MatchesCollection(uid);
            DocumentReference matchDocument = matchesSubCollection.Document(matchId);
            return matchDocument;
        }

        public CollectionReference SessionsCollection(string uid)
        {
            DocumentReference coachDocument = CoachDoc(uid);
            CollectionReference sessionsSubCollection = coachDocument.Collection("sessions");
            return sessionsSubCollection;
        }

        public DocumentReference SessionDoc(string uid, string sessionId)
        {
            CollectionReference sessionsSubCollection = SessionsCollection(uid);
            DocumentReference sessionDocument = sessionsSubCollection.Document(sessionId);
            return sessionDocument;
        }

        public CollectionReference FormationsCollection(string uid)
        {
            return CoachDoc(uid).Collection("formations");
        }

        public DocumentReference FormationDoc(string uid, string formationId)
        {
            return FormationsCollection(uid).Document(formationId);
        }
    }
}