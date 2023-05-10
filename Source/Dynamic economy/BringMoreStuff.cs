using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace DynamicEconomy;

[HarmonyPatch(typeof(StockGenerator), "RandomCountOf")]
public class BringMoreStuff
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
            //Log.Message("Multipiled trader money by " + gameComp.TraderSilverMultipiler);
            return (int)(__result * gameComp.TraderSilverMultipiler);
        }

        // Here is a little bit weird, because it would make settlements depend on caravan modifiers
        var modifier = gameComp.GetPriceMultipilerFor(def, TradeAction.PlayerBuys, null, ConsideredFactors.Stockpile);
        return
            (int)Math.Floor(__result *
                            modifier); //leaving it linear is ok, i guess. sqrt(modifier) had too little effect
    }
}