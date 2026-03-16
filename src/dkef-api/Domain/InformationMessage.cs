namespace Dkef.Domain;

public sealed record InformationMessage(
    string Name,
    string Phone,
    string Email,
    string Message
);