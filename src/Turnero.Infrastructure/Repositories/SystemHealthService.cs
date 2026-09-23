using Turnero.Infrastructure.Monitoring;

namespace Turnero.Infrastructure.Repositories;

/// <summary>
/// Adaptador de Infrastructure: verifica la salud del sistema (PostgreSQL via
/// EF Core y memoria del contenedor via cgroup). La presentación consume el
/// DTO SystemHealthStatus sin conocer EF Core ni detalles del contenedor.
/// </summary>
public class SystemHealthService(ApplicationDbContext dbContext) : ISystemHealthService
{
    public async Task<SystemHealthStatus> GetStatusAsync()
    {
        var status = new SystemHealthStatus();
        var overallHealthy = true;

        // PostgreSQL
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync();
            status.Checks.Add(new SystemHealthCheck
            {
                Name = "PostgreSQL",
                Status = canConnect ? "healthy" : "unhealthy",
                Icon = canConnect ? "bi-database-check" : "bi-database-x",
                Description = canConnect
                    ? "Conexión establecida correctamente"
                    : "No se puede conectar a la base de datos"
            });
            if (!canConnect) overallHealthy = false;
        }
        catch (Exception ex)
        {
            status.Checks.Add(new SystemHealthCheck
            {
                Name = "PostgreSQL",
                Status = "unhealthy",
                Icon = "bi-database-x",
                Description = ex.Message
            });
            overallHealthy = false;
        }

        // Memoria del contenedor
        var memDegraded = false;
        try
        {
            var memInfo = ContainerMemoryMonitor.ReadMemoryInfo();
            if (memInfo.HasValue)
            {
                status.MemoryUsageMb = memInfo.Value.CurrentBytes / (1024.0 * 1024.0);
                status.MemoryLimitMb = memInfo.Value.LimitBytes / (1024.0 * 1024.0);
                status.MemoryPercentage = memInfo.Value.Percentage;
                status.MemoryStatus = memInfo.Value.Percentage >= 0.95 ? "degraded" : "healthy";
                memDegraded = memInfo.Value.Percentage >= 0.95;
                if (memDegraded) overallHealthy = false;
            }
        }
        catch
        {
            // cgroup stats not available
        }

        status.OverallHealthy = overallHealthy;
        return status;
    }
}
