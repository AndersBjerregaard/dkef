namespace Dkef.Domain;

public sealed record ChangeEmailRequest(
    string OldEmail,
    string NewEmail,
    string Name,
    string ConfirmLink,
    string RevokeLink
);
