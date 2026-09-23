namespace Turnero.Web;

public class DependencyDiagnosticsHostedService(IServiceProvider serviceProvider, ILogger<DependencyDiagnosticsHostedService> logger)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Namespace raíz donde viven los servicios de aplicación
        const string baseNamespace = "Turnero.Application.Services";

        logger.LogInformation("🔍 Iniciando diagnóstico de dependencias en {Namespace}", baseNamespace);

        // Buscamos todos los tipos del assembly de Application
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("Turnero.Application") == true)
            .ToArray();

        var serviceTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                t.IsInterface &&
                t.Namespace != null &&
                t.Namespace.StartsWith(baseNamespace) &&
                !t.IsGenericType)
            .ToList();

        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        foreach (var serviceType in serviceTypes)
        {
            try
            {
                // Intentamos resolver la interfaz registrada
                sp.GetRequiredService(serviceType);
                logger.LogInformation("✅ {TypeName} registrado correctamente.", serviceType.FullName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ FALTA registro o implementación válida para {TypeName}", serviceType.FullName);
            }
        }

        logger.LogInformation("✅ Diagnóstico de dependencias completado ({Count} servicios verificados).", serviceTypes.Count);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}