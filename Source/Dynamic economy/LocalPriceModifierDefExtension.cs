using System.Collections.Generic;
using Verse;

namespace DynamicEconomy;

public class LocalPriceModifierDefExtension : DefModExtension
{
    public List<BaseCategoryPriceMultipilerInfo> categoryPriceMultipilers = new List<BaseCategoryPriceMultipilerInfo>();
    public List<BaseThingPriceMultipilerInfo> thingPriceMultipilers = new List<BaseThingPriceMultipilerInfo>();
}