using Ganss.Xss;

namespace Dkef.Contracts;

public abstract class PostObject
{
    /// <summary>
    /// Uses <see cref="Ganss.Xss.HtmlSanitizer"/> in order to sanitize
    /// any malicious injected input.
    /// </summary>
    public abstract void Sanitize(HtmlSanitizer sanitizer);
}