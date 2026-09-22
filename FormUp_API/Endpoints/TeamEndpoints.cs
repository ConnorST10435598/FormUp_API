using FormUp_API.Models.Requests;
using FormUp_API.Services;
using System.Security.Claims;

namespace FormUp_API.Endpoints
{
    public static class TeamEndpoints
    {
        public static WebApplication MapTeamEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/team");

            group.MapGet("/", async (CoachService coachService, ClaimsPrincipal user) =>
            {
                var coach = await coachService.GetOrCreateCoachAsync(user);
                return Results.Ok(new
                {
                    teamName = coach.TeamName,
                    ageGroup = coach.AgeGroup,
                    hasTeam = !string.IsNullOrWhiteSpace(coach.TeamName)
                });
            });

            group.MapPut("/", async (SaveTeamRequest request, CoachService coachService, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                await coachService.GetOrCreateCoachAsync(user);
                string uid = user.GetCoachUid();

                var updates = new Dictionary<string, object>
                {
                    ["teamName"] = request.TeamName,
                    ["ageGroup"] = request.AgeGroup
                };

                await fireService.CoachDoc(uid).UpdateAsync(updates);
                return Results.Ok(new { teamName = request.TeamName, ageGroup = request.AgeGroup, hasTeam = !string.IsNullOrWhiteSpace(request.TeamName) });
            });
            return app;
        }
    }
}
