namespace Turnero.Controllers;

/// <summary>
/// Base controller providing shared helpers for all MVC controllers.
/// </summary>
public abstract class TurneroBaseController : Controller
{
    /// <summary>
    /// Sets ViewBag.ErrorMessage for a not-found entity and returns the NotFound view.
    /// </summary>
    protected IActionResult NotFoundError(string entityType, string id)
    {
        ViewBag.ErrorMessage = $"{entityType} with Id = {id} cannot be found";
        return View("NotFound");
    }

    /// <summary>
    /// Resolves the antiforgery token and stores it in ViewData["RequestVerificationToken"].
    /// Replaces the 3-line pattern repeated across 7 controllers (12 occurrences total).
    /// </summary>
    protected void SetAntiforgeryToken()
    {
        var token = HttpContext.RequestServices.GetRequiredService<IAntiforgery>()
            .GetAndStoreTokens(HttpContext)
            .RequestToken;
        ViewData["RequestVerificationToken"] = token;
    }

    /// <summary>
    /// Returns the current user's Medic ID if they are a medic, null otherwise.
    /// Replaces the identical CheckMedic method in TurnsController and VisitsController.
    /// </summary>
    protected async Task<string?> CheckMedic()
    {
        var user = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (user == null) return null;

        var getMedics = HttpContext.RequestServices.GetRequiredService<IGetMedicsServices>();
        var medic = await getMedics.GetMedicByUserId(user);
        return medic?.Id.ToString();
    }

    /// <summary>
    /// Converts an enum to a List<SelectListItem> for use in dropdowns.
    /// Replaces the repeated Enum.GetValues + SelectListItem pattern across controllers.
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
    /// Converts enum names to a List<SelectListItem> for use in dropdowns.
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
    /// Returns the cached list of medics, loading from the service if not yet cached.
    /// Replaces the identical cache.Get → Task.Run → .Result pattern across 3 controllers.
    /// </summary>
    protected async Task<List<MedicDto>> GetCachedMedicsAsync()
    {
        var cache = HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
        var medics = cache.Get<List<MedicDto>>("medics");
        if (medics == null)
        {
            var getMedics = HttpContext.RequestServices.GetRequiredService<IGetMedicsServices>();
            await Task.Run(() => { medics = getMedics.GetCachedMedics().Result; });
        }
        return medics ?? [];
    }

    /// <summary>
    /// Returns the cached list of time turns, loading from the service if not yet cached.
    /// Replaces the identical cache.Get → Task.Run → .Result pattern across 3 controllers.
    /// </summary>
    protected async Task<List<TimeTurn>> GetCachedTimeTurnsAsync()
    {
        var cache = HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
        var time = cache.Get<List<TimeTurn>>("timeTurns");
        if (time == null)
        {
            var getTimeTurns = HttpContext.RequestServices.GetRequiredService<IGetTimeTurnsServices>();
            await Task.Run(() => { time = getTimeTurns.GetCachedTimes().Result; });
        }
        return time ?? [];
    }
}
