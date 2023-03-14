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
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)).AddDefaultIdentity<IdentityUser>(options =>
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

builder.Services.AddScoped<IExportService, ExportService>();

builder.Services.AddScoped<ILoggerServices, LoggerServices>();

builder.Services.AddScoped<ITimeTurnRepository, TimeTurnRepository>();
builder.Services.AddScoped<IMedicRepository, MedicRepository>();
builder.Services.AddScoped<ITurnRepository, TurnsRepository>();

builder.Services.AddScoped<IExportRepository, ExportRepository>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Host.UseWindowsService();

builder.Services.AddAntiforgery(options => {
    // Set Cookie properties using CookieBuilder properties†.
    options.FormFieldName = "AntiForgeryField";
    options.HeaderName = "X-CSRF-TOKEN-TURNERO";
    options.SuppressXFrameOptionsHeader = false;
});

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.Run();