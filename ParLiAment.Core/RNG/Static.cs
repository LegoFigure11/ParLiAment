using ParLiAment.Core.Interfaces;
using PKHeX.Core;
using static ParLiAment.Core.RNG.Validator;

namespace ParLiAment.Core.RNG;

public static class Static
{
    public static Task<List<StaticFrame>> Generate(uint seed, uint startAdv, ulong endAdv, StaticConfig cfg)
    {
        return Task.Run(() =>
        {
            List<StaticFrame> results = [];

            //if (cfg.UseDelay) seed = LCRNG.Skip(seed, cfg.Delay);
            //var outer = LCRNG.Skip(seed, startAdv);

            for (ulong i = startAdv; i <= startAdv + endAdv; i++)
            {
                
            }

            return results;
        });
    }
}
