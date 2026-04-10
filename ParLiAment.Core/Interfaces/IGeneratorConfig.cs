using ParLiAment.Core.Enums;
using ParLiAment.Core.RNG;
using PKHeX.Core;

namespace ParLiAment.Core.Interfaces;

internal interface IGeneratorConfig
{
    public bool UseDelay { get; set; }
    public uint Delay { get; set; }

    public bool FiltersEnabled { get; set; }
}

internal interface IIVGeneratorConfig
{
    uint[] TargetMinIVs { get; }
    uint[] TargetMaxIVs { get; }
    IVSearchType[] SearchTypes { get; }

}

public class StaticConfig : IGeneratorConfig, IIVGeneratorConfig
{
    public bool UseDelay { get; set; } = true;
    public uint Delay { get; set; } = 0;

    public bool FiltersEnabled { get; set; } = true;

    public ushort TID { get; set; } = 0;
    public ushort SID { get; set; } = 0;

    public ushort TSV => (ushort)RNGUtil.GetShinyValue(TID, SID);

    public bool BuggedRoamer { get; set; } = false;

    public ShinyType TargetShiny { get; set; } = ShinyType.Any;
    public Nature TargetNature { get; set; } = Nature.Random;

    public uint[] TargetMinIVs { get; set; } = [0, 0, 0, 0, 0, 0];
    public uint[] TargetMaxIVs { get; set; } = [31, 31, 31, 31, 31, 31];
    public IVSearchType[] SearchTypes { get; set; } = [IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range];

    public StaticEncounter Encounter { get; set; } = new();
}

public class WildConfig : IGeneratorConfig, IIVGeneratorConfig
{

    public bool UseDelay { get; set; } = true;
    public uint Delay { get; set; } = 0;

    public bool FiltersEnabled { get; set; } = true;

    public ushort TID { get; set; } = 0;
    public ushort SID { get; set; } = 0;

    public ushort TSV => (ushort)RNGUtil.GetShinyValue(TID, SID);

    public EncounterSlotEncounter[] Table { get; set; } = [];

    public ShinyType TargetShiny { get; set; } = ShinyType.Any;
    public Nature TargetNature { get; set; } = Nature.Random;

    public bool FilterBySpecies { get; set; } = false;
    public ushort TargetSpecies { get; set; } = 0;

    public uint[] TargetMinIVs { get; set; } = [0, 0, 0, 0, 0, 0];
    public uint[] TargetMaxIVs { get; set; } = [31, 31, 31, 31, 31, 31];
    public IVSearchType[] SearchTypes { get; set; } = [IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range];

    public bool RarePID { get; set; } = false;

    public StaticEncounter Encounter { get; set; } = new();
}

/*
public class ChainPokemonConfig : IGeneratorConfig
{

    public byte Cluster { get; set; } = 0;
    public ushort Species { get; set; } = 0;
    public int ChainLength { get; set; } = 0;
    public int GuaranteedIVs => ChainLength switch
    {
        99 => 5,
        39 => 3,
        29 => 2,
        19 => 1,
        _  => 0,
    };

    public bool IsSync { get; set; } = false;
    public bool GenerateNature => !IsSync; // Sync causes Nature generation to be skipped, matters for H/W
    public Nature TargetNature { get; set; } = Nature.Random;
    public bool IsShinyPatch { get; set; } = false;
    public Shiny Shiny => IsShinyPatch ? Shiny.Always : Shiny.Random;

    public ShinyType TargetShiny { get; set; } = ShinyType.Any;

    public bool ForceHiddenAbility { get; set; } = false;

    public uint[] TargetMinIVs { get; set; } = [0, 0, 0, 0, 0, 0];
    public uint[] TargetMaxIVs { get; set; } = [31, 31, 31, 31, 31, 31];
    public IVSearchType[] SearchTypes { get; set; } = [IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range];

    public bool RareEC { get; set; } = false;

    public bool UseDelay { get; set; } = false;
    public uint Delay { get; set; } = 0;

    public uint TID { get; set; } = 0;
    public uint SID { get; set; } = 0;
    public uint TSV => RNG.RNGUtil.GetShinyValue(TID, SID);

    public bool FiltersEnabled { get; set; } = false;
}
*/
