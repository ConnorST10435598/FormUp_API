using System.Security.Cryptography;
using System.Text;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using FormUp_API.Models;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace FormUp_API.Services;

// Called by GitHub Actions every 5 minutes sends one FCM reminder per event and scheduled time.
public sealed class ReminderService
{
    private readonly FirestoreService _firestore;
    private readonly FirebaseMessaging _fcm;
    private readonly ILogger<ReminderService> _logger;

    public ReminderService(FirestoreService firestore, IConfiguration config, ILogger<ReminderService> logger)
    {
        _firestore = firestore;
        _logger = logger;
        var json = config["Firebase:CredentialsJson"];
        var credential = string.IsNullOrWhiteSpace(json)
            ? GoogleCredential.GetApplicationDefault()
            : GoogleCredential.FromJson(json);
        var app = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
        {
            Credential = credential,
            ProjectId = config["Firebase:ProjectId"]
        });
        _fcm = FirebaseMessaging.GetMessaging(app);
    }

    private CollectionReference Devices(string uid) => _firestore.CoachDoc(uid).Collection("deviceTokens");
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    public async Task RegisterTokenAsync(string uid, string token)
    {
        await Devices(uid).Document(Hash(token)).SetAsync(new Dictionary<string, object>
        {
            ["token"] = token,
            ["updatedAt"] = Timestamp.GetCurrentTimestamp()
        });
    }

    public async Task SetEnabledAsync(string uid, bool enabled)
    {
        await _firestore.CoachDoc(uid).SetAsync(new Dictionary<string, object>
        {
            ["matchRemindersEnabled"] = enabled
        }, SetOptions.MergeAll);
    }

    public async Task<int> SendDueAsync()
    {
        var now = DateTime.UtcNow;
        // Loose window tolerates small GitHub schedule delays Firestore receipt  is used to prevent duplicate sends
        var earliest = now.AddHours(11).AddMinutes(45);
        var latest = now.AddHours(12).AddMinutes(5);
        int sent = 0;
        var coaches = await _firestore.FireDatabase.Collection("coaches").GetSnapshotAsync();
        foreach (var coach in coaches.Documents)
        {
            if (coach.ContainsField("matchRemindersEnabled") && !coach.GetValue<bool>("matchRemindersEnabled")) continue;
            var tokens = await Devices(coach.Id).GetSnapshotAsync();
            if (tokens.Count == 0) continue;

            var matches = await _firestore.MatchesCollection(coach.Id).GetSnapshotAsync();
            foreach (var doc in matches.Documents)
            {
                var match = doc.ConvertTo<FootballMatch>();
                if (match.TeamScore.HasValue || match.OpponentScore.HasValue) continue;
                if (match.MatchDate.ToUniversalTime() < earliest || match.MatchDate.ToUniversalTime() > latest) continue;
                var title = "Match in 12 hours";
                var body = $"Your match against {match.Opponent} starts in about 12 hours.";
                sent += await SendOnceAsync(coach.Id, "match", doc.Id, match.MatchDate.ToUniversalTime(), title, body, "schedule", doc.Id, tokens);
            }

            var sessions = await _firestore.SessionsCollection(coach.Id).GetSnapshotAsync();
            foreach (var doc in sessions.Documents)
            {
                var session = doc.ConvertTo<TrainingSession>();
                if (session.SessionDate.ToUniversalTime() < earliest || session.SessionDate.ToUniversalTime() > latest) continue;
                sent += await SendOnceAsync(coach.Id, "training", doc.Id, session.SessionDate.ToUniversalTime(),
                    "Training in 12 hours", "Your training session starts in about 12 hours.", "schedule", null, tokens);
            }
        }
        return sent;
    }

    private async Task<int> SendOnceAsync(string uid, string type, string id, DateTime starts,
        string title, string body, string kind, string? fixtureId, QuerySnapshot tokens)
    {
        var receipt = _firestore.CoachDoc(uid).Collection("reminderReceipts")
            .Document(Hash($"{type}|{id}|{starts:O}"));
        try
        {
            // used so another overlapping scheduler run can't send a duplicate.
            await receipt.CreateAsync(new Dictionary<string, object>
            {
                ["eventType"] = type,
                ["eventId"] = id,
                ["scheduledFor"] = Timestamp.FromDateTime(starts),
                ["createdAt"] = Timestamp.GetCurrentTimestamp()
            });
        }
        catch (Grpc.Core.RpcException e) when (e.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
        {
            return 0;
        }

        int succeeded = 0;
        try
        {
            foreach (var doc in tokens.Documents)
            {
                var data = new Dictionary<string, string>
                {
                    ["title"] = title,
                    ["body"] = body,
                    ["kind"] = kind,
                    ["notificationId"] = $"{type}-{id}-{starts.Ticks}"
                };
                if (!string.IsNullOrEmpty(fixtureId)) data["fixtureId"] = fixtureId;
                try
                {
                    await _fcm.SendAsync(new Message
                    {
                        Token = doc.GetValue<string>("token"),
                        Data = data,
                        Android = new AndroidConfig { Priority = Priority.High }
                    });
                    succeeded++;
                }
                catch (FirebaseMessagingException e)
                {
                    _logger.LogWarning(e, "FCM send failed for coach {Uid}", uid);
                    if (e.MessagingErrorCode == MessagingErrorCode.Unregistered)
                        await doc.Reference.DeleteAsync();
                }
            }
            // No valid deliveries so allow next scheduled run to retry.
            if (succeeded == 0) await receipt.DeleteAsync();
            return succeeded;
        }
        catch
        {
            if (succeeded == 0) await receipt.DeleteAsync();
            throw;
        }
    }
}
