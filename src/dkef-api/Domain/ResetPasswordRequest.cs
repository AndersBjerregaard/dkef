namespace Dkef.Domain;

public sealed record ResetPasswordRequest(string Email, string Name, string ChangeLink);
