using FormUp_API.Models;
using FormUp_API.Models.Requests;
using FormUp_API.Services;
using System.Security.Claims;

namespace FormUp_API.Endpoints
{
    public static class SessionEndpoint
    {
        public static WebApplication MapSessionEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/session");

            group.MapGet("/", async (FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var snapshot = await fireService.SessionsCollection(uid).GetSnapshotAsync();
                //getting all session info sorted by date of session as a list
                var sessions = snapshot.Documents.Select(doc => doc.ConvertTo<TrainingSession>()).OrderBy(p => p.SessionDate).Select(m => new { id = m.Id, sessionDate = m.SessionDate, notes = m.Notes }).ToList();

                return Results.Ok(sessions);
            });

            group.MapGet("/{id}", async (string id, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.SessionDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Training Session not found");
                }
                return Results.Ok(snapshot.ConvertTo<TrainingSession>());
            });

            group.MapPost("/", async (CreateTrainingSessionRequest request, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var session = new TrainingSession
                {
                    SessionDate = request.SessionDate.UtcDateTime,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    Attendance = new()

                };

                var documentReference = await fireService.SessionsCollection(uid).AddAsync(session);
                session.Id = documentReference.Id;
                return Results.Created($"/api/session/{session.Id}", session);

            });

            group.MapPatch("/{id}", async (string id, UpdateTrainingSessionRequest request, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.SessionDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Training Session not found");
                }

                var updates = new Dictionary<string, object>();

                if (request.SessionDate.HasValue)
                {
                    updates["sessionDate"] = request.SessionDate.Value.UtcDateTime;
                }

                if (request.Notes != null)
                {
                    updates["notes"] = request.Notes;
                }

                

                //if no updates were sent then skip the firestore update to avoid a 500 error
                if (updates.Count == 0)
                {
                    return Results.Ok(snapshot.ConvertTo<TrainingSession>());
                }

                await documentReference.UpdateAsync(updates);
                var updatedSnapshot = await documentReference.GetSnapshotAsync();
                return Results.Ok(updatedSnapshot.ConvertTo<TrainingSession>());
            });

            group.MapDelete("/{id}", async (string id, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.SessionDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Training Session not found");
                }

                await documentReference.DeleteAsync();
                return Results.NoContent();
            });
            return app;
        }
    }
}
