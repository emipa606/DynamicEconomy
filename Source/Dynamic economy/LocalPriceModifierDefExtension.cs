using System.Collections.Generic;
using Verse;

namespace DynamicEconomy;

public class LocalPriceModifierDefExtension : DefModExtension
{
    public readonly List<BaseCategoryPriceMultipilerInfo> categoryPriceMultipilers =
        [];

    public readonly List<BaseThingPriceMultipilerInfo> thingPriceMultipilers = [];
}