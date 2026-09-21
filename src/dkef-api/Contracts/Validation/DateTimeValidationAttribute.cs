using System.ComponentModel.DataAnnotations;

using Dkef.Utilities;

namespace Dkef.Contracts.Validation;

public class DateTimeValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string dateTimeString && CopenhagenDateTime.TryParseToUtc(dateTimeString, out _))
        {
            return ValidationResult.Success!;
        }

        return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be a valid date and time.");
    }
}
