using System.ComponentModel.DataAnnotations;

namespace Dkef.Contracts.Validation;

public class GuidValidationAttribute : ValidationAttribute
{
    public bool AllowEmpty { get; set; }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (AllowEmpty && string.IsNullOrEmpty(value as string))
        {
            return ValidationResult.Success!;
        }

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
