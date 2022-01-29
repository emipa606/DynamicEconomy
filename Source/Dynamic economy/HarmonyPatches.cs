using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

using RimWorld;
using RimWorld.Planet;

namespace DynamicEconomy
{
    [StaticConstructorOnStartup]
    class Patcher
    {
        static Patcher()
        {
            DynamicEconomyMod.harmonyInstance.PatchAll();
        }
    }


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

    [HarmonyPatch(typeof(Tradeable), "GetPriceTooltip")]
    public class AddGlobalFactorsToTooltip
    {
        public static void Postfix(ref string __result, Tradeable __instance, TradeAction action)
        {
            if (__instance.HasAnyThing && TradeSession.TradeCurrency == TradeCurrency.Silver)
            {
                if (__instance.AnyThing!=null && __instance.AnyThing.def==DynamicEconomyDefOf.PsiCoin)
                {
                    __result += "\n\n" + "AffectingFactorsUnknown".Translate();
                    return;
                }

                var gameComp = GameComponent_EconomyStateTracker.CurGameInstance;

                
                var mod = gameComp.GetOrCreateIfNeededComplexModifier(TradeSession.trader).GetOrCreateIfNeededTradeablePriceModifier(__instance.ThingDef);


                if (mod==null)
                {
                    __result += "\n\n" + "NoAffectingFactors".Translate();
                    return;
                }

                


                __result += "\n";
                float baseMul = mod.GetPriceMultipiler(action, ConsideredFactors.Base);
                if (baseMul!=1f)
                {
                    if (TradeSession.trader is Settlement)
                        __result += "\n" + "LocalFactor".Translate() + ": x" + baseMul.ToString("F2");
                    else if (TradeSession.trader is TradeShip)
                        __result += "\n" + "OrbitalTraderRandomFactor".Translate() + ": x" + baseMul.ToString("F2");
                    else
                        __result += "\n" + "BasePriceFactor_NotASettlementOrAShip".Translate() + ": x" + baseMul.ToString("F2");
                }


                if (action == TradeAction.PlayerBuys)
                {
                    float factorDynamic = mod.GetPriceMultipiler(action, ConsideredFactors.Dynamic);
                    if (factorDynamic!=1f)
                        __result += ("\n" + "PlayerPurchasesFactor".Translate() + ": x" + factorDynamic.ToString("F2"));

                    float factorEvent = mod.GetPriceMultipiler(action, ConsideredFactors.Event);
                    if (factorEvent != 1f)
                        __result += "\n" + "EventFactor".Translate() + ": x" + factorEvent.ToString("F2");
                }
                else if (action==TradeAction.PlayerSells)
                {
                    float factorDynamic = mod.GetPriceMultipiler(action, ConsideredFactors.Dynamic);
                    if (factorDynamic != 1f)
                        __result += ("\n" + "PlayerSalesFactor".Translate() + ": x" + factorDynamic.ToString("F2"));

                    float factorEvent = mod.GetPriceMultipiler(action, ConsideredFactors.Event);
                    if (factorEvent != 1f)
                        __result += "\n" + "EventFactor".Translate() + ": x" + factorEvent.ToString("F2");
                }
            }
        }
    }

    [HarmonyPatch(typeof(TradeDeal), "TryExecute")]
    public class RecordThingTransfers
    {
        [HarmonyPrefix]
        public static void RecordPotentialTransfers(List<Tradeable> ___tradeables, out List<List<Pair<ThingDef,float>>> __state)              //first sublist is for purchased items, 2nd is for sold ones
        {
            __state = new List<List<Pair<ThingDef, float>>>(2);
            if (TradeSession.TradeCurrency != TradeCurrency.Silver)
                return;
            __state.Add(new List<Pair<ThingDef, float>>());
            __state.Add(new List<Pair<ThingDef, float>>());

            foreach (var tradeable in ___tradeables)
            {
                if (!tradeable.IsCurrency && tradeable.CountToTransfer!=0)
                {
                    if (tradeable.ActionToDo == TradeAction.PlayerBuys)
                    {
                        __state[0].Add(new Pair<ThingDef, float>(tradeable.ThingDef, tradeable.CurTotalCurrencyCostForDestination));
                    }
                    else if (tradeable.ActionToDo == TradeAction.PlayerSells)
                    {
                        __state[1].Add(new Pair<ThingDef, float>(tradeable.ThingDef, tradeable.CurTotalCurrencyCostForDestination));
                    }
                }
            }
        }

        [HarmonyPostfix]
        public static void RecordTransfersIfActuallyExecuted(bool actuallyTraded, List<List<Pair<ThingDef,float>>> __state)
        {
            if (actuallyTraded && TradeSession.TradeCurrency == TradeCurrency.Silver)
            {
                var gameComp = GameComponent_EconomyStateTracker.CurGameInstance;


                foreach (var transfer in __state[0])
                {

                    if (Prefs.DevMode)
                    {
                        ThingCategoryDef cat;
                        var type = ComplexPriceModifier.GetModifierCategoryFor(transfer.First, out cat);

                        if (cat != null)
                            Log.Message("Bought " + transfer.First.defName + ", which is " + type.ToString() + "-type and its defining category is " + cat.defName);
                        else
                            Log.Message("Bought " + transfer.First.defName + ", which is " + type.ToString() + "-type and has no defining category");

                    }

                    gameComp.RecordThingTransfer(transfer.First, transfer.Second, TradeAction.PlayerBuys, TradeSession.trader);
                }
                foreach (var transfer in __state[1])
                {
                    if (Prefs.DevMode)
                    {
                        ThingCategoryDef cat;
                        var type = ComplexPriceModifier.GetModifierCategoryFor(transfer.First, out cat);

                        if (cat != null)
                            Log.Message("Sold " + transfer.First.defName + ", which is " + type.ToString() + "-type and its defining category is " + cat.defName);
                        else
                            Log.Message("Sold " + transfer.First.defName + ", which is " + type.ToString() + "-type and has no defining category");
                    }
                    

                    gameComp.RecordThingTransfer(transfer.First, transfer.Second, TradeAction.PlayerSells, TradeSession.trader);
                }
            }
        }
    }


    [HarmonyPatch(typeof(StockGenerator), "RandomCountOf")]
    public class BringMoreStuff
    {
        public static int Postfix(int __result, ThingDef def)
        {
            if (__result == 1)
                return __result;
            var gameComp = GameComponent_EconomyStateTracker.CurGameInstance;

            if (def==ThingDefOf.Silver)
            {
                Log.Message("Multipiled trader money by " + gameComp.TraderSilverMultipiler);
                return (int)(__result * gameComp.TraderSilverMultipiler);
            }

            
            float modifier = gameComp.GetPriceMultipilerFor(def, TradeAction.PlayerBuys, null);
            return modifier > 1 ? (int)Math.Floor(__result * modifier) : __result;              //leaving it linear is ok, i guess. sqrt(modifier) had too little effect

        }
    }

    [HarmonyPatch(typeof(Building_CommsConsole), "GetFloatMenuOptions")]
    public class GetCompDEEventRollMenuOptions
    {
        public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> res, Building_CommsConsole __instance, Pawn myPawn)
        {
            bool itWorks = false;
            foreach (var op in res)
            {
                if (!op.Disabled)
                    itWorks = true;
                yield return op;
            }

            if (itWorks)
            {
                foreach (var op in __instance.GetComp<CompDEEventRoll>().CompFloatMenuOptions(myPawn))
                    yield return op;
            }
        }
    }


    // market value should be adjusted directrly for psicoin since storyteller will always count its market value w/o modifiers
    [HarmonyPatch(typeof(StatExtension), "GetStatValue")]
    public class AdjustPsiCoinMarketValue
    {
        public static void Postfix(ref float __result, Thing thing, StatDef stat)
        {
            if (stat == StatDefOf.MarketValue && thing.def == DynamicEconomyDefOf.PsiCoin)
            {
                __result = GameComponent_EconomyStateTracker.CurGameInstance.PsiCoinManager.psiCoinPrice;
            }
        }
    }

    [HarmonyPatch(typeof(TradeShip), MethodType.Constructor, new Type[] { typeof(TraderKindDef), typeof(Faction) })]
    public class NotifyEconomyStateTrackerOfArrival
    {
        public static void Postfix(TradeShip __instance)
        {
            Log.Message("TradeShip created");
            GameComponent_EconomyStateTracker.CurGameInstance.GetOrCreateIfNeededComplexModifier(__instance);
        }
    }

    [HarmonyPatch(typeof(TradeShip), "Depart")]
    public class NotifyEconomyStateTrackerOfDeparture
    {
        public static void Postfix(TradeShip __instance)
        {
            GameComponent_EconomyStateTracker.CurGameInstance.RemoveModifierForShip(__instance);
        }
    }
}
