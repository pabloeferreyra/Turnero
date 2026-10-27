namespace Turnero.Web.Models;

public sealed class MercadoPagoSubscriptionsViewModel
{
    public decimal MonthlyAmount { get; init; }
    public string CurrencyId { get; init; } = "ARS";
    public bool IsAdmin { get; init; }
    public bool CanCreate { get; init; }
    public IReadOnlyList<MercadoPagoSubscriptionRowViewModel> Subscriptions { get; init; } = [];
}

public sealed class MercadoPagoSubscriptionRowViewModel
{
    public Guid Id { get; init; }
    public string? UserEmail { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? MercadoPagoId { get; init; }
    public string? CheckoutUrl { get; init; }
    public decimal Amount { get; init; }
    public string CurrencyId { get; init; } = "ARS";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
