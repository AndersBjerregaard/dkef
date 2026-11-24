using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using Dkef.Configuration;

namespace Dkef.Services;

/// <summary>Immutable, package-local definitions. All identifiers use exact ordinal matching.</summary>
public sealed partial class SiteTextRegistry
{
    private readonly FrozenDictionary<string, SiteTextGroupDefinition> groups;
    private readonly FrozenDictionary<string, FrozenDictionary<string, SiteTextFieldDefinition>> fields;
    public ImmutableArray<SiteTextGroupDefinition> Groups { get; }

    private SiteTextRegistry(SiteTextRegistryDefinition definition)
    {
        Groups = definition.Groups.OrderBy(group => group.Order).Select(group => group with
        {
            Sections = group.Sections.OrderBy(section => section.Order).Select(section => section with
            {
                Fields = section.Fields.OrderBy(field => field.Order).ToImmutableArray()
            }).ToImmutableArray()
        }).ToImmutableArray();
        groups = Groups.ToFrozenDictionary(group => group.Id, StringComparer.Ordinal);
        fields = Groups.ToFrozenDictionary(group => group.Id,
            group => group.Sections.SelectMany(section => section.Fields)
                .ToFrozenDictionary(field => field.Key, StringComparer.Ordinal), StringComparer.Ordinal);
    }

    public SiteTextGroupDefinition? FindGroup(string group) => groups.GetValueOrDefault(group);
    public FrozenDictionary<string, SiteTextFieldDefinition> GetFields(string group) => fields[group];

    public static SiteTextRegistry Load(string path)
    {
        try { return Parse(File.ReadAllText(path)); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"Unable to load site-text registry at {path}.", ex);
        }
    }

    public static SiteTextRegistry Parse(string json)
    {
        try
        {
            var definition = JsonSerializer.Deserialize<SiteTextRegistryDefinition>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = false,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                RespectRequiredConstructorParameters = true,
                RespectNullableAnnotations = true,
                AllowDuplicateProperties = false
            }) ?? throw new InvalidOperationException("Registry must be an object.");
            Validate(definition);
            return new SiteTextRegistry(definition);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or ArgumentException)
        {
            throw new InvalidOperationException("Invalid site-text registry: " + ex.Message, ex);
        }
    }

    private static void Validate(SiteTextRegistryDefinition definition)
    {
        CheckOrdered(definition.Groups, group => group.Id, group => group.Order, "groups");
        var sanitizer = SiteTextValueRules.CreateSanitizer();
        foreach (var group in definition.Groups)
        {
            CheckLabel(group.Label);
            CheckOrdered(group.Sections, section => section.Id, section => section.Order, group.Id);
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var section in group.Sections)
            {
                CheckLabel(section.Label);
                CheckOrdered(section.Fields, field => field.Key, field => field.Order, group.Id + "/" + section.Id);
                foreach (var field in section.Fields)
                {
                    CheckLabel(field.Label);
                    if (!keys.Add(field.Key)) throw new InvalidOperationException($"Duplicate key: {group.Id}/{field.Key}.");
                    if (field.Input is not ("text" or "multiline") || field.Format is not ("plain" or "email" or "phone")
                        || field.MaxLength < 1)
                        throw new InvalidOperationException($"Invalid field metadata: {group.Id}/{field.Key}.");
                    if (field.Default is null || field.Default.Contains('\r')
                        || SiteTextValueRules.Validate(field, field.Default) is not null
                        || SiteTextValueRules.Sanitize(field.Default, sanitizer) != field.Default)
                        throw new InvalidOperationException($"Invalid plain-text default: {group.Id}/{field.Key}.");
                }
            }
        }
    }

    private static void CheckOrdered<T>(ImmutableArray<T> values, Func<T, string> id, Func<T, int> order, string scope)
    {
        if (values.IsDefaultOrEmpty) throw new InvalidOperationException($"Empty definitions: {scope}.");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var orders = new HashSet<int>();
        foreach (var value in values)
        {
            if (value is null || id(value) is not { } identifier || !Identifier().IsMatch(identifier)
                || !ids.Add(identifier) || order(value) < 1 || !orders.Add(order(value)))
                throw new InvalidOperationException($"Invalid/duplicate identifier or order: {scope}.");
        }
    }

    private static void CheckLabel(string label)
    {
        if (label is null || !SiteTextValueRules.HasText(label)) throw new InvalidOperationException("Empty editor label.");
    }

    [GeneratedRegex(@"\A[a-z][a-z0-9]*(?:[.-][a-z0-9]+)*\z", RegexOptions.CultureInvariant)]
    private static partial Regex Identifier();
}
