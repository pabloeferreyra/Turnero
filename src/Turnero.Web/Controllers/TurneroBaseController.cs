using Microsoft.AspNetCore.Antiforgery;

namespace Turnero.Web.Controllers;

/// <summary>
/// Base controller providing shared helpers for all MVC controllers.
/// Las dependencias comunes se reciben por inyección de constructor; cada
/// controller derivado las encadena con <c>: base(...)</c>. No hay Service
/// Locator: nada resuelve servicios desde el contenedor en runtime.
/// </summary>
public abstract class TurneroBaseController(
    IGetMedicsServices getMedics,
    IGetTimeTurnsServices getTimeTurns,
    IAntiforgery antiforgery) : Controller
{
    /// <summary>Servicio de médicos (Application layer).</summary>
    protected IGetMedicsServices GetMedics { get; } = getMedics;

    /// <summary>Servicio de horarios (Application layer).</summary>
    protected IGetTimeTurnsServices GetTimeTurns { get; } = getTimeTurns;

    /// <summary>Antiforgery para tokens de verificación de formularios.</summary>
    protected IAntiforgery Antiforgery { get; } = antiforgery;

    /// <summary>
    /// Sets ViewData["ErrorTitle"]-style error info via the ErrorViewModel and returns the Error view.
    /// </summary>
    protected IActionResult NotFoundError(string entityType, string id)
    {
        return View("Error", new ErrorViewModel
        {
            ErrorTitle = $"{entityType} not found",
            ErrorMessage = $"{entityType} with Id = {id} cannot be found"
        });
    }

    /// <summary>
    /// Resolves the antiforgery token and stores it in ViewData["RequestVerificationToken"].
    /// </summary>
    protected void SetAntiforgeryToken()
    {
        var token = Antiforgery
            .GetAndStoreTokens(HttpContext)
            .RequestToken;
        ViewData["RequestVerificationToken"] = token;
    }

    /// <summary>
    /// Returns the current user's Medic ID if they are a medic, null otherwise.
    /// </summary>
    protected async Task<string?> CheckMedic()
    {
        var user = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (user == null) return null;

        var medic = await GetMedics.GetMedicByUserId(user);
        return medic?.Id.ToString();
    }

    /// <summary>
    /// Converts an enum to a List&lt;SelectListItem&gt; for use in dropdowns.
    /// </summary>
    protected static List<SelectListItem> EnumToSelectList<TEnum>(
        Func<TEnum, string>? textSelector = null) where TEnum : struct, Enum
    {
        return [.. Enum.GetValues<TEnum>()
            .Select(e => new SelectListItem
            {
                Value = ((int)(object)e).ToString(),
                Text = textSelector?.Invoke(e) ?? e.ToString()
            })];
    }

    /// <summary>
    /// Converts enum names to a List&lt;SelectListItem&gt; for use in dropdowns.
    /// Useful when you need string-based values (e.g., VaccinesEnum with underscores).
    /// </summary>
    protected static List<SelectListItem> EnumNamesToSelectList<TEnum>(
        Func<string, string>? textTransform = null) where TEnum : struct, Enum
    {
        return [.. Enum.GetNames<TEnum>()
            .Select(name => new SelectListItem
            {
                Value = name,
                Text = textTransform?.Invoke(name) ?? name
            })];
    }

    /// <summary>
    /// Returns the cached list of medics from IMemoryCache, loading from DB if needed.
    /// </summary>
    protected async Task<List<MedicDto>> GetCachedMedicsAsync()
    {
        var medics = await GetMedics.GetCachedMedics();
        return medics ?? [];
    }

    /// <summary>
    /// Returns the cached list of time turns from IMemoryCache, loading from DB if needed.
    /// </summary>
    protected async Task<List<TimeTurn>> GetCachedTimeTurnsAsync()
    {
        var time = await GetTimeTurns.GetCachedTimes();
        return time ?? [];
    }
}
