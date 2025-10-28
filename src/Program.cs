using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using GeoStud.Api.Data;
using GeoStud.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Determine database provider based on environment or command line arguments
var useSqlite = builder.Environment.EnvironmentName == "Local" || 
                builder.Environment.EnvironmentName == "Development" ||
                args.Contains("--sqlite") || 
                args.Contains("--local") ||
                Environment.GetEnvironmentVariable("FORCE_SQLITE") == "true";

// Override environment if SQLite arguments are provided
if (args.Contains("--sqlite") || args.Contains("--local") || Environment.GetEnvironmentVariable("FORCE_SQLITE") == "true")
{
    builder.Environment.EnvironmentName = "Local";
}

Console.WriteLine($"🔧 Database Provider: {(useSqlite ? "SQLite" : "SQL Server")}");
Console.WriteLine($"🌍 Environment: {builder.Environment.EnvironmentName}");

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Database - Configure based on mode
if (useSqlite)
{
    var sqliteConnection = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=GeoStudLocal.db";
    builder.Services.AddDbContext<GeoStudDbContext>(options =>
        options.UseSqlite(sqliteConnection));
    Console.WriteLine("📊 Using SQLite database");
    Console.WriteLine($"🔗 Connection: {sqliteConnection}");
}
else
{
    var sqlServerConnection = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(sqlServerConnection))
    {
        Console.WriteLine("❌ SQL Server connection string is empty!");
        Console.WriteLine("🔧 Available connection strings:");
        var connectionStrings = builder.Configuration.GetSection("ConnectionStrings").GetChildren();
        foreach (var connStr in connectionStrings)
        {
            Console.WriteLine($"  - {connStr.Key}: {connStr.Value}");
        }
        throw new InvalidOperationException("SQL Server connection string is not configured");
    }
    
    builder.Services.AddDbContext<GeoStudDbContext>(options =>
        options.UseSqlServer(sqlServerConnection));
    Console.WriteLine("📊 Using SQL Server database");
    Console.WriteLine($"🔗 Connection: {sqlServerConnection}");
}

// Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"))),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Custom Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var title = useSqlite ? "GeoStud API (Local Development)" : "GeoStud API";
    var description = useSqlite 
        ? "API for GeoStud application with student survey data collection - Local Development Mode"
        : "API for GeoStud application with student survey data collection";

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = title,
        Version = "v1",
        Description = description,
        Contact = new OpenApiContact
        {
            Name = "GeoStud Team",
            Email = "support@geostud.com"
        }
    });

    // Add JWT Authentication to Swagger
    //c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    //{
    //    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
    //    Name = "Authorization",
    //    In = ParameterLocation.Header,
    //    Type = SecuritySchemeType.ApiKey,
    //    Scheme = "Bearer"
    //});

    //c.AddSecurityRequirement(new OpenApiSecurityRequirement
    //{
    //    {
    //        new OpenApiSecurityScheme
    //        {
    //            Reference = new OpenApiReference
    //            {
    //                Type = ReferenceType.SecurityScheme,
    //                Id = "Bearer"
    //            }
    //        },
    //        Array.Empty<string>()
    //    }
    //});

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// CORS - Configure based on mode
if (useSqlite)
{
    // More permissive CORS for local development
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("LocalDevelopment", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
    Console.WriteLine("🌐 CORS: Local development mode (permissive)");
}
else
{
    // Production CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", policy =>
        {
            policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "*" })
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });
    Console.WriteLine("🌐 CORS: Production mode (restricted)");
}

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    var swaggerTitle = useSqlite ? "GeoStud API v1 (Local)" : "GeoStud API v1";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", swaggerTitle);
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    c.DocumentTitle = useSqlite ? "GeoStud API - Local Development" : "GeoStud API";
});

app.UseHttpsRedirection();

// Use appropriate CORS policy
if (useSqlite)
{
    app.UseCors("LocalDevelopment");
}
else
{
    app.UseCors("AllowSpecificOrigins");
}

// Add custom middleware
app.UseMiddleware<GeoStud.Api.Middleware.RequestLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GeoStudDbContext>();
    
    // Apply migrations
    context.Database.Migrate();
    
    // Seed initial data
    await GeoStud.Api.Data.SeedData.SeedAsync(context);
    
    // Seed Rostov locations
    await GeoStud.Api.Data.RostovSeedData.SeedRostovLocationsAsync(context);
    
    if (useSqlite)
    {
        Console.WriteLine("✅ Local database initialized with SQLite");
        Console.WriteLine("📊 Database file: GeoStudLocal.db");
    }
    else
    {
        Console.WriteLine("✅ Database initialized with SQL Server");
    }
}

app.Run();
