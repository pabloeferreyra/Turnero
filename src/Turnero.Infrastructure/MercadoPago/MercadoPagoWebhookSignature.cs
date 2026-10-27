using System.Globalization;
using System.Security.Cryptography;

namespace Turnero.Infrastructure.MercadoPago;

public static class MercadoPagoWebhookSignature
{
    public static bool IsValid(
        string? signature,
        string? requestId,
        string? dataId,
        string? secret)
    {
        if (string.IsNullOrWhiteSpace(signature)
            || string.IsNullOrWhiteSpace(requestId)
            || string.IsNullOrWhiteSpace(dataId)
            || string.IsNullOrWhiteSpace(secret)
            || signature.Length > 1024
            || requestId.Length > 256
            || dataId.Length > 256)
        {
            return false;
        }

        string? timestamp = null;
        var signatures = new List<string>();
        foreach (var part in signature.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = part.IndexOf('=');
            if (separator <= 0)
            {
                return false;
            }

            var key = part[..separator];
            var value = part[(separator + 1)..];
            if (key.Equals("ts", StringComparison.OrdinalIgnoreCase))
            {
                if (timestamp is not null)
                {
                    return false;
                }

                timestamp = value;
            }
            else if (key.Equals("v1", StringComparison.OrdinalIgnoreCase))
            {
                signatures.Add(value);
            }
        }

        if (string.IsNullOrWhiteSpace(timestamp)
            || !long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out _)
            || signatures.Count == 0)
        {
            return false;
        }

        var manifest = $"id:{dataId.ToLowerInvariant()};request-id:{requestId};ts:{timestamp};";
        var expected = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes(manifest));

        foreach (var value in signatures)
        {
            try
            {
                var supplied = Convert.FromHexString(value);
                if (CryptographicOperations.FixedTimeEquals(expected, supplied))
                {
                    return true;
                }
            }
            catch (FormatException)
            {
                // Ignore malformed signature candidates and test any remaining v1 values.
            }
        }

        return false;
    }
}
