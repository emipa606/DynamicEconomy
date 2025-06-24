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
    private const float BaseCostToDoubleFactor = 7000f;
    public float baseBuyFactor;

    public float baseSellFactor;
    public float baseStockpileFactor;
    private float playerBuysFactor;

    private float playerBuysFactorEvent; // unaffected by trades              

    private float playerSellsFactor;
    private float playerSellsFactorEvent;
    public static float CostToDoubleFactor => DESettings.CostToDoublePriceMultiplier * BaseCostToDoubleFactor;
    public static float CostToHalveFactor => DESettings.CostToHalvePriceMultiplier * BaseCostToDoubleFactor;

    public virtual void ExposeData()
    {
        Scribe_Values.Look(ref playerSellsFactor, "colonySellsF");
        Scribe_Values.Look(ref playerBuysFactor, "colonyBuysF");

        Scribe_Values.Look(ref playerSellsFactorEvent, "colonySellsFEvent");
        Scribe_Values.Look(ref playerBuysFactorEvent, "colonyBuysFEvent");

        Scribe_Values.Look(ref baseSellFactor, "colonySellsBase");
        Scribe_Values.Look(ref baseBuyFactor, "colonyBuysBase");
    }

    public float GetPriceMultiplier(TradeAction action, ConsideredFactors factor = ConsideredFactors.All)
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
                return action switch
                {
                    TradeAction.PlayerBuys => playerBuysFactorEvent * playerBuysFactor * baseBuyFactor,
                    TradeAction.PlayerSells => playerSellsFactorEvent * playerSellsFactor * baseSellFactor, _ => 1f
                };
        }
    }
    //public bool HasNoEffect =>playerBuysFactor == 1f && playerSellsFactor == 1f && playerBuysFactorEvent==1f && playerSellsFactorEvent==1f;


    private void resetFactors()
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

        resetFactors();
    }

    public void TickLongUpdate()
    {
        playerSellsFactor += DESettings.SellingPriceFactorGrowthRate;
        if (playerSellsFactor > 1)
        {
            playerSellsFactor = 1;
        }

        playerBuysFactor *= 1 - DESettings.BuyingPriceFactorDropRate;
        if (playerBuysFactor < 1)
        {
            playerBuysFactor = 1;
        }


        switch (playerSellsFactorEvent)
        {
            case < 1f:
            {
                playerSellsFactorEvent += DESettings.SellingPriceFactorGrowthRate;
                if (playerSellsFactorEvent > 1f)
                {
                    playerSellsFactorEvent = 1;
                }

                break;
            }
            case > 1f:
            {
                playerSellsFactorEvent *= 1 - DESettings.BuyingPriceFactorDropRate;
                if (playerSellsFactorEvent < 1f)
                {
                    playerSellsFactorEvent = 1;
                }

                break;
            }
        }

        switch (playerBuysFactorEvent)
        {
            case < 1f:
            {
                playerBuysFactorEvent += DESettings.SellingPriceFactorGrowthRate;
                if (playerBuysFactorEvent > 1f)
                {
                    playerBuysFactorEvent = 1;
                }

                break;
            }
            case > 1f:
            {
                playerBuysFactorEvent *= 1 - DESettings.BuyingPriceFactorDropRate;
                if (playerBuysFactorEvent < 1f)
                {
                    playerBuysFactorEvent = 1;
                }

                break;
            }
        }
    }

    public void RecordNewDeal(TradeAction action, float baseTotalPrice)
    {
        baseTotalPrice = Math.Abs(baseTotalPrice); //just in case

        switch (action)
        {
            case TradeAction.None:
                return;
            case TradeAction.PlayerBuys:
                playerBuysFactor += baseTotalPrice / CostToDoubleFactor;
                break;
            default:
                playerSellsFactor /= (float)Math.Pow(2, baseTotalPrice / CostToHalveFactor);
                break;
        }
    }

    public void SetBaseFactors(float playerSellsBase, float playerBuysBase)
    {
        baseBuyFactor = playerBuysBase;
        baseSellFactor = playerSellsBase;
    }

    public void SetEventFactors(float colonySellsFactor, float colonyBuysFactor)
    {
        playerSellsFactorEvent = colonySellsFactor;
        playerBuysFactorEvent = colonyBuysFactor;
    }
}