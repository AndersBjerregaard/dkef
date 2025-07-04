namespace Dkef.Configuration;

public class DomainUrlConfiguration(string baseUrl)
{
    public string BaseUrl { get; } = baseUrl;
}