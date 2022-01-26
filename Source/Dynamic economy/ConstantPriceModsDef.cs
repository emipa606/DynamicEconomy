using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DynamicEconomy
{
    public class ConstantPriceModsDef : Def
    {
        public List<BaseThingPriceMultipilerInfo> thingPriceMultipilers = new List<BaseThingPriceMultipilerInfo>();
        public List<BaseCategoryPriceMultipilerInfo> categoryPriceMultipilers = new List<BaseCategoryPriceMultipilerInfo>();

        // TODO add some methods ie GetPriceMods
    }
}
