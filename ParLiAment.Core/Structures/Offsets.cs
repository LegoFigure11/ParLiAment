using PKHeX.Core;
using System.Linq;
using static PKHeX.Core.GameVersion;
using static PKHeX.Core.LanguageID;


namespace ParLiAment.Core.Structures;

public abstract class Offsets
{
    public const string LAGameVersion = "1.1.1";
    public const string TitleID = "01001F5010DFA000";

    public IReadOnlyList<long> BoxStartPokemonPointer { get; } = [0x42BA6B0, 0x1F0, 0x68];
    public IReadOnlyList<long> MyStatusPointer { get; } = [0x42BA6B0, 0x218, 0x68];
    public IReadOnlyList<long> CurrentBoxPointer { get; } = [0x42BA6B0, 0x1F8, 0x4A9];
    public IReadOnlyList<long> OverworldPointer { get; } = [0x42C30E8, 0x1A9];
    public IReadOnlyList<long> MainRNGPointer { get; } = [0x42A7000, 0xD8, 0x0];
    public IReadOnlyList<long> SpawnersPointer { get; } = [0x42BA6B0, 0x2E0, 0x70];
    public IReadOnlyList<long> MapLocationPointerOffline { get; } = [0x42F18C0, 0x90, 0x58, 0xD8, 0x278, 0x1B8, 0xF8, 0x0];
    public IReadOnlyList<long> SaveBlockManaphySubeventProgress { get; } = [0x42A6EE0, 0x290, 0x68, 0x240, 0x0, 0x18];
    public IReadOnlyList<long> SaveBlockPhioneCaptureCount { get; } = [0x42A6EE0, 0x228, 0x1D8, 0x0, 0x440, 0x0, 0x0, 0x18];

    public List<long> WildPokemonPointer { get; } = [0x42A6F00, 0x98, 0x90, 0x10, 0x58, 0x00]; // Zyro https://github.com/zyro670/PokeViewer.NET/blob/main/PokeViewer.NET/SubForms/WideViewerLA.cs#L49

    public const int BoxFormatSlotSize = 0x168;
}
