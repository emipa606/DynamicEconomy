using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace DynamicEconomy;

[HarmonyPatch(typeof(Tradeable), "GetPriceTooltip")]
public class AddGlobalFactorsToTooltip
{
    public static void Postfix(ref string __result, Tradeable __instance, TradeAction action)
    {
        if (!__instance.HasAnyThing || TradeSession.TradeCurrency != TradeCurrency.Silver)
        {
            return;
        }

        if (__instance.AnyThing != null && __instance.AnyThing.def == DynamicEconomyDefOf.PsiCoin)
        {
            __result += "\n\n" + "DE_AffectingFactorsUnknown".Translate();
            return;
        }

        var gameComp = GameComponent_EconomyStateTracker.CurGameInstance;


        var mod = gameComp.GetOrCreateIfNeededComplexModifier(TradeSession.trader);


        if (mod == null)
        {
            __result += "\n\n" + "DE_NoAffectingFactors".Translate();
            return;
        }


        __result += "\n";
        var baseMul = mod.GetPriceMultipilerFor(__instance.ThingDef, action, ConsideredFactors.Base);
        if (baseMul != 1f)
        {
            switch (TradeSession.trader)
            {
                case Settlement:
                    __result += "\n" + "DE_LocalFactor".Translate() + ": x" + baseMul.ToString("F2");
                    break;
                case TradeShip:
                    __result += "\n" + "DE_OrbitalTraderRandomFactor".Translate() + ": x" + baseMul.ToString("F2");
                    break;
                default:
                    __result += "\n" + "DE_BasePriceFactor_NotASettlementOrAShip".Translate() + ": x" +
                                baseMul.ToString("F2");
                    break;
            }
        }


        switch (action)
        {
            case TradeAction.PlayerBuys:
            {
                var factorDynamic = mod.GetPriceMultipilerFor(__instance.ThingDef, action, ConsideredFactors.Dynamic);
                if (factorDynamic != 1f)
                {
                    __result += "\n" + "DE_PlayerPurchasesFactor".Translate() + ": x" + factorDynamic.ToString("F2");
                }

                var factorEvent = mod.GetPriceMultipilerFor(__instance.ThingDef, action, ConsideredFactors.Event);
                if (factorEvent != 1f)
                {
                    __result += "\n" + "DE_EventFactor".Translate() + ": x" + factorEvent.ToString("F2");
                }

                break;
            }
            case TradeAction.PlayerSells:
            {
                var factorDynamic = mod.GetPriceMultipilerFor(__instance.ThingDef, action, ConsideredFactors.Dynamic);
                if (factorDynamic != 1f)
                {
                    __result += "\n" + "DE_PlayerSalesFactor".Translate() + ": x" + factorDynamic.ToString("F2");
                }

                var factorEvent = mod.GetPriceMultipilerFor(__instance.ThingDef, action, ConsideredFactors.Event);
                if (factorEvent != 1f)
                {
                    __result += "\n" + "DE_EventFactor".Translate() + ": x" + factorEvent.ToString("F2");
                }

                break;
            }
        }
    }
}