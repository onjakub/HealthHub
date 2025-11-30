using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthHub.Application.Commands;
using HealthHub.Application.DTOs;
using HealthHub.Application.Handlers;
using HealthHub.Application.Queries;
using HealthHub.Application.Services;
using HealthHub.Domain.Interfaces;
using HealthHub.Infrastructure.Data;
using HealthHub.Infrastructure.Repositories;
using HealthHub.Presentation.GraphQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI (optional)
builder.Services.AddOpenApi();

// Configuration
var connectionString = builder.Configuration.GetConnectionString("Default")
                        ?? Environment.GetEnvironmentVariable("DB_CONNECTION")
                        ?? "Host=localhost;Database=healthhub;Username=health;Password=healthpwd";

// EF Core - PostgreSQL
builder.Services.AddDbContext<HealthHubDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register repositories
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDiagnosticResultRepository, DiagnosticResultRepository>();

// Register services
builder.Services.AddScoped<ILoggingService, LoggingService>();

// Register command handlers
builder.Services.AddScoped<ICommandHandler<CreatePatientCommand, PatientDto>, CreatePatientCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdatePatientCommand, PatientDto>, UpdatePatientCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AddDiagnosticResultCommand, DiagnosticResultDto>, AddDiagnosticResultCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateDiagnosticResultCommand, DiagnosticResultDto>, UpdateDiagnosticResultCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeletePatientCommand, bool>, DeletePatientCommandHandler>();

// Register query handlers
builder.Services.AddScoped<IQueryHandler<GetPatientsQuery, IEnumerable<PatientDto>>, GetPatientsQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetPatientByIdQuery, PatientDetailDto?>, GetPatientByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetPatientDiagnosticResultsQuery, IEnumerable<DiagnosticResultDto>>, GetPatientDiagnosticResultsQueryHandler>();

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key") ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? "dev-secret-change-me-please-very-long";
var jwtIssuer = jwtSection.GetValue<string>("Issuer") ?? "HealthHub";
var jwtAudience = jwtSection.GetValue<string>("Audience") ?? "HealthHubAudience";

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// CORS services (required when using app.UseCors)
builder.Services.AddCors();

// GraphQL
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddFiltering()
    .AddSorting()
    .AddProjections();

// Static files for React frontend
builder.Services.AddRouting();

// Configure static files to serve React build
builder.Services.Configure<StaticFileOptions>(options =>
{
    options.ServeUnknownFileTypes = false;
    // Cache configuration will be applied after app is built
});

// HTTPS redirection disabled for development - application runs on HTTP only
// builder.Services.AddHttpsRedirection(options =>
// {
//     options.HttpsPort = 7017;
// });

var app = builder.Build();

// Apply DB creation/migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HealthHubDbContext>();
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();
app.UseDefaultFiles();

// Configure static files with caching
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        // Cache static files for 1 hour in production
        if (!context.Context.Request.Path.StartsWithSegments("/api") &&
            !context.Context.Request.Path.StartsWithSegments("/graphql"))
        {
            var headers = context.Context.Response.GetTypedHeaders();
            headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
            {
                Public = true,
                MaxAge = app.Environment.IsDevelopment() ? TimeSpan.Zero : TimeSpan.FromHours(1)
            };
        }
    }
});

// Enable CORS for React frontend
app.UseCors(policy => policy
    .WithOrigins(
        "http://localhost:3000",  // React dev server
        "http://localhost:3001",  // Alternative React dev port
        "https://localhost:3000", // HTTPS dev server
        "https://localhost:3001"  // Alternative HTTPS dev port
    )
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

// SPA routing middleware - handle client-side routing
app.Use(async (context, next) =>
{
    await next();

    // If it's an API or GraphQL request, don't handle as SPA
    if (context.Request.Path.StartsWithSegments("/api") ||
        context.Request.Path.StartsWithSegments("/graphql") ||
        context.Request.Path.StartsWithSegments("/health"))
    {
        return;
    }

    // For non-API routes and if the file doesn't exist, serve index.html
    if (context.Response.StatusCode == 404 &&
        !System.IO.Path.HasExtension(context.Request.Path.Value) &&
        !context.Request.Path.Value?.StartsWith("/api") == true &&
        !context.Request.Path.Value?.StartsWith("/graphql") == true)
    {
        context.Request.Path = "/index.html";
        context.Response.StatusCode = 200;
        await next();
    }
});

app.UseAuthentication();
app.UseAuthorization();

// No MVC controllers are used; GraphQL and minimal APIs are mapped below

// Simple token issuing endpoint for demo purposes
app.MapPost("/auth/token", async (HttpContext context) =>
    {
        // Parse JSON body
        try
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            var json = System.Text.Json.JsonDocument.Parse(body);
            var username = json.RootElement.GetProperty("username").GetString();
            var password = json.RootElement.GetProperty("password").GetString();
            
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("{\"error\":\"Invalid credentials\"}");
                return;
            }

            // Simple demo authentication - accept any credentials for testing
            var claims = new List<Claim> { new(ClaimTypes.Name, username) };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            
            // Manual JSON serialization to avoid .NET 10.0 PipeWriter issue
            var responseJson = $"{{\"token\":\"{jwt}\"}}";
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(responseJson);
        }
        catch (Exception)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("{\"error\":\"Invalid request format\"}");
        }
    })
    .AllowAnonymous();

app.MapGraphQL("/graphql");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

// SPA fallback: any non-API, non-static route serves index.html from wwwroot
// This ensures React Router works correctly
app.MapFallbackToFile("/index.html");

app.Run();