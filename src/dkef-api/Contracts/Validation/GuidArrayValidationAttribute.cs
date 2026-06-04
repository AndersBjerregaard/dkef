using System.ComponentModel.DataAnnotations;

namespace Dkef.Contracts.Validation;

public sealed class GuidArrayValidationAttribute : ValidationAttribute
{
    public bool AllowEmpty { get; set; }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (AllowEmpty && value is null)
        {
            return ValidationResult.Success!;
        }

        if (value is null)
        {
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} cannot be null.");
        }

        if (value.GetType() != typeof(string[]))
        {
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be of type string[].");
        }

        string[] valueArray = (value as string[])!;

        if (AllowEmpty && valueArray.Length == 0)
        {
            return ValidationResult.Success!;
        }

        foreach (string guidString in valueArray)
        {
            if (!Guid.TryParse(guidString, out _))
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} all elements must be valid guids.");
            }
        }

        return ValidationResult.Success!;
    }
}