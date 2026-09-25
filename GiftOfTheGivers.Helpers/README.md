# GiftOfTheGivers.Helpers

Shared helpers for the Gift of the Givers web app and Azure Functions.

- `DonationValidator` checks a money donation (amount, currency, donation type and donor name) and returns a list of error messages. An empty list means the donation is valid.
- `CertificateNumberGenerator` creates tax certificate numbers in the format `GOTG-yyyyMMdd-XXXXXX` from the UTC issue date.

```csharp
using GiftOfTheGivers.Helpers;

var errors = DonationValidator.Validate(500m, "ZAR", "once-off", "Test Donor", anonymous: false);
if (errors.Count == 0)
{
    var certificateNumber = CertificateNumberGenerator.Create(DateTime.UtcNow);
}
```

Published to the `GiftOfTheGiversHelpers` Azure Artifacts feed in the Gift of the Givers Relief Management System project.
