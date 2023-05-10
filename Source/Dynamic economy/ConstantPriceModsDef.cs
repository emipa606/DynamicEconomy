using System.Collections.Generic;
using Verse;

namespace DynamicEconomy;

public class ConstantPriceModsDef : Def
{
    public List<BaseCategoryPriceMultipilerInfo> categoryPriceMultipilers = new List<BaseCategoryPriceMultipilerInfo>();
    public List<BaseThingPriceMultipilerInfo> thingPriceMultipilers = new List<BaseThingPriceMultipilerInfo>();

    // TODO add some methods ie GetPriceMods
}