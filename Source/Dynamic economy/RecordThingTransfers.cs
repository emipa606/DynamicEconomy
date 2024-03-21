using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace DynamicEconomy;

[HarmonyPatch(typeof(TradeDeal), "TryExecute")]
public class RecordThingTransfers
{
    [HarmonyPrefix]
    public static void RecordPotentialTransfers(List<Tradeable> ___tradeables,
        out List<List<Pair<ThingDef, float>>> __state) //first sublist is for purchased items, 2nd is for sold ones
    {
        __state = new List<List<Pair<ThingDef, float>>>(2);
        if (TradeSession.TradeCurrency != TradeCurrency.Silver)
        {
            return;
        }

        __state.Add([]);
        __state.Add([]);

        foreach (var tradeable in ___tradeables)
        {
            if (tradeable.IsCurrency || tradeable.CountToTransfer == 0)
            {
                continue;
            }

            if (tradeable.ActionToDo == TradeAction.PlayerBuys)
            {
                __state[0].Add(new Pair<ThingDef, float>(tradeable.ThingDef,
                    tradeable.CurTotalCurrencyCostForDestination));
            }
            else if (tradeable.ActionToDo == TradeAction.PlayerSells)
            {
                __state[1].Add(new Pair<ThingDef, float>(tradeable.ThingDef,
                    tradeable.CurTotalCurrencyCostForDestination));
            }
        }
    }

    [HarmonyPostfix]
    public static void RecordTransfersIfActuallyExecuted(bool actuallyTraded, List<List<Pair<ThingDef, float>>> __state)
    {
        if (!actuallyTraded || TradeSession.TradeCurrency != TradeCurrency.Silver)
        {
            return;
        }

        var gameComp = GameComponent_EconomyStateTracker.CurGameInstance;


        foreach (var transfer in __state[0])
        {
            if (Prefs.DevMode)
            {
                var type = ComplexPriceModifier.GetModifierCategoryFor(transfer.First, out var cat);

                if (Prefs.DevMode)
                {
                    Log.Message(
                        cat != null
                            ? $"Bought {transfer.First.defName}, which is {type}-type and its defining category is {cat.defName}"
                            : $"Bought {transfer.First.defName}, which is {type}-type and has no defining category");
                }
            }

            gameComp.RecordThingTransfer(transfer.First, transfer.Second, TradeAction.PlayerBuys,
                TradeSession.trader);
        }

        foreach (var transfer in __state[1])
        {
            if (Prefs.DevMode)
            {
                var type = ComplexPriceModifier.GetModifierCategoryFor(transfer.First, out var cat);

                if (Prefs.DevMode)
                {
                    Log.Message(
                        cat != null
                            ? $"Sold {transfer.First.defName}, which is {type}-type and its defining category is {cat.defName}"
                            : $"Sold {transfer.First.defName}, which is {type}-type and has no defining category");
                }
            }


            gameComp.RecordThingTransfer(transfer.First, transfer.Second, TradeAction.PlayerSells,
                TradeSession.trader);
        }
    }
}