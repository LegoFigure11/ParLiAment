using ParLiAment.Core.Interfaces;
using PKHeX.Core;
using static ParLiAment.Core.RNG.Validator;

namespace ParLiAment.Core.RNG;

public static class Static
{
    public static Task<List<PokemonFrame>> Generate(ulong s0, ulong s1, ulong startAdv, ulong endAdv, StaticConfig cfg)
    {
        return Task.Run(() =>
        {
            List<PokemonFrame> results = [];

            bool FiltersEnabled = cfg.FiltersEnabled;
            int FixedIVs = cfg.FixedIVs;
            bool GenerateGender = cfg.GenerateGender;

            bool IsShiny;
            uint ShinyXOR;
            uint PID, EC, FakeIDs;

            bool PassIVs;

            byte Gender;
            byte Ability;
            Nature Nature;

            if (cfg.UseDelay) (s0, s1) = RNGUtil.XoroshiroJump(s0, s1, cfg.Delay);

            var outer = new Xoroshiro128Plus(s0, s1);

            for (ulong i = startAdv; i <= startAdv + endAdv; i++)
            {
                var os = outer.GetState();
                var rng = new Xoroshiro128Plus(os.s0, os.s1);

                EC = (uint)rng.NextInt();

                FakeIDs = (uint)rng.NextInt();

                PID = (uint)rng.NextInt();
                ShinyXOR = RNGUtil.GetShinyXOR(PID, FakeIDs);
                IsShiny = ShinyXOR < 16;
                if (IsShiny) PID ^= 0x10000000;

                PassIVs = true;
                Span<int> ivs = [-1, -1, -1, -1, -1, -1];
                for (var iv = 0; iv < FixedIVs; iv++)
                {
                    int index;
                    do { index = (int)rng.NextInt(6); }
                    while (ivs[index] != -1);

                    ivs[index] = 31;

                    if (cfg.FiltersEnabled && !CheckIV(ivs[index], cfg.TargetMinIVs[index], cfg.TargetMaxIVs[index], cfg.SearchTypes[index]))
                    {
                        PassIVs = false;
                        break;
                    }
                }

                if (!PassIVs)
                {
                    outer.Next();
                    continue;
                }

                for (var iv = 0; iv < ivs.Length; iv++)
                {
                    if (ivs[iv] == -1)
                    {
                        ivs[iv] = (int)rng.NextInt(32);
                        if (cfg.FiltersEnabled && !CheckIV(ivs[iv], cfg.TargetMinIVs[iv], cfg.TargetMaxIVs[iv], cfg.SearchTypes[iv]))
                        {
                            PassIVs = false;
                            break;
                        }
                    }
                }

                if (!PassIVs)
                {
                    outer.Next();
                    continue;
                }

                Ability = (byte)rng.NextInt(2);
                cfg._pk.SetAbilityIndex(Ability);

                Gender = cfg._pk.Gender;
                if (GenerateGender)
                {
                    Gender = (byte)(rng.NextInt(253) + 1);
                }

                Nature = (Nature)rng.NextInt(25);
                if (cfg.FiltersEnabled && !CheckNature((byte)Nature, cfg.TargetNature))
                {
                    outer.Next();
                    continue;
                }

                var f = new PokemonFrame()
                {
                    _advances = i,
                    _seed0 = os.s0,
                    _seed1 = os.s1,

                    _ec = EC,
                    _pid = PID,

                    Gender = Gender switch
                    {
                        PersonalInfo.RatioMagicGenderless => '-',
                        PersonalInfo.RatioMagicMale => 'M',
                        PersonalInfo.RatioMagicFemale => 'F',
                        _ => Gender < cfg.Gender ? 'F' : 'M',
                    },

                    _nature = Nature,
                    _ivs = ivs.ToArray(),

                    Ability = Abilities[cfg._pk.Ability],
                };
                results.Add(f);
                outer.Next();
            }

            return results;
        });
    }
}
