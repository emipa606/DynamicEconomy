using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace DynamicEconomy
{
    class DESettings : ModSettings
    {
        public const float DefaultBuyingPriceFactorDropRate = 0.001f;           //Per tickLong
        public const float DefaultSellinPriceFactorGrowthRate = 0.0006f;
        public const float DefaultBuyingPriceFactorTechLevel = 0.25f;
        
        public static float buyingPriceFactorDropRate=0.001f;
        public static float buyingPriceFactorTechLevel=0.25f;
        public static float sellingPriceFactorGrowthRate=0.0006f;

        public static float costToDoublePriceMultipiler=1f;
        public static float costToHalvePriceMultipiler=1f;

        public static float turnoverEffectOnTraderCurrencyMultipiler=1f;
        public static float turnoverEffectDropRateMultipiler=1f;

        public static float randyCoinRandomOfsettMultipiler=1f;

        public static float orbitalTraderRandomPriceOffset=0.1f;

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label("Price factor drop rate multipiler: x" + (buyingPriceFactorDropRate / DefaultBuyingPriceFactorDropRate).ToString("F2"));
            listingStandard.Label("Currently it takes around " + ((int)Math.Log(0.5f, (1-buyingPriceFactorDropRate))*2000).ToStringTicksToDays() + " for price multipiler to be halfed");
            buyingPriceFactorDropRate = listingStandard.Slider(buyingPriceFactorDropRate/DefaultBuyingPriceFactorDropRate, 0.01f, 10f)*DefaultBuyingPriceFactorDropRate;            //TODO make it logarithmic

            listingStandard.Label("Price factor growth rate multipiler: x" + (sellingPriceFactorGrowthRate / DefaultSellinPriceFactorGrowthRate).ToString("F2"));
            listingStandard.Label("Currently it takes around " + ((int)(0.5f / sellingPriceFactorGrowthRate)*2000).ToStringTicksToDays() + " for price multipiler to grow by 0.5");
            sellingPriceFactorGrowthRate = listingStandard.Slider(sellingPriceFactorGrowthRate / DefaultSellinPriceFactorGrowthRate, 0.01f, 10f) * DefaultSellinPriceFactorGrowthRate;

            listingStandard.Label("Price factor for each tech level difference: x" + (buyingPriceFactorTechLevel / DefaultBuyingPriceFactorTechLevel).ToString("F2"));
            buyingPriceFactorTechLevel = listingStandard.Slider(buyingPriceFactorTechLevel / DefaultBuyingPriceFactorTechLevel, 0.01f, 10f) * DefaultBuyingPriceFactorTechLevel; 

            listingStandard.Label("Player's purchases effect on price multipilers: x" + costToDoublePriceMultipiler.ToString("F2"));
            listingStandard.Label("Currently it takes " + (int)TradeablePriceModifier.CostToDoubleFactor + " silver sent for a thing to double it's price");
            costToDoublePriceMultipiler = listingStandard.Slider(costToDoublePriceMultipiler, 0.05f, 10f);

            listingStandard.Label("Player's sales effect on price multipilers: x" + costToHalvePriceMultipiler.ToString("F2"));
            listingStandard.Label("Currently it takes " + (int)TradeablePriceModifier.CostToHalveFactor + " silver received for a thing to half it's price");
            costToHalvePriceMultipiler = listingStandard.Slider(costToHalvePriceMultipiler, 0.05f, 10f);

            listingStandard.Label("Player's turnover effect on trader's currency amount: x" + turnoverEffectOnTraderCurrencyMultipiler.ToString("F2"));
            listingStandard.Label("Currently it takes " + (int)(turnoverEffectOnTraderCurrencyMultipiler * GameComponent_EconomyStateTracker.BaseTurnoverToDoubleTradersCurrency) + " of thing's value to double trader's money");
            listingStandard.Label("Keep in mind that turnover effect drops with time, so actual multipiler for next trader will be less");
            turnoverEffectOnTraderCurrencyMultipiler = listingStandard.Slider(turnoverEffectOnTraderCurrencyMultipiler, 0.05f, 10f);

            listingStandard.Label("Turnover effect drop rate: x" + turnoverEffectDropRateMultipiler.ToString("F2"));
            listingStandard.Label("Currently it takes around " + ((int)Math.Log(0.5f, (1 - turnoverEffectDropRateMultipiler*GameComponent_EconomyStateTracker.BaseTurnoverEffectDrop)) * 2000).ToStringTicksToDays() + " for currency multipiler to be halfed");
            turnoverEffectDropRateMultipiler = listingStandard.Slider(turnoverEffectDropRateMultipiler, 0.05f, 10f);

            listingStandard.Label("Psicoin price randomness multipiler: " + randyCoinRandomOfsettMultipiler.ToString("F2"));
            randyCoinRandomOfsettMultipiler = listingStandard.Slider(randyCoinRandomOfsettMultipiler, 0.01f, 10f);

            listingStandard.Label("Orbital trader's price offset range: " + orbitalTraderRandomPriceOffset.ToString("F2"));
            orbitalTraderRandomPriceOffset = listingStandard.Slider(orbitalTraderRandomPriceOffset, 0f, 0.9f);

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
            Scribe_Values.Look(ref turnoverEffectOnTraderCurrencyMultipiler, "turnoverEffectOnTraderCurrencyMultipiler", 1f);
            Scribe_Values.Look(ref turnoverEffectDropRateMultipiler, "turnoverEffectDropRateMultipiler", 1f);
            Scribe_Values.Look(ref randyCoinRandomOfsettMultipiler, "randyCoinRandomOfsettMultipiler", 1f);
            Scribe_Values.Look(ref orbitalTraderRandomPriceOffset, "orbitalTraderRandomPriceOffset", 0.3f);
        }
    }
}
