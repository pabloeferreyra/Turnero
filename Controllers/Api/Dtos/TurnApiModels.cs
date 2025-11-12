using System.ComponentModel.DataAnnotations;

namespace Turnero.Controllers.Api.Dtos;

public record TurnCreateRequest(
    [property: Required]string Name,
    [property: Required]string Dni,
    [property: Required]Guid MedicId,
    [property: Required]DateOnly DateTurn,
    [property: Required]Guid TimeId,
    string? SocialWork,
    string? Reason);

public record TurnUpdateRequest(
    [property: Required]Guid Id,
    [property: Required]string Name,
    [property: Required]string Dni,
    [property: Required]Guid MedicId,
    [property: Required]DateOnly DateTurn,
    [property: Required]Guid TimeId,
    string? SocialWork,
    string? Reason,
    bool Accessed);

public record TurnResponse(
    Guid Id,
    string Name,
    string Dni,
    Guid MedicId,
    string? MedicName,
    DateOnly Date,
    Guid TimeId,
    string Time,
    string? SocialWork,
    string? Reason,
    bool Accessed);
