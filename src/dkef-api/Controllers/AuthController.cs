using Dkef.Configuration;
using Dkef.Contracts;
using Dkef.Domain;
using Dkef.Repositories;
using Dkef.Services.Interfaces;

using Ganss.Xss;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    ForgotPasswordRepository forgotPasswordRepository,
    UserManager<Contact> userManager,
    IJwtService jwtService,
    IEmailService emailService,
    HostConfig hostConfig,
    HtmlSanitizer sanitizer,
    Serilog.ILogger logger
) : ControllerBase
{
    [HttpPost]
    [Route("forgot")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        Contact? contact = await userManager.FindByEmailAsync(dto.Email);

        if (contact is null)
        {
            logger.Warning("Forgot password attempt with invalid email: {Email}", dto.Email);
            return Ok();
        }

        // Generate a password reset token using the custom DatabaseTokenProvider
        string token = await userManager.GeneratePasswordResetTokenAsync(contact);

        string resetUrl = $"{hostConfig.Audience}/reset-password?token={token}";

        var resetRequest = new ResetPasswordRequest(
            Email: contact.Email!,
            Name: contact.Name,
            ChangeLink: resetUrl
        );

        // Don't await email task - fire and forget
        _ = emailService.SendResetPasswordAsync(resetRequest);

        return Ok();
    }

    [HttpGet]
    [Route("forgot/{id}", Name = nameof(GetForgotPasswordRequest))]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetForgotPasswordRequest(Guid id)
    {
        ForgotPassword? request = await forgotPasswordRepository.GetByIdAsync(id);

        if (request is null)
        {
            return NotFound("Forgot password request not found.");
        }

        if (request.IsUsed || !request.IsValid)
        {
            return BadRequest("The password reset token is either used or expired.");
        }

        return Ok(request);
    }

    [HttpPost]
    [Route("reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        // Parse the token as a GUID to look up the password reset request
        if (!Guid.TryParse(dto.Token, out Guid tokenId))
        {
            return BadRequest("Invalid password reset token format.");
        }

        // Look up the forgot password request by token
        ForgotPassword? forgotPasswordRequest = await forgotPasswordRepository.GetByIdAsync(tokenId);
        if (forgotPasswordRequest is null)
        {
            return BadRequest("Invalid password reset request.");
        }

        // Validate the token is still valid
        if (forgotPasswordRequest.IsUsed || !forgotPasswordRequest.IsValid)
        {
            return BadRequest("The password reset token is either used or expired.");
        }

        // Get the contact associated with the token
        Contact? contact = await userManager.FindByIdAsync(forgotPasswordRequest.ContactId.ToString());
        if (contact is null)
        {
            return BadRequest("Invalid password reset request.");
        }

        // Use UserManager's ResetPasswordAsync which will automatically validate the token
        // using our DatabaseTokenProvider
        IdentityResult result = await userManager.ResetPasswordAsync(contact, dto.Token, dto.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Failed to reset password.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        // Mark the token as used in the database
        await forgotPasswordRepository.SetAsUsedAsync(tokenId);

        return Ok(new { message = "Password has been reset successfully." });
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        Contact? contact = await userManager.FindByEmailAsync(dto.Email);
        if (contact is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var passwordValid = await userManager.CheckPasswordAsync(contact, dto.Password);
        if (!passwordValid)
        {
            return Unauthorized("Invalid email or password.");
        }

        // Generate JWT token and refresh token for the authenticated user
        var tokens = await jwtService.GenerateTokensAsync(contact);

        return Ok(new
        {
            message = "Login successful.",
            accessToken = tokens.AccessToken,
            refreshToken = tokens.RefreshToken,
            expiresIn = tokens.ExpiresIn
        });
    }

    [HttpPost]
    [Route("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        try
        {
            var tokens = await jwtService.RefreshTokenAsync(dto.RefreshToken);

            return Ok(new
            {
                message = "Token refreshed successfully.",
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken,
                expiresIn = tokens.ExpiresIn
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occurred while refreshing the token.", details = ex.Message });
        }
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        // Check if user with email already exists
        Contact? existingContact = await userManager.FindByEmailAsync(dto.Email);
        if (existingContact is not null)
        {
            return BadRequest("A user with this email already exists.");
        }

        // Create new contact
        var contact = new Contact
        {
            UserName = dto.Email,
            Email = dto.Email,
            Name = sanitizer.Sanitize(dto.Name),
            PrimarySection = dto.PrimarySection,
            SecondarySection = dto.SecondarySection,
            Title = sanitizer.Sanitize(dto.Title),
            Address = sanitizer.Sanitize(dto.Address),
            ZIP = sanitizer.Sanitize(dto.ZIP),
            City = sanitizer.Sanitize(dto.City),
            PrivatePhoneNumber = sanitizer.Sanitize(dto.Phone),
            PhoneNumber = sanitizer.Sanitize(dto.Phone),
            EmploymentStatus = sanitizer.Sanitize(dto.EmploymentStatus),
            MagazineDelivery = sanitizer.Sanitize(dto.MagazineDelivery),
            Subscription = sanitizer.Sanitize(dto.Subscription),
            CompanyName = sanitizer.Sanitize(dto.CompanyName),
            CompanyAddress = sanitizer.Sanitize(dto.CompanyAddress),
            CompanyZIP = sanitizer.Sanitize(dto.CompanyZIP),
            CompanyCity = sanitizer.Sanitize(dto.CompanyCity),
            CompanyPhone = sanitizer.Sanitize(dto.CompanyPhone),
            CVRNumber = sanitizer.Sanitize(dto.CVRNumber),
            EANNumber = sanitizer.Sanitize(dto.EANNumber),
            EnrollmentDate = DateTime.UtcNow
        };

        // Create user with password using UserManager
        IdentityResult result = await userManager.CreateAsync(contact, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Failed to register user.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        // Send 'new registration' email to secretary
        var newMemberRegistered = new NewMemberRegistered(
            FullName: contact.Name,
            Email: contact.Email,
            PrimarySection: contact.PrimarySection.Value,
            Phone: contact.PhoneNumber,
            Title: contact.Title,
            EmploymentStatus: contact.EmploymentStatus,
            Address: contact.Address,
            ZIP: contact.ZIP,
            City: contact.City,
            SecondarySection: contact.SecondarySection,
            MagazineDelivery: contact.MagazineDelivery,
            Subscription: contact.Subscription,
            CompanyName: contact.CompanyName,
            CompanyAddress: contact.CompanyAddress,
            CompanyZIP: contact.CompanyZIP,
            CompanyCity: contact.CompanyCity,
            CompanyPhone: contact.CompanyPhone,
            CVRNumber: contact.CVRNumber,
            EAANNumber: contact.EANNumber
        );

        // Don't await email task - fire and forget
        _ = emailService.SendNewMemberRegisteredAsync(newMemberRegistered);

        // Generate JWT token and refresh token for the newly registered user
        var tokens = await jwtService.GenerateTokensAsync(contact);

        return CreatedAtAction(
            nameof(Register),
            new
            {
                message = "Registration successful.",
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken,
                expiresIn = tokens.ExpiresIn
            }
        );
    }

    [HttpPost]
    [Route("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
    {
        try
        {
            await jwtService.RevokeRefreshTokenAsync(dto.RefreshToken);
            return Ok(new { message = "Logout successful. Refresh token has been revoked." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "An error occurred during logout.", details = ex.Message });
        }
    }
}
