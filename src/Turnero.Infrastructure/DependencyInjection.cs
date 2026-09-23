using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Turnero.Infrastructure.Identity;

namespace Turnero.Infrastructure;

/// <summary>
/// Registro de dependencias de la capa Infrastructure.
/// La presentación solo necesita invocar services.AddInfrastructure(configuration).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Inicializa el SDK de Firebase (si hay credenciales configuradas)
        FirebaseInitializer.Initialize(configuration);
        // DatabaseOptions tipadas (reemplaza al estático AppSettings.ConnectionString)
        var connectionString = configuration.GetConnectionString("PostgresConnection")
            ?? configuration["ConnectionStrings:PostgresConnection"]
            ?? throw new InvalidOperationException(
                "Missing configuration for ConnectionStrings:PostgresConnection. Mount .env into /app/.env or set ConnectionStrings__PostgresConnection in the container environment.");

        services.Configure<DatabaseOptions>(options =>
        {
            options.PostgresConnection = connectionString;
        });

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // ASP.NET Identity (persistencia de usuarios/roles: es infraestructura)
        services.AddDefaultIdentity<IdentityUser>(options =>
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

        // Repositorios
        services.AddScoped<ITimeTurnRepository, TimeTurnRepository>();
        services.AddScoped<IMedicRepository, MedicRepository>();
        services.AddScoped<ITurnRepository, TurnsRepository>();
        services.AddScoped<ITurnDTORepository, TurnDTORepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IVisitRepository, VisitRepository>();
        services.AddScoped<IAllergiesRepository, AllergiesRepository>();
        services.AddScoped<IParentsDataRepository, ParentsDataRepository>();
        services.AddScoped<IPersonalBackgroundRepository, PersonalBackgroundRepository>();
        services.AddScoped<IPerinatalBackgroundRepository, PerinatalBackgroundRepositroy>();
        services.AddScoped<IVaccinesRepository, VaccinesRepository>();
        services.AddScoped<IPermMedRepository, PermMedRepository>();
        services.AddScoped<IGrowthChartRepository, GrowthChartRepository>();
        services.AddScoped<ICongErrorsRepository, CongErrorsRepository>();

        // Servicios de aplicación con acceso a datos (adaptadores Infrastructure)
        services.AddScoped<IPatientClinicalHistoryService, PatientClinicalHistoryService>();
        services.AddScoped<ISystemHealthService, SystemHealthService>();

        // Monitoreo de memoria del contenedor (hosted service)
        services.AddHostedService<ContainerMemoryMonitor>();

        // Firebase (via HttpClient factory)
        services.AddHttpClient<IFirebaseAuthService, FirebaseAuthService>(httpClient =>
        {
            var tokenUri = configuration["Authentication:TokenUri"];
            if (!string.IsNullOrEmpty(tokenUri))
            {
                httpClient.BaseAddress = new Uri(tokenUri);
            }
        });

        return services;
    }
}
