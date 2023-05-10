using HarmonyLib;
using RimWorld;
using Verse;

namespace DynamicEconomy;

[HarmonyPatch(typeof(TradeShip), MethodType.Constructor, typeof(TraderKindDef), typeof(Faction))]
public class NotifyEconomyStateTrackerOfArrival
{
    public static void Postfix(TradeShip __instance)
    {
        Log.Message("TradeShip created");
        GameComponent_EconomyStateTracker.CurGameInstance.GetOrCreateIfNeededComplexModifier(__instance);
    }
}