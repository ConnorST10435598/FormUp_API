using FormUp_API.Models;
using FormUp_API.Models.Requests;
using FormUp_API.Services;
using System.Security.Claims;

namespace FormUp_API.Endpoints
{
    public static class SquadStatsEndpoints
    {
        public static WebApplication MapTeamStatsEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/match");

            group.MapGet("/{id}/squad", async (string id, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.MatchDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Match not found");
                }

                var match = snapshot.ConvertTo<FootballMatch>();

                var playersSnapshot = await fireService.PlayersCollection(uid).GetSnapshotAsync();
                //fetching all players in squad as a list and sorted by name
                var players = playersSnapshot.Documents.Select(doc => doc.ConvertTo<Player>()).OrderBy(p => p.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase).ToList();

                var squadList = players.Select(p =>
                {
                    var entry = match.Squad.GetValueOrDefault(p.Id); //gets player object in dictionary if it exists, otherwise it returns a null
                    return new
                    {
                        playerId = p.Id,
                        name = p.Name,
                        position = p.Position, // Registered player position
                        status = entry?.Status ?? "Available",
                        selection = entry?.Selection ?? "NotSelected",

                        // Match-specific tactical arrangement
                        assignedPosition = entry?.AssignedPosition ?? string.Empty,
                        pitchX = entry?.PitchX,
                        pitchY = entry?.PitchY
                    };
                }).ToList();

                return Results.Ok(squadList);

            });

            group.MapPut("/{id}/squad", async (string id, List<SquadEntryRequest> request, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.MatchDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Match not found");
                }

                var squadDictionary = new Dictionary<string, SquadEntry>();
                foreach (var iten in request)
                {
                    if (string.IsNullOrWhiteSpace(iten.PlayerId)) { continue; }
                    squadDictionary[iten.PlayerId] = new SquadEntry
                    {
                        Status = iten.Status,
                        Selection = iten.Selection,
                        AssignedPosition = iten.AssignedPosition,
                        PitchX = iten.PitchX,
                        PitchY = iten.PitchY
                    };
                }
                await documentReference.UpdateAsync(new Dictionary<string, object>
                {
                    ["squad"] = squadDictionary
                });
                return Results.Ok(request);


            });

            group.MapPut("/{id}/stats", async (string id, List<StatsEntryRequest> request, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.MatchDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Match not found");
                }

                var statsDictionary = new Dictionary<string, MatchPlayerStats>();
                foreach (var item in request)
                {
                    if (string.IsNullOrWhiteSpace(item.PlayerId)) { continue; }
                    statsDictionary[item.PlayerId] = new MatchPlayerStats
                    {
                        Goals = item.Goals,
                        Assists = item.Assists,
                        Cleansheet = item.Cleansheet,
                        MinutesPlayed = item.MinutesPlayed,
                        Rating = item.Rating
                    };
                }
                await documentReference.UpdateAsync(new Dictionary<string, object>
                {
                    ["matchStats"] = statsDictionary
                });
                return Results.Ok(request);


            });

            group.MapPut("/{id}/formation", async (
                string id,
                SaveMatchFormationRequest request,
                FirestoreService fireService,
                ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();

                var documentReference = fireService.MatchDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Match not found");
                }

                if (string.IsNullOrWhiteSpace(request.FormationName) ||
                    string.IsNullOrWhiteSpace(request.FormationCode))
                {
                    return Results.BadRequest(new
                    {
                        message = "Formation name and formation code are required."
                    });
                }

                await documentReference.UpdateAsync(new Dictionary<string, object>
                {
                    ["formationName"] = request.FormationName.Trim(),
                    ["formationCode"] = request.FormationCode.Trim()
                });

                return Results.Ok(new
                {
                    formationName = request.FormationName.Trim(),
                    formationCode = request.FormationCode.Trim()
                });
            });

            return app;

        }
    }
}