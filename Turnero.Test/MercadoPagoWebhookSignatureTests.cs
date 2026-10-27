using System.Security.Cryptography;
using System.Text;
using Turnero.Infrastructure.MercadoPago;

namespace Turnero.Test;

public class MercadoPagoWebhookSignatureTests
{
    private const string Secret = "webhook-secret";
    private const string Timestamp = "1704908010";
    private const string RequestId = "request-123";
    private const string DataId = "abc123";

    [Fact]
    public void IsValid_AcceptsMatchingSignature()
    {
        var signature = CreateSignature(DataId, RequestId, Timestamp, Secret);

        Assert.True(MercadoPagoWebhookSignature.IsValid(signature, RequestId, DataId, Secret));
    }

    [Fact]
    public void IsValid_RejectsTamperedDataId()
    {
        var signature = CreateSignature(DataId, RequestId, Timestamp, Secret);

        Assert.False(MercadoPagoWebhookSignature.IsValid(signature, RequestId, "different-id", Secret));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ts=invalid,v1=00")]
    [InlineData("ts=1,v1=not-hex")]
    public void IsValid_RejectsMalformedSignatures(string signature)
    {
        Assert.False(MercadoPagoWebhookSignature.IsValid(signature, RequestId, DataId, Secret));
    }

    private static string CreateSignature(string dataId, string requestId, string timestamp, string secret)
    {
        var manifest = $"id:{dataId.ToLowerInvariant()};request-id:{requestId};ts:{timestamp};";
        var hash = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes(manifest));
        return $"ts={timestamp},v1={Convert.ToHexStringLower(hash)}";
    }
}
