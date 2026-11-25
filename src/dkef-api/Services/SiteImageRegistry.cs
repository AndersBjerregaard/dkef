using System.Text.Json;

namespace Dkef.Services;

public static class SiteImageRegistry
{
    private static readonly IReadOnlyDictionary<string, string[]> Groups = Load();

    private static Dictionary<string, string[]> Load()
    {
        using var stream = typeof(SiteImageRegistry).Assembly.GetManifestResourceStream("Dkef.Configuration.siteImageSlots.json")
            ?? throw new InvalidOperationException("Missing site-image slot registry.");
        return JsonSerializer.Deserialize<Dictionary<string, string[]>>(stream)
            ?? throw new InvalidOperationException("Invalid site-image slot registry.");
    }

    public static bool HasGroup(string group) => Groups.ContainsKey(group);
    public static bool HasSlot(string group, string slot) =>
        Groups.TryGetValue(group, out var slots) && slots.Contains(slot, StringComparer.Ordinal);
}
