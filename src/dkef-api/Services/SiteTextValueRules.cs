using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

using Dkef.Configuration;

using Ganss.Xss;

namespace Dkef.Services;

public static partial class SiteTextValueRules
{
    public static string Normalize(string value) => value.Replace("\r\n", "\n").Replace('\r', '\n');

    public static bool HasText(string value) => NonWhitespace().IsMatch(value);

    // This configuration is feature-local: never mutate the shared rich-text sanitizer.
    public static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer { KeepChildNodes = true };
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedCssProperties.Clear();
        return sanitizer;
    }

    public static string Sanitize(string value, HtmlSanitizer sanitizer) =>
        // The sanitizer serializes HTML entities; decode exactly once to store actual plain text.
        // Decoded literal markup remains plain text and must only be rendered by interpolation.
        Normalize(System.Net.WebUtility.HtmlDecode(sanitizer.Sanitize(Normalize(value))));

    public static string? Validate(SiteTextFieldDefinition field, string value)
    {
        if (value.Length > field.MaxLength) return $"Teksten må højst være {field.MaxLength} tegn.";
        value = Normalize(value);
        if (field.Required && !HasText(value)) return "Feltet skal udfyldes.";
        if (field.Input == "text" && value.IndexOfAny(['\n', '\u0085', '\u2028', '\u2029']) >= 0)
            return "Feltet må ikke indeholde linjeskift.";
        if (value.Length == 0 && !field.Required) return null;
        if (field.Format == "email" && (!new EmailAddressAttribute().IsValid(value)
            || Whitespace().IsMatch(value) || !BareEmail().IsMatch(value)))
            return "Indtast en gyldig e-mailadresse uden navn eller mellemrum.";
        if (field.Format == "phone" && (!Phone().IsMatch(value) || !Digit().IsMatch(value)))
            return "Telefonnummeret må kun indeholde cifre, mellemrum, +, -, ( og ).";
        return null;
    }

    [GeneratedRegex(@"[^\t-\r \u0085\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]")]
    private static partial Regex NonWhitespace();
    [GeneratedRegex(@"[\t-\r \u0085\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]")]
    private static partial Regex Whitespace();
    [GeneratedRegex("^[^@<>(),;:\"]+@[^@<>(),;:\"]+$", RegexOptions.CultureInvariant)]
    private static partial Regex BareEmail();
    [GeneratedRegex(@"\A[0-9 +()\-]+\z")]
    private static partial Regex Phone();
    [GeneratedRegex("[0-9]")]
    private static partial Regex Digit();
}
