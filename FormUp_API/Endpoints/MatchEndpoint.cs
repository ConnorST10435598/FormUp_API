using FormUp_API.Models;
using FormUp_API.Models.Requests;
using FormUp_API.Services;
using System.Security.Claims;

namespace FormUp_API.Endpoints
{
    public static class MatchEndpoint
    {
        public static WebApplication MapMatchEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/match");

            group.MapGet("/", async (FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var snapshot = await fireService.MatchesCollection(uid).GetSnapshotAsync();
                //getting all match info sorted by date of match as a list
                var matches = snapshot.Documents.Select(doc => doc.ConvertTo<FootballMatch>()).OrderBy(p => p.MatchDate).Select(m => new {id=m.Id, opponent = m.Opponent, matchDate = m.MatchDate, venue = m.Venue, teamScore = m.TeamScore, opponentScore = m.OpponentScore}).ToList();

                return Results.Ok(matches);
            });

            group.MapGet("/{id}", async (string id, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.MatchDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Match not found");
                }
                return Results.Ok(snapshot.ConvertTo<FootballMatch>());
            });

            group.MapPost("/", async (CreateMatchRequest request, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var match = new FootballMatch
                {
                    Opponent = request.Opponent,
                    MatchDate = request.MatchDate.UtcDateTime,
                    Venue = request.Venue,
                    TeamScore = request.TeamScore,
                    OpponentScore = request.OpponentScore,
                    CreatedAt = DateTime.UtcNow,
                    Squad = new(),
                    Stats = new(),
                    Attendance = new()

                };

                var documentReference = await fireService.MatchesCollection(uid).AddAsync(match);
                match.Id = documentReference.Id;
                return Results.Created($"/api/match/{match.Id}", match);

            });

            group.MapPatch("/{id}", async (string id, UpdateMatchRequest request, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.MatchDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Match not found");
                }

                var updates = new Dictionary<string, object>();
                if (request.Opponent != null)
                {
                    updates["opponent"] = request.Opponent;
                }

                if (request.MatchDate.HasValue)
                {
                    updates["matchDate"] = request.MatchDate.Value.UtcDateTime;
                }

                if (request.Venue != null)
                {
                    updates["venue"] = request.Venue;
                }

                if (request.TeamScore.HasValue)
                {
                    updates["teamScore"] = request.TeamScore;
                }
                if (request.OpponentScore.HasValue)
                {
                    updates["opponentScore"] = request.OpponentScore;
                }

                //if no updates were sent then skip the firestore update to avoid a 500 error
                if (updates.Count == 0)
                {
                    return Results.Ok(snapshot.ConvertTo<FootballMatch>());
                }

                await documentReference.UpdateAsync(updates);
                var updatedSnapshot = await documentReference.GetSnapshotAsync();
                return Results.Ok(updatedSnapshot.ConvertTo<FootballMatch>());
            });

            group.MapDelete("/{id}", async (string id, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.MatchDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Match not found");
                }

                await documentReference.DeleteAsync();
                return Results.NoContent();
            });
            return app;
        }

    }
}
