using HarmonyLib;
using RimWorld;

namespace DynamicEconomy;

[HarmonyPatch(typeof(Tradeable), nameof(Tradeable.GetPriceFor))]
public class Tradeable_GetPriceFor
{
    public static float Postfix(float __result, Tradeable __instance, TradeAction action)
    {
        var gameComp = GameComponent_EconomyStateTracker.CurGameInstance;

        __result *= gameComp.GetPriceMultiplierFor(__instance.ThingDef, action, TradeSession.trader);
        return __result;
    }
}