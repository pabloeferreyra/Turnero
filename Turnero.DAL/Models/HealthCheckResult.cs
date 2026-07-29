namespace Turnero.DAL.Models;

/// <summary>
/// Represents the result of a health check for a service dependency
/// (e.g., PostgreSQL, Redis).
/// </summary>
public class HealthCheckResult
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
