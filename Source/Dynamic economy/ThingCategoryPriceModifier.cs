using Verse;

namespace DynamicEconomy;

public class ThingCategoryPriceModifier : TradeablePriceModifier
{
    private ThingCategoryDef def;

    public ThingCategoryPriceModifier()
    {
    }

    public ThingCategoryPriceModifier(ThingCategoryDef def, float baseSellFactor = 1f, float baseBuyFactor = 1f)
    {
        Init(baseSellFactor, baseBuyFactor);
        this.def = def;
    }

    public ThingCategoryDef Def => def;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref def, "thingCategoryDef");
    }
}