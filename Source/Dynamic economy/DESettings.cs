using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace DynamicEconomy;

internal class DESettings : ModSettings
{
    private const float DefaultBuyingPriceFactorDropRate = 0.001f; //Per tickLong
    private const float DefaultSellingPriceFactorGrowthRate = 0.0006f;
    private const float DefaultBuyingPriceFactorTechLevel = 0.25f;

    public static float BuyingPriceFactorDropRate = 0.001f;
    public static float BuyingPriceFactorTechLevel = 0.25f;
    public static float SellingPriceFactorGrowthRate = 0.0006f;

    public static float CostToDoublePriceMultiplier = 1f;
    public static float CostToHalvePriceMultiplier = 1f;

    public static float TurnoverEffectOnTraderCurrencyMultiplier = 1f;
    public static float TurnoverEffectDropRateMultiplier = 1f;

    public static float RandyCoinRandomOffsetMultiplier = 1f;

    public static float OrbitalTraderRandomPriceOffset = 0.1f;

    public static void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);

        Text.Font = GameFont.Tiny;
        listingStandard.Label(
            "DE_Setting_PriceFactor".Translate($"{BuyingPriceFactorDropRate / DefaultBuyingPriceFactorDropRate:F2}"));
        listingStandard.Label("DE_Setting_PriceFactorHalfed".Translate(
            $"{((int)Math.Log(0.5f, 1 - BuyingPriceFactorDropRate) * 2000).ToStringTicksToDays()}"));

        BuyingPriceFactorDropRate =
            listingStandard.Slider(BuyingPriceFactorDropRate / DefaultBuyingPriceFactorDropRate, 0.01f, 10f) *
            DefaultBuyingPriceFactorDropRate; //TODO make it logarithmic

        listingStandard.Label(
            "DE_Setting_GrowthFactor".Translate(
                $"{SellingPriceFactorGrowthRate / DefaultSellingPriceFactorGrowthRate:F2}"));
        listingStandard.Label(
            "DE_Setting_GrowthFactorIncreased".Translate(
                $"{((int)(0.5f / SellingPriceFactorGrowthRate) * 2000).ToStringTicksToDays()}"));
        SellingPriceFactorGrowthRate =
            listingStandard.Slider(SellingPriceFactorGrowthRate / DefaultSellingPriceFactorGrowthRate, 0.01f, 10f) *
            DefaultSellingPriceFactorGrowthRate;

        listingStandard.Label(
            "DE_Setting_TechFactor".Translate($"{BuyingPriceFactorTechLevel / DefaultBuyingPriceFactorTechLevel:F2}"));
        BuyingPriceFactorTechLevel =
            listingStandard.Slider(BuyingPriceFactorTechLevel / DefaultBuyingPriceFactorTechLevel, 0.01f, 10f) *
            DefaultBuyingPriceFactorTechLevel;

        listingStandard.Label("DE_Setting_PlayerEffect".Translate($"{CostToDoublePriceMultiplier:F2}"));
        listingStandard.Label(
            "DE_Setting_PlayerEffectDouble".Translate((int)TradeablePriceModifier.CostToDoubleFactor));
        CostToDoublePriceMultiplier = listingStandard.Slider(CostToDoublePriceMultiplier, 0.05f, 10f);

        listingStandard.Label("DE_Setting_SaleEffect".Translate($"{CostToHalvePriceMultiplier:F2}"));
        listingStandard.Label("DE_Setting_SaleEffectHalfed".Translate((int)TradeablePriceModifier.CostToHalveFactor));
        CostToHalvePriceMultiplier = listingStandard.Slider(CostToHalvePriceMultiplier, 0.05f, 10f);

        listingStandard.Label("DE_Setting_TurnoverDouble".Translate($"{TurnoverEffectOnTraderCurrencyMultiplier:F2}"));
        listingStandard.Label("DE_Setting_TurnoverDoubleInfo".Translate((int)(TurnoverEffectOnTraderCurrencyMultiplier *
                                                                              GameComponent_EconomyStateTracker
                                                                                  .BaseTurnoverToDoubleTradersCurrency)));
        listingStandard.Label("DE_Setting_KeepInMind".Translate());
        TurnoverEffectOnTraderCurrencyMultiplier =
            listingStandard.Slider(TurnoverEffectOnTraderCurrencyMultiplier, 0.05f, 10f);

        listingStandard.Label("DE_Setting_TurnoverEffect".Translate($"{TurnoverEffectDropRateMultiplier:F2}"));
        listingStandard.Label("DE_Setting_TurnoverEffectHalfed".Translate(
            $"{((int)Math.Log(0.5f, 1 - (TurnoverEffectDropRateMultiplier * GameComponent_EconomyStateTracker.BaseTurnoverEffectDrop)) * 2000).ToStringTicksToDays()}"));
        TurnoverEffectDropRateMultiplier = listingStandard.Slider(TurnoverEffectDropRateMultiplier, 0.05f, 10f);

        listingStandard.Label("DE_Setting_PsicoinRandomness".Translate($"{RandyCoinRandomOffsetMultiplier:F2}"));
        RandyCoinRandomOffsetMultiplier = listingStandard.Slider(RandyCoinRandomOffsetMultiplier, 0.01f, 10f);

        listingStandard.Label("DE_Setting_OrbitalTrader".Translate($"{OrbitalTraderRandomPriceOffset:F2}"));
        OrbitalTraderRandomPriceOffset = listingStandard.Slider(OrbitalTraderRandomPriceOffset, 0f, 0.9f);

        if (DynamicEconomyMod.CurrentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("DE_Setting_CurrentModVersion".Translate(DynamicEconomyMod.CurrentVersion));
            GUI.contentColor = Color.white;
        }

        Text.Font = GameFont.Small;
        listingStandard.End();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref BuyingPriceFactorDropRate, "priceFDropRate", DefaultBuyingPriceFactorDropRate);
        Scribe_Values.Look(ref BuyingPriceFactorTechLevel, "priceFTechLevel", DefaultBuyingPriceFactorTechLevel);
        Scribe_Values.Look(ref SellingPriceFactorGrowthRate, "priceFGrowthRate", DefaultSellingPriceFactorGrowthRate);
        Scribe_Values.Look(ref CostToDoublePriceMultiplier, "costToDoublePriceMultipiler", 1f);
        Scribe_Values.Look(ref CostToHalvePriceMultiplier, "costToHalvePriceMultipiler", 1f);
        Scribe_Values.Look(ref TurnoverEffectOnTraderCurrencyMultiplier, "turnoverEffectOnTraderCurrencyMultipiler",
            1f);
        Scribe_Values.Look(ref TurnoverEffectDropRateMultiplier, "turnoverEffectDropRateMultipiler", 1f);
        Scribe_Values.Look(ref RandyCoinRandomOffsetMultiplier, "randyCoinRandomOfsettMultipiler", 1f);
        Scribe_Values.Look(ref OrbitalTraderRandomPriceOffset, "orbitalTraderRandomPriceOffset", 0.3f);
    }
}