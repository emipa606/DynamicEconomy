using System.Collections.Generic;
using Verse;

namespace DynamicEconomy;

public class ConstantPriceModsDef : Def
{
    public readonly List<BaseCategoryPriceMultipilerInfo> categoryPriceMultipilers = [];
    public readonly List<BaseThingPriceMultipilerInfo> thingPriceMultipilers = [];

    // TODO add some methods ie GetPriceMods
}