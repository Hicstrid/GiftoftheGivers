using System.Globalization;
using System.Text.Json;
using GiftOfTheGivers.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GiftOfTheGivers.Functions.Functions;

public class GenerateTaxCertificate
{
    private static readonly string[] AllowedCurrencies = { "ZAR", "USD", "EUR" };
    private static readonly string[] AllowedDonationTypes = { "once-off", "recurring" };
    private const decimal MaxAmount = 1_000_000m;
    private const int MaxNameLength = 100;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly ILogger<GenerateTaxCertificate> _logger;

    public GenerateTaxCertificate(ILogger<GenerateTaxCertificate> logger)
    {
        _logger = logger;
    }

    // POST with a JSON body, or GET with query string values so it can be tested from a browser
    [Function(nameof(GenerateTaxCertificate))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("Tax certificate request received ({Method}).", req.Method);

        try
        {
            var errors = new List<string>();
            TaxCertificateRequest? request;

            if (HttpMethods.IsPost(req.Method))
            {
                request = await ReadFromBodyAsync(req, errors);
            }
            else
            {
                request = ReadFromQuery(req.Query, errors);
            }

            // Only validate the values once the input itself could be read
            if (request != null && errors.Count == 0)
            {
                Validate(request, errors);
            }

            if (errors.Count > 0 || request == null)
            {
                _logger.LogWarning("Tax certificate request rejected: {Errors}", string.Join(" ", errors));
                return new BadRequestObjectResult(new { message = "Invalid certificate request.", errors });
            }

            var issuedDate = DateTime.UtcNow;
            var response = new TaxCertificateResponse
            {
                CertificateNumber = CreateCertificateNumber(issuedDate),
                DonorName = request.Anonymous ? "Anonymous" : request.DonorName!.Trim(),
                Amount = decimal.Round(request.Amount!.Value, 2),
                Currency = request.Currency!.Trim().ToUpperInvariant(),
                DonationType = request.DonationType!.Trim().ToLowerInvariant(),
                IssuedDate = issuedDate
            };

            _logger.LogInformation("Issued certificate {CertificateNumber} for {Currency} {Amount} ({DonationType}).",
                response.CertificateNumber, response.Currency, response.Amount, response.DonationType);

            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while generating a tax certificate.");
            return new ObjectResult(new { message = "The certificate could not be generated. Please try again later." })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    private async Task<TaxCertificateRequest?> ReadFromBodyAsync(HttpRequest req, List<string> errors)
    {
        using var reader = new StreamReader(req.Body);
        var body = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
        {
            errors.Add("Request body is empty. Send a JSON object with donorName, amount, currency and donationType.");
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<TaxCertificateRequest>(body, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("Could not parse request body: {Error}", ex.Message);
            errors.Add("Request body is not valid JSON or has a value of the wrong type (amount must be a number, anonymous must be true/false).");
            return null;
        }
    }

    private static TaxCertificateRequest ReadFromQuery(IQueryCollection query, List<string> errors)
    {
        var request = new TaxCertificateRequest
        {
            DonorName = query["donorName"],
            Currency = query["currency"],
            DonationType = query["donationType"]
        };

        string? amount = query["amount"];
        if (!string.IsNullOrWhiteSpace(amount))
        {
            if (decimal.TryParse(amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedAmount))
            {
                request.Amount = parsedAmount;
            }
            else
            {
                errors.Add($"Amount '{amount}' is not a valid number.");
            }
        }

        string? anonymous = query["anonymous"];
        if (!string.IsNullOrWhiteSpace(anonymous))
        {
            if (bool.TryParse(anonymous, out var parsedAnonymous))
            {
                request.Anonymous = parsedAnonymous;
            }
            else
            {
                errors.Add("Anonymous must be true or false.");
            }
        }

        return request;
    }

    private static void Validate(TaxCertificateRequest request, List<string> errors)
    {
        if (request.Amount == null)
        {
            errors.Add("Amount is required.");
        }
        else if (request.Amount <= 0)
        {
            errors.Add("Amount must be greater than zero.");
        }
        else if (request.Amount > MaxAmount)
        {
            errors.Add($"Amount cannot be more than {MaxAmount.ToString("N0", CultureInfo.InvariantCulture)}.");
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            errors.Add("Currency is required (ZAR, USD or EUR).");
        }
        else if (!AllowedCurrencies.Contains(request.Currency.Trim().ToUpperInvariant()))
        {
            errors.Add($"Currency '{request.Currency}' is not supported. Use ZAR, USD or EUR.");
        }

        if (string.IsNullOrWhiteSpace(request.DonationType))
        {
            errors.Add("Donation type is required (once-off or recurring).");
        }
        else if (!AllowedDonationTypes.Contains(request.DonationType.Trim().ToLowerInvariant()))
        {
            errors.Add($"Donation type '{request.DonationType}' is not supported. Use once-off or recurring.");
        }

        if (!request.Anonymous)
        {
            if (string.IsNullOrWhiteSpace(request.DonorName))
            {
                errors.Add("Donor name is required unless the donation is anonymous.");
            }
            else if (request.DonorName.Trim().Length > MaxNameLength)
            {
                errors.Add($"Donor name cannot be longer than {MaxNameLength} characters.");
            }
        }
    }

    // Format: GOTG-yyyyMMdd-XXXXXX
    private static string CreateCertificateNumber(DateTime issuedDate)
    {
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"GOTG-{issuedDate:yyyyMMdd}-{suffix}";
    }
}
