using HarmonyLib;
using RimWorld;

namespace DynamicEconomy;

[HarmonyPatch(typeof(TradeShip), "Depart")]
public class NotifyEconomyStateTrackerOfDeparture
{
    public static void Postfix(TradeShip __instance)
    {
        GameComponent_EconomyStateTracker.CurGameInstance.RemoveModifierForShip(__instance);
    }
}