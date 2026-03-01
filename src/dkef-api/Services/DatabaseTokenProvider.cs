using Dkef.Domain;
using Dkef.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Dkef.Services;

/// <summary>
/// Custom token provider that uses the database (via ForgotPasswordRepository) 
/// to generate and validate password reset tokens instead of in-memory cache.
/// </summary>
public class DatabaseTokenProvider : IUserTwoFactorTokenProvider<Contact>
{
    private readonly ForgotPasswordRepository _forgotPasswordRepository;

    public DatabaseTokenProvider(ForgotPasswordRepository forgotPasswordRepository)
    {
        _forgotPasswordRepository = forgotPasswordRepository;
    }

    public async Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<Contact> manager, Contact user)
    {
        // This provider can always generate tokens for password reset
        return await Task.FromResult(true);
    }

    public async Task<string> GenerateAsync(string purpose, UserManager<Contact> manager, Contact user)
    {
        if (purpose != "ResetPassword")
        {
            throw new NotSupportedException($"Purpose '{purpose}' is not supported by DatabaseTokenProvider.");
        }

        // Create a new forgot password request in the database
        // Only set the ContactId (foreign key), not the Contact navigation property
        // to avoid EF Core trying to insert the Contact entity
        ForgotPassword forgotPasswordRequest = new()
        {
            ContactId = user.Id
        };

        ForgotPassword createdRequest = await _forgotPasswordRepository.CreateAsync(forgotPasswordRequest);

        // Return the GUID as the token
        return createdRequest.Id.ToString();
    }

    public async Task<bool> ValidateAsync(string purpose, string token, UserManager<Contact> manager, Contact user)
    {
        if (purpose != "ResetPassword")
        {
            return false;
        }

        // Try to parse the token as a GUID
        if (!Guid.TryParse(token, out Guid tokenId))
        {
            return false;
        }

        // Retrieve the forgot password request from the database
        ForgotPassword? request = await _forgotPasswordRepository.GetByIdAsync(tokenId);

        if (request is null)
        {
            return false;
        }

        // Validate the token: must belong to the user and still be valid
        return request.ContactId == user.Id && request.IsValid;
    }
}
