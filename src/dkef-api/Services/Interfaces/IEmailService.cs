using Dkef.Domain;

namespace Dkef.Services.Interfaces;

public interface IEmailService
{
    Task SendContactInquiryAsync(InformationMessage message);
    Task SendChangeEmailAsync(ChangeEmailRequest request);
    Task SendResetPasswordAsync(ResetPasswordRequest request);
    Task SendNewMemberRegisteredAsync(NewMemberRegistered request);
}
