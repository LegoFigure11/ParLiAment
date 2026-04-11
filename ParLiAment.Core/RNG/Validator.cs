using ParLiAment.Core.Enums;
using ParLiAment.Core.Interfaces;
using PKHeX.Core;

namespace ParLiAment.Core.RNG;
internal static class Validator
{
    internal readonly static IReadOnlyList<string> Natures = Utils.Strings.Natures;
    internal readonly static IReadOnlyList<string> Types = Utils.Strings.Types;
    internal readonly static IReadOnlyList<string> Abilities = Utils.Strings.Ability;

    public static bool CheckNature(byte nature, Nature target) => target == Nature.Random || nature == (byte)target;

    public static bool CheckIV(int iv, uint min, uint max, IVSearchType type)
    {
        return !(type == IVSearchType.Range && (iv < min || iv > max) || type == IVSearchType.Or && iv != min && iv != max);
    }

    public static bool CheckHeight(uint height, ScaleType target) => target switch
    {
        ScaleType.XXXS => height == 0,
        ScaleType.XXS => height >= 1 && height <= 24,
        ScaleType.XS => height >= 25 && height <= 59,
        ScaleType.S => height >= 60 && height <= 99,
        ScaleType.M => height >= 100 && height <= 155,
        ScaleType.L => height >= 156 && height <= 195,
        ScaleType.XL => height >= 196 && height <= 230,
        ScaleType.XXL => height >= 231 && height <= 254,
        ScaleType.XXXL => height == 255,
        ScaleType.MinOrMax => height == 0 || height == 255,
        _ => true,
    };
}
