using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace DynamicEconomy;

[HarmonyPatch(typeof(StockGenerator), "RandomCountOf")]
public class StockGenerator_RandomCountOf
{
    public static int Postfix(int __result, TraderKindDef ___trader, ThingDef def)
    {
        if (___trader.orbital)
        {
            return __result;
        }

        var gameComp = GameComponent_EconomyStateTracker.CurGameInstance;

        if (def == ThingDefOf.Silver)
        {
            return (int)(__result * gameComp.TraderSilverMultipiler);
        }

        // Here is a little bit weird, because it would make settlements depend on caravan modifiers
        var modifier = gameComp.GetPriceMultiplierFor(def, TradeAction.PlayerBuys, null, ConsideredFactors.Stockpile);
        return
            (int)Math.Floor(__result *
                            modifier); //leaving it linear is ok, I guess. sqrt(modifier) had too little effect
    }
}