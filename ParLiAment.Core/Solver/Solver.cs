using Microsoft.Z3;
using PKHeX.Core;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace ParLiAment.Core;

// Adapted with no functional changes from https://gist.github.com/Lusamine/1a7f9e4418b618daa75f7c9e9c2a9e91
// Thanks Lusamine!

public static class Solver
{
    private static readonly Context ctx = new(new Dictionary<string, string> { { "model", "true" } });

    /// <summary>
    /// Middle level seed calculation for the Generator Seed
    /// </summary>
    /// <param name="seed">Bottom level Entity seed.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<ulong> FindPotentialGenSeeds(ulong seed)
    {
        var exp = CreateGenSeedModel(seed);

        // Z3 is a theorem prover, and only proves if the model can be solved.
        // To yield multiple possible values from the expression, we must re-evaluate with added constraints.
        // Add each previously valid result as an "and this must not be a result".
        // Yield until no results found.
        while (Check(exp) is { } m)
        {
            foreach (var kvp in m.Consts) // should only be 1
            {
                var tmp = (BitVecNum)kvp.Value;
                var possible = tmp.UInt64;
                yield return possible;

                // Force the model to ignore the above result for s0, so we may get a new result if any.
                var constraint = ctx.MkNot(ctx.MkEq(GenSeedResult, tmp));
                exp = ctx.MkAnd(exp, constraint);
            }
        }
    }

    private static readonly BitVecExpr GenSeedResult = ctx.MkBVConst("s0", 64);
    private static readonly BitVecExpr GenSeedExpression = GetBaseGenSeedModel();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BoolExpr CreateGenSeedModel(ulong seed)
    {
        var real_seed = ctx.MkBV(seed, 64);
        return ctx.MkEq(real_seed, GenSeedExpression);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitVecExpr GetBaseGenSeedModel()
    {
        BitVecExpr s0 = GenSeedResult;
        BitVecExpr s1 = ctx.MkBV(Xoroshiro128Plus.XOROSHIRO_CONST, 64);

        // var slotRand = ctx.MkBVAdd(s0, s1);
        s1 = ctx.MkBVXOR(s0, s1);
        var tmp = ctx.MkBVRotateLeft(24, s0);
        var tmp2 = ctx.MkBV(1 << 16, 64);
        s0 = ctx.MkBVXOR(tmp, ctx.MkBVXOR(s1, ctx.MkBVMul(s1, tmp2)));
        s1 = ctx.MkBVRotateLeft(37, s1);
        return ctx.MkBVAdd(s0, s1); // genseed
                                    // no rot/xor needed, the add result is enough.
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Model? Check(BoolExpr cond)
    {
        Microsoft.Z3.Solver solver = ctx.MkSolver();
        solver.Assert(cond);
        Status q = solver.Check();
        if (q != Status.SATISFIABLE)
            return null;
        return solver.Model;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetGroupSeed(ulong seed)
    {
        return unchecked(seed - Xoroshiro128Plus.XOROSHIRO_CONST);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidGroupSeed(ulong seed, ReadOnlySpan<uint> ecs)
    {
        int matched = 0;

        var rng = new Xoroshiro128Plus(seed);
        for (int count = 0; count < 4; count++)
        {
            var genseed = rng.Next();
            _ = rng.Next(); // unknown

            var slotrng = new Xoroshiro128Plus(genseed);
            _ = slotrng.Next(); // slot
            var mon_seed = slotrng.Next();
            // _ = slotrng.Next(); // level

            var monrng = new Xoroshiro128Plus(mon_seed);
            var ec = (uint)monrng.NextInt();

            var index = ecs.IndexOf(ec);
            if (index != -1)
                matched++;

            var newseed = rng.Next();
            rng = new Xoroshiro128Plus(newseed);
        }
        return matched == ecs.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitVecExpr AdvanceSymbolicNext(Context ctx, ref BitVecExpr s0, ref BitVecExpr s1)
    {
        var and_val = ctx.MkBV(0xFFFFFFFFFFFFFFFF, 64);
        var res = ctx.MkBVAND(ctx.MkBVAdd(s0, s1), and_val);
        s1 = ctx.MkBVXOR(s0, s1);
        var tmp = ctx.MkBVRotateLeft(24, s0);
        var tmp2 = ctx.MkBV(1 << 16, 64);
        s0 = ctx.MkBVXOR(tmp, ctx.MkBVXOR(s1, ctx.MkBVMul(s1, tmp2)));
        s1 = ctx.MkBVRotateLeft(37, s1);
        return res;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<ulong> GetAllSeeds(PA8 mon)
    {
        List<ulong> result = [];
        ConcurrentBag<ulong> seeds = [];

        if (mon.IsShiny)
        {
            FindPotentialSeedsShiny(seeds, mon.PID & 0xFFFF, mon.EncryptionConstant);
        }
        else
        {
            FindPotentialSeeds(seeds, mon.PID, mon.EncryptionConstant);
            if (IsPotentialAntiShiny(mon.TID16, mon.SID16, mon.PID))
                FindPotentialSeeds(seeds, mon.PID ^ 0x1000_0000, mon.EncryptionConstant);
        }

        foreach (var seed in seeds)
        {
            for (int ivs = 0; ivs <= 4; ivs++)
            {
                // Verify the IVs; only 0, 3, and 4 fixed IVs exist.
                if (ivs > 0 && ivs < 3)
                    ivs = 3;
                if (mon.FlawlessIVCount >= ivs && IsMatch(seed, ivs, mon, out _))
                    result.Add(seed);
            }
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPotentialAntiShiny(int tid, int sid, uint pid)
    {
        return GetIsShiny(tid, sid, pid ^ 0x1000_0000);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetIsShiny(int tid, int sid, uint pid)
    {
        return GetShinyXor(pid, (uint)((sid << 16) | tid)) < 16;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FindPotentialSeeds(ConcurrentBag<ulong> all_seeds, uint pid, uint ec)
    {
        ulong start_seed = ec - unchecked((uint)Xoroshiro128Plus.XOROSHIRO_CONST);

        Parallel.For(0, 0xFFFF, i =>
        {
            var test = start_seed | ((ulong)i << 48);
            for (int x = 0; x < 65536; x++)
            {
                var seed = CheckSeed(test, pid);
                if (seed != 0)
                    all_seeds.Add(seed);
                test += 0x1_0000_0000;
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FindPotentialSeedsShiny(ConcurrentBag<ulong> all_seeds, uint pid, uint ec)
    {
        ulong start_seed = ec - unchecked((uint)Xoroshiro128Plus.XOROSHIRO_CONST);
        Console.WriteLine($"Starting seed: {start_seed:x16}");

        Parallel.For(0, 0xFFFF, i =>
        {
            var test = start_seed | (ulong)((ulong)i << 48);
            for (int x = 0; x < 65536; x++)
            {
                var seed = CheckSeedShiny(test, pid);
                if (seed != 0)
                    all_seeds.Add(seed);
                test += 0x1_0000_0000;
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong CheckSeed(ulong seed, uint monpid)
    {
        var rng = new Xoroshiro128Plus(seed);
        rng.NextInt(); // EC
        rng.NextInt(); // fakeTID

        var pid = rng.NextInt();
        if (pid == monpid)
            return seed;
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong CheckSeedShiny(ulong seed, uint monpid)
    {
        ulong seed0 = seed;
        var rng = new Xoroshiro128Plus(seed0);
        rng.NextInt(); // EC
        rng.NextInt(); // fakeTID

        var pid = rng.NextInt();
        if (monpid == (pid & 0xFFFF))
            return seed0;
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMatch(ulong seed, int fixed_ivs, PA8 mon, out uint shiny_xor)
    {
        Span<int> IVs = stackalloc int[6];
        mon.GetIVs(IVs);
        (IVs[5], IVs[3], IVs[4]) = (IVs[3], IVs[4], IVs[5]);

        var rng = new Xoroshiro128Plus(seed);
        rng.NextInt(); // EC
        uint fakeTID = (uint)rng.NextInt(); // TID

        uint pid = (uint)rng.NextInt(); // PID

        // Record whether this was a shiny seed or not.
        shiny_xor = GetShinyXor(pid, fakeTID);

        if (!mon.IsShiny && mon.PID != pid && mon.PID != (pid ^ 0x10000000))
            return false;
        if (mon.IsShiny && (mon.PID & 0xFFFF) != (pid & 0xFFFF))
            return false;

        int[] check_ivs = [-1, -1, -1, -1, -1, -1];
        for (int i = 0; i < fixed_ivs; i++)
        {
            uint slot;
            do
            {
                slot = (uint)rng.NextInt(6);
            } while (check_ivs[slot] != -1);

            if (IVs[(int)slot] != 31)
                return false;

            check_ivs[slot] = 31;
        }
        for (int i = 0; i < 6; i++)
        {
            if (check_ivs[i] != -1)
                continue; // already verified?

            uint iv = (uint)rng.NextInt(32);
            if (iv != IVs[i])
                return false;
        }

        var ability = (int)rng.NextInt(2) + 1; // Ability 1 or 2 only -- potentially could be changed with transfers?
        if (ability != mon.AbilityNumber)
            return false;

        var genderratio = PersonalTable.LA[mon.Species].Gender;
        if (genderratio is not (PersonalInfo.RatioMagicGenderless or PersonalInfo.RatioMagicFemale or PersonalInfo.RatioMagicMale))
        {
            var gender = (int)rng.NextInt(253) + 1 < genderratio ? 1 : 0; // Gender
            if (gender != mon.Gender)
                return false;
        }

        var nature = (int)rng.NextInt(25); // Nature -- no synchronize in LA
        if (nature != (int)mon.Nature)
            return false;

        if (mon.IsAlpha)
            return true;

        // Hacky way to skip static encounters, validate yourself.
        if (mon.HeightScalar == 127 && mon.WeightScalar == 127)
            return true;

        var height = (int)rng.NextInt(0x81) + (int)rng.NextInt(0x80);
        if (height != mon.HeightScalar)
            return false;
        var weight = (int)rng.NextInt(0x81) + (int)rng.NextInt(0x80);
        if (weight != mon.WeightScalar)
            return false;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint GetShinyXor(uint pid, uint oid)
    {
        var xor = pid ^ oid;
        return (xor ^ (xor >> 16)) & 0xFFFF;
    }
}
