using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace DynamicEconomy;

internal class DESettings : ModSettings
{
    public const float DefaultBuyingPriceFactorDropRate = 0.001f; //Per tickLong
    public const float DefaultSellinPriceFactorGrowthRate = 0.0006f;
    public const float DefaultBuyingPriceFactorTechLevel = 0.25f;

    public static float buyingPriceFactorDropRate = 0.001f;
    public static float buyingPriceFactorTechLevel = 0.25f;
    public static float sellingPriceFactorGrowthRate = 0.0006f;

    public static float costToDoublePriceMultipiler = 1f;
    public static float costToHalvePriceMultipiler = 1f;

    public static float turnoverEffectOnTraderCurrencyMultipiler = 1f;
    public static float turnoverEffectDropRateMultipiler = 1f;

    public static float randyCoinRandomOfsettMultipiler = 1f;

    public static float orbitalTraderRandomPriceOffset = 0.1f;

    public static void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);

        Text.Font = GameFont.Tiny;
        listingStandard.Label(
            "DE_Setting_PriceFactor".Translate($"{buyingPriceFactorDropRate / DefaultBuyingPriceFactorDropRate:F2}"));
        listingStandard.Label("DE_Setting_PriceFactorHalfed".Translate(
            $"{((int)Math.Log(0.5f, 1 - buyingPriceFactorDropRate) * 2000).ToStringTicksToDays()}"));

        buyingPriceFactorDropRate =
            listingStandard.Slider(buyingPriceFactorDropRate / DefaultBuyingPriceFactorDropRate, 0.01f, 10f) *
            DefaultBuyingPriceFactorDropRate; //TODO make it logarithmic

        listingStandard.Label(
            "DE_Setting_GrowthFactor".Translate(
                $"{sellingPriceFactorGrowthRate / DefaultSellinPriceFactorGrowthRate:F2}"));
        listingStandard.Label(
            "DE_Setting_GrowthFactorIncreased".Translate(
                $"{((int)(0.5f / sellingPriceFactorGrowthRate) * 2000).ToStringTicksToDays()}"));
        sellingPriceFactorGrowthRate =
            listingStandard.Slider(sellingPriceFactorGrowthRate / DefaultSellinPriceFactorGrowthRate, 0.01f, 10f) *
            DefaultSellinPriceFactorGrowthRate;

        listingStandard.Label(
            "DE_Setting_TechFactor".Translate($"{buyingPriceFactorTechLevel / DefaultBuyingPriceFactorTechLevel:F2}"));
        buyingPriceFactorTechLevel =
            listingStandard.Slider(buyingPriceFactorTechLevel / DefaultBuyingPriceFactorTechLevel, 0.01f, 10f) *
            DefaultBuyingPriceFactorTechLevel;

        listingStandard.Label("DE_Setting_PlayerEffect".Translate($"{costToDoublePriceMultipiler:F2}"));
        listingStandard.Label(
            "DE_Setting_PlayerEffectDouble".Translate((int)TradeablePriceModifier.CostToDoubleFactor));
        costToDoublePriceMultipiler = listingStandard.Slider(costToDoublePriceMultipiler, 0.05f, 10f);

        listingStandard.Label("DE_Setting_SaleEffect".Translate($"{costToHalvePriceMultipiler:F2}"));
        listingStandard.Label("DE_Setting_SaleEffectHalfed".Translate((int)TradeablePriceModifier.CostToHalveFactor));
        costToHalvePriceMultipiler = listingStandard.Slider(costToHalvePriceMultipiler, 0.05f, 10f);

        listingStandard.Label("DE_Setting_TurnoverDouble".Translate($"{turnoverEffectOnTraderCurrencyMultipiler:F2}"));
        listingStandard.Label("DE_Setting_TurnoverDoubleInfo".Translate((int)(turnoverEffectOnTraderCurrencyMultipiler *
                                                                              GameComponent_EconomyStateTracker
                                                                                  .BaseTurnoverToDoubleTradersCurrency)));
        listingStandard.Label("DE_Setting_KeepInMind".Translate());
        turnoverEffectOnTraderCurrencyMultipiler =
            listingStandard.Slider(turnoverEffectOnTraderCurrencyMultipiler, 0.05f, 10f);

        listingStandard.Label("DE_Setting_TurnoverEffect".Translate($"{turnoverEffectDropRateMultipiler:F2}"));
        listingStandard.Label("DE_Setting_TurnoverEffectHalfed".Translate(
            $"{((int)Math.Log(0.5f, 1 - (turnoverEffectDropRateMultipiler * GameComponent_EconomyStateTracker.BaseTurnoverEffectDrop)) * 2000).ToStringTicksToDays()}"));
        turnoverEffectDropRateMultipiler = listingStandard.Slider(turnoverEffectDropRateMultipiler, 0.05f, 10f);

        listingStandard.Label("DE_Setting_PsicoinRandomness".Translate($"{randyCoinRandomOfsettMultipiler:F2}"));
        randyCoinRandomOfsettMultipiler = listingStandard.Slider(randyCoinRandomOfsettMultipiler, 0.01f, 10f);

        listingStandard.Label("DE_Setting_OrbitalTrader".Translate($"{orbitalTraderRandomPriceOffset:F2}"));
        orbitalTraderRandomPriceOffset = listingStandard.Slider(orbitalTraderRandomPriceOffset, 0f, 0.9f);

        if (DynamicEconomyMod.currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("DE_Setting_CurrentModVersion".Translate(DynamicEconomyMod.currentVersion));
            GUI.contentColor = Color.white;
        }

        Text.Font = GameFont.Small;
        listingStandard.End();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref buyingPriceFactorDropRate, "priceFDropRate", DefaultBuyingPriceFactorDropRate);
        Scribe_Values.Look(ref buyingPriceFactorTechLevel, "priceFTechLevel", DefaultBuyingPriceFactorTechLevel);
        Scribe_Values.Look(ref sellingPriceFactorGrowthRate, "priceFGrowthRate", DefaultSellinPriceFactorGrowthRate);
        Scribe_Values.Look(ref costToDoublePriceMultipiler, "costToDoublePriceMultipiler", 1f);
        Scribe_Values.Look(ref costToHalvePriceMultipiler, "costToHalvePriceMultipiler", 1f);
        Scribe_Values.Look(ref turnoverEffectOnTraderCurrencyMultipiler, "turnoverEffectOnTraderCurrencyMultipiler",
            1f);
        Scribe_Values.Look(ref turnoverEffectDropRateMultipiler, "turnoverEffectDropRateMultipiler", 1f);
        Scribe_Values.Look(ref randyCoinRandomOfsettMultipiler, "randyCoinRandomOfsettMultipiler", 1f);
        Scribe_Values.Look(ref orbitalTraderRandomPriceOffset, "orbitalTraderRandomPriceOffset", 0.3f);
    }
}