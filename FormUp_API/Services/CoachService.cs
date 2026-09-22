using FormUp_API.Models;
using System.Security.Claims;

namespace FormUp_API.Services
{
    public class CoachService
    {
        private readonly FirestoreService _firestoreService;
        public CoachService(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
        }

        //fetches the existing coach doc for the authenticated user or creates and saves a default coach doc using Jwt token claims
        public async Task<Coach>GetOrCreateCoachAsync(ClaimsPrincipal user) 
        {
            string uid = user.GetCoachUid();
            var documentReference = _firestoreService.CoachDoc(uid);
            var snapshot = await documentReference.GetSnapshotAsync();

            if (snapshot.Exists) 
            {
                return snapshot.ConvertTo<Coach>();
            }

            //extracting the email claim
            string email = user.FindFirst("email")?.Value ?? user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

            //extracting the name claim
            string? nameClaim = user.FindFirst("name")?.Value ?? user.FindFirst(ClaimTypes.Name)?.Value;

            //getting the display name either from nameclaim or from the name before @ sign in email
            string displayName = !string.IsNullOrWhiteSpace(nameClaim) ? nameClaim : (!string.IsNullOrEmpty(email) && email.Contains('@') ? email.Split('@')[0] : "Coach");

            string? profilePhoto = user.FindFirst("picture")?.Value;

            var newCoach = new Coach
            {
                Uid = uid,
                Email = email,
                DisplayName = displayName,
                ProfilePhotoUrl = profilePhoto,
                PreferredLanguage = "en",
                TeamName = null,
                AgeGroup = null,
                CreatedAt = DateTime.UtcNow
            };
            await documentReference.SetAsync(newCoach);
            return newCoach;
        }
    }
}
