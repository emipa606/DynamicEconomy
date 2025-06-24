using RimWorld;
using Verse;

namespace DynamicEconomy;

public class OrbitalTraderPriceModifier : ComplexPriceModifier
{
    public TradeShip ship;

    public OrbitalTraderPriceModifier()
    {
    } //only for easier exposing

    public OrbitalTraderPriceModifier(TradeShip tradeShip)
    {
        ship = tradeShip;
    }

    public override ThingCategoryPriceModifier GetOrCreateIfNeededTradeablePriceModifier(
        ThingCategoryDef thingCategoryDef)
    {
        var res = base.GetOrCreateIfNeededTradeablePriceModifier(thingCategoryDef);
        if (res == null)
        {
            return null;
        }

        if (DESettings.OrbitalTraderRandomPriceOffset == 0f ||
            res.GetPriceMultiplier(TradeAction.PlayerBuys, ConsideredFactors.Base) != 1f) //if newly created
        {
            return res;
        }

        var randBase = 1 + (Rand.Sign * Rand.Value * DESettings.OrbitalTraderRandomPriceOffset);
        res.SetBaseFactors(randBase, randBase);

        return res;
    }

    protected override TradeablePriceModifier GetOrCreateIfNeededTradeablePriceModifier(ThingDef thingDef)
    {
        var res = base.GetOrCreateIfNeededTradeablePriceModifier(thingDef);
        if (res == null)
        {
            return null;
        }

        if (res.GetPriceMultiplier(TradeAction.PlayerBuys, ConsideredFactors.Base) != 1f) //if newly created
        {
            return res;
        }

        var randBase = 1 + (Rand.Sign * Rand.Value * DESettings.OrbitalTraderRandomPriceOffset);
        res.SetBaseFactors(randBase, randBase);

        return res;
    }

    public override float GetPriceMultipilerFor(ThingDef thingDef, TradeAction action,
        ConsideredFactors factor = ConsideredFactors.All)
    {
        var mod = GetOrCreateIfNeededTradeablePriceModifier(thingDef);
        return mod?.GetPriceMultiplier(action, factor) ?? 1f;
    }

    public override void RecordNewDeal(ThingDef thingDef, float totalCost, TradeAction action)
    {
        // nothing. literally. trader will leave soon and player will never see them again, no reason to waste resources on dynamic factor adjustments
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref ship, "ship");
    }
}