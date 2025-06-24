using HarmonyLib;
using RimWorld;

namespace DynamicEconomy;

[HarmonyPatch(typeof(TradeShip), nameof(TradeShip.Depart))]
public class TradeShip_Depart
{
    public static void Postfix(TradeShip __instance)
    {
        GameComponent_EconomyStateTracker.CurGameInstance.RemoveModifierForShip(__instance);
    }
}