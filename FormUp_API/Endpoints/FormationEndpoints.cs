
using FormUp_API.Models;
using FormUp_API.Models.Requests;
using FormUp_API.Services;
using System.Security.Claims;

namespace FormUp_API.Endpoints
{
    public static class FormationEndpoints
    {
        public static WebApplication MapFormationEndpoints(
            this WebApplication app)
        {
            var group = app.MapGroup("/api/formations");

            // GET: Retrieve all saved formations for the coach.
            group.MapGet("/", async (
                FirestoreService fireService,
                ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();

                var snapshot = await fireService
                    .FormationsCollection(uid)
                    .GetSnapshotAsync();

                var formations = snapshot.Documents
                    .Select(doc => doc.ConvertTo<SavedFormation>())
                    .OrderBy(f => f.Name)
                    .ToList();

                return Results.Ok(formations);
            });

            // GET: Retrieve one saved formation.
            group.MapGet("/{id}", async (
                string id,
                FirestoreService fireService,
                ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();

                var document = fireService.FormationDoc(uid, id);
                var snapshot = await document.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(
                        statusCode: 404,
                        title: "Formation not found");
                }

                return Results.Ok(snapshot.ConvertTo<SavedFormation>());
            });

            // POST: Create a new saved formation.
            group.MapPost("/", async (
                SaveFormationRequest request,
                FirestoreService fireService,
                ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();

                if (!IsValidFormation(request))
                {
                    return Results.BadRequest(new
                    {
                        message = "A formation needs a name, code and exactly 11 valid position slots."
                    });
                }

                var now = DateTime.UtcNow;

                var formation = new SavedFormation
                {
                    Name = request.Name.Trim(),
                    Code = request.Code.Trim(),
                    Positions = MapPositions(request.Positions),
                    CreatedAt = now,
                    UpdatedAt = now
                };

                var document = await fireService
                    .FormationsCollection(uid)
                    .AddAsync(formation);

                formation.Id = document.Id;

                return Results.Created(
                    $"/api/formations/{document.Id}",
                    formation);
            });

            // PUT: Update an existing saved formation.
            group.MapPut("/{id}", async (
                string id,
                SaveFormationRequest request,
                FirestoreService fireService,
                ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();

                if (!IsValidFormation(request))
                {
                    return Results.BadRequest(new
                    {
                        message = "A formation needs a name, code and exactly 11 valid position slots."
                    });
                }

                var document = fireService.FormationDoc(uid, id);
                var snapshot = await document.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(
                        statusCode: 404,
                        title: "Formation not found");
                }

                await document.UpdateAsync(
                    new Dictionary<string, object>
                    {
                        ["name"] = request.Name.Trim(),
                        ["code"] = request.Code.Trim(),
                        ["positions"] = MapPositions(request.Positions),
                        ["updatedAt"] = DateTime.UtcNow
                    });

                var updatedSnapshot = await document.GetSnapshotAsync();

                return Results.Ok(
                    updatedSnapshot.ConvertTo<SavedFormation>());
            });

            // DELETE: Delete a saved formation.
            group.MapDelete("/{id}", async (
                string id,
                FirestoreService fireService,
                ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();

                var document = fireService.FormationDoc(uid, id);
                var snapshot = await document.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(
                        statusCode: 404,
                        title: "Formation not found");
                }

                await document.DeleteAsync();

                return Results.NoContent();
            });

            return app;
        }

        private static bool IsValidFormation(
            SaveFormationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Code) ||
                request.Positions == null ||
                request.Positions.Count != 11)
            {
                return false;
            }

            if (request.Positions.Any(p =>
                string.IsNullOrWhiteSpace(p.SlotId) ||
                string.IsNullOrWhiteSpace(p.Label) ||
                double.IsNaN(p.X) ||
                double.IsNaN(p.Y) ||
                double.IsInfinity(p.X) ||
                double.IsInfinity(p.Y) ||
                p.X < 0 || p.X > 100 ||
                p.Y < 0 || p.Y > 100))
            {
                return false;
            }

            return request.Positions
                .Select(p => p.SlotId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() == 11;
        }

        private static List<FormationSlot> MapPositions(
            List<FormationSlotRequest> positions)
        {
            return positions.Select(p => new FormationSlot
            {
                SlotId = p.SlotId.Trim(),
                Label = p.Label.Trim(),
                X = p.X,
                Y = p.Y
            }).ToList();
        }
    }
}
