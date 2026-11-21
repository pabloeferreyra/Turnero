namespace Turnero.Web.Models;

/// <summary>Profesional disponible para la solicitud pública.</summary>
public sealed record PublicMedicResponse(Guid? Id, string? Name);

/// <summary>Horario disponible para la solicitud pública.</summary>
public sealed record PublicTimeResponse(Guid Id, string? Time);
