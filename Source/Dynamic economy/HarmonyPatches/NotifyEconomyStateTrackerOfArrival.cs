using HarmonyLib;
using RimWorld;

namespace DynamicEconomy;

[HarmonyPatch(typeof(TradeShip), MethodType.Constructor, typeof(TraderKindDef), typeof(Faction))]
public static class NotifyEconomyStateTrackerOfArrival
{
    public static void Postfix(TradeShip __instance)
    {
        GameComponent_EconomyStateTracker.CurGameInstance.GetOrCreateIfNeededComplexModifier(__instance);
    }
}