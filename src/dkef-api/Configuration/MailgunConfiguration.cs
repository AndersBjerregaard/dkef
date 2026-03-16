namespace Dkef.Configuration;

public sealed record MailgunConfiguration(
    string Domain,
    string To
);