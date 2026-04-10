using PKHeX.Core;

namespace ParLiAment.Core.RNG;

public static class RNGUtil
{
    public const uint MAX_TRACKED_ADVANCES = 50_000; // 50,000 chosen arbitrarily to prevent an infinite loop

    public const ulong XOROSHIRO_CONST = 0x82A2B175229D6A5B;

    public static uint GetAdvancesPassed(ulong s0, ulong s1, ulong _s0, ulong _s1, ulong limit = MAX_TRACKED_ADVANCES)
    {
        if (s0 == _s0 && s1 == _s1) return 0;
        var rng = new Xoroshiro128Plus(s0, s1);
        uint i = 0;
        do
        {
            i++;
            rng.Next();

            var (cur0, cur1) = rng.GetState();
            if (cur0 == _s0 && cur1 == _s1) break;

        } while (i < limit);

        return i;
    }

    public static uint GetShinyValue(uint x, uint y) => x ^ y;
    public static uint GetShinyValue(uint x) => (x >> 16) ^ (x & 0xFFFF);

    public static uint GetShinyXOR(uint pid, uint tsv) => GetShinyValue(GetShinyValue(pid), tsv);
}

