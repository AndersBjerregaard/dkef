using Dkef.Configuration;
using Dkef.Domain;
using Dkef.Services.Interfaces;

namespace Dkef.Services;

public sealed class MicrosoftGraphEmailService(
) : IEmailService
{
    public ValueTask SendChangeEmailAsync(ChangeEmailRequest request)
    {
        throw new NotImplementedException();
    }

    public ValueTask SendContactInquiryAsync(InformationMessage message)
    {
        throw new NotImplementedException();
    }

    public ValueTask SendNewMemberRegisteredAsync(NewMemberRegistered request)
    {
        throw new NotImplementedException();
    }

    public ValueTask SendResetPasswordAsync(ResetPasswordRequest request)
    {
        throw new NotImplementedException();
    }
}
