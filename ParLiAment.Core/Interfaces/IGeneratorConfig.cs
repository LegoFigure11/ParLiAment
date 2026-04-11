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

    public Nature TargetNature { get; set; } = Nature.Random;

    public uint[] TargetMinIVs { get; set; } = [0, 0, 0, 0, 0, 0];
    public uint[] TargetMaxIVs { get; set; } = [31, 31, 31, 31, 31, 31];
    public IVSearchType[] SearchTypes { get; set; } = [IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range];

    public PA8 _pk { get; set; } = new();
    public int FixedIVs => Encounters.GetFixedIVs((Species)_pk.Species);
    public byte Gender => PersonalTable.LA[_pk.Species].Gender;
    public bool GenerateGender => Gender is not PersonalInfo.RatioMagicGenderless and not PersonalInfo.RatioMagicMale and not PersonalInfo.RatioMagicFemale;
}

public class SpawnerConfig : IGeneratorConfig, IIVGeneratorConfig
{
    public bool UseDelay { get; set; } = true;
    public uint Delay { get; set; } = 0;

    public bool FiltersEnabled { get; set; } = true;

    public ushort TID { get; set; } = 0;
    public ushort SID { get; set; } = 0;

    public ushort TSV => (ushort)RNGUtil.GetShinyValue(TID, SID);

    public Nature TargetNature { get; set; } = Nature.Random;
    public ScaleType TargetScale { get; set; } = ScaleType.Any;

    public uint[] TargetMinIVs { get; set; } = [0, 0, 0, 0, 0, 0];
    public uint[] TargetMaxIVs { get; set; } = [31, 31, 31, 31, 31, 31];
    public IVSearchType[] SearchTypes { get; set; } = [IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range, IVSearchType.Range];

    public PA8 _pk { get; set; } = new();
    public int FixedIVs => Encounters.GetFixedIVs((Species)_pk.Species);
    public bool GenerateHW => FixedIVs == 0;
    public byte Gender => PersonalTable.LA[_pk.Species].Gender;
    public bool GenerateGender => Gender is not PersonalInfo.RatioMagicGenderless and not PersonalInfo.RatioMagicMale and not PersonalInfo.RatioMagicFemale;
}
