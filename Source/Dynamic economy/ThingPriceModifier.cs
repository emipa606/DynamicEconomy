using Verse;

namespace DynamicEconomy;

public class ThingPriceModifier : TradeablePriceModifier
{
    public ThingDef def;

    public ThingPriceModifier()
    {
    }

    public ThingPriceModifier(ThingDef def, float baseSellFactor = 1f, float baseBuyFactor = 1f)
    {
        Init(baseSellFactor, baseBuyFactor);

        this.def = def;
    }

    public ThingDef Def => def;


    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref def, "thingDef");
    }
}