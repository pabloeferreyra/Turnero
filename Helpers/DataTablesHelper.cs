namespace Turnero.Helpers;

/// <summary>
/// Shared helpers for DataTables server-side processing.
/// Extracts common pagination, sorting, and patientId resolution logic
/// that was previously duplicated across AllergiesController, GrowthChartController,
/// VisitsController, PermMedController, and VaccinesController.
/// </summary>
public static class DataTablesHelper
{
    /// <summary>
    /// Extracts DataTables draw/start/length parameters from the HTTP request.
    /// </summary>
    public static (string draw, int pageSize, int skip) GetDataTableParams(HttpRequest request)
    {
        var draw = request.Form["draw"].FirstOrDefault() ?? "1";
        var start = request.Form["start"].FirstOrDefault();
        var length = request.Form["length"].FirstOrDefault();

        int pageSize = length != null ? int.Parse(length) : 0;
        int skip = start != null ? int.Parse(start) : 0;

        return (draw, pageSize, skip);
    }

    /// <summary>
    /// Resolves a patientId from multiple request sources: query parameter, form data, or JSON body.
    /// This replaces the ~50-line patientId extraction block that was copy-pasted across controllers.
    /// </summary>
    public static async Task<string?> ExtractPatientIdAsync(
        HttpRequest request,
        Guid? patientIdFromQuery = null,
        ILogger? logger = null)
    {
        if (patientIdFromQuery.HasValue)
        {
            logger?.LogDebug("ExtractPatientId: using patientId from querystring: {PatientId}", patientIdFromQuery.Value);
            return patientIdFromQuery.Value.ToString();
        }

        string? searchCandidate = null;

        // 1) Try form data
        if (request.HasFormContentType)
        {
            var form = request.Form;

            if (form.TryGetValue("patientId", out var val) || form.TryGetValue("patientid", out val))
            {
                searchCandidate = val.FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(searchCandidate))
            {
                var searchKey = form.Keys.FirstOrDefault(k =>
                    k.EndsWith("[search][value]", StringComparison.OrdinalIgnoreCase));
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

        // 2) Try query string
        if (string.IsNullOrWhiteSpace(searchCandidate) && request.Query.TryGetValue("patientId", out var qv))
        {
            searchCandidate = qv.FirstOrDefault();
        }

        // 3) Try JSON body
        if (string.IsNullOrWhiteSpace(searchCandidate)
            && HttpMethods.IsPost(request.Method)
            && !string.IsNullOrWhiteSpace(request.ContentType)
            && request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;

                if (!string.IsNullOrWhiteSpace(body))
                {
                    var dict = System.Text.Json.JsonSerializer
                        .Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(body);
                    if (dict != null)
                    {
                        if (dict.TryGetValue("patientId", out var je)
                            && je.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            searchCandidate = je.GetString();
                        }
                        else
                        {
                            var kv = dict.FirstOrDefault(k =>
                                k.Key.Equals("patientId", StringComparison.OrdinalIgnoreCase)
                                || k.Key.Contains("patient", StringComparison.OrdinalIgnoreCase));
                            if (!string.IsNullOrEmpty(kv.Key))
                                searchCandidate = kv.Value.ToString();
                        }
                    }
                }
            }
            catch
            {
                // Swallow JSON parse errors — fall through with null
            }
        }

        return searchCandidate;
    }

    /// <summary>
    /// Parses the extracted patientId string into a Guid?, returning null if invalid.
    /// </summary>
    public static Guid? ParsePatientId(string? searchCandidate)
    {
        if (string.IsNullOrWhiteSpace(searchCandidate))
            return null;

        return Guid.TryParse(searchCandidate, out var patientGuid) ? patientGuid : null;
    }

    /// <summary>
    /// Applies DataTables column sorting to a list using reflection.
    /// </summary>
    public static List<T> ApplySorting<T>(List<T> data, HttpRequest request)
    {
        var orderColumnIndex = request.Form["order[0][column]"].FirstOrDefault();
        var sortColumn = !string.IsNullOrEmpty(orderColumnIndex)
            ? request.Form[$"columns[{orderColumnIndex}][name]"].FirstOrDefault()
            : null;
        var sortDir = request.Form["order[0][dir]"].FirstOrDefault();

        if (!string.IsNullOrEmpty(sortColumn))
        {
            try
            {
                if (string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase))
                    data = [.. data.OrderBy(v => GetPropValue(v, sortColumn))];
                else
                    data = [.. data.OrderByDescending(v => GetPropValue(v, sortColumn))];
            }
            catch
            {
                // Invalid sort column — return unsorted
            }
        }

        return data;
    }

    /// <summary>
    /// Applies DataTables pagination (skip/take) to a list.
    /// Replaces the identical SetPage method from 5 controllers.
    /// </summary>
    public static List<T> ApplyPaging<T>(List<T> data, int pageSize, int skip)
    {
        if (data == null) return [];
        if (pageSize == -1) return data;
        if (skip != 0 && pageSize > 0) return [.. data.Skip(skip).Take(pageSize)];
        if (pageSize > 0) return [.. data.Take(pageSize)];
        return data;
    }

    /// <summary>
    /// Reflection-based property accessor for DataTables column sorting.
    /// </summary>
    public static object? GetPropValue(object? obj, string propName)
    {
        if (obj == null || string.IsNullOrWhiteSpace(propName)) return null;
        var prop = obj.GetType().GetProperty(propName);
        return prop?.GetValue(obj);
    }

    /// <summary>
    /// Generic DataTables initialization for patient-scoped entities.
    /// Extracts params, resolves patientId, fetches data, applies sorting/paging,
    /// and returns the standard DataTables JSON response.
    /// Replaces the identical 15-line pattern repeated across 5+ controllers.
    /// </summary>
    /// <typeparam name="T">The entity type (e.g. Allergies, Vaccines, Visit).</typeparam>
    /// <param name="request">The HTTP request containing DataTables form params.</param>
    /// <param name="patientId">Optional patientId from query string.</param>
    /// <param name="fetchData">Async function that fetches data for a given patient Guid.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public static async Task<object> InitializePatientDataTablesAsync<T>(
        HttpRequest request,
        Guid? patientId,
        Func<Guid, Task<List<T>>> fetchData,
        ILogger? logger = null)
    {
        var (draw, pageSize, skip) = GetDataTableParams(request);
        var searchCandidate = await ExtractPatientIdAsync(request, patientId, logger);
        var patientGuid = ParsePatientId(searchCandidate);

        List<T> data = patientGuid.HasValue
            ? await fetchData(patientGuid.Value) ?? []
            : [];

        data = ApplySorting(data, request);
        var recordsTotal = data.Count;
        data = ApplyPaging(data, pageSize, skip);

        logger?.LogDebug("InitializePatientDataTables: returning {Count} items (patientId: {PatientId})",
            data.Count, patientId?.ToString() ?? "null");

        return new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
    }
}
