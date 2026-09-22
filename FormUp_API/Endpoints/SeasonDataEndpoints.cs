using FormUp_API.Models;
using FormUp_API.Services;
using System.Security.Claims;

namespace FormUp_API.Endpoints
{
    public static class SeasonDataEndpoints
    {
        public static WebApplication MapSeasonDataEndpoints(this WebApplication app)
        {
            app.MapGet("/api/season-data", async (FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();

                var playersTask = fireService.PlayersCollection(uid).GetSnapshotAsync();
                var matchesTask = fireService.MatchesCollection(uid).GetSnapshotAsync();
                var sessionsTask = fireService.SessionsCollection(uid).GetSnapshotAsync();


                var playersSnapshot = await playersTask;
                var matchesSnapshot = await matchesTask;
                var sessionsSnapshot = await sessionsTask;

                var players = playersSnapshot.Documents.Select(doc => doc.ConvertTo<Player>()).OrderBy(p => p.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase).ToList();
                var matches = matchesSnapshot.Documents.Select(doc => doc.ConvertTo<FootballMatch>()).OrderBy(p => p.MatchDate).ToList();
                var sessions = sessionsSnapshot.Documents.Select(doc => doc.ConvertTo<TrainingSession>()).OrderBy(p => p.SessionDate).ToList();
                return Results.Ok(new { players, matches, sessions });


            });
            return app;
        }
    }
}
