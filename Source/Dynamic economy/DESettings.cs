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
        public const float DefaultPriceFactorDropRate = 0.003f;
        public const float DefaultPriceFactorGrowthRate = 0.003f;

        public static float priceFactorDropRate;
        public static float priceFactorGrowthRate;




        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label("Price factor drop rate multipiler");
            priceFactorDropRate = listingStandard.Slider(priceFactorDropRate/DefaultPriceFactorDropRate, 0.01f, 10f)*DefaultPriceFactorDropRate;

            listingStandard.Label("Price factor growth rate multipiler");
            priceFactorGrowthRate = listingStandard.Slider(priceFactorGrowthRate / DefaultPriceFactorGrowthRate, 0.01f, 10f) * DefaultPriceFactorGrowthRate;

            listingStandard.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref priceFactorDropRate, "priceFDropRate", DefaultPriceFactorDropRate);
            Scribe_Values.Look(ref priceFactorGrowthRate, "priceFGrowthRate", DefaultPriceFactorGrowthRate);
        }
    }
}
