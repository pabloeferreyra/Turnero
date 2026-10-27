namespace Turnero.Domain.Entities;

public class ApplicationSubscription : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string ExternalReference { get; set; } = string.Empty;
    public string? MercadoPagoId { get; set; }
    public string Status { get; set; } = "creating";
    public decimal Amount { get; set; }
    public string CurrencyId { get; set; } = "ARS";
    public string? CheckoutUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
