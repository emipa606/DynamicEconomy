using System;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace DynamicEconomy;

public class SettlementPriceModifier : ComplexPriceModifier
{
    public Settlement settlement;

    public SettlementPriceModifier()
    {
    } //only for easier exposing

    public SettlementPriceModifier(Settlement settlement)
    {
        if (settlement != null && settlement.Faction != Faction.OfPlayer)
        {
            this.settlement = settlement;


            var hills = Find.WorldGrid[settlement.Tile].hilliness;
            ConstantPriceModsDef hillModDef;
            switch (hills)
            {
                case Hilliness.Flat:
                    hillModDef = DynamicEconomyDefOf.Hillness_Flat;
                    break;

                case Hilliness.SmallHills:
                    hillModDef = DynamicEconomyDefOf.Hillness_SmallHills;
                    break;

                case Hilliness.LargeHills:
                    hillModDef = DynamicEconomyDefOf.Hillness_LargeHills;
                    break;

                case Hilliness.Mountainous:
                    hillModDef = DynamicEconomyDefOf.Hillness_Mountainous;
                    break;

                default:
                    Log.Error($"Settlement is placed in {hills} region for which there is no base modifier");
                    hillModDef = null;
                    break;
            }

            if (hillModDef != null)
            {
                // it is safe to add mods directly since lists are empty at this moment
                RegisterCategoryModifiers(hillModDef.categoryPriceMultipilers);
                RegisterThingModifiers(hillModDef.thingPriceMultipilers);
                //foreach (var mod in hillModDef.categoryPriceMultipilers)
                //{
                //    thingCategoryPriceModifiers.Add(new ThingCategoryPriceModifier(
                //        DefDatabase<ThingCategoryDef>.GetNamed(mod.categoryDefName),
                //        mod.baseMultipiler,
                //        mod.baseMultipiler));
                //}

                //foreach (var mod in hillModDef.thingPriceMultipilers)
                //{
                //    thingPriceModifiers.Add(new ThingPriceModifier(
                //        DefDatabase<ThingDef>.GetNamed(mod.thingDefName),
                //        mod.baseMultipiler,
                //        mod.baseMultipiler));
                //}
            }


            var extension = settlement.Biome.GetModExtension<LocalPriceModifierDefExtension>();
            if (extension != null)
            {
                RegisterCategoryModifiers(extension.categoryPriceMultipilers);
                RegisterThingModifiers(extension.thingPriceMultipilers);
                return;
            }

            Log.Warning($"Havent found any biome modifier for {settlement.Biome.defName}");

            var faction = settlement.Faction.def.GetModExtension<LocalPriceModifierDefExtension>();
            if (faction != null)
            {
                Log.Warning(
                    $"Found faction modifier for {settlement.Faction.def.defName} / {faction.categoryPriceMultipilers.Count}");
                RegisterCategoryModifiers(faction.categoryPriceMultipilers);
                RegisterThingModifiers(faction.thingPriceMultipilers);
                return;
            }

            Log.Warning($"Havent found any faction modifier for {settlement.Faction.def.defName}");

            //foreach (var biomeMod in extension.thingPriceMultipilers)
            //{
            //    var mod = GetOrCreateIfNeededTradeablePriceModifier(DefDatabase<ThingDef>.GetNamed(biomeMod.thingDefName));
            //    if (mod != null)
            //    {
            //        mod.baseBuyFactor *= biomeMod.baseMultipiler;
            //        mod.baseSellFactor *= biomeMod.baseMultipiler;
            //    }
            //}

            //foreach (var biomeMod in extension.categoryPriceMultipilers)
            //{
            //    var mod = GetOrCreateIfNeededTradeablePriceModifier(DefDatabase<ThingCategoryDef>.GetNamed(biomeMod.categoryDefName));
            //    if (mod != null)
            //    {
            //        mod.baseBuyFactor *= biomeMod.baseMultipiler;
            //        mod.baseSellFactor *= biomeMod.baseMultipiler;
            //    }
            //}
        }
        else
        {
            throw new ArgumentException("Cant set settlement modifier for null- or player settlement");
        }
    }

    public override float GetPriceMultipilerFor(ThingDef thingDef, TradeAction action,
        ConsideredFactors factor = ConsideredFactors.All)
    {
        var result = base.GetPriceMultipilerFor(thingDef, action, factor);

        if (settlement?.Faction == null)
        {
            return result;
        }

        if (action != TradeAction.PlayerBuys || factor != ConsideredFactors.All && factor != ConsideredFactors.Base)
        {
            return result;
        }

        var Faction = settlement.Faction.def;
        if (thingDef.techLevel > Faction.techLevel + 1)
        {
            result *= 1 + (DESettings.buyingPriceFactorTechLevel *
                           (thingDef.techLevel - Faction.techLevel - 1));
        }

        return result;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref settlement, "settlement");
    }
}