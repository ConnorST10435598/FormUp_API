using System.Security.Claims;

namespace FormUp_API.Services
{
    public static class CoachExtension
    {
        //extracts the coaches firebase uid from the validated jwt claims and throws an exception if missing
        public static string GetCoachUid(this ClaimsPrincipal user) 
        {
            var uid = user.FindFirst("sub")?.Value ?? user.FindFirst("user_id")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(uid)) 
            {
                throw new UnauthorizedAccessException("Coach UID is missing from the token");
            }
            return uid;
        }
    }
}
