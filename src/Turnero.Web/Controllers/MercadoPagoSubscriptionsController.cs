using System.Net.Http;
using System.Text.Json;
using Turnero.Infrastructure.Data;
using Turnero.Infrastructure.MercadoPago;

namespace Turnero.Web.Controllers;

public class MercadoPagoSubscriptionsController(
    ApplicationDbContext db,
    UserManager<IdentityUser> userManager,
    MercadoPagoSubscriptionService mercadoPago,
    IOptions<MercadoPagoOptions> options,
    IHostEnvironment environment,
    ILogger<MercadoPagoSubscriptionsController> logger) : Controller
{
    private readonly MercadoPagoOptions settings = options.Value;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole(RolesConstants.Admin);
        var currentUserId = userManager.GetUserId(User);
        var query = db.ApplicationSubscriptions.AsNoTracking();
        if (!isAdmin)
        {
            query = query.Where(subscription => subscription.UserId == currentUserId);
        }

        var subscriptions = await query
            .OrderByDescending(subscription => subscription.CreatedAt)
            .ToListAsync(cancellationToken);

        var userIds = subscriptions.Select(subscription => subscription.UserId).Distinct().ToArray();
        var userEmails = isAdmin
            ? await userManager.Users
                .Where(user => userIds.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id, user => user.Email ?? string.Empty, cancellationToken)
            : new Dictionary<string, string>();

        var hasOpenSubscription = await db.ApplicationSubscriptions.AnyAsync(
            subscription => subscription.UserId == currentUserId
                && (subscription.Status == "creating"
                    || subscription.Status == "creation_unknown"
                    || subscription.Status == "pending"
                    || subscription.Status == "authorized"
                    || subscription.Status == "paused"),
            cancellationToken);

        var model = new MercadoPagoSubscriptionsViewModel
        {
            MonthlyAmount = settings.MonthlyAmount,
            CurrencyId = settings.CurrencyId,
            IsAdmin = isAdmin,
            CanCreate = !hasOpenSubscription,
            Subscriptions = subscriptions.Select(subscription => new MercadoPagoSubscriptionRowViewModel
            {
                Id = subscription.Id,
                UserEmail = isAdmin ? userEmails.GetValueOrDefault(subscription.UserId) : null,
                Status = subscription.Status,
                MercadoPagoId = subscription.MercadoPagoId,
                CheckoutUrl = subscription.CheckoutUrl,
                Amount = subscription.Amount,
                CurrencyId = subscription.CurrencyId,
                CreatedAt = subscription.CreatedAt,
                UpdatedAt = subscription.UpdatedAt
            }).ToArray()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return BadRequest("La cuenta autenticada debe tener un correo electrónico.");
        }

        try
        {
            mercadoPago.ValidateCreateConfiguration();
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "Mercado Pago subscription checkout is not configured.");
            TempData["Message"] = "La configuración de Mercado Pago está incompleta. Contacta al administrador.";
            return RedirectToAction(nameof(Index));
        }

        var alreadyHasOpenSubscription = await db.ApplicationSubscriptions.AnyAsync(
            subscription => subscription.UserId == user.Id
                && (subscription.Status == "creating"
                    || subscription.Status == "creation_unknown"
                    || subscription.Status == "pending"
                    || subscription.Status == "authorized"
                    || subscription.Status == "paused"),
            cancellationToken);
        if (alreadyHasOpenSubscription)
        {
            TempData["Message"] = "Ya existe una suscripción en curso o autorizada para esta cuenta.";
            return RedirectToAction(nameof(Index));
        }

        var externalReference = Guid.NewGuid().ToString("N");
        var subscriptionRecord = new ApplicationSubscription
        {
            UserId = user.Id,
            ExternalReference = externalReference,
            Status = "creating",
            Amount = settings.MonthlyAmount,
            CurrencyId = settings.CurrencyId
        };

        db.ApplicationSubscriptions.Add(subscriptionRecord);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var concurrentSubscriptionExists = await db.ApplicationSubscriptions.AnyAsync(
                subscription => subscription.UserId == user.Id
                    && (subscription.Status == "creating"
                        || subscription.Status == "creation_unknown"
                        || subscription.Status == "pending"
                        || subscription.Status == "authorized"
                        || subscription.Status == "paused"),
                cancellationToken);
            if (!concurrentSubscriptionExists)
            {
                throw;
            }

            TempData["Message"] = "Ya existe una suscripción en curso o autorizada para esta cuenta.";
            return RedirectToAction(nameof(Index));
        }

        var payerEmail = environment.IsDevelopment()
            && !string.IsNullOrWhiteSpace(settings.TestPayerEmail)
                ? settings.TestPayerEmail
                : user.Email;

        try
        {
            var preapproval = await mercadoPago.CreateAsync(
                externalReference,
                payerEmail,
                cancellationToken);

            subscriptionRecord.MercadoPagoId = preapproval.Id;
            subscriptionRecord.Status = preapproval.Status;
            subscriptionRecord.CheckoutUrl = preapproval.InitPoint;
            subscriptionRecord.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            return Redirect(preapproval.InitPoint!);
        }
        catch (MercadoPagoApiException exception)
        {
            subscriptionRecord.Status = (int)exception.StatusCode >= 500
                ? "creation_unknown"
                : "creation_failed";
            subscriptionRecord.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            logger.LogWarning(
                "Mercado Pago subscription creation returned HTTP {StatusCode}.",
                (int)exception.StatusCode);
            TempData["Message"] = (int)exception.StatusCode >= 500
                ? "Mercado Pago no confirmó el resultado de la operación. No vuelvas a solicitarla hasta comprobar el estado."
                : $"Mercado Pago rechazó la operación (HTTP {(int)exception.StatusCode}).";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException exception)
        {
            subscriptionRecord.Status = "creation_unknown";
            subscriptionRecord.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            logger.LogWarning(exception, "Mercado Pago subscription creation could not be confirmed.");
            TempData["Message"] = "No se pudo confirmar el resultado con Mercado Pago. No vuelvas a solicitarla hasta comprobar el estado.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidDataException exception)
        {
            subscriptionRecord.Status = "creation_unknown";
            subscriptionRecord.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            logger.LogError(exception, "Mercado Pago returned an invalid subscription response.");
            TempData["Message"] = "Mercado Pago devolvió una respuesta inesperada. Contacta al administrador antes de reintentar.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Return(string? externalReference, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(externalReference))
        {
            return BadRequest();
        }

        var subscription = await db.ApplicationSubscriptions
            .SingleOrDefaultAsync(
                record => record.ExternalReference == externalReference,
                cancellationToken);
        if (subscription is null || !CanManage(subscription))
        {
            return NotFound();
        }

        if (await RefreshStatusAsync(subscription, cancellationToken))
        {
            TempData["Message"] = "El estado de la suscripción se actualizó desde Mercado Pago.";
        }
        else if (TempData["Message"] is null)
        {
            TempData["Message"] = "No se pudo verificar la suscripción. El estado no fue modificado.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refresh(Guid id, CancellationToken cancellationToken)
    {
        var subscription = await db.ApplicationSubscriptions
            .SingleOrDefaultAsync(record => record.Id == id, cancellationToken);
        if (subscription is null || !CanManage(subscription))
        {
            return NotFound();
        }

        if (await RefreshStatusAsync(subscription, cancellationToken))
        {
            TempData["Message"] = "El estado de la suscripción se actualizó desde Mercado Pago.";
        }
        else if (TempData["Message"] is null)
        {
            TempData["Message"] = "No se pudo verificar la suscripción. El estado no fue modificado.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [RequestSizeLimit(16_384)]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        var dataId = Request.Query["data.id"].FirstOrDefault();
        var notificationType = Request.Query["type"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(notificationType))
        {
            notificationType = Request.Query["topic"].FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(notificationType))
        {
            try
            {
                using var payload = await JsonDocument.ParseAsync(Request.Body, cancellationToken: cancellationToken);
                if (payload.RootElement.TryGetProperty("type", out var typeValue)
                    && typeValue.ValueKind == JsonValueKind.String)
                {
                    notificationType = typeValue.GetString();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (JsonException)
            {
                return BadRequest();
            }
        }

        if (!string.Equals(notificationType, "subscription_preapproval", StringComparison.Ordinal))
        {
            return Ok();
        }

        if (string.IsNullOrWhiteSpace(settings.WebhookSecret))
        {
            logger.LogError("Mercado Pago webhook secret is not configured.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        if (string.IsNullOrWhiteSpace(dataId))
        {
            return BadRequest();
        }

        if (!MercadoPagoWebhookSignature.IsValid(
                Request.Headers["x-signature"].FirstOrDefault(),
                Request.Headers["x-request-id"].FirstOrDefault(),
                dataId,
                settings.WebhookSecret))
        {
            return Unauthorized();
        }

        var preapproval = await mercadoPago.GetAsync(dataId, cancellationToken);
        if (!string.Equals(preapproval.Id, dataId, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(preapproval.ExternalReference))
        {
            logger.LogWarning("Mercado Pago webhook returned a mismatched subscription identifier.");
            return Ok();
        }

        var subscription = await db.ApplicationSubscriptions.SingleOrDefaultAsync(
            record => record.ExternalReference == preapproval.ExternalReference,
            cancellationToken);
        if (subscription is null)
        {
            logger.LogWarning("Received a Mercado Pago webhook without a matching local subscription.");
            return Ok();
        }

        if (subscription.MercadoPagoId is not null
            && !string.Equals(subscription.MercadoPagoId, preapproval.Id, StringComparison.Ordinal))
        {
            logger.LogError("Mercado Pago webhook identifier did not match the stored subscription.");
            return Ok();
        }

        subscription.MercadoPagoId = preapproval.Id;
        subscription.Status = preapproval.Status;
        subscription.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    private bool CanManage(ApplicationSubscription subscription) =>
        User.IsInRole(RolesConstants.Admin)
        || string.Equals(
            subscription.UserId,
            userManager.GetUserId(User),
            StringComparison.Ordinal);

    private async Task<bool> RefreshStatusAsync(
        ApplicationSubscription subscription,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subscription.MercadoPagoId))
        {
            return false;
        }

        try
        {
            var preapproval = await mercadoPago.GetAsync(subscription.MercadoPagoId, cancellationToken);
            if (!string.Equals(preapproval.Id, subscription.MercadoPagoId, StringComparison.Ordinal)
                || !string.Equals(
                    preapproval.ExternalReference,
                    subscription.ExternalReference,
                    StringComparison.Ordinal))
            {
                logger.LogWarning("Mercado Pago returned a subscription that did not match the local record.");
                return false;
            }

            subscription.Status = preapproval.Status;
            subscription.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (MercadoPagoApiException exception)
        {
            logger.LogWarning(
                "Mercado Pago subscription lookup returned HTTP {StatusCode}.",
                (int)exception.StatusCode);
            TempData["Message"] = $"Mercado Pago respondió HTTP {(int)exception.StatusCode} al consultar la suscripción.";
            return false;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Mercado Pago subscription lookup could not be completed.");
            TempData["Message"] = "No se pudo conectar con Mercado Pago para consultar la suscripción.";
            return false;
        }
        catch (InvalidDataException exception)
        {
            logger.LogWarning(exception, "Mercado Pago returned an invalid subscription lookup response.");
            TempData["Message"] = "Mercado Pago devolvió una respuesta inesperada al consultar la suscripción.";
            return false;
        }
    }
}
