namespace Turnero.Infrastructure.MercadoPago;

public sealed class MercadoPagoOptions
{
    public string AccessToken { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string PublicBaseUrl { get; set; } = string.Empty;
    public decimal MonthlyAmount { get; set; }
    public string CurrencyId { get; set; } = "ARS";
}
