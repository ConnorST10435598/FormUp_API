using FormUp_API.Models;
using FormUp_API.Models.Requests;
using FormUp_API.Services;
using System.Security.Claims;

namespace FormUp_API.Endpoints
{
    //doing this in its own folder instead of in program.cs to keep program.cs clean
    public static class CoachEndpoints
    {
        //mapping endpoints for minimal api
        public static WebApplication MapCoachEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/coach");

            //get call for /api/coach
            group.MapGet("/", async (CoachService coachService, ClaimsPrincipal user) =>
            {
                var coach = await coachService.GetOrCreateCoachAsync(user);
                return Results.Ok(coach);
            });

            //patch call to update coach
            group.MapPatch("/", async (UpdateCoachRequest request, CoachService coachService, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                var coach = await coachService.GetOrCreateCoachAsync(user);
                string uid = user.GetCoachUid();

                var updates = new Dictionary<string, object>();
                if (request.DisplayName != null)
                {
                    updates["displayName"] = request.DisplayName;
                }

                if (request.ProfilePhotoUrl != null)
                {
                    updates["profilePhotoUrl"] = request.ProfilePhotoUrl;
                }

                if (request.PreferredLanguage != null)
                {
                    updates["preferredLanguage"] = request.PreferredLanguage;
                }

                //if no updates were sent then skip the firestore update to avoid a 500 error
                if (updates.Count == 0)
                {
                    return Results.Ok(coach);
                }

                await fireService.CoachDoc(uid).UpdateAsync(updates);

                var updatedSnapshot = await fireService.CoachDoc(uid).GetSnapshotAsync();
                var updatedCoach = updatedSnapshot.ConvertTo<Coach>();
                return Results.Ok(updatedCoach);
            });
            return app;
        }
    }
}
