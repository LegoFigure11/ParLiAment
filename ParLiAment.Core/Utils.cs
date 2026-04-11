using ParLiAment.Core.Enums;
using ParLiAment.Core.Structures;
using PKHeX.Core;
using System.Reflection;

namespace ParLiAment.Core;

public static class Utils
{
    private static readonly Assembly thisAssembly;
    private static readonly Dictionary<string, string> resourceNameMap;

    public static readonly GameStrings Strings = GameInfo.GetStrings("en");

    static Utils()
    {
        thisAssembly = Assembly.GetExecutingAssembly();
        resourceNameMap = BuildLookup(thisAssembly.GetManifestResourceNames());
    }

    private static Dictionary<string, string> BuildLookup(IReadOnlyCollection<string> names)
    {
        var res = new Dictionary<string, string>(names.Count);
        foreach (var name in names)
        {
            var fname = GetFileName(name);
            res.TryAdd(fname, name);
        }
        return res;
    }

    private static string GetFileName(string name)
    {
        var period = name.LastIndexOf('.', name.Length - 6);
        var start = period + 1;
        System.Diagnostics.Debug.Assert(start != 0);

        // text file fetch excludes ".txt" (mixed case...); other extensions are used (all lowercase).
        return name.EndsWith(".txt", StringComparison.Ordinal)
            ? name[start..^4].ToLowerInvariant()
            : name[start..];
    }

    public static string? GetStringResource(string name)
    {
        if (!resourceNameMap.TryGetValue(name.ToLowerInvariant(), out var resourceName))
            return null;

        using var resource = thisAssembly.GetManifestResourceStream(resourceName);
        if (resource is null)
            return null;

        using var reader = new StreamReader(resource);
        return reader.ReadToEnd();
    }

    public static byte[]? GetBinaryResource(string name)
    {
        if (!resourceNameMap.TryGetValue(name.ToLowerInvariant(), out var resourceName))
            return null;

        using var resource = thisAssembly.GetManifestResourceStream(resourceName);
        if (resource is null)
            return null;

        using var reader = new BinaryReader(resource);
        return reader.ReadBytes((int)resource.Length);
    }

    public static Version? GetLatestVersion()
    {
        const string endpoint = "https://api.github.com/repos/LegoFigure11/ParLiAment/releases/latest";
        var response = NetUtil.GetStringFromURL(new Uri(endpoint));
        if (response is null) return null;

        const string tag = "tag_name";
        var index = response.IndexOf(tag, StringComparison.Ordinal);
        if (index == -1) return null;

        var first = response.IndexOf('"', index + tag.Length + 1) + 1;
        if (first == 0) return null;

        var second = response.IndexOf('"', first);
        if (second == -1) return null;

        var tagString = response.AsSpan()[first..second].TrimStart('v');

        var patchIndex = tagString.IndexOf('-');
        if (patchIndex != -1) tagString = tagString.ToString().Remove(patchIndex).AsSpan();

        return !Version.TryParse(tagString, out var latestVersion) ? null : latestVersion;
    }

    public static string ParsePA8(PA8 pk)
    {
        var n = Environment.NewLine;

        var form = pk.Form == 0 ? string.Empty : $"-{pk.Form}";
        var gender = pk.Gender switch
        {
            0 => " (M)",
            1 => " (F)",
            _ => string.Empty,
        };
        var shiny = pk.ShinyXor switch
        {
            0 => "■ - ",
            < 16 => "★ - ",
            _ => string.Empty,
        };

        var item = pk.HeldItem > 0 ? $" @ {Strings.Item[pk.HeldItem]}" : string.Empty;

        var moves = pk.Moves.TakeWhile(move => move != 0).Aggregate(string.Empty, (current, move) => current + $"{n}- {Strings.Move[move]}");

        return $"{shiny}{(Species)pk.Species}{form}{gender}{item}{n}EC: {pk.EncryptionConstant:X8}{n}PID: {pk.PID:X8}{n}{Strings.Natures[(int)pk.Nature]} Nature{n}Ability: {Strings.Ability[pk.Ability]}{n}IVs: {pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}{n}H/W: {pk.HeightScalar:D3}/{pk.WeightScalar:D3}{moves}";
    }

    public static IVSearchType GetIVSearchType(string labelText) =>
        labelText == "||" ? IVSearchType.Or : IVSearchType.Range;
}
