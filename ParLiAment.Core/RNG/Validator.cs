using ParLiAment.Core.Enums;
using ParLiAment.Core.Interfaces;
using PKHeX.Core;

namespace ParLiAment.Core.RNG;
internal static class Validator
{
    internal readonly static IReadOnlyList<string> Natures = Utils.Strings.Natures;
    internal readonly static IReadOnlyList<string> Types = Utils.Strings.Types;
    internal readonly static IReadOnlyList<string> Abilities = Utils.Strings.Ability;

    public static bool CheckIsShiny(uint xor, ShinyType target) => target switch
    {
        ShinyType.Square => xor == 0,
        ShinyType.Star   => xor is > 0 and < 8,
        ShinyType.Either => xor < 16,
        ShinyType.None   => xor >= 16,
        _                => true,
    };

    public static bool CheckEC(uint ec, bool rare) => !rare || (rare && ec % 100 == 0);

    public static bool CheckNature(byte nature, Nature target) => target == Nature.Random || nature == (byte)target;

    public static bool CheckIVs(byte[] IVs, IIVGeneratorConfig cfg)
    {
        for (var i = 0; i < 6; i++)
        {
            var iv = IVs[i];

            if (cfg.SearchTypes[i] == IVSearchType.Range && (iv < cfg.TargetMinIVs[i] || iv > cfg.TargetMaxIVs[i]) ||
                cfg.SearchTypes[i] == IVSearchType.Or && iv != cfg.TargetMinIVs[i] && iv != cfg.TargetMaxIVs[i])
            {
                return false;
            }
        }
        return true;
    }
}
