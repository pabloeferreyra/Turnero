using Turnero.SL.Services.CongErrorServices;

var builder = WebApplication.CreateBuilder(args);

MapsterConfig.RegisterMappings();
#region Configuration
AddDotEnvFile(builder.Configuration, ResolveDotEnvPath(builder.Environment));
builder.Configuration.AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

string? firebaseCredentialsPath = GetFirebaseCredentialsPath(builder.Configuration);

static string? GetFirebaseCredentialsPath(IConfiguration configuration)
{
    var configuredPath = configuration["Firebase:CredentialsPath"];

    if (!string.IsNullOrWhiteSpace(configuredPath))
    {
        return Path.GetFullPath(configuredPath);
    }

    var googleCredentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
    return string.IsNullOrWhiteSpace(googleCredentialsPath)
        ? null
        : Path.GetFullPath(googleCredentialsPath);
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

// Diagnóstico adicional (solo en desarrollo)
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();

    // Validación manual para servicios críticos
    builder.Services.AddHostedService<DependencyDiagnosticsHostedService>();
}

#endregion

#region Database Configuration
AppSettings.ConnectionString = builder.Configuration.GetConnectionString("PostgresConnection")
    ?? builder.Configuration["ConnectionStrings:PostgresConnection"]
    ?? throw new InvalidOperationException("Missing configuration for ConnectionStrings:PostgresConnection. Mount .env into /app/.env or set ConnectionStrings__PostgresConnection in the container environment.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(AppSettings.ConnectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();
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

#region Firebase Configuration
if (!string.IsNullOrWhiteSpace(firebaseCredentialsPath) && File.Exists(firebaseCredentialsPath))
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebaseCredentialsPath)
    });
}
#endregion

builder.Services.AddScoped<LoggerService>();

#region Dependency Injection - Services
// Turn Services
builder.Services.AddScoped<IInsertTurnsServices, InsertTurnsServices>();
builder.Services.AddScoped<IUpdateTurnsServices, UpdateTurnsServices>();
builder.Services.AddScoped<IGetTurnsServices, GetTurnsServices>();
builder.Services.AddScoped<IGetTurnDTOServices, GetTurnDTOServices>();

// Medic Services
builder.Services.AddScoped<IInsertMedicServices, InsertMedicServices>();
builder.Services.AddScoped<IUpdateMedicServices, UpdateMedicServices>();
builder.Services.AddScoped<IGetMedicsServices, GetMedicsServices>();

// Time Turn Services
builder.Services.AddScoped<IInsertTimeTurnServices, InsertTimeTurnServices>();
builder.Services.AddScoped<IDeleteTimeTurnServices, DeleteTimeTurnServices>();
builder.Services.AddScoped<IGetTimeTurnsServices, GetTimeTurnsServices>();

// Patient Services
builder.Services.AddScoped<IInsertPatientService, InsertPatientService>();
builder.Services.AddScoped<IGetPatientService, GetPatientService>();
builder.Services.AddScoped<IUpdatePatientService, UpdatePatientService>();

//History Services

// Visit Services
builder.Services.AddScoped<IGetVisitService, GetVisitService>();
builder.Services.AddScoped<IInsertVisitService, InsertVisitService>();

//Allergies Services
builder.Services.AddScoped<IGetAllergiesServices, GetAllergiesServices>();
builder.Services.AddScoped<IInsertAllergiesServices, InsertAllergiesServices>();
builder.Services.AddScoped<IUpdateAllergiesServices, UpdateAllergiesServices>();
builder.Services.AddScoped<IDeleteAllergiesServices, DeleteAllergiesServices>();

//ParentsDataServices
builder.Services.AddScoped<IGetParentsDataService, GetParentsDataService>();
builder.Services.AddScoped<IUpdateParentsDataService, UpdateParentsDataService>();
builder.Services.AddScoped<IDeleteParentsDataService, DeleteParentsDataService>();

//PersonalBackgroundServices
builder.Services.AddScoped<IGetPersonalBackgroundService, GetPersonalBackgroundService>();
builder.Services.AddScoped<IUpdatePersonalBackgroundService, UpdatePersonalBackgroundService>();

//PerinatalBackgroundServices
builder.Services.AddScoped<IGetPerinatalBackgroundService, GetPerinatalBackgroundService>();
builder.Services.AddScoped<IUpdatePerinatalBackgroundService, UpdatePerinatalBackgroundService>();

//VaccinesServices
builder.Services.AddScoped<IGetVaccinesServices, GetVaccinesServices>();
builder.Services.AddScoped<IUpdateVaccinesServices, UpdateVaccinesServices>();
builder.Services.AddScoped<IInsertVaccinesServices, InsertVaccinesServices>();
builder.Services.AddScoped<IDeleteVacinesServices, DeleteVacinesServices>();

//PermMed Services
builder.Services.AddScoped<IGetPermMedService, GetPermMedService>();
builder.Services.AddScoped<IInsertPermMedService, InsertPermMedService>();
builder.Services.AddScoped<IDeletePermMedService, DeletePermMedService>();

//GrowthChart Services
builder.Services.AddScoped<IGetGrowthChartService, GetGrowthChartService>();
builder.Services.AddScoped<IUpdateGrowthChartService, UpdateGrowthChartService>();
builder.Services.AddScoped<IInsertGrowthChartService, InsertGrowthChartService>();
builder.Services.AddScoped<IDeleteGrowthChartService, DeleteGrowthChartService>();

//CongErrors Services
builder.Services.AddScoped<IGetCongErrorService, GetCongErrorService>();
builder.Services.AddScoped<IUpdateCongErrorService, UpdateCongErrorService>();
#endregion

#region Dependency Injection - Repositories
builder.Services.AddScoped<ITimeTurnRepository, TimeTurnRepository>();
builder.Services.AddScoped<IMedicRepository, MedicRepository>();
builder.Services.AddScoped<ITurnRepository, TurnsRepository>();
builder.Services.AddScoped<ITurnDTORepository, TurnDTORepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();
builder.Services.AddScoped<IAllergiesRepository, AllergiesRepository>();
builder.Services.AddScoped<IParentsDataRepository, ParentsDataRepository>();
builder.Services.AddScoped<IPersonalBackgroundRepository, PersonalBackgroundRepository>();
builder.Services.AddScoped<IPerinatalBackgroundRepository, PerinatalBackgroundRepositroy>();
builder.Services.AddScoped<IVaccinesRepository, VaccinesRepository>();
builder.Services.AddScoped<IPermMedRepository, PermMedRepository>();
builder.Services.AddScoped<IGrowthChartRepository, GrowthChartRepository>();
builder.Services.AddScoped<ICongErrorsRepository, CongErrorsRepository>();
#endregion

#region HTTP Client
builder.Services.AddHttpClient<IFirebaseService, FirebaseService>(httpClient =>
{
    var tokenUri = builder.Configuration["Authentication:TokenUri"];
    if (!string.IsNullOrEmpty(tokenUri))
    {
        httpClient.BaseAddress = new Uri(tokenUri);
    }
});
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
    options.ExpirationScanFrequency = TimeSpan.FromDays(7);
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
    var timeTurnsRepository = scope.ServiceProvider.GetRequiredService<ITimeTurnRepository>();
    var medicsRepository = scope.ServiceProvider.GetRequiredService<IMedicRepository>();

    await timeTurnsRepository.GetCachedTimes();
    await medicsRepository.GetCachedMedics();
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

// Custom cache control middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.CacheControl = "public, max-age=300";
    await next();
});

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
#endregion



await app.RunAsync();
