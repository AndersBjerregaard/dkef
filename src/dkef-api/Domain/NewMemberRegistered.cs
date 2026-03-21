namespace Dkef.Domain;

public sealed record NewMemberRegistered(
    string FullName,
    string Email,
    string Phone
);
