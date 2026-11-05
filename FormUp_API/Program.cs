using FormUp_API.Endpoints;
using FormUp_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Read the Firebase project ID from configuration and fail fast if it is missing
var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];
if (string.IsNullOrWhiteSpace(firebaseProjectId))
{
    throw new InvalidOperationException("Firebase:ProjectId is missing from configuration.");
}

// Firebase ID tokens are issued and signed by Google's securetoken service
var firebaseIssuer = $"https://securetoken.google.com/{firebaseProjectId}";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = firebaseIssuer;
        options.MapInboundClaims = false; // keep claim names such as "sub" as they appear in the token
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = firebaseIssuer,
            ValidateAudience = true,
            ValidAudience = firebaseProjectId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

// Every endpoint requires a signed-in coach unless it is explicitly marked as anonymous
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddSingleton<CoachService>();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();


    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Public health check, the only endpoint that does not need a token
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }))
   .AllowAnonymous();

app.MapCoachEndpoints();
app.MapPlayerEndpoints();
app.MapTeamEndpoints();
app.MapMatchEndpoints();
app.MapSessionEndpoints();
app.MapTeamStatsEndpoints();
app.MapAttendanceEndpoints();
app.MapSeasonDataEndpoints();
app.MapFormationEndpoints();

app.Run();