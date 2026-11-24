using System.Text.Json.Nodes;

using Dkef.Services;

using Xunit;

namespace Dkef.Tests;

public class SiteTextRegistryTests
{
    internal static string Json => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Configuration", "siteTextDefinitions.json"));
    internal static SiteTextRegistry Registry => SiteTextRegistry.Parse(Json);

    [Fact]
    public void LoadsAllApprovedFieldsFromOutputAndMatchesOrdinally()
    {
        var registry = SiteTextRegistry.Load(Path.Combine(AppContext.BaseDirectory, "Configuration", "siteTextDefinitions.json"));
        Assert.Equal([15, 27, 5, 26], registry.Groups.Select(group => registry.GetFields(group.Id).Count));
        Assert.Null(registry.FindGroup("Home"));
        Assert.Null(registry.FindGroup(" home"));
        Assert.False(registry.GetFields("home").ContainsKey("Hero.title"));
        Assert.All(registry.Groups.SelectMany(group => registry.GetFields(group.Id).Values), field =>
        {
            Assert.Null(SiteTextValueRules.Validate(field, field.Default));
            Assert.Equal(field.Default, SiteTextValueRules.Sanitize(field.Default, SiteTextValueRules.CreateSanitizer()));
        });
    }

    [Theory]
    [InlineData("duplicate-group")]
    [InlineData("duplicate-section")]
    [InlineData("duplicate-field")]
    [InlineData("duplicate-cross-section-key")]
    [InlineData("group-order")]
    [InlineData("section-order")]
    [InlineData("field-order")]
    [InlineData("fractional-order")]
    [InlineData("input")]
    [InlineData("format")]
    [InlineData("limit")]
    [InlineData("default-length")]
    [InlineData("default-whitespace")]
    [InlineData("default-markup")]
    [InlineData("default-newline")]
    [InlineData("default-email")]
    [InlineData("missing-member")]
    [InlineData("unknown-member")]
    [InlineData("null")]
    [InlineData("empty")]
    [InlineData("identifier")]
    public void RejectsInvalidDefinitions(string mutation)
    {
        var root = JsonNode.Parse(Json)!;
        var groups = root["groups"]!.AsArray();
        var group = groups[0]!;
        var sections = group["sections"]!.AsArray();
        var fields = sections[0]!["fields"]!.AsArray();
        var field = fields[0]!;
        switch (mutation)
        {
            case "duplicate-group": groups.Add(group.DeepClone()); break;
            case "duplicate-section": sections.Add(sections[0]!.DeepClone()); break;
            case "duplicate-field": fields.Add(field.DeepClone()); break;
            case "duplicate-cross-section-key": sections[1]!["fields"]![0]!["key"] = field["key"]!.DeepClone(); break;
            case "group-order": groups[1]!["order"] = 1; break;
            case "section-order": sections[1]!["order"] = 1; break;
            case "field-order": field["order"] = 0; break;
            case "fractional-order": field["order"] = 1.5; break;
            case "input": field["input"] = "html"; break;
            case "format": field["format"] = "url"; break;
            case "limit": field["maxLength"] = -1; break;
            case "default-length": field["maxLength"] = 1; break;
            case "default-whitespace": field["default"] = " \t\u0085\ufeff"; break;
            case "default-markup": field["default"] = "<b>Hej</b>"; break;
            case "default-newline": field["default"] = "Hej\nigen"; break;
            case "default-email": field["format"] = "email"; break;
            case "missing-member": field.AsObject().Remove("required"); break;
            case "unknown-member": field["other"] = true; break;
            case "null": fields[0] = null; break;
            case "empty": fields.Clear(); break;
            case "identifier": group["id"] = "About"; break;
        }
        var error = Assert.Throws<InvalidOperationException>(() => SiteTextRegistry.Parse(root.ToJsonString()));
        Assert.StartsWith("Invalid site-text registry:", error.Message);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{")]
    [InlineData("{\"groups\":[],\"groups\":[]}")]
    public void RejectsMalformedOrDuplicateJson(string json) =>
        Assert.Throws<InvalidOperationException>(() => SiteTextRegistry.Parse(json));

    [Fact]
    public void MissingFileFailsClearly() =>
        Assert.Throws<InvalidOperationException>(() => SiteTextRegistry.Load("no-such-site-text-registry.json"));
}
