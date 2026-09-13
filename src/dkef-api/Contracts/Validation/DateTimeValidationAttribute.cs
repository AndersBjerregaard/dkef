using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Dkef.Contracts.Validation;

public class DateTimeValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success!;
        }

        if (value is string dateTimeString)
        {
            if (string.IsNullOrWhiteSpace(dateTimeString))
            {
                return ValidationResult.Success!;
            }

            // You can specify different DateTimeStyles or CultureInfo for more robust parsing
            if (System.DateTime.TryParse(dateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                return ValidationResult.Success!;
            }
        }
        return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be a valid date and time.");
    }
}
