using ParLiAment.Core.RNG;
using PKHeX.Core;

namespace ParLiAment.Core.Interfaces;

internal interface IBasicFrame
{
    string Advances { get; }
}

public interface IIVFrame
{
    byte HP { get; }
    byte Atk { get; }
    byte Def { get; }
    byte SpA { get; }
    byte SpD { get; }
    byte Spe { get; }
}

public interface IHWFrame
{
    string Height { get; }
    string Weight { get; }
}

public class StaticFrame : IBasicFrame, IIVFrame
{
    internal ulong _advances { get; set; } = 0;
    internal ulong _seed0 { get; set; } = 0;
    internal ulong _seed1 { get; set; } = 0;
    internal uint _pid { get; set; } = 0;
    internal uint _ec { get; set; } = 0;
    internal int[] _ivs { get; set; } = [];
    internal Nature _nature { get; set; } = PKHeX.Core.Nature.Random;

    public string Advances => $"{_advances:N0}";
    public string EC => $"{_ec:X8}";
    public string PID => $"{_pid:X8}";

    public string Ability { get; set; } = string.Empty;
    public string Nature => Validator.Natures[(int)_nature];

    public byte HP => (byte)_ivs[0];
    public byte Atk => (byte)_ivs[1];
    public byte Def => (byte)_ivs[2];
    public byte SpA => (byte)_ivs[3];
    public byte SpD => (byte)_ivs[4];
    public byte Spe => (byte)_ivs[5];

    public char Gender { get; set; } = '-';

    public string Seed0 => $"{_seed0:X16}";
    public string Seed1 => $"{_seed1:X16}";
}

public class SpawnerFrame : IBasicFrame, IIVFrame, IHWFrame
{
    internal ulong _advances { get; set; } = 0;
    internal ulong _seed0 { get; set; } = 0;
    internal ulong _seed1 { get; set; } = 0;
    internal uint _pid { get; set; } = 0;
    internal uint _ec { get; set; } = 0;
    internal byte _height { get; set; } = 127;
    internal byte _weight { get; set; } = 127;
    internal int[] _ivs { get; set; } = [];
    internal Nature _nature { get; set; } = PKHeX.Core.Nature.Random;

    public string Advances => $"{_advances:N0}";
    public string EC => $"{_ec:X8}";
    public string PID => $"{_pid:X8}";

    public string Ability { get; set; } = string.Empty;
    public string Nature => Validator.Natures[(int)_nature];

    public byte HP => (byte)_ivs[0];
    public byte Atk => (byte)_ivs[1];
    public byte Def => (byte)_ivs[2];
    public byte SpA => (byte)_ivs[3];
    public byte SpD => (byte)_ivs[4];
    public byte Spe => (byte)_ivs[5];

    public char Gender { get; set; } = '-';

    public string Height => RNGUtil.GetHeightString(_height);
    public string Weight => RNGUtil.GetHeightString(_weight);

    public string GeneratorSeed => $"{_seed0:X16}";
    public string PokemonSeed => $"{_seed1:X16}";
}
