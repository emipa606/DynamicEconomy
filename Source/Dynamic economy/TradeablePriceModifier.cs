using System;
using RimWorld;
using Verse;

namespace DynamicEconomy;

/// <summary>
///     A simple price modifier, contains only multipilers and provides methods for them
/// </summary>
/// No info about what price of which thing is multipiled
public class TradeablePriceModifier : IExposable
{
    public const float BaseCostToDoubleFactor = 7000f;
    public float baseBuyFactor;

    public float baseSellFactor;
    public float baseStockpileFactor;
    public float playerBuysFactor;

    public float playerBuysFactorEvent; // unaffected by trades              

    public float playerSellsFactor;
    public float playerSellsFactorEvent;
    public static float CostToDoubleFactor => DESettings.costToDoublePriceMultipiler * BaseCostToDoubleFactor;
    public static float CostToHalveFactor => DESettings.costToHalvePriceMultipiler * BaseCostToDoubleFactor;

    public virtual void ExposeData()
    {
        Scribe_Values.Look(ref playerSellsFactor, "colonySellsF");
        Scribe_Values.Look(ref playerBuysFactor, "colonyBuysF");

        Scribe_Values.Look(ref playerSellsFactorEvent, "colonySellsFEvent");
        Scribe_Values.Look(ref playerBuysFactorEvent, "colonyBuysFEvent");

        Scribe_Values.Look(ref baseSellFactor, "colonySellsBase");
        Scribe_Values.Look(ref baseBuyFactor, "colonyBuysBase");
    }

    public float GetPriceMultipiler(TradeAction action, ConsideredFactors factor = ConsideredFactors.All)
    {
        switch (factor)
        {
            case ConsideredFactors.Dynamic:
                return action == TradeAction.PlayerBuys ? playerBuysFactor :
                    action == TradeAction.PlayerSells ? playerSellsFactor : 1f;

            case ConsideredFactors.Event:
                return action == TradeAction.PlayerBuys ? playerBuysFactorEvent :
                    action == TradeAction.PlayerSells ? playerSellsFactorEvent : 1f;

            case ConsideredFactors.Base:
                return action == TradeAction.PlayerBuys ? baseBuyFactor :
                    action == TradeAction.PlayerSells ? baseSellFactor : 1f;

            case ConsideredFactors.Stockpile:
                return playerBuysFactorEvent * baseStockpileFactor;

            case ConsideredFactors.All:
            default:
                return action == TradeAction.PlayerBuys ? playerBuysFactorEvent * playerBuysFactor * baseBuyFactor :
                    action == TradeAction.PlayerSells ? playerSellsFactorEvent * playerSellsFactor * baseSellFactor :
                    1f;
        }
    }
    //public bool HasNoEffect =>playerBuysFactor == 1f && playerSellsFactor == 1f && playerBuysFactorEvent==1f && playerSellsFactorEvent==1f;


    public void ResetFactors()
    {
        playerBuysFactor = 1f;
        playerSellsFactor = 1f;

        playerBuysFactorEvent = 1f;
        playerSellsFactorEvent = 1f;
    }


    protected void Init(float sellFactor = 1f, float buyFactor = 1f)
    {
        baseBuyFactor = buyFactor;
        baseSellFactor = sellFactor;

        ResetFactors();
    }

    public void TickLongUpdate()
    {
        playerSellsFactor += DESettings.sellingPriceFactorGrowthRate;
        if (playerSellsFactor > 1)
        {
            playerSellsFactor = 1;
        }

        playerBuysFactor *= 1 - DESettings.buyingPriceFactorDropRate;
        if (playerBuysFactor < 1)
        {
            playerBuysFactor = 1;
        }


        if (playerSellsFactorEvent < 1f)
        {
            playerSellsFactorEvent += DESettings.sellingPriceFactorGrowthRate;
            if (playerSellsFactorEvent > 1f)
            {
                playerSellsFactorEvent = 1;
            }
        }
        else if (playerSellsFactorEvent > 1f)
        {
            playerSellsFactorEvent *= 1 - DESettings.buyingPriceFactorDropRate;
            if (playerSellsFactorEvent < 1f)
            {
                playerSellsFactorEvent = 1;
            }
        }

        if (playerBuysFactorEvent < 1f)
        {
            playerBuysFactorEvent += DESettings.sellingPriceFactorGrowthRate;
            if (playerBuysFactorEvent > 1f)
            {
                playerBuysFactorEvent = 1;
            }
        }
        else if (playerBuysFactorEvent > 1f)
        {
            playerBuysFactorEvent *= 1 - DESettings.buyingPriceFactorDropRate;
            if (playerBuysFactorEvent < 1f)
            {
                playerBuysFactorEvent = 1;
            }
        }
    }

    public void RecordNewDeal(TradeAction action, float baseTotalPrice)
    {
        baseTotalPrice = Math.Abs(baseTotalPrice); //just in case

        if (action == TradeAction.None)
        {
            return;
        }

        if (action == TradeAction.PlayerBuys)
        {
            playerBuysFactor += baseTotalPrice / CostToDoubleFactor;
        }
        else
        {
            playerSellsFactor /= (float)Math.Pow(2, baseTotalPrice / CostToHalveFactor);
        }
    }

    public void SetBaseFactors(float playerSellsBase, float playerBuysBase)
    {
        baseBuyFactor = playerBuysBase;
        baseSellFactor = playerSellsBase;
    }

    public void ForceSetFactors(float colonySellsFactor, float colonyBuysFactor)
    {
        playerBuysFactor = colonyBuysFactor;
        playerSellsFactor = colonySellsFactor;
    }

    public void SetEventFactors(float colonySellsFactor, float colonyBuysFactor)
    {
        playerSellsFactorEvent = colonySellsFactor;
        playerBuysFactorEvent = colonyBuysFactor;
    }
}