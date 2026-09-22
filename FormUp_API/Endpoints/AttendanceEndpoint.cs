using FormUp_API.Models;
using FormUp_API.Models.Requests;
using FormUp_API.Services;
using Google.Cloud.Firestore;
using System.Security.Claims;

namespace FormUp_API.Endpoints
{
    public static class AttendanceEndpoint
    {

        public static WebApplication MapAttendanceEndpoints(this WebApplication app)
        {
            //attendance for training sessions
            var sessionGroup = app.MapGroup("/api/session");

            sessionGroup.MapGet("/{id}/attendance", async (string id, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.SessionDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Session not found");
                }
                return await ReadAttendanceAsync(uid, snapshot, fireService);
            });

            sessionGroup.MapPut("{id}/attendance", async (string id, List<AttendanceEntryRequest> request, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.SessionDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Session not found");
                }
                return await SaveAttendanceAsync(documentReference, request);
            });


            //attendance for matches
            var matchGroup = app.MapGroup("/api/match");
            matchGroup.MapGet("/{id}/attendance", async (string id, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.MatchDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Match not found");
                }
                return await ReadAttendanceAsync(uid, snapshot, fireService);
            });

            matchGroup.MapPut("/{id}/attendance", async (string id, List<AttendanceEntryRequest> request, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.MatchDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Match not found");
                }
                return await SaveAttendanceAsync(documentReference, request);
            });
            return app;
        }



        private static async Task<IResult> SaveAttendanceAsync(DocumentReference documentReference, List<AttendanceEntryRequest> request)
        {
            var attendanceDictionary = new Dictionary<string, AttendanceEntry>();

            foreach (var item in request)
            {
                if (string.IsNullOrWhiteSpace(item.PlayerId))
                {
                    continue;
                }
                attendanceDictionary[item.PlayerId] = new AttendanceEntry
                {
                    Status = item.Status,
                    Reason = item.Reason
                };
            }
            await documentReference.UpdateAsync(new Dictionary<string, object>
            {
                ["attendance"] = attendanceDictionary
            });
            return Results.Ok(request);
        }

        private static async Task<IResult> ReadAttendanceAsync(string uid, DocumentSnapshot snapshot, FirestoreService fireService)
        {
            // Declaring the variable where the dictionary will be stored
            Dictionary<string, AttendanceEntry> attendanceMap;

            // Try to retrieve the attendance data from the snapshot
            bool hasAttendanceData = snapshot.TryGetValue("attendance", out attendanceMap);

            // Checking if the search failed or if the fetched dictionary is null
            if (hasAttendanceData == false || attendanceMap == null)
            {
                // Create a new empty dictionary 
                attendanceMap = new Dictionary<string, AttendanceEntry>();
            }

            // Fetching all current squad players sorted by name
            var playersSnapshot = await fireService.PlayersCollection(uid).GetSnapshotAsync();
            var players = playersSnapshot.Documents.Select(doc => doc.ConvertTo<Player>()).OrderBy(p => p.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase).ToList();

            // Sendign player details with attendance status or default to NotRecorded
            var result = players.Select(player =>
            {
                bool hasEntry = attendanceMap.TryGetValue(player.Id, out var entry);
                return new
                {
                    playerId = player.Id,
                    name = player.Name,
                    position = player.Position,
                    status = hasEntry ? entry.Status : "NotRecorded",
                    reason = hasEntry ? entry.Reason : null
                };
            }).ToList();

            return Results.Ok(result);
        }
    }
}
