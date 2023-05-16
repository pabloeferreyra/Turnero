using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Turnero.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;
using Turnero.Services;
using System.IO;
using Turnero.Hubs;
using System.Runtime.InteropServices;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using Turnero.Models;

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

var connectionString = builder.Configuration["ConnectionStrings:SQLite"];
Console.WriteLine(connectionString);
 builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString)).AddDefaultIdentity<IdentityUser>(options =>
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
    options =>{
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
builder.Services.AddScoped<ITimeTurnRepository, TimeTurnRepository>();
builder.Services.AddScoped<IMedicRepository, MedicRepository>();
builder.Services.AddScoped<ITurnRepository, TurnsRepository>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddSignalR().AddJsonProtocol();
builder.Services.AddHttpClient();

builder.Host.UseWindowsService();

builder.Services.AddMemoryCache();

builder.Services.Configure<MemoryCacheOptions>(options =>
{
    options.ExpirationScanFrequency = TimeSpan.FromDays(7);
});

IMemoryCache cache = builder.Services.BuildServiceProvider()
                                     .GetRequiredService<IMemoryCache>();
var timeTurns = new List<TimeTurnViewModel>();
var medics = new List <MedicDto>();
cache.Set("timeTurns", timeTurns);
cache.Set("medics", medics);


var app = builder.Build();

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
    context.Response.Headers["Cache-Control"] = "public, max-age=3600"; // Permite cachear la respuesta durante 1 hora (3600 segundos)
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

app.Run();