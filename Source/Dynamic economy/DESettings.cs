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
        public const float DefaultSellinPriceFactorGrowthRate = 0.0004f;

        public static float buyingPriceFactorDropRate;
        public static float sellingPriceFactorGrowthRate;

        public static float costToDoublePriceMultipiler;


        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label("Price factor drop rate multipiler: " + (buyingPriceFactorDropRate / DefaultBuyingPriceFactorDropRate).ToString("F2"));
            listingStandard.Label("Currently it takes around " + ((int)Math.Log(0.5f, (1-buyingPriceFactorDropRate))*2000).ToStringTicksToDays() + " for price multipiler to be halfed");
            buyingPriceFactorDropRate = listingStandard.Slider(buyingPriceFactorDropRate/DefaultBuyingPriceFactorDropRate, 0.01f, 10f)*DefaultBuyingPriceFactorDropRate;            //TODO make it logarithmic

            listingStandard.Label("Price factor growth rate multipiler: " + (sellingPriceFactorGrowthRate / DefaultSellinPriceFactorGrowthRate).ToString("F2"));
            listingStandard.Label("Currently it takes around " + ((int)(0.5f / sellingPriceFactorGrowthRate)*2000).ToStringTicksToDays() + " for price multipiler to grow by 0.5");
            sellingPriceFactorGrowthRate = listingStandard.Slider(sellingPriceFactorGrowthRate / DefaultSellinPriceFactorGrowthRate, 0.01f, 10f) * DefaultSellinPriceFactorGrowthRate;

            listingStandard.Label("Player's transactions effect on price multipilers: " + costToDoublePriceMultipiler.ToString("F2"));
            listingStandard.Label("Currently it takes " + (int)TradeablePriceModifier.CostToDoubleFactor + " silver received/sent for a thing to half/double it's price");
            costToDoublePriceMultipiler = listingStandard.Slider(costToDoublePriceMultipiler, 0.05f, 10f);


            listingStandard.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref buyingPriceFactorDropRate, "priceFDropRate", DefaultBuyingPriceFactorDropRate);
            Scribe_Values.Look(ref sellingPriceFactorGrowthRate, "priceFGrowthRate", DefaultSellinPriceFactorGrowthRate);
            Scribe_Values.Look(ref costToDoublePriceMultipiler, "costToDoublePriceMultipiler", 1f);
        }
    }
}
