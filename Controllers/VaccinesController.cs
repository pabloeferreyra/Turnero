namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class VaccinesController(IGetVaccinesServices get,
    IUpdateVaccinesServices update,
    IInsertVaccinesServices insert,
    ILogger<VaccinesController> logger) : Controller
{
    public async Task<IActionResult> Index(Guid? id)
    {
        if (id == null)
        {
            return BadRequest("El ID del paciente es obligatorio.");
        }
        var data = await get.GetByPatientId(id.Value);
        return PartialView("_Table", data);
    }

    [HttpPost]
    public async Task<IActionResult> InitializeVaccines(Guid? patientId)
    {
        logger.LogInformation("Initializing vaccines for patient {PatientId}", patientId);
        if (patientId == Guid.Empty)
        {
            return BadRequest("Patient ID is required.");
        }

        try
        {
            var (draw, pageSize, skip, data, recordsTotal) = await SetTable(patientId);
            data = SetPage(pageSize, skip, data);
            var json = new
            {
                draw,
                recordsFiltered = recordsTotal,
                recordsTotal,
                data
            };
            logger.LogInformation("Successfully initialized vaccines for patient {PatientId}", patientId);
            return Ok(json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing vaccines for patient {PatientId}", patientId);
            return StatusCode(500, "An error occurred while initializing vaccines.");
        }
    }


    [HttpGet]
    public IActionResult Create(Guid? id)
    {
        if(id == null || id == Guid.Empty)
            return BadRequest("El ID del paciente es obligatorio.");
        ViewData["PatientId"] = id.Value.ToString();
        var model = new Vaccines { PatientId = id.Value };
        var token = HttpContext.RequestServices.GetRequiredService<IAntiforgery>()
            .GetAndStoreTokens(HttpContext)
            .RequestToken;
        ViewData["RequestVerificationToken"] = token;
        ViewBag.Description = Enum.GetNames<VaccinesEnum>().Select(v => new SelectListItem
        {
            Text = v.Replace("_", " "),
            Value = v
        }).ToList();
        return PartialView("_Create", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Create(VaccinesDto dto)
    {
        if (dto == null || dto.PatientId == Guid.Empty)
            return BadRequest();

        Vaccines vaccine = new()
        {
            Description = dto.Description != "Otra" ? dto.Description : dto.OtherDescription,
            DateApplied = dto.DateApplied,
            PatientId = dto.PatientId
        };

        try
        {
            
            await insert.Insert(vaccine);
            logger.LogInformation("Vaccine created successfully for patient {PatientId}", vaccine.PatientId);
            return Ok();
        }
        catch
        {
            logger.LogWarning("Failed to create Vaccine for patient {PatientId}", vaccine.PatientId);
            return StatusCode(500);
        }
    }

    #region private methods
    private async Task<(string draw, int pageSize, int skip, List<Vaccines> data, int recordsTotal)> SetTable(Guid? patientIdFromQuery = null)
    {
        var draw = Request.Form["draw"].FirstOrDefault() ?? "1";
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();

        string? searchCandidate = null;
        if (patientIdFromQuery.HasValue)
        {
            searchCandidate = patientIdFromQuery.Value.ToString();
            logger.LogDebug("SetTableAsync: using patientId from querystring: {patientId}", searchCandidate);
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

        IQueryable<Vaccines> allergiesQueryable = Enumerable.Empty<Vaccines>().AsQueryable();
        if (Guid.TryParse(searchCandidate, out var patientGuid))
        {
            var result = await get.GetByPatientId(patientGuid);
            allergiesQueryable = result.AsQueryable() ?? Enumerable.Empty<Vaccines>().AsQueryable();
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

    private static List<Vaccines> SetPage(int pageSize, int skip, List<Vaccines> data)
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
