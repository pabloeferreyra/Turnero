namespace Turnero.Infrastructure.MercadoPago;

public sealed class MercadoPagoApiException(HttpStatusCode statusCode)
    : Exception($"Mercado Pago returned HTTP {(int)statusCode}.")
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
