using System.Text.Json.Serialization;

namespace Turnero.Infrastructure.MercadoPago;

public sealed class MercadoPagoSubscriptionService(
    HttpClient httpClient,
    IOptions<MercadoPagoOptions> options)
{
    private readonly MercadoPagoOptions settings = options.Value;

    public void ValidateCreateConfiguration()
    {
        ValidateAccessToken();

        if (settings.MonthlyAmount <= 0 || settings.MonthlyAmount != decimal.Round(settings.MonthlyAmount, 2))
        {
            throw new InvalidOperationException("MercadoPago:MonthlyAmount must be positive and have no more than two decimal places.");
        }

        if (string.IsNullOrWhiteSpace(settings.CurrencyId)
            || settings.CurrencyId.Length != 3
            || settings.CurrencyId.Any(c => c is < 'A' or > 'Z'))
        {
            throw new InvalidOperationException("MercadoPago:CurrencyId must be a three-letter uppercase currency code.");
        }

        if (!Uri.TryCreate(settings.PublicBaseUrl, UriKind.Absolute, out var baseUri)
            || baseUri.Scheme != Uri.UriSchemeHttps
            || !string.IsNullOrEmpty(baseUri.UserInfo)
            || baseUri.AbsolutePath != "/"
            || !string.IsNullOrEmpty(baseUri.Query)
            || !string.IsNullOrEmpty(baseUri.Fragment))
        {
            throw new InvalidOperationException("MercadoPago:PublicBaseUrl must be an HTTPS origin without a path.");
        }
    }

    public async Task<MercadoPagoPreapproval> CreateAsync(
        string externalReference,
        string payerEmail,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateConfiguration();

        var baseUri = new Uri(settings.PublicBaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
        var request = new CreatePreapprovalRequest
        {
            Reason = "Suscripción mensual",
            ExternalReference = externalReference,
            PayerEmail = payerEmail,
            AutoRecurring = new AutoRecurringRequest
            {
                Frequency = 1,
                FrequencyType = "months",
                TransactionAmount = settings.MonthlyAmount,
                CurrencyId = settings.CurrencyId
            },
            NotificationUrl = new Uri(baseUri, "MercadoPagoSubscriptions/Webhook").AbsoluteUri,
            BackUrl = new Uri(
                baseUri,
                $"MercadoPagoSubscriptions/Return?externalReference={Uri.EscapeDataString(externalReference)}")
                .AbsoluteUri,
            Status = "pending"
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "preapproval")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            settings.AccessToken);

        using var response = await httpClient.SendAsync(message, cancellationToken);
        EnsureSuccess(response);

        var result = await response.Content.ReadFromJsonAsync<MercadoPagoPreapproval>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidDataException("Mercado Pago returned an empty subscription response.");

        if (string.IsNullOrWhiteSpace(result.Id)
            || string.IsNullOrWhiteSpace(result.Status)
            || !string.Equals(result.ExternalReference, externalReference, StringComparison.Ordinal))
        {
            throw new InvalidDataException("Mercado Pago returned an incomplete or mismatched subscription.");
        }

        if (!IsValidCheckoutUrl(result.InitPoint))
        {
            throw new InvalidDataException("Mercado Pago returned an invalid checkout URL.");
        }

        return result;
    }

    public async Task<MercadoPagoPreapproval> GetAsync(
        string mercadoPagoId,
        CancellationToken cancellationToken = default)
    {
        ValidateAccessToken();

        using var message = new HttpRequestMessage(
            HttpMethod.Get,
            $"preapproval/{Uri.EscapeDataString(mercadoPagoId)}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            settings.AccessToken);

        using var response = await httpClient.SendAsync(message, cancellationToken);
        EnsureSuccess(response);

        var result = await response.Content.ReadFromJsonAsync<MercadoPagoPreapproval>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidDataException("Mercado Pago returned an empty subscription response.");

        if (string.IsNullOrWhiteSpace(result.Id)
            || string.IsNullOrWhiteSpace(result.ExternalReference)
            || string.IsNullOrWhiteSpace(result.Status))
        {
            throw new InvalidDataException("Mercado Pago returned an incomplete subscription.");
        }

        return result;
    }

    public bool IsValidCheckoutUrl(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || !string.IsNullOrEmpty(uri.UserInfo))
        {
            return false;
        }

        return uri.Host is "www.mercadopago.com.ar"
            or "mercadopago.com.ar"
            or "sandbox.mercadopago.com.ar"
            or "www.mercadopago.com"
            or "mercadopago.com"
            or "sandbox.mercadopago.com";
    }

    private void ValidateAccessToken()
    {
        if (string.IsNullOrWhiteSpace(settings.AccessToken))
        {
            throw new InvalidOperationException("MercadoPago:AccessToken is not configured.");
        }
    }

    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new MercadoPagoApiException(response.StatusCode);
        }
    }

    private sealed class CreatePreapprovalRequest
    {
        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;

        [JsonPropertyName("external_reference")]
        public string ExternalReference { get; set; } = string.Empty;

        [JsonPropertyName("payer_email")]
        public string PayerEmail { get; set; } = string.Empty;

        [JsonPropertyName("auto_recurring")]
        public AutoRecurringRequest AutoRecurring { get; set; } = new();

        [JsonPropertyName("notification_url")]
        public string NotificationUrl { get; set; } = string.Empty;

        [JsonPropertyName("back_url")]
        public string BackUrl { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    private sealed class AutoRecurringRequest
    {
        [JsonPropertyName("frequency")]
        public int Frequency { get; set; }

        [JsonPropertyName("frequency_type")]
        public string FrequencyType { get; set; } = string.Empty;

        [JsonPropertyName("transaction_amount")]
        public decimal TransactionAmount { get; set; }

        [JsonPropertyName("currency_id")]
        public string CurrencyId { get; set; } = string.Empty;
    }
}

public sealed class MercadoPagoPreapproval
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("external_reference")]
    public string ExternalReference { get; set; } = string.Empty;

    [JsonPropertyName("init_point")]
    public string? InitPoint { get; set; }
}
