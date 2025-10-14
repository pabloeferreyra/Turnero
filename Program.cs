var builder = WebApplication.CreateBuilder(args);

MapsterConfig.RegisterMappings();
#region Path Configuration
string firebasePath = GetFirebasePath(builder.Configuration["secretsFolder"]);

static string GetFirebasePath(string secretsFolder) =>
    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                      "Microsoft", "UserSecrets", secretsFolder, "firebase.json")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                      ".microsoft", "usersecrets", secretsFolder, "firebase.json");
#endregion

#region Configuration
builder.Configuration.AddUserSecrets<Program>();
#endregion

#region Database Configuration
AppSettings.ConnectionString = builder.Configuration.GetConnectionString("LocalConnection");
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
    options.Filters.Add(new AuthorizeFilter(policy));
})  
.AddXmlSerializerFormatters();

builder.Services.AddRazorPages();
#endregion

#region Firebase Configuration
if (File.Exists(firebasePath))
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebasePath)
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
builder.Services.AddScoped<IInsertHistoryService, InsertHistoryService>();
builder.Services.AddScoped<IGetHistoryService, GetHistoryService>();
builder.Services.AddScoped<IUpdateHistoryService, UpdateHistoryService>();

// Visit Services
builder.Services.AddScoped<IGetVisitService, GetVisitService>();
builder.Services.AddScoped<IInsertVisitService, InsertVisitService>();

#endregion

#region Dependency Injection - Repositories
builder.Services.AddScoped<ITimeTurnRepository, TimeTurnRepository>();
builder.Services.AddScoped<IMedicRepository, MedicRepository>();
builder.Services.AddScoped<ITurnRepository, TurnsRepository>();
builder.Services.AddScoped<ITurnDTORepository, TurnDTORepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();
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