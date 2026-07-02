using LibraryManagementSystem.Data;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// SERILOG CONFIGURATION
// Log path: root/logs/yyyyMMdd/logs-yyyyMMdd-HH.txt
// New file is auto created every hour by Serilog rolling interval
// =============================================
var logDate = DateTime.Now.ToString("yyyyMMdd");

var logPath = Path.Combine(
    AppContext.BaseDirectory,
    "logs",
    logDate,
    $"logs-{logDate}-.txt");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: logPath,
        rollingInterval: RollingInterval.Hour,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// =============================================
// READ API VERSION FROM APPSETTINGS
// Used in Swagger doc and available globally
// =============================================
var apiVersion = builder.Configuration["ApiVersion"] ?? "v1";

// =============================================
// DATABASE CONFIGURATION
// =============================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// =============================================
// JWT AUTHENTICATION CONFIGURATION
// =============================================
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

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
        IssuerSigningKey = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtKey))
    };
});

// =============================================
// SWAGGER CONFIGURATION
// Uses ApiVersion from appsettings.json
// =============================================
builder.Services.AddSwaggerGen(options =>
{
    // Use ApiVersion from appsettings for Swagger doc
    options.SwaggerDoc(apiVersion, new OpenApiInfo
    {
        Title = "Library Management System API",
        Version = apiVersion,
        Description = $"API Version: {apiVersion}\n\n" +
                      "A simple CRUD API for managing books using .NET 8, " +
                      "Entity Framework Core, JWT Authentication, and Serilog.\n\n" +
                      "Route format: {version}/api/{{controller}}\n" +
                      $"Example: {apiVersion}/api/Books"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token only (without 'Bearer ' prefix).\n\n" +
                       "Example: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =============================================
// REGISTER APPLICATION SERVICES
// =============================================
builder.Services.AddScoped<TokenService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// =============================================
// BUILD APPLICATION AND CONFIGURE PIPELINE
// =============================================
var app = builder.Build();

// ── Database Initialization ────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();

    db.Database.EnsureCreated();
    SeedData.Initialize(services);
}

// ── Enable Swagger ─────────────────────────────────────────────
// Uses ApiVersion from appsettings for the swagger endpoint
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // Swagger endpoint uses ApiVersion from appsettings
    options.SwaggerEndpoint(
        $"/swagger/{apiVersion}/swagger.json",
        $"Library API {apiVersion}");

    options.RoutePrefix = "swagger";
});

// ── Middleware Pipeline ────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();