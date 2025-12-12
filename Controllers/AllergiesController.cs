namespace Turnero.Controllers;

public class AllergiesController(IInsertAllergiesServices insertAllergies,
    IUpdateAllergiesServices updateAllergies,
    IDeleteAllergiesServices deleteAllergies,
    IGetAllergiesServices getAllergies,
    ILogger<AllergiesController> logger) : Controller
{

    [HttpGet]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            return BadRequest("Allergy ID is required.");
        ViewData["AllergyId"] = id.ToString();
        var Details = await getAllergies.Get(id.Value);
        return PartialView("_Details", Details);
    }

    [HttpGet]
    public IActionResult Create(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            return BadRequest("Patient ID is required.");
        ViewData["PatientId"] = id.ToString();
        var model = new Allergies { PatientId = id.Value };
        var token = HttpContext.RequestServices.GetRequiredService<IAntiforgery>()
            .GetAndStoreTokens(HttpContext)
            .RequestToken;
        ViewData["RequestVerificationToken"] = token;
        ViewBag.Occurrency = Enum.GetValues<Occurrency>()
            .Select(a => new SelectListItem
            {
                Value = ((int)a).ToString(),
                Text = a.ToString()
            }).ToList();
        ViewBag.Severity = Enum.GetValues<Severity>()
            .Select(a => new SelectListItem
            {
                Value = ((int)a).ToString(),
                Text = a.ToString()
            }).ToList();
        ViewBag.Type = Enum.GetValues<AllergyType>()
            .Select(a => new SelectListItem
            {
                Value = ((int)a).ToString(),
                Text = a.ToString()
            }).ToList();
        return PartialView("_Create", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Create(Allergies allergy)
    {
        if (allergy == null || allergy.PatientId == Guid.Empty)
        {
            return BadRequest();
        }
        try
        {
            await insertAllergies.InsertAllergy(allergy);
            logger.LogInformation("Allergy created successfully for patient {PatientId}", allergy.PatientId);
            return Ok();
        }
        catch
        {
            logger.LogWarning("Failed to create allergy for patient {PatientId}", allergy.PatientId);
            return StatusCode(500);
        }
    }

    [HttpGet]
    public IActionResult Index(Guid id)
    {
        logger.LogInformation("GetAllergies called for patient {patientId}", id);
        ViewData["PatientId"] = id.ToString();
        return PartialView("_AllergiesTable");
    }

    [HttpPost]
    public async Task<IActionResult> InitializeAllergies(Guid? patientId)
    {
        logger.LogInformation("Initializing allergies for patient {PatientId}", patientId);
        if (patientId == Guid.Empty)
        {
            return BadRequest("Patient ID is required.");
        }
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
            logger.LogInformation("InitializeAllergies returning Ok with {Count} items (patientId: {PatientId})", data?.Count ?? 0, patientId?.ToString() ?? "null");
            return Ok(json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "InitializeAllergies failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
            return NotFound();
        var allergy = await getAllergies.Get(id);
        if (allergy == null)
        {
            ViewBag.ErrorMesage = $"Allergy with Id = {id} cannot be found";
            return NotFound();
        }
        var token = HttpContext.RequestServices.GetRequiredService<IAntiforgery>()
            .GetAndStoreTokens(HttpContext)
            .RequestToken;
        ViewData["RequestVerificationToken"] = token;
        ViewBag.Occurrency = Enum.GetValues<Occurrency>()
            .Select(a => new SelectListItem
            {
                Value = ((int)a).ToString(),
                Text = a.ToString()
            }).ToList();
        ViewBag.Severity = Enum.GetValues<Severity>()
            .Select(a => new SelectListItem
            {
                Value = ((int)a).ToString(),
                Text = a.ToString()
            }).ToList();
        ViewBag.Type = Enum.GetValues<AllergyType>()
            .Select(a => new SelectListItem
            {
                Value = ((int)a).ToString(),
                Text = a.ToString()
            }).ToList();
        return PartialView("_Create", allergy);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Edit(Allergies allergy)
    {
        if (allergy == null || allergy.PatientId == Guid.Empty)
        {
            return BadRequest();
        }
        try
        {
            await updateAllergies.UpdateAllergy(allergy);
            logger.LogInformation("Allergy updated successfully for patient {PatientId}", allergy.PatientId);
            return Ok();
        }
        catch
        {
            logger.LogWarning("Failed to update allergy for patient {PatientId}", allergy.PatientId);
            return StatusCode(500);
        }
    }

    [HttpDelete]
    public async Task<StatusCodeResult> Delete(Guid id)
    {
        var allergy = await getAllergies.Get(id);
        deleteAllergies.DeleteAllergy(allergy);
        return Ok();
    }

    #region private
    private async Task<(string draw, int pageSize, int skip, List<Allergies> data, int recordsTotal)> SetTableAsync(Guid? patientIdFromQuery = null)
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

        IQueryable<Allergies> allergiesQueryable = Enumerable.Empty<Allergies>().AsQueryable();
        if (Guid.TryParse(searchCandidate, out var patientGuid))
        {
            var result = await getAllergies.GetAllergies(patientGuid);
            allergiesQueryable = result ?? Enumerable.Empty<Allergies>().AsQueryable();
        }

        var list = allergiesQueryable.ToList();

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

    private static List<Allergies> SetPage(int pageSize, int skip, List<Allergies> data)
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
    #endregion
}
