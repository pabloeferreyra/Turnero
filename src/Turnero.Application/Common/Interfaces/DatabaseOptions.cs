namespace Turnero.Application.Common.Interfaces;

/// <summary>
/// Connection string inyectado via IOptions (reemplaza al estático
/// AppSettings.ConnectionString, estado global mutable que violaba CA).
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    public string PostgresConnection { get; set; } = string.Empty;
}
