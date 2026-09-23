
var builder = WebApplication.CreateBuilder(args);

MapsterConfig.RegisterMappings();
#region Configuration
AddDotEnvFile(builder.Configuration, ResolveDotEnvPath(builder.Environment));
builder.Configuration.AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

static void AddDotEnvFile(IConfigurationBuilder configurationBuilder, string filePath)
{
    if (!File.Exists(filePath))
    {
        return;
    }

    var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    foreach (var rawLine in File.ReadAllLines(filePath))
    {
        var line = rawLine.Trim();

        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
        {
            continue;
        }

        var separatorIndex = line.IndexOf('=');
        if (separatorIndex <= 0)
        {
            continue;
        }

        var key = line[..separatorIndex].Trim();
        var value = line[(separatorIndex + 1)..].Trim();

        if (string.IsNullOrWhiteSpace(key))
        {
            continue;
        }

        values[key.Replace("__", ":")] = value;
    }

    configurationBuilder.AddInMemoryCollection(values!);
}

static string ResolveDotEnvPath(IHostEnvironment environment)
{
    var configuredPath = Environment.GetEnvironmentVariable("TURNERO_DOTENV_PATH");
    if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
    {
        return configuredPath;
    }

    var candidatePaths = new[]
    {
        Path.Combine(environment.ContentRootPath, ".env"),
        Path.Combine(AppContext.BaseDirectory, ".env"),
        Path.Combine(Directory.GetCurrentDirectory(), ".env")
    };

    foreach (var candidatePath in candidatePaths)
    {
        if (File.Exists(candidatePath))
        {
            return candidatePath;
        }
    }

    return candidatePaths[0];
}
#endregion

#region validations
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Logging centralizado en AppLogs.log para todas las capas
builder.Logging.ClearProviders();
builder.Logging.AddProvider(new FileLoggerProvider("AppLogs.log"));

// Diagnóstico adicional (solo en desarrollo)
if (builder.Environment.IsDevelopment())
{
    // Validación manual para servicios críticos
    builder.Services.AddHostedService<DependencyDiagnosticsHostedService>();
}

#endregion

#region Application & Infrastructure Services
builder.Services.AddSingleton<LoggerService>();

// Clean Architecture: registro de dependencias por capa.
// AddInfrastructure registra DbContext, Identity (EF stores), repositorios y servicios de datos.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
#endregion

#region Authentication & Authorization
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
    {
        var validIssuer = builder.Configuration["Authentication:ValidIssuer"];
        var audience = builder.Configuration["Authentication:Audience"];

        jwtOptions.Authority = validIssuer;
        jwtOptions.Audience = audience;
        jwtOptions.TokenValidationParameters.ValidIssuer = validIssuer;
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role"));
#endregion

#region Session Configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
#endregion

#region MVC & Razor Pages
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddRazorPages();
#endregion





#region Development Tools
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
#endregion

#region SignalR
builder.Services.AddSignalR()
    .AddJsonProtocol();
#endregion

#region Windows Service
builder.Services.AddWindowsService();
#endregion

#region Caching
builder.Services.AddMemoryCache(options =>
{
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(10);
});
#endregion

#region Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes
        .Concat(["image/x-icon"]);
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
#endregion

var app = builder.Build();

#region Cache Initialization
using (var scope = app.Services.CreateScope())
{
    // Precalienta caches vía los servicios de Application (no via repositorios de Infrastructure)
    var getMedics = scope.ServiceProvider.GetRequiredService<IGetMedicsServices>();
    var getTimeTurns = scope.ServiceProvider.GetRequiredService<IGetTimeTurnsServices>();

    try
    {
        await getMedics.GetCachedMedics();
        await getTimeTurns.GetCachedTimes();
        app.Logger.LogInformation("Cache initialized: medics and timeTurns loaded.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Cache initialization warning. App will continue with DB fallback.");
    }
}
#endregion

#region Working Directory
Directory.SetCurrentDirectory(app.Environment.ContentRootPath);
#endregion

#region Middleware Pipeline

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseHsts();
}

app.UseResponseCompression();
app.UseHttpsRedirection();

// Static files with caching
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 86400; // 24 horas
        ctx.Context.Response.Headers[HeaderNames.CacheControl] =
            $"public,max-age={durationInSeconds}";
    }
});

app.UseStaticFiles(); // Default static files

app.UseRouting();
app.UseSession();

// CORS should be before Authentication/Authorization
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<TurnsTableHub>("/TurnsTableHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.UseCookiePolicy();

// Health check endpoint con verificación de PostgreSQL y memoria del contenedor.
// La lógica vive en Infrastructure (ISystemHealthService); aquí solo se serializa.
app.MapGet("/health", async (ISystemHealthService systemHealth) =>
{
    var status = await systemHealth.GetStatusAsync();
    var checks = status.Checks
        .Select(c => new { name = c.Name, status = c.Status, description = c.Description })
        .ToList();
    var overallHealthy = status.OverallHealthy;
    var memDegraded = status.MemoryStatus == "degraded";

    var result = new
    {
        status = overallHealthy ? "healthy" : (memDegraded ? "degraded" : "unhealthy"),
        timestamp = DateTime.UtcNow,
        logs = new
        {
            downloadUrl = "/health/logs",
            description = "Descarga el archivo AppLogs.log del contenedor"
        },
        checks,
        memory = status.MemoryPercentage.HasValue
            ? new
            {
                usageMb = $"{status.MemoryUsageMb:F0}",
                limitMb = $"{status.MemoryLimitMb:F0}",
                usagePercent = $"{status.MemoryPercentage * 100:F1}%"
            }
            : null
    };

    return overallHealthy
        ? Results.Ok(result)
        : Results.Json(result, statusCode: 503);
});

app.MapGet("/health/logs", () =>
{
    var logPath = Path.Combine(AppContext.BaseDirectory, "AppLogs.log");

    if (!File.Exists(logPath))
    {
        return Results.NotFound(new { error = "Todavía no existe AppLogs.log en este entorno." });
    }

    return Results.File(logPath, "text/plain", fileDownloadName: "AppLogs.log");
});
#endregion


await app.RunAsync();
