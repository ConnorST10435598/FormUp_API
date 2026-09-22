using FormUp_API.Models;
using FormUp_API.Models.Requests;
using FormUp_API.Services;
using System.Security.Claims;

namespace FormUp_API.Endpoints
{
    public static class PlayerEndpoint
    {
        public static WebApplication MapPlayerEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/players");

            group.MapGet("/", async (FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var snapshot = await fireService.PlayersCollection(uid).GetSnapshotAsync();
                //apply docs to player and doing case insensitive sorting by name
                var players = snapshot.Documents.Select(doc => doc.ConvertTo<Player>()).OrderBy(p => p.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase).ToList();

                return Results.Ok(players);
            });

            group.MapGet("/{id}", async (string id, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.PlayerDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Player not found");
                }
                return Results.Ok(snapshot.ConvertTo<Player>());
            });

            group.MapPost("/", async (CreatePlayerRequest request, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var player = new Player
                {
                    Name = request.Name,
                    Position = request.Position,
                    DateOfBirth = request.DateOfBirth?.ToString("yyyy-MM-dd"),
                    CreatedAt = DateTime.UtcNow
                };

                var documentReference = await fireService.PlayersCollection(uid).AddAsync(player);
                player.Id = documentReference.Id;
                return Results.Created($"/api/players/{player.Id}", player);

            });

            group.MapPatch("/{id}", async (string id, UpdatePlayerRequest request, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.PlayerDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Player not found");
                }

                var updates = new Dictionary<string, object>();
                if (request.Name != null)
                {
                    updates["name"] = request.Name;
                }

                if (request.Position != null)
                {
                    updates["position"] = request.Position;
                }

                if (request.DateOfBirth.HasValue)
                {
                    updates["dateOfBirth"] = request.DateOfBirth.Value.ToString("yyyy-MM-dd");
                }

                //if no updates were sent then skip the firestore update to avoid a 500 error
                if (updates.Count == 0)
                {
                    return Results.Ok(snapshot.ConvertTo<Player>());
                }

                await documentReference.UpdateAsync(updates);
                var updatedSnapshot = await documentReference.GetSnapshotAsync();
                return Results.Ok(updatedSnapshot.ConvertTo<Player>());
            });

            group.MapDelete("/{id}", async (string id, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.PlayerDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Player not found");
                }

                await documentReference.DeleteAsync();
                return Results.NoContent();
            });
            return app;
        }
    }
}
