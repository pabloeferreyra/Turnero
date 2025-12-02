namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class VisitsController(IGetVisitService getVisit,
    IInsertVisitService insertVisit,
    ILogger<VisitsController> logger,
    IGetMedicsServices getMedics) : Controller
{
    [HttpGet]
    public IActionResult Details(Guid? id)
    {
        if (id == null || id == Guid.Empty)
        {
            return BadRequest("Visit ID is required.");
        }
        ViewData["VisitId"] = id.ToString();
        var Details = getVisit.Get(id.Value).Result;
        return PartialView("_Details", Details);
    }

    [HttpGet]
    public IActionResult Create(Guid? id)
    {
        if (id == null || id == Guid.Empty)
        {
            return BadRequest("Patient ID is required.");
        }
        ViewData["PatientId"] = id.ToString();
        // Pass a Visit model so the Razor partial has a non-null Model
        var model = new Visit { PatientId = id.Value };
        var token = HttpContext.RequestServices.GetRequiredService<IAntiforgery>()
                .GetAndStoreTokens(HttpContext)
                .RequestToken;
        ViewData["RequestVerificationToken"] = token;
        return PartialView("_CreateVisit", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Create(Visit visit)
    {
        string isMedic = await CheckMedic();
        var medicId = Guid.TryParse(isMedic, out var mid) ? mid : (Guid?)null;
        if (medicId != null)
        {
            visit.MedicId = (Guid)medicId;
        }
        if (visit == null || visit.PatientId == Guid.Empty)
        {
            return BadRequest();
        }
        try
        {
            await insertVisit.Create(visit);
            logger.LogInformation("Visit created successfully for patient {PatientId}", visit.PatientId);
            return Ok();
        }
        catch
        {
            logger.LogWarning("Failed to create visit for patient {PatientId}", visit.PatientId);
            return StatusCode(500);
        }

    }

    [HttpGet]
    public IActionResult GetVisits(Guid id)
    {
        logger.LogInformation("GetVisits called for patient {PatientId}", id);
        ViewData["PatientId"] = id.ToString();
        return PartialView("_VisitsTable");
    }

    [HttpPost]
    public async Task<IActionResult> InitializeVisits(Guid? patientId)
    {
        logger.LogInformation("InitializeVisits called. Request path: {Path}. Query patientId: {QueryPatientId}", Request.Path, patientId?.ToString() ?? "null");
        try
        {
            var (draw, pageSize, skip, data, recordsTotal) = await SetTableAsync(patientId);
            data = SetPage(pageSize, skip, data);

            var json = new
            {
                draw,
                recordsFiltered = recordsTotal,
                recordsTotal,
                data
            };

            logger.LogInformation("InitializeVisits returning Ok with {Count} items (patientId: {PatientId})", data?.Count ?? 0, patientId?.ToString() ?? "null");
            return Ok(json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "InitializeVisits failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #region private
    private async Task<(string draw, int pageSize, int skip, List<VisitDTO> data, int recordsTotal)> SetTableAsync(Guid? patientIdFromQuery = null)
    {
        var draw = Request.Form["draw"].FirstOrDefault() ?? "1";
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();

        string? searchCandidate = null;

        if (patientIdFromQuery.HasValue)
        {
            searchCandidate = patientIdFromQuery.Value.ToString();
            logger.LogDebug("SetTableAsync: using patientId from querystring: {PatientId}", searchCandidate);
        }
        else
        {
            if (Request.HasFormContentType)
            {
                var form = Request.Form;

                if (form.TryGetValue("patientId", out var val) || form.TryGetValue("patientid", out val))
                {
                    searchCandidate = val.FirstOrDefault();
                }

                if (string.IsNullOrWhiteSpace(searchCandidate))
                {
                    var searchKey = form.Keys.FirstOrDefault(k => k.EndsWith("[search][value]", StringComparison.OrdinalIgnoreCase));
                    if (searchKey != null)
                        searchCandidate = form[searchKey].FirstOrDefault();
                }

                if (string.IsNullOrWhiteSpace(searchCandidate))
                {
                    foreach (var key in form.Keys)
                    {
                        var candidate = form[key].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(candidate) && Guid.TryParse(candidate, out _))
                        {
                            searchCandidate = candidate;
                            break;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(searchCandidate) && Request.Query.TryGetValue("patientId", out var qv))
            {
                searchCandidate = qv.FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(searchCandidate)
                && HttpMethods.IsPost(Request.Method)
                && !string.IsNullOrWhiteSpace(Request.ContentType)
                && Request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Request.EnableBuffering();
                    using var reader = new StreamReader(Request.Body, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    Request.Body.Position = 0;

                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(body);
                        if (dict != null)
                        {
                            if (dict.TryGetValue("patientId", out var je) && je.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                searchCandidate = je.GetString();
                            }
                            else
                            {
                                var kv = dict.FirstOrDefault(k => k.Key.Equals("patientId", StringComparison.OrdinalIgnoreCase)
                                                                  || k.Key.Contains("patient", StringComparison.OrdinalIgnoreCase));
                                if (!string.IsNullOrEmpty(kv.Key))
                                    searchCandidate = kv.Value.ToString();
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }

        searchCandidate ??= string.Empty;

        IQueryable<VisitDTO> visitsQueryable = Enumerable.Empty<VisitDTO>().AsQueryable();
        if (Guid.TryParse(searchCandidate, out var patientGuid))
        {
            var result = await getVisit.SearchVisits(patientGuid);
            visitsQueryable = result ?? Enumerable.Empty<VisitDTO>().AsQueryable();
        }

        var list = visitsQueryable.ToList();

        var orderColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
        var sortColumn = !string.IsNullOrEmpty(orderColumnIndex)
            ? Request.Form[$"columns[{orderColumnIndex}][name]"].FirstOrDefault()
            : null;
        var sortDir = Request.Form["order[0][dir]"].FirstOrDefault();

        if (!string.IsNullOrEmpty(sortColumn))
        {
            try
            {
                if (string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase))
                    list = [.. list.OrderBy(v => GetPropValue(v, sortColumn))];
                else
                    list = [.. list.OrderByDescending(v => GetPropValue(v, sortColumn))];
            }
            catch
            {
            }
        }

        int pageSize = length != null ? int.Parse(length) : 0;
        int skip = start != null ? int.Parse(start) : 0;
        int recordsTotal = list.Count;

        return (draw, pageSize, skip, list, recordsTotal);
    }

    private static List<VisitDTO> SetPage(int pageSize, int skip, List<VisitDTO> data)
    {
        if (data == null) return [];
        if (pageSize == -1) return data;
        if (skip != 0 && pageSize > 0) return [.. data.Skip(skip).Take(pageSize)];
        if (pageSize > 0) return [.. data.Take(pageSize)];
        return data;
    }

    private static object? GetPropValue(object? obj, string propName)
    {
        if (obj == null || string.IsNullOrWhiteSpace(propName)) return null;
        var prop = obj.GetType().GetProperty(propName);
        return prop?.GetValue(obj);
    }

    private async Task<string> CheckMedic()
    {
        var user = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var isMedic = await getMedics.GetMedicByUserId(user);
        return isMedic?.Id.ToString();
    }
    #endregion
}
