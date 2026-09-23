using Google.Apis.Auth.OAuth2;

namespace Turnero.Infrastructure.Identity;

/// <summary>
/// Inicializa el SDK FirebaseAdmin una única vez a partir de la configuración
/// (JSON inline, campos sueltos o archivo). Vive en Infrastructure: la
/// presentación no conoce el SDK ni las credenciales de Google.
/// </summary>
public static class FirebaseInitializer
{
    private static bool _initialized;

    public static void Initialize(IConfiguration configuration)
    {
        if (_initialized)
        {
            return;
        }

        var credential = GetFirebaseCredential(configuration);
        if (credential is not null)
        {
            FirebaseAdmin.FirebaseApp.Create(new FirebaseAdmin.AppOptions
            {
                Credential = credential
            });
            _initialized = true;
        }
    }

    private static GoogleCredential? GetFirebaseCredential(IConfiguration configuration)
    {
        var credentialsJson = configuration["Firebase:CredentialsJson"];
        if (!string.IsNullOrWhiteSpace(credentialsJson))
        {
            return GoogleCredential.FromJson(credentialsJson);
        }

        var credentialFields = new Dictionary<string, string?>
        {
            ["type"] = configuration["Firebase:Type"],
            ["project_id"] = configuration["Firebase:ProjectId"],
            ["private_key_id"] = configuration["Firebase:PrivateKeyId"],
            ["private_key"] = configuration["Firebase:PrivateKey"]?.Replace("\\n", "\n"),
            ["client_email"] = configuration["Firebase:ClientEmail"],
            ["client_id"] = configuration["Firebase:ClientId"],
            ["auth_uri"] = configuration["Firebase:AuthUri"],
            ["token_uri"] = configuration["Firebase:TokenUri"],
            ["auth_provider_x509_cert_url"] = configuration["Firebase:AuthProviderX509CertUrl"],
            ["client_x509_cert_url"] = configuration["Firebase:ClientX509CertUrl"],
            ["universe_domain"] = configuration["Firebase:UniverseDomain"]
        };

        if (credentialFields.Any(field => string.IsNullOrWhiteSpace(field.Value)))
        {
            return GetFirebaseCredentialFromPath(configuration);
        }

        return GoogleCredential.FromJson(System.Text.Json.JsonSerializer.Serialize(credentialFields));
    }

    private static GoogleCredential? GetFirebaseCredentialFromPath(IConfiguration configuration)
    {
        var configuredPath = configuration["Firebase:CredentialsPath"];

        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return GoogleCredential.FromFile(Path.GetFullPath(configuredPath));
        }

        var googleCredentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        return string.IsNullOrWhiteSpace(googleCredentialsPath)
            ? null
            : GoogleCredential.FromFile(Path.GetFullPath(googleCredentialsPath));
    }
}
