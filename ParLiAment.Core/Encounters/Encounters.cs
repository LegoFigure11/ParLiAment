using PKHeX.Core;

namespace ParLiAment.Core;

public static class Encounters
{
    private static readonly byte[]? _main;
    private static readonly byte[]? _spawner;

    private static readonly List<PA8> Main = [];
    private static readonly List<PA8> Spawner = [];

    static Encounters()
    {
        _main = Utils.GetBinaryResource("main.owl") ?? [];
        _spawner = Utils.GetBinaryResource("spawner.owl") ?? [];

        for (var i = 0; i < _main.Length; i += 376)
        {
            var data = _main[i..(i + 0x168)];
            var pa8 = new PA8(data);
            Main.Add(pa8);
        }

        for (var i = 0; i < _spawner.Length; i += 376)
        {
            var data = _spawner[i..(i + 0x168)];
            var pa8 = new PA8(data);
            Spawner.Add(pa8);
        }
    }

    public static List<string> GetMainEncounters()
    {
        var ret = new List<string>();
        foreach(var pkm in Main)
        {
            var form = pkm.Form != 0 ? $"-{pkm.Form}" : string.Empty;
            ret.Add(SpeciesName.GetSpeciesName(pkm.Species, pkm.Language) + form);
        }
        return ret;
    }

    public static List<string> GetSpawnerEncounters()
    {
        var ret = new List<string>();
        foreach (var pkm in Spawner)
        {
            var form = pkm.Form != 0 ? $"-{pkm.Form}" : string.Empty;
            ret.Add(SpeciesName.GetSpeciesName(pkm.Species, pkm.Language) + form);
        }
        return ret;
    }

    public static PA8 GetMainEncounter(int index) => Main[index];
    public static PA8 GetSpawnerEncounter(int index) => Spawner[index];

    public static int GetFixedIVs(Species s) => s switch
    {
        Species.Tornadus or Species.Thundurus or Species.Landorus or Species.Enamorus or
        Species.Cresselia or Species.Darkrai or Species.Shaymin or Species.Heatran or
        Species.Manaphy or Species.Uxie or Species.Mesprit or Species.Azelf or
        Species.Dialga or Species.Palkia or Species.Giratina or Species.Regigigas or
        Species.Arceus
            => 3,
        Species.Phione or Species.Vulpix or Species.Cyndaquil or Species.Oshawott or Species.Rowlet
            => 0,
        _ => -1,
    };
}
