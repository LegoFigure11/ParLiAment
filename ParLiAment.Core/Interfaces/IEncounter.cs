using PKHeX.Core;
using System.Text.Json.Serialization;

namespace ParLiAment.Core.Interfaces;

public interface IBaseStaticEncounter
{
    ushort _species { get; set; }
    byte Level { get; set; }
    string Location { get; set; }
}

public interface IBaseWildEncounter
{
    ushort _species { get; set; }
    byte MinLevel { get; set; }
    byte MaxLevel { get; set; }
}

internal interface IDerivedEncounterData
{
    PersonalInfo3 Personal { get; }
    string Name { get; }
}

public class StaticEncounter : IBaseStaticEncounter, IDerivedEncounterData
{
    public ushort _species { get; set; } = 1;
    public byte Level { get; set; } = 5;
    public string Location { get; set; } = string.Empty;

    [JsonIgnore] public string Name => SpeciesName.GetSpeciesName(_species, 2);
    [JsonIgnore] public PersonalInfo3 Personal => PersonalTable.FR[_species];
}

public class EncounterSlotEncounter : IBaseWildEncounter, IDerivedEncounterData
{
    public ushort _species { get; set; } = 1;
    public byte MinLevel { get; set; } = 5;
    public byte MaxLevel { get; set; } = 5;

    [JsonIgnore] public string Name => SpeciesName.GetSpeciesName(_species, 2);
    [JsonIgnore] public PersonalInfo3 Personal => PersonalTable.FR[_species];
}
