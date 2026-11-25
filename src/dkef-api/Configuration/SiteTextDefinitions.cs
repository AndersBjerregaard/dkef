using System.Collections.Immutable;

namespace Dkef.Configuration;

public sealed record SiteTextRegistryDefinition(ImmutableArray<SiteTextGroupDefinition> Groups);

public sealed record SiteTextGroupDefinition(
    string Id, string Label, int Order, ImmutableArray<SiteTextSectionDefinition> Sections);

public sealed record SiteTextSectionDefinition(
    string Id, string Label, int Order, ImmutableArray<SiteTextFieldDefinition> Fields);

public sealed record SiteTextFieldDefinition(
    string Key, string Default, string Label, int Order, string Input, bool Required, int MaxLength, string Format);
