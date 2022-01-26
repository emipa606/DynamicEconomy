using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DynamicEconomy
{
    [DefOf]
    public static class DynamicEconomyDefOf
    {
        public static QuestScriptDef HighDemandQuest;

        public static JobDef GatherInfo;
        
        public static ConstantPriceModsDef Hillness_Flat;
        public static ConstantPriceModsDef Hillness_SmallHills;
        public static ConstantPriceModsDef Hillness_LargeHills;
        public static ConstantPriceModsDef Hillness_Mountainous;
    }
}
