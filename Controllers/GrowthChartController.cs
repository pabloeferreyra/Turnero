using System.Collections.Generic;
using System.Threading.Tasks;

namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class GrowthChartController(IGetGrowthChartService get, 
    IInsertGrowthChartService insert,
    IUpdateGrowthChartService update,
    IDeleteGrowthChartService delete,
    ILogger<GrowthChartController> logger) : Controller
{
    public async Task<IActionResult> Index(Guid? id)
    {
        if (id == null)
            return BadRequest("El ID del paciente es requerido.");
        var data = await get.Get(id.Value);
        return PartialView("_Table", data);
    }

    [HttpPost]
    public async Task<IActionResult> Initialize(Guid? patientId)
    {
        if (patientId == Guid.Empty)
            return BadRequest("El ID del paciente es requerido.");
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Initializing Growth Chart for PatientId: {PatientId}", patientId);
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
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Successfully intialized Growth Chart for patient {PatientId}", patientId);
            return Ok(json);
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(ex, "Error initializing Growth Chart for patient {PatientId}", patientId);
            return StatusCode(500, "An error occurred while initializing Growth Chart.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            return BadRequest("El ID del paciente es obligatorio.");
        ViewData["PatientId"] = id.Value.ToString();
        var growthChart = new GrowthChart { PatientId = id.Value };
        var token = HttpContext.RequestServices.GetRequiredService<IAntiforgery>()
            .GetAndStoreTokens(HttpContext)
            .RequestToken;
        ViewData["RequestVerificationToken"] = token;
        return PartialView("_Create", growthChart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Create(GrowthChart growthChart)
    {
        if (!ModelState.IsValid || (growthChart == null || growthChart.PatientId == Guid.Empty))
            return BadRequest();
        try
        {
            await insert.Create(growthChart);
            logger.LogInformation("Created Growth Chart entry for PatientId: {PatientId}", growthChart.PatientId);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating Growth Chart entry for PatientId: {PatientId}", growthChart.PatientId);
            return StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            return BadRequest("El ID del registro es obligatorio.");
        var growthChart = await get.Get(id.Value);
        if (growthChart == null)
            return NotFound("Registro no encontrado.");
        var token = HttpContext.RequestServices.GetRequiredService<IAntiforgery>()
            .GetAndStoreTokens(HttpContext)
            .RequestToken;
        ViewData["RequestVerificationToken"] = token;
        return PartialView("_Create", growthChart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Edit(GrowthChart growthChart)
    {
        if (!ModelState.IsValid || (growthChart == null || growthChart.Id == Guid.Empty))
            return BadRequest();
        try
        {
            await update.Edit(growthChart);
            logger.LogInformation("Updated Growth Chart entry Id: {Id}", growthChart.Id);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating Growth Chart entry Id: {Id}", growthChart.Id);
            return StatusCode(500);
        }
    }

    [HttpDelete]
    public async Task<StatusCodeResult> Delete(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            return BadRequest();
        try
        {
            await delete.Delete(id.Value);
            logger.LogInformation("Deleted Growth Chart entry Id: {Id}", id);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting Growth Chart entry Id: {Id}", id);
            return StatusCode(500);
        }
    }

    #region private methods
    private async Task<(string draw, int pageSize, int skip, List<GrowthChart> data, int recordsTotal)> SetTable(Guid? patientIdFromQuery = null)
    {
        var draw = Request.Form["draw"].FirstOrDefault() ?? "1";
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();

        string? searchCandidate = null;
        if (patientIdFromQuery.HasValue)
        {
            searchCandidate = patientIdFromQuery.Value.ToString();
            logger.LogDebug("Using patientId from query string: {PatientId}", searchCandidate);
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
        }
        
        searchCandidate ??= string.Empty;
        IQueryable<GrowthChart> queriable = Enumerable.Empty<GrowthChart>().AsQueryable();
        if(Guid.TryParse(searchCandidate, out var patientGuid))
        {
            var result = await get.Get(patientGuid);
            queriable = result.AsQueryable() ?? Enumerable.Empty<GrowthChart>().AsQueryable();
        }
        var list = queriable.ToList();
            
        var orderColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
        var sortColumn = !string.IsNullOrEmpty(orderColumnIndex)
        ? Request.Form[$"columns[{orderColumnIndex}][name]"].FirstOrDefault()
        : null;

        var sortDir = Request.Form["order[0][dir]"].FirstOrDefault();

        if (!string.IsNullOrEmpty(sortColumn)){
            try
            {
                if(string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase))
                    list = [.. list.OrderBy(v => GetPropValue(v, sortColumn))];
                else
                    list = [.. list.OrderByDescending(v => GetPropValue(v, sortColumn))];
            }
            catch { }
        }
        
        int pageSize = length != null ? int.Parse(length) : 0;
        int skip = start != null ? int.Parse(start) : 0;
        int recordsTotal = list.Count;

        return (draw, pageSize, skip, list, recordsTotal);
    }

    private static List<GrowthChart> SetPage(int pageSize, int skip, List<GrowthChart> data)
    {
        if (data == null) return [];
        if (pageSize == -1) return data;
        if (skip != 0 && pageSize > 0) return [.. data.Skip(skip).Take(pageSize)];
        if(pageSize > 0) return [.. data.Take(pageSize)];
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
