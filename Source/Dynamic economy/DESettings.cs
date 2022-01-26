using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace DynamicEconomy
{
    class DESettings : ModSettings
    {
        public const float DefaultBuyingPriceFactorDropRate = 0.003f;
        public const float DefaultSellinPriceFactorGrowthRate = 0.002f;

        public static float buyingPriceFactorDropRate;
        public static float sellingPriceFactorGrowthRate;




        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label("Price factor drop rate multipiler: " + buyingPriceFactorDropRate / DefaultBuyingPriceFactorDropRate);
            buyingPriceFactorDropRate = listingStandard.Slider(buyingPriceFactorDropRate/DefaultBuyingPriceFactorDropRate, 0.01f, 10f)*DefaultBuyingPriceFactorDropRate;            //TODO make it logarithmic

            listingStandard.Label("Price factor growth rate multipiler: " + sellingPriceFactorGrowthRate / DefaultBuyingPriceFactorDropRate);
            sellingPriceFactorGrowthRate = listingStandard.Slider(sellingPriceFactorGrowthRate / DefaultSellinPriceFactorGrowthRate, 0.01f, 10f) * DefaultSellinPriceFactorGrowthRate;

            listingStandard.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref buyingPriceFactorDropRate, "priceFDropRate", DefaultBuyingPriceFactorDropRate);
            Scribe_Values.Look(ref sellingPriceFactorGrowthRate, "priceFGrowthRate", DefaultSellinPriceFactorGrowthRate);
        }
    }
}
