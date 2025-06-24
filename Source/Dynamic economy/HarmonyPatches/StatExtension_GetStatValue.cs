using HarmonyLib;
using RimWorld;
using Verse;

namespace DynamicEconomy;

[HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
public class StatExtension_GetStatValue
{
    public static void Postfix(ref float __result, Thing thing, StatDef stat)
    {
        var gameComp = GameComponent_EconomyStateTracker.CurGameInstance;

        if (gameComp is { PsiCoinManager: not null } && stat == StatDefOf.MarketValue &&
            thing.def == DynamicEconomyDefOf.PsiCoin)
        {
            __result = gameComp.PsiCoinManager.psiCoinPrice;
        }
    }
}