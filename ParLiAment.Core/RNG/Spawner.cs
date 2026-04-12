using ParLiAment.Core.Interfaces;
using PKHeX.Core;
using static ParLiAment.Core.RNG.Validator;

namespace ParLiAment.Core.RNG;

public static class Spawner
{
    public static Task<List<SpawnerFrame>> Generate(ulong groupSeed, ulong startAdv, ulong endAdv, SpawnerConfig cfg)
    {
        return Task.Run(() =>
        {
            List<SpawnerFrame> results = [];

            bool FiltersEnabled = cfg.FiltersEnabled;
            int FixedIVs = cfg.FixedIVs;
            bool GenerateGender = cfg.GenerateGender;
            bool GenerateHW = cfg.GenerateHW;

            bool IsShiny;
            uint ShinyXOR;
            uint PID, EC, FakeIDs;

            bool PassIVs;

            byte Gender;
            byte Ability;
            Nature Nature;

            ulong init = unchecked(groupSeed);
            var rng = new Xoroshiro128Plus(init);

            for (ulong i = startAdv; i <= endAdv; i++)
            {
                var generatorSeed = rng.Next();

                _ = rng.Next(); // Alpha Move

                var slotRng = new Xoroshiro128Plus(generatorSeed);
                _ = slotRng.Next(); // Slot
                var PokemonSeed = slotRng.Next();

                var pokeRng = new Xoroshiro128Plus(PokemonSeed);

                EC = (uint)pokeRng.NextInt();

                FakeIDs = (uint)pokeRng.NextInt();

                PID = (uint)pokeRng.NextInt();
                ShinyXOR = RNGUtil.GetShinyXOR(PID, FakeIDs);
                IsShiny = ShinyXOR < 16;
                if (IsShiny) PID ^= 0x10000000;

                PassIVs = true;
                Span<int> ivs = [-1, -1, -1, -1, -1, -1];
                for (var iv = 0; iv < FixedIVs; iv++)
                {
                    int index;
                    do { index = (int)pokeRng.NextInt(6); }
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
                    var _s = rng.Next();
                    rng = new Xoroshiro128Plus(_s);
                    continue;
                }

                for (var iv = 0; iv < ivs.Length; iv++)
                {
                    if (ivs[iv] == -1)
                    {
                        ivs[iv] = (int)pokeRng.NextInt(32);
                        if (cfg.FiltersEnabled && !CheckIV(ivs[iv], cfg.TargetMinIVs[iv], cfg.TargetMaxIVs[iv], cfg.SearchTypes[iv]))
                        {
                            PassIVs = false;
                            break;
                        }
                    }
                }

                if (!PassIVs)
                {
                    var _s = rng.Next();
                    rng = new Xoroshiro128Plus(_s);
                    continue;
                }

                Ability = (byte)pokeRng.NextInt(2);
                cfg._pk.SetAbilityIndex(Ability);

                Gender = cfg._pk.Gender;
                if (GenerateGender)
                {
                    Gender = (byte)(pokeRng.NextInt(253) + 1);
                }

                Nature = (Nature)pokeRng.NextInt(25);
                if (cfg.FiltersEnabled && !CheckNature((byte)Nature, cfg.TargetNature))
                {
                    var _s = rng.Next();
                    rng = new Xoroshiro128Plus(_s);
                    continue;
                }

                byte Height = 127, Weight = 127;
                if (GenerateHW)
                {
                    Height = (byte)(pokeRng.NextInt(0x81) + pokeRng.NextInt(0x80));
                    if (cfg.FiltersEnabled && !CheckHeight(Height, cfg.TargetScale))
                    {
                        var _s = rng.Next();
                        rng = new Xoroshiro128Plus(_s);
                        continue;
                    }

                    Weight = (byte)(pokeRng.NextInt(0x81) + pokeRng.NextInt(0x80));
                }

                var f = new SpawnerFrame()
                {
                    _advances = i,
                    _seed0 = generatorSeed,
                    _seed1 = PokemonSeed,

                    _ec = EC,
                    _pid = PID,

                    Gender = cfg.Gender switch
                    {
                        PersonalInfo.RatioMagicGenderless => '-',
                        PersonalInfo.RatioMagicMale => 'M',
                        PersonalInfo.RatioMagicFemale => 'F',
                        _ => Gender < cfg.Gender ? 'F' : 'M',
                    },

                    _nature = Nature,
                    _ivs = ivs.ToArray(),

                    Ability = Abilities[cfg._pk.Ability],

                    _height = Height,
                    _weight = Weight,
                };
                results.Add(f);

                var ns = rng.Next();
                rng = new Xoroshiro128Plus(ns);
            }

            return results;
        });
    }
}
