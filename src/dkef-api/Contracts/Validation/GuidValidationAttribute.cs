using System.ComponentModel.DataAnnotations;

namespace Dkef.Contracts.Validation;

public class GuidValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not null && value is string guidString)
        {
            if (Guid.TryParse(guidString, out _))
            {
                return ValidationResult.Success!;
            }
        }
        return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be a valid GUID.");
    }
}