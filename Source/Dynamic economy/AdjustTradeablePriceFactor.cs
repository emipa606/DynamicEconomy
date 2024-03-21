using HarmonyLib;
using RimWorld;

namespace DynamicEconomy;

[HarmonyPatch(typeof(Tradeable), "GetPriceFor")]
public class AdjustTradeablePriceFactor
{
    public static float Postfix(float __result, Tradeable __instance, TradeAction action)
    {
        var gameComp = GameComponent_EconomyStateTracker.CurGameInstance;

        __result *= gameComp.GetPriceMultipilerFor(__instance.ThingDef, action, TradeSession.trader);
        return __result;
    }
}