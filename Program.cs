using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using Turnero.Data;
using Turnero.Hubs;
using Turnero.Services;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

var builder = WebApplication.CreateBuilder(args);

if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    builder.WebHost.ConfigureKestrel((context, options) =>
    {
        var config = builder.Configuration.GetSection("Kestrel");
        options.Configure(config);
    });
}

#region Path
string secretsPath;

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    secretsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets", builder.Configuration["secretsFolder"], "secrets.json");
}
else
{
    secretsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".microsoft", "usersecrets", builder.Configuration["secretsFolder"], "secrets.json");
}

#endregion

#region secrets
builder.Configuration.AddJsonFile(secretsPath, optional: false);

builder.Configuration.AddUserSecrets<Program>();
#endregion

var connectionString = builder.Configuration["ConnectionStrings:PostgresConnection"];
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseNpgsql(connectionString)).AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);

    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

builder.Services.AddMvc(
    options =>
    {
        var policy = new AuthorizationPolicyBuilder()
                         .RequireAuthenticatedUser()
                         .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
    }
).AddXmlSerializerFormatters();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DeleteRolePolicy",
        policy => policy.RequireClaim("Delete Role"));
});

builder.Services.AddRazorPages();

builder.Services.AddScoped<IInsertTurnsServices, InsertTurnsServices>();
builder.Services.AddScoped<IUpdateTurnsServices, UpdateTurnsServices>();
builder.Services.AddScoped<IGetTurnsServices, GetTurnsServices>();
builder.Services.AddScoped<IInsertMedicServices, InsertMedicServices>();
builder.Services.AddScoped<IUpdateMedicServices, UpdateMedicServices>();
builder.Services.AddScoped<IInsertMedicServices, InsertMedicServices>();
builder.Services.AddScoped<IGetMedicsServices, GetMedicsServices>();
builder.Services.AddScoped<IInsertTimeTurnServices, InsertTimeTurnServices>();
builder.Services.AddScoped<IDeleteTimeTurnServices, DeleteTimeTurnServices>();
builder.Services.AddScoped<IGetTimeTurnsServices, GetTimeTurnsServices>();
builder.Services.AddSingleton<ILoggerServices, LoggerServices>();
builder.Services.AddScoped<IGetTurnDTOServices, GetTurnDTOServices>();

builder.Services.AddScoped<ITimeTurnRepository, TimeTurnRepository>();
builder.Services.AddScoped<IMedicRepository, MedicRepository>();
builder.Services.AddScoped<ITurnRepository, TurnsRepository>();
builder.Services.AddScoped<ITurnDTORepository, TurnDTORepository>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddSignalR().AddJsonProtocol();
builder.Services.AddHttpClient();

builder.Host.UseWindowsService();

var cache = new MemoryCache(new MemoryCacheOptions());
builder.Services.AddSingleton<IMemoryCache>(cache);

builder.Services.Configure<MemoryCacheOptions>(options =>
{
    options.ExpirationScanFrequency = TimeSpan.FromDays(7);
});

builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/x-icon" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    var timeTurnsRepository = serviceProvider.GetRequiredService<ITimeTurnRepository>();
    var medicsRepository = serviceProvider.GetRequiredService<IMedicRepository>();

    await timeTurnsRepository.GetCachedTimes();
    await medicsRepository.GetCachedMedics();
}

IWebHostEnvironment env = app.Environment;

Directory.SetCurrentDirectory(env.ContentRootPath);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "public, max-age=300"; // Permite cachear la respuesta durante 1 hora (3600 segundos)
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapHub<TurnsTableHub>("/TurnsTableHub");

app.UseAuthentication();

app.UseAuthorization();

app.UsePathBase("/Demo");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.UseCookiePolicy();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseResponseCompression();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 86400; // Duración de la caché en segundos (24 horas)
        ctx.Context.Response.Headers[HeaderNames.CacheControl] =
            "public,max-age=" + durationInSeconds;
    }
});

app.Run();