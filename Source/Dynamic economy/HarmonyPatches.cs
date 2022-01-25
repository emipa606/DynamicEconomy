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
            Settlement settlementOfTrade = TradeSession.trader as Settlement;
            var gameComp = GameComponent_EconomyStateTracker.CurGameInstance;

            __result *= gameComp.GetPriceMultipilerFor(__instance.ThingDef, action, settlementOfTrade);
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
                var gameComp = GameComponent_EconomyStateTracker.CurGameInstance;
                Settlement settlementOfTrade = TradeSession.trader as Settlement;

                //TODO searching for modifier 3 times is not good. optimize

                var modType = ComplexPriceModifier.GetModifierCategoryFor(__instance.ThingDef, out _);
                if (modType==ModifierCategory.Constant || modType==ModifierCategory.None)
                {
                    __result += ("\n\nUnaffected by any factors.");
                    return;
                }

                if (action == TradeAction.PlayerBuys)
                {
                    __result += ("\n\n" + "Colony's import volumes multipiler: x" + gameComp.GetPriceMultipilerFor(__instance.ThingDef, action, settlementOfTrade, ConsideredFactors.Dynamic).ToString("F2"));
                    float factorEvent = gameComp.GetPriceMultipilerFor(__instance.ThingDef, action, settlementOfTrade, ConsideredFactors.Event);
                    if (factorEvent != 1f)
                        __result += "\n" + "Event price multipiler: x" + factorEvent.ToString("F2");
                }
                else if (action==TradeAction.PlayerSells)
                {
                    __result += ("\n\n" + "Colony's export volumes modifier: x" + gameComp.GetPriceMultipilerFor(__instance.ThingDef, action, settlementOfTrade, ConsideredFactors.Dynamic).ToString("F2"));
                    float factorEvent = gameComp.GetPriceMultipilerFor(__instance.ThingDef, action, settlementOfTrade, ConsideredFactors.Event);
                    if (factorEvent != 1f)
                        __result += "\n" + "Event price multipiler: x" + factorEvent.ToString("F2");
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

                Settlement settlementOfTrade = TradeSession.trader as Settlement;           //if null, trader is a pawn, so trade happens at player settlement
                if (settlementOfTrade != null)
                    Log.Message("Traded with settlement");

                

                foreach (var transfer in __state[0])
                {

                    //Debug part
                    ThingCategoryDef cat;
                    var type = ComplexPriceModifier.GetModifierCategoryFor(transfer.First, out cat);

                    if (cat != null)
                        Log.Message("Bought " + transfer.First.defName + ", which is " + type.ToString() + "-type and its defining category is " + cat.defName);
                    else
                        Log.Message("Bought " + transfer.First.defName + ", which is " + type.ToString() + "-type and has no defining category");

                    //end of debug part

                    gameComp.RecordThingTransfer(transfer.First, transfer.Second, TradeAction.PlayerBuys, settlementOfTrade);
                }
                foreach (var transfer in __state[1])
                {
                    //Debug
                    ThingCategoryDef cat;
                    var type = ComplexPriceModifier.GetModifierCategoryFor(transfer.First, out cat);

                    if (cat != null)
                        Log.Message("Sold " + transfer.First.defName + ", which is " + type.ToString() + "-type and its defining category is " + cat.defName);
                    else
                        Log.Message("Sold " + transfer.First.defName + ", which is " + type.ToString() + "-type and has no defining category");

                    // end of debug

                    gameComp.RecordThingTransfer(transfer.First, transfer.Second, TradeAction.PlayerSells, settlementOfTrade);
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
            float modifier = gameComp.GetPriceMultipilerFor(def, TradeAction.PlayerBuys);
            return modifier > 1 ? (int)Math.Floor(__result * modifier) : __result;              //leaving it linear is ok, i guess. sqrt(modifier) had too little effect

        }
    }

    [HarmonyPatch(typeof(Building_CommsConsole), "GetFloatMenuOptions")]
    public class GetCompDEEventRollMenuOptions
    {
        public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> res, Building_CommsConsole __instance,  Pawn myPawn)
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
}
