using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthHub.Application.Commands;
using HealthHub.Application.DTOs;
using HealthHub.Application.Handlers;
using HealthHub.Application.Queries;
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

// GraphQL
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddFiltering()
    .AddSorting()
    .AddProjections();

// Static files for simple frontend
builder.Services.AddRouting();

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

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Simple token issuing endpoint for demo purposes
app.MapPost("/auth/token", (string username, string password) =>
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return Results.BadRequest(new { error = "Invalid credentials" });
        }

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
        return Results.Ok(new { token = jwt });
    })
    .AllowAnonymous();

app.MapGraphQL("/graphql");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.Run();