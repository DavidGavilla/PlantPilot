using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PlantCare.Api.Data;
using PlantCare.Api.Services.Auth;
using PlantCare.Api.Services.Farms;
using PlantCare.Api.Services.Interfaces.Auth;
using PlantCare.Api.Services.Interfaces.Farms;
using PlantCare.Api.Services.Interfaces.Plants;
using PlantCare.Api.Services.Interfaces.Workspaces;
using PlantCare.Api.Services.Plants;
using PlantCare.Api.Services.Workspaces;

var builder = WebApplication.CreateBuilder(args);

const string DevCorsPolicy = "DevCors";

// Add services
builder.Services.AddControllers();

builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.UseNetTopologySuite()
    )
);

// Narrow, named CORS policy for the local Vite dev frontend only — never AllowAnyOrigin.
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
    {
        // AllowCredentials lets the browser send/receive the httpOnly session cookie on cross-origin
        // requests too (e.g. if the Vite proxy is bypassed). It cannot be combined with
        // AllowAnyOrigin — WithOrigins above already restricts to explicit origins, so this is safe.
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<IWorkspaceAccessService, WorkspaceAccessService>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IFarmService, FarmService>();
builder.Services.AddScoped<IPlantsService, PlantsService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSection["Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        // Session now travels as an httpOnly cookie, never a header the frontend could set (and
        // never localStorage) — read the token from the cookie instead of the Authorization header.
        // No Authorization-header fallback: this app only ever talks to itself through the
        // same-origin Vite proxy, so cookie-only transport is simpler and keeps a single source of
        // truth for how a session is carried.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("plantpilot_session", out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(DevCorsPolicy);
}

app.UseHttpsRedirection();

// Order matters: authentication (who are you) must run before authorization (are you allowed).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Required for WebApplicationFactory<Program> in the test project to see the top-level-statements
// -generated Program class — a no-op for the running app.
public partial class Program { }

