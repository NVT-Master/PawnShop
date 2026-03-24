using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PawnShopV2.Application.Services.Interfaces;
using PawnShopV2.Infrastructure.Data;
using PawnShopV2.Infrastructure.Seeders;
using PawnShopV2.Infrastructure.Services;
using PawnShopV2.Infrastructure.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=NVT;Database=pawnshopv2;Trusted_Connection=True;TrustServerCertificate=True;";
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";

if (databaseProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
{
    var sqliteDataSource = connectionString
        .Split(';', StringSplitOptions.RemoveEmptyEntries)
        .FirstOrDefault(part => part.TrimStart().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
        ?.Split('=', 2)[1]
        ?.Trim();

    if (!string.IsNullOrWhiteSpace(sqliteDataSource))
    {
        var sqliteDirectory = Path.GetDirectoryName(sqliteDataSource);
        if (!string.IsNullOrWhiteSpace(sqliteDirectory))
        {
            Directory.CreateDirectory(sqliteDirectory);
        }
    }

    builder.Services.AddDbContext<PawnShopDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    builder.Services.AddDbContext<PawnShopDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJWTAuthentication2024PawnShopV2!";
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
        ValidIssuer = jwtSettings["Issuer"] ?? "PawnShopAPI",
        ValidAudience = jwtSettings["Audience"] ?? "PawnShopClient",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Register Services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IAssetCategoryService, AssetCategoryService>();
builder.Services.AddScoped<IContractService, ContractService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Database migration and seeding with retry for Docker
var maxRetries = 10;
var delay = TimeSpan.FromSeconds(5);
for (int i = 0; i < maxRetries; i++)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PawnShopDbContext>();
        await context.Database.EnsureCreatedAsync();
        await DataSeeder.SeedAsync(context, app.Configuration);
        Console.WriteLine("Database initialized successfully!");
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database connection attempt {i + 1}/{maxRetries} failed: {ex.Message}");
        if (i == maxRetries - 1) throw;
        await Task.Delay(delay);
    }
}

app.Run();
