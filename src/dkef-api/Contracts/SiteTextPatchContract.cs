using System.Collections.Frozen;
using System.Text.Json;

using Dkef.Services;

using Microsoft.AspNetCore.Mvc;

namespace Dkef.Contracts;

/// <summary>Pure wire-contract checks for reuse by the future controller; no persistence or authorization.</summary>
public static class SiteTextPatchContract
{
    public const string RequestErrorKey = "request";

    // Six bytes cover a JSON \uXXXX per UTF-16 unit. Reserve envelope/punctuation and bounded whitespace.
    // Item 04 must enforce this on the stream, not just Content-Length, before buffering/parsing.
    public static long MaxRequestBytes(SiteTextRegistry registry, string group) =>
        1024L + registry.GetFields(group).Values.Sum(field => 6L * (field.Key.Length + field.MaxLength) + 8);

    public static SiteTextPatchValidationResult Validate(
        ReadOnlyMemory<byte> utf8Json, SiteTextRegistry registry, string group)
    {
        if (registry.FindGroup(group) is null) throw new ArgumentException("Unknown site-text group.", nameof(group));
        if (utf8Json.Length > MaxRequestBytes(registry, group))
            return Invalid(RequestErrorKey, "Anmodningen er for stor.");
        try
        {
            using var document = JsonDocument.Parse(utf8Json, new JsonDocumentOptions { MaxDepth = 4 });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return EnvelopeError();
            var members = root.EnumerateObject().ToArray();
            if (members.Length != 1 || ReadJsonString(() => members[0].Name) != "changes"
                || members[0].Value.ValueKind != JsonValueKind.Object) return EnvelopeError();
            var changes = members[0].Value.EnumerateObject().ToArray();
            var fields = registry.GetFields(group);
            if (changes.Length == 0 || changes.Length > fields.Count) return EnvelopeError();
            var submitted = new Dictionary<string, string>(StringComparer.Ordinal);
            var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var change in changes)
            {
                var key = ReadJsonString(() => change.Name);
                if (!seen.Add(key)) return EnvelopeError();
                var path = "changes." + key;
                if (!fields.TryGetValue(key, out var field))
                    errors[path] = ["Feltet findes ikke i denne tekstgruppe."];
                else if (change.Value.ValueKind != JsonValueKind.String)
                    errors[path] = ["Værdien skal være tekst."];
                else
                {
                    var value = ReadJsonString(() => change.Value.GetString()!);
                    // Raw length is checked before stripping markup or normalizing CRLF.
                    if (value.Length > field.MaxLength)
                        errors[path] = [$"Teksten må højst være {field.MaxLength} tegn."];
                    else submitted.Add(key, value);
                }
            }
            if (errors.Count != 0) return new(null, errors);
            var request = new SiteTextPatchRequest(submitted);
            request.Sanitize(SiteTextValueRules.CreateSanitizer());
            foreach (var (key, value) in request.Changes)
                if (SiteTextValueRules.Validate(fields[key], value) is { } error)
                    errors["changes." + key] = [error];
            return errors.Count == 0 ? new(request, errors) : new(null, errors);
        }
        catch (JsonException) { return EnvelopeError(); }
    }

    private static string ReadJsonString(Func<string> read)
    {
        // JsonDocument defers decoding strings; invalid escaped surrogates/UTF-8 can throw here,
        // rather than during Parse. Treat only these JSON access failures as malformed input.
        try { return read(); }
        catch (InvalidOperationException ex) { throw new JsonException("Invalid JSON string encoding.", ex); }
    }

    private static SiteTextPatchValidationResult EnvelopeError() =>
        Invalid(RequestErrorKey, "Anmodningen skal indeholde ét ikke-tomt changes-objekt med entydige felter.");

    private static SiteTextPatchValidationResult Invalid(string key, string error) =>
        new(null, new Dictionary<string, string[]> { [key] = [error] });
}

public sealed class SiteTextPatchValidationResult(
    SiteTextPatchRequest? request, IDictionary<string, string[]> errors)
{
    public SiteTextPatchRequest? Request { get; } = request;
    public FrozenDictionary<string, string[]> Errors { get; } = errors.ToFrozenDictionary(StringComparer.Ordinal);

    public ValidationProblemDetails ToProblemDetails() => new(Errors.ToDictionary(pair => pair.Key, pair => pair.Value))
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "En eller flere tekster er ugyldige."
    };
}
