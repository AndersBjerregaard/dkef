using Dkef.Domain;

namespace Dkef.Services.Interfaces;

public interface IEmailService
{
    ValueTask SendContactInquiryAsync(InformationMessage message);
    ValueTask SendChangeEmailAsync(ChangeEmailRequest request);
    ValueTask SendResetPasswordAsync(ResetPasswordRequest request);
}
