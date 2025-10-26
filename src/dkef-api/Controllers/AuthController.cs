using Dkef.Contracts;
using Dkef.Domain;
using Dkef.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dkef.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    ForgotPasswordRepository _forgotPasswordRepository,
    IContactRepository _contactRepository,
    UserManager<Contact> _userManager
) : ControllerBase
{
    [HttpPost]
    [Route("forgot")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        Contact? contact = await _contactRepository.GetByEmailAsync(dto.Email);
        if (contact is null)
        {
            return NotFound("No user found with the provided email.");
        }

        // Generate a password reset token using the custom DatabaseTokenProvider
        string token = await _userManager.GeneratePasswordResetTokenAsync(contact);

        // The token is the GUID string that can be used to reset the password
        // You would typically send this token via email to the user
        return CreatedAtAction(
            nameof(GetForgotPasswordRequest),
            new { id = token },
            new { message = "Password reset token generated.", token = token }
        );
    }

    [HttpGet]
    [Route("forgot/{id}", Name = nameof(GetForgotPasswordRequest))]
    public async Task<IActionResult> GetForgotPasswordRequest(Guid id)
    {
        ForgotPassword? request = await _forgotPasswordRepository.GetByIdAsync(id);

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
        Contact? contact = await _contactRepository.GetByEmailAsync(dto.Email);
        if (contact is null)
        {
            return BadRequest("Invalid password reset request.");
        }

        // Use UserManager's ResetPasswordAsync which will automatically validate the token
        // using our DatabaseTokenProvider
        IdentityResult result = await _userManager.ResetPasswordAsync(contact, dto.Token, dto.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Failed to reset password.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        // Mark the token as used in the database
        if (Guid.TryParse(dto.Token, out Guid tokenId))
        {
            await _forgotPasswordRepository.SetAsUsedAsync(tokenId);
        }

        return Ok(new { message = "Password has been reset successfully." });
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        Contact? contact = await _contactRepository.GetByEmailAsync(dto.Email);
        if (contact is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(contact, dto.Password);
        if (!passwordValid)
        {
            return Unauthorized("Invalid email or password.");
        }

        // Here you would typically generate a JWT or similar token for the authenticated user
        return Ok(new { message = "Login successful." });
    }

    [HttpPost]
    [Route("change")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        Contact? contact = await _contactRepository.GetByEmailAsync(dto.Email);
        if (contact is null)
        {
            return NotFound("No user found with the provided email.");
        }

        IdentityResult result = await _userManager.ChangePasswordAsync(contact, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Failed to change password.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        return Ok(new { message = "Password changed successfully." });
    }
}
