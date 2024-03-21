using RimWorld;
using Verse;

namespace DynamicEconomy;

public class TraderCaravansPriceModifier : ComplexPriceModifier
{
    public Faction faction;

    public TraderCaravansPriceModifier()
    {
    }

    public TraderCaravansPriceModifier(Faction faction)
    {
        this.faction = faction;
        if (faction == null)
        {
            return;
        }

        var extension = faction.def.GetModExtension<LocalPriceModifierDefExtension>();
        if (extension != null)
        {
            RegisterCategoryModifiers(extension.categoryPriceMultipilers);
            RegisterThingModifiers(extension.thingPriceMultipilers);
            return;
        }

        Log.Warning($"Havent found any faction modifier for {faction.def.defName}");
    }

    public override float GetPriceMultipilerFor(ThingDef thingDef, TradeAction action,
        ConsideredFactors factor = ConsideredFactors.All)
    {
        var result = base.GetPriceMultipilerFor(thingDef, action, factor);

        if (faction == null)
        {
            return result;
        }

        if (action != TradeAction.PlayerBuys || factor != ConsideredFactors.All && factor != ConsideredFactors.Base)
        {
            return result;
        }

        if (thingDef.techLevel > faction.def.techLevel + 1)
        {
            result *= 1 + (DESettings.buyingPriceFactorTechLevel *
                           (thingDef.techLevel - faction.def.techLevel - 1));
        }

        return result;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref faction, "faction");
    }
}