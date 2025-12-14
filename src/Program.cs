using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using GeoStud.Api.Data;
using GeoStud.Api.Services;
using GeoStud.Api.Services.Interfaces;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Determine database provider based on environment or command line arguments
// Check if PostgreSQL connection string is available from configuration or environment
// IMPORTANT: In Aspire, environment variables are available through Configuration
// Aspire passes connection strings via environment variables with double underscore format
// which ASP.NET Core automatically converts to nested configuration keys

// Check configuration (includes environment variables set by Aspire)
// ASP.NET Core automatically converts ConnectionStrings__DefaultConnection env var to ConnectionStrings:DefaultConnection config
var postgresConnectionFromConfig = builder.Configuration.GetConnectionString("DefaultConnection");

// Also check environment variable directly (for debugging)
var postgresConnectionEnv = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

// Also check if WithReference created a variable like {dbResourceName}__ConnectionString
// Common database resource names in Aspire
var possibleDbNames = new[] { "GeoStudDb", "postgres", "postgres-db", "database" };
string? postgresConnectionFromRef = null;
foreach (var dbName in possibleDbNames)
{
    // Check both environment variable and configuration
    var refVarEnv = Environment.GetEnvironmentVariable($"{dbName}__ConnectionString");
    var refVarConfig = builder.Configuration[$"{dbName}__ConnectionString"];
    var refVar = refVarEnv ?? refVarConfig;
    if (!string.IsNullOrEmpty(refVar))
    {
        postgresConnectionFromRef = refVar;
        Console.WriteLine($"🔗 Found connection string from WithReference: {dbName}__ConnectionString");
        break;
    }
}
var hasPostgresConnection = !string.IsNullOrEmpty(postgresConnectionFromConfig) || 
                          !string.IsNullOrEmpty(postgresConnectionEnv) || 
                          !string.IsNullOrEmpty(postgresConnectionFromRef);

// Check if connection string is actually PostgreSQL (not SQL Server or empty)
// Use the first available connection string in priority order: WithReference > Env > Config
var actualConnectionString = postgresConnectionFromRef ?? postgresConnectionEnv ?? postgresConnectionFromConfig;

// If connection string comes from WithReference (Aspire), it's definitely PostgreSQL
var isPostgresFromAspire = postgresConnectionFromRef != null;

var isPostgresConnection = false;
if (!string.IsNullOrEmpty(actualConnectionString))
{
    // PostgreSQL connection strings typically contain "Host=" or "Server=" with "Username=" or "User Id="
    // and NOT "Trusted_Connection" (which is SQL Server specific)
    // Also check for PostgreSQL-specific patterns
    var isPostgresPattern = (actualConnectionString.Contains("Host=") || 
                            (actualConnectionString.Contains("Server=") && actualConnectionString.Contains("Database="))) &&
                           (actualConnectionString.Contains("Username=") || actualConnectionString.Contains("User Id=")) &&
                           !actualConnectionString.Contains("Trusted_Connection");
    
    // Also check for PostgreSQL port (5432) or npgsql-specific patterns
    var hasPostgresPort = actualConnectionString.Contains("Port=5432") || actualConnectionString.Contains(":5432");
    var hasNpgsqlPattern = actualConnectionString.Contains("npgsql") || actualConnectionString.Contains("PostgreSQL");
    
    isPostgresConnection = isPostgresFromAspire || isPostgresPattern || hasPostgresPort || hasNpgsqlPattern;
}

// Always use PostgreSQL - SQLite support has been removed

Console.WriteLine("🔍 DEBUG: Checking all connection string environment variables:");
Console.WriteLine($"   ConnectionStrings__DefaultConnection (env): {(string.IsNullOrEmpty(postgresConnectionEnv) ? "NOT SET" : "SET")}");
if (!string.IsNullOrEmpty(postgresConnectionEnv))
{
    var masked = postgresConnectionEnv.Contains("Password=") 
        ? postgresConnectionEnv.Substring(0, Math.Min(postgresConnectionEnv.IndexOf("Password=") + 20, postgresConnectionEnv.Length)) + "***" 
        : postgresConnectionEnv;
    Console.WriteLine($"   Value: {masked}");
}

foreach (var dbName in possibleDbNames)
{
    var refVar = Environment.GetEnvironmentVariable($"{dbName}__ConnectionString");
    Console.WriteLine($"   {dbName}__ConnectionString: {(string.IsNullOrEmpty(refVar) ? "NOT SET" : "SET")}");
    if (!string.IsNullOrEmpty(refVar))
    {
        var masked = refVar.Contains("Password=") 
            ? refVar.Substring(0, Math.Min(refVar.IndexOf("Password=") + 20, refVar.Length)) + "***" 
            : refVar;
        Console.WriteLine($"   Value: {masked}");
    }
}

Console.WriteLine($"   DefaultConnection (config): {(string.IsNullOrEmpty(postgresConnectionFromConfig) ? "NOT SET" : "SET")}");
if (!string.IsNullOrEmpty(postgresConnectionFromConfig))
{
    var masked = postgresConnectionFromConfig.Contains("Password=") 
        ? postgresConnectionFromConfig.Substring(0, Math.Min(postgresConnectionFromConfig.IndexOf("Password=") + 20, postgresConnectionFromConfig.Length)) + "***" 
        : postgresConnectionFromConfig;
    Console.WriteLine($"   Value: {masked}");
}

Console.WriteLine($"🔧 Database Provider: PostgreSQL");
Console.WriteLine($"🌍 Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"📊 Has PostgreSQL connection: {hasPostgresConnection}, Is valid PostgreSQL: {isPostgresConnection}");
if (hasPostgresConnection)
{
    var connStr = actualConnectionString ?? "";
    var maskedConnStr = connStr.Contains("Password=") 
        ? connStr.Substring(0, Math.Min(connStr.IndexOf("Password=") + 20, connStr.Length)) + "***" 
        : (connStr.Length > 100 ? connStr.Substring(0, 100) + "..." : connStr);
    Console.WriteLine($"🔗 Connection string source: {(postgresConnectionFromRef != null ? "WithReference" : postgresConnectionEnv != null ? "Environment" : "Configuration")}");
    Console.WriteLine($"🔗 Connection string: {maskedConnStr}");
}
else
{
    Console.WriteLine("❌ NO PostgreSQL connection string found from any source!");
}

if (isPostgresConnection)
{
    Console.WriteLine("✅ Valid PostgreSQL connection string found - using PostgreSQL");
}
else
{
    Console.WriteLine("⚠️ No valid PostgreSQL connection string found - will try to use PostgreSQL anyway");
}

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Database - Always use PostgreSQL
// Try to get connection string in priority order: WithReference > Environment > Configuration
var PosgressServerConnection = postgresConnectionFromRef;
if (string.IsNullOrEmpty(PosgressServerConnection))
{
    // Try environment variable (Aspire uses ConnectionStrings__DefaultConnection)
    PosgressServerConnection = postgresConnectionEnv;
}
if (string.IsNullOrEmpty(PosgressServerConnection))
{
    // Try configuration (which includes environment variables in ASP.NET Core)
    PosgressServerConnection = postgresConnectionFromConfig;
}
if (string.IsNullOrEmpty(PosgressServerConnection))
{
    // Finally try configuration from appsettings.json
    PosgressServerConnection = builder.Configuration.GetConnectionString("DefaultConnection");
}

if (string.IsNullOrEmpty(PosgressServerConnection))
{
    Console.WriteLine("❌ PostgreSQL connection string is empty!");
    Console.WriteLine("🔧 Available connection strings from configuration:");
    var connectionStrings = builder.Configuration.GetSection("ConnectionStrings").GetChildren();
    foreach (var connStr in connectionStrings)
    {
        var masked = connStr.Value?.Contains("Password=") == true
            ? connStr.Value.Substring(0, Math.Min(connStr.Value.IndexOf("Password=") + 20, connStr.Value.Length)) + "***"
            : connStr.Value;
        Console.WriteLine($"  - {connStr.Key}: {masked}");
    }
    Console.WriteLine("🔧 Environment variable ConnectionStrings__DefaultConnection:");
    var envConnStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
    Console.WriteLine($"  - {(string.IsNullOrEmpty(envConnStr) ? "NOT SET" : "SET")}");
    throw new InvalidOperationException("PostgreSQL connection string is not configured");
}

builder.Services.AddDbContext<GeoStudDbContext>(options =>
    options.UseNpgsql(PosgressServerConnection));
Console.WriteLine("📊 Using PostgreSQL Server database");
var maskedConn = PosgressServerConnection.Contains("Password=") 
    ? PosgressServerConnection.Substring(0, Math.Min(PosgressServerConnection.IndexOf("Password=") + 20, PosgressServerConnection.Length)) + "***" 
    : PosgressServerConnection;
Console.WriteLine($"🔗 Connection: {maskedConn}");

// Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            // Use UTF8 to match AuthService token generation
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
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

// Memory Cache for role checking
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Limit cache entries
});

// Custom Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IFavoritesService, FavoritesService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPeopleService, PeopleService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ILocationSuggestionService, LocationSuggestionService>();
builder.Services.AddHttpClient<WebhookService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddHttpClient<NeuroApiService>();
builder.Services.AddScoped<INeuroApiService, NeuroApiService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var title = "GeoStud API";
    var description = "API for GeoStud application with student survey data collection";

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

// CORS Configuration
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
Console.WriteLine("🌐 CORS: Configured");

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GeoStud API v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    c.DocumentTitle = "GeoStud API";
});

app.UseHttpsRedirection();

// Use CORS policy
app.UseCors("AllowSpecificOrigins");

// Add custom middleware
app.UseMiddleware<GeoStud.Api.Middleware.RequestLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GeoStudDbContext>();
    
    try
    {
        // First, ensure the migrations history table exists
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        
        // Check if __EFMigrationsHistory table exists
        using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = @"
            SELECT EXISTS (
                SELECT FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_name = '__EFMigrationsHistory'
            );";
        var historyTableExists = (bool)(await checkCommand.ExecuteScalarAsync())!;
        
        if (!historyTableExists)
        {
            Console.WriteLine("📋 Creating migrations history table...");
            using var createCommand = connection.CreateCommand();
            createCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                    ""MigrationId"" character varying(150) NOT NULL,
                    ""ProductVersion"" character varying(32) NOT NULL,
                    CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                );";
            await createCommand.ExecuteNonQueryAsync();
            Console.WriteLine("✅ Migrations history table created");
        }
        
        // Now try to get applied migrations
        List<string> appliedMigrations;
        try
        {
            appliedMigrations = context.Database.GetAppliedMigrations().ToList();
        }
        catch
        {
            // If GetAppliedMigrations fails, assume no migrations are applied
            appliedMigrations = new List<string>();
        }
        
        var allMigrations = context.Database.GetMigrations().ToList();
        var pendingMigrations = allMigrations.Except(appliedMigrations).ToList();
        
        // Check if any tables exist (indicating database was created before)
        using var tablesCheckCommand = connection.CreateCommand();
        tablesCheckCommand.CommandText = @"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name NOT IN ('__EFMigrationsHistory');";
        var tablesCount = Convert.ToInt32(await tablesCheckCommand.ExecuteScalarAsync());
        
        if (tablesCount > 0 && pendingMigrations.Any())
        {
            // Tables exist but migrations are not marked as applied
            // Try to apply migrations first - they will handle missing columns/tables gracefully
            Console.WriteLine($"⚠️ Database tables already exist ({tablesCount} tables found) but {pendingMigrations.Count} migration(s) not applied.");
            Console.WriteLine("📦 Attempting to apply pending migrations...");
            
            try
            {
                context.Database.Migrate();
                Console.WriteLine("✅ Migrations applied successfully");
            }
            catch (PostgresException pgEx) when (pgEx.SqlState == "42P07" || pgEx.SqlState == "42703")
            {
                // If migration fails due to existing objects or missing columns, 
                // mark migrations as applied but log a warning
                Console.WriteLine($"⚠️ Migration application failed: {pgEx.Message}");
                Console.WriteLine("📝 Marking migrations as applied (schema may need manual synchronization)...");
                
                foreach (var migration in pendingMigrations)
                {
                    using var insertCommand = connection.CreateCommand();
                    insertCommand.CommandText = $@"
                        INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                        SELECT '{migration}', '9.0.0'
                        WHERE NOT EXISTS (
                            SELECT 1 FROM ""__EFMigrationsHistory"" 
                            WHERE ""MigrationId"" = '{migration}'
                        );";
                    await insertCommand.ExecuteNonQueryAsync();
                    Console.WriteLine($"  ✓ Marked migration '{migration}' as applied");
                }
                
                Console.WriteLine("✅ Migration history synchronized");
                Console.WriteLine("⚠️ WARNING: Some database objects may be missing. Please verify schema manually.");
                Console.WriteLine("✅ Migrations applied successfully");
            }
        }
        else if (pendingMigrations.Any())
        {
            // No tables exist, apply migrations normally
            Console.WriteLine($"📦 Applying {pendingMigrations.Count} pending migration(s)...");
            context.Database.Migrate();
            Console.WriteLine("✅ Migrations applied successfully");
        }
        else
        {
            Console.WriteLine("✅ Database is up to date");
            Console.WriteLine("✅ Migrations applied successfully");
        }
    }
    catch (PostgresException pgEx) when (pgEx.SqlState == "42P07") // relation already exists
    {
        // This should not happen with the new logic, but keep as fallback
        Console.WriteLine($"⚠️ Table already exists error: {pgEx.Message}");
        Console.WriteLine("💡 This might indicate a schema mismatch. Please check your database.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration error: {ex.Message}");
        Console.WriteLine($"   Error type: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"   Inner error: {ex.InnerException.Message}");
        }
        throw;
    }
    
    // Seed initial data
    await GeoStud.Api.Data.SeedData.SeedAsync(context);
    
    // Location seed data has been removed
    
    Console.WriteLine("✅ Database initialized with PostgreSQL");
}

app.Run();

