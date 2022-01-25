using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace DynamicEconomy
{
    public enum ModifierCategory
    {
        None,
        Constant,
        Standalone,
        Group
    }

    public enum Factor
    {
        Dynamic,
        Event,
        All
    }

    

    public class TradeablePriceModifier : IExposable
    {
        public float colonySellsFactor;
        public float colonyBuysFactor;

        public float colonyBuysFactorEvent;
        public float colonySellsFactorEvent;

        public float baseSellFactor;
        public float baseBuyFactor;

        public float GetPriceMultipiler(TradeAction action, Factor factor=Factor.All)
        {
            switch(factor)
            {
                case Factor.Dynamic:
                    return action == TradeAction.PlayerBuys ? colonyBuysFactor : (action == TradeAction.PlayerSells ? colonySellsFactor : 1f);

                case Factor.Event:
                    return action == TradeAction.PlayerBuys ? colonyBuysFactorEvent : (action == TradeAction.PlayerSells ? colonySellsFactorEvent : 1f);

                case Factor.All:
                default:
                    return action == TradeAction.PlayerBuys ? colonyBuysFactorEvent*colonyBuysFactor : (action == TradeAction.PlayerSells ? colonySellsFactorEvent*colonySellsFactor : 1f);
            }    
        }
        public bool HasNoEffect =>colonyBuysFactor == 1f && colonySellsFactor == 1f && colonyBuysFactorEvent==1f && colonySellsFactorEvent==1f;


        public void ResetFactors()
        {
            colonyBuysFactor = baseBuyFactor;
            colonySellsFactor = baseSellFactor;

            colonyBuysFactorEvent = 1f;
            colonySellsFactorEvent = 1f;
        }


        protected void Init(float baseSellFactor = 1f, float baseBuyFactor = 1f)
        {
            this.baseBuyFactor = baseBuyFactor;
            this.baseSellFactor = baseSellFactor;

            ResetFactors();
        }

        public void TickLongUpdate()
        {
            colonySellsFactor += DESettings.priceFactorGrowthRate;
            if (colonySellsFactor > 1)
                colonySellsFactor = 1;

            colonyBuysFactor *= (1 - DESettings.priceFactorDropRate);
            if (colonyBuysFactor < 1)
                colonyBuysFactor = 1;


            if (colonySellsFactorEvent < 1f)
            {
                colonySellsFactorEvent += DESettings.priceFactorGrowthRate;
                if (colonySellsFactorEvent > 1f)
                    colonySellsFactorEvent = 1;

            }
            else if (colonySellsFactorEvent > 1f)
            {
                colonySellsFactorEvent *= 1 - DESettings.priceFactorDropRate;
                if (colonySellsFactorEvent < 1f)
                    colonySellsFactorEvent = 1;
            }

            if (colonyBuysFactorEvent < 1f)
            {
                colonyBuysFactorEvent += DESettings.priceFactorGrowthRate;
                if (colonyBuysFactorEvent > 1f)
                    colonyBuysFactorEvent = 1;

            }
            else if (colonyBuysFactorEvent > 1f)
            {
                colonyBuysFactorEvent *= 1 - DESettings.priceFactorDropRate;
                if (colonyBuysFactorEvent < 1f)
                    colonyBuysFactorEvent = 1;
            }
        }

        public void RecordNewDeal(TradeAction action, float baseTotalPrice)
        {
            baseTotalPrice = Math.Abs(baseTotalPrice);          //just in case

            if (action == TradeAction.None)
                return;

            if (action == TradeAction.PlayerBuys)
            {
                colonyBuysFactor += baseTotalPrice / 7000f;                //TODO replace 7k with external var
            }
            else
            {
                colonySellsFactor /= (1f + baseTotalPrice / 7000f);        //TODO replace 7k with external var
            }
        }

        public void SetBaseFactors(float playerSellsBase, float playerBuysBase)
        {
            baseBuyFactor = playerBuysBase;
            baseSellFactor = playerSellsBase;

        }

        public void ForceSetFactors(float colonySellsFactor, float colonyBuysFactor)
        {
            this.colonyBuysFactor = colonyBuysFactor;
            this.colonySellsFactor = colonySellsFactor;
        }

        public void SetEventFactors(float colonySellsFactor, float colonyBuysFactor)
        {
            this.colonySellsFactorEvent = colonySellsFactor;
            this.colonyBuysFactorEvent = colonyBuysFactor;
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref colonySellsFactor, "colonySellsF");
            Scribe_Values.Look(ref colonyBuysFactor, "colonyBuysF");

            Scribe_Values.Look(ref colonySellsFactorEvent, "colonySellsFEvent");
            Scribe_Values.Look(ref colonyBuysFactorEvent, "colonyBuysFEvent");

            Scribe_Values.Look(ref baseSellFactor, "colonySellsBase");
            Scribe_Values.Look(ref baseBuyFactor, "colonyBuysBase");
        }
    }





    public class ThingPriceModifier : TradeablePriceModifier
    {
        public ThingDef def;

        public ThingDef Def => def;

        public ThingPriceModifier(ThingDef def, float baseSellFactor=1f, float baseBuyFactor=1f)
        {
            Init(baseSellFactor, baseBuyFactor);
            this.def = def;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "thingDef");
        }
    }

    public class ThingCategoryPriceModifier : TradeablePriceModifier
    {
        ThingCategoryDef def;

        public ThingCategoryDef Def => def;

        public ThingCategoryPriceModifier(ThingCategoryDef def, float baseSellFactor = 1f, float baseBuyFactor = 1f)
        {
            Init(baseSellFactor, baseBuyFactor);
            this.def = def;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "thingCategoryDef");
        }
    }








    public class ComplexPriceModifier : IExposable
    {
        public List<ThingPriceModifier> thingPriceModifiers;
        public List<ThingCategoryPriceModifier> thingCategoryPriceModifiers;

        protected TradeablePriceModifier GetOrCreateIfNeededTradeablePriceModifier(ThingDef thingDef)         //returns null for ModifierCategory.None
        {
            ThingCategoryDef thingCategory;
            var modCategory = GetModifierCategoryFor(thingDef, out thingCategory);
            TradeablePriceModifier modifier=null;
            if (modCategory == ModifierCategory.Standalone)
            {
                modifier = thingPriceModifiers.Find(mod => mod.Def == thingDef);
                if (modifier == null)
                {
                    modifier = new ThingPriceModifier(thingDef);
                    thingPriceModifiers.Add((ThingPriceModifier)modifier);
                }
                else
                {
                    Log.Message("Found modifier for " + thingDef.defName);
                }
            }
            else if (modCategory == ModifierCategory.Group)
            {
                modifier = thingCategoryPriceModifiers.Find(mod => mod.Def == thingCategory);
                if (modifier == null)
                {
                    modifier = new ThingCategoryPriceModifier(thingCategory);
                    thingCategoryPriceModifiers.Add((ThingCategoryPriceModifier)modifier);
                }

                Log.Message("Found modifier for " + thingDef.defName);
            }

            return modifier;
        }

        protected ThingCategoryPriceModifier GetOrCreateIfNeededTradeablePriceModifier(ThingCategoryDef thingCategoryDef)         //returns null for ModifierCategory.None or Constant
        {
            var modCategory = GetModifierCategoryFor(thingCategoryDef);
            ThingCategoryPriceModifier modifier;

            if (modCategory == ModifierCategory.Group)
            {
                modifier = thingCategoryPriceModifiers.Find(mod => mod.Def == thingCategoryDef);
                if (modifier == null)
                {
                    modifier = new ThingCategoryPriceModifier(thingCategoryDef);
                    thingCategoryPriceModifiers.Add(modifier);
                }

                return modifier;
            }
            else
                return null;
        }


        //TODO find a better place for this

        private static List<ThingCategoryDef> groupableThingsCategories = new List<ThingCategoryDef>()                  //apparel and other stuff  that should be grouped
        {
            ThingCategoryDefOf.Apparel,
            ThingCategoryDefOf.Weapons,
            ThingCategoryDefOf.BuildingsArt,
            ThingCategoryDefOf.Medicine
        };

        private static List<ThingCategoryDef> standaloneThingsCategories = new List<ThingCategoryDef>() {               //steel, wood, components etc. 
            ThingCategoryDefOf.ResourcesRaw,
            ThingCategoryDefOf.Manufactured
        };                 

        public ComplexPriceModifier()
        {
            //this.faction = faction;
            thingPriceModifiers = new List<ThingPriceModifier>();
            thingCategoryPriceModifiers = new List<ThingCategoryPriceModifier>();

        }

        public virtual float GetPriceMultipilerFor(ThingDef thingDef, TradeAction action, Factor factor = Factor.All)
        {
            ThingCategoryDef thingCat;
            var type = GetModifierCategoryFor(thingDef, out thingCat);

            if (type == ModifierCategory.Standalone)
            {
                var modifier = thingPriceModifiers.Find(priceMod => priceMod.Def == thingDef);
                if (modifier == null)
                    return 1f;

                return modifier.GetPriceMultipiler(action, factor);
            }

            else if (type==ModifierCategory.Group)
            {
                var modifier = thingCategoryPriceModifiers.Find(priceMod => priceMod.Def == thingCat);
                if (modifier == null)
                    return 1f;

                return modifier.GetPriceMultipiler(action, factor);
            }

            return 1f;
        }


        public virtual void RecordNewDeal(ThingDef thingDef, float totalCost, TradeAction action)
        {
            if (action == TradeAction.None)
                return;

            var modifier = GetOrCreateIfNeededTradeablePriceModifier(thingDef);
            if (modifier!=null)
                modifier.RecordNewDeal(action, totalCost);
        }

        public virtual void TickLong()
        {
            for (int i=0;i<thingPriceModifiers.Count;i++)
            {
                thingPriceModifiers[i].TickLongUpdate();
                // if (thingPriceModifiers[i].HasNoEffect)
                //     thingPriceModifiers.RemoveAt(i);
            }
            for (int i = 0; i < thingCategoryPriceModifiers.Count; i++)
            {
                thingCategoryPriceModifiers[i].TickLongUpdate();
                // if (thingCategoryPriceModifiers[i].HasNoEffect)
                //     thingCategoryPriceModifiers.RemoveAt(i);
            }
        }

        

        public static ModifierCategory GetModifierCategoryFor(ThingDef thingDef, out ThingCategoryDef definigCategory)              
        {
            //Works for O(n^2) in worst case scenario, but category tree isnt that big. Has to be tested anyway

            ThingCategoryDef node = thingDef.thingCategories.Last();        //I hope the last ones in te list are going to be last categories in ierarchy
            bool isLast = false;
            while (!isLast)
            {
                isLast = true;
                foreach (var child in node.childCategories)
                {
                    if (thingDef.thingCategories.Contains(child))
                    {
                        node = child;
                        isLast = false;
                        break;
                    }
                }
            }

            for (int i=0;i<thingDef.thingCategories.Count;i++)
            {
                if (groupableThingsCategories.Contains(node))
                {
                    definigCategory = node;
                    return ModifierCategory.Group;
                }

                if (standaloneThingsCategories.Contains(node))
                {
                    definigCategory = node;
                    return ModifierCategory.Standalone;
                }

                node = node.parent;
            }

            definigCategory = null;
            return ModifierCategory.Constant;

                              
        }

        public static ModifierCategory GetModifierCategoryFor(ThingCategoryDef thingCategoryDef)
        {
            if (standaloneThingsCategories.Contains(thingCategoryDef))
                return ModifierCategory.Standalone;

            if (groupableThingsCategories.Contains(thingCategoryDef))
                return ModifierCategory.Group;

            else return ModifierCategory.Constant;
        }

        public void SetBaseModifier(ThingDef def, float baseSellFactor, float baseBuyFactor)
        {
            var modifier = GetOrCreateIfNeededTradeablePriceModifier(def);
            if (modifier==null)
            {
                Log.Error("tried to set base multipilers for None-category thing");
                return;
            }

            modifier.SetBaseFactors(baseSellFactor, baseBuyFactor);
        }

        public void AddEventModifier(ThingCategoryDef def, float playerSellsFactor, float playerBuysFactor)
        {
            var modifier = GetOrCreateIfNeededTradeablePriceModifier(def);
            modifier.SetEventFactors(playerSellsFactor, playerBuysFactor);
        }

        public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref thingCategoryPriceModifiers, "thingCategoryMods", LookMode.Deep);
            Scribe_Collections.Look(ref thingPriceModifiers, "thingsMods", LookMode.Deep);
        }
    }






    public class SettlementPriceModifier : ComplexPriceModifier
    {
        private bool forPlayerSettlement;
        public bool ForPlayerSettlement=>forPlayerSettlement;   
        private Settlement settlement;
        public Settlement Settlement => ForPlayerSettlement ? Find.WorldObjects.Settlements.Find(s => s.Faction == Faction.OfPlayer) : settlement;                    
        public SettlementPriceModifier(Settlement settlement) : base()                  // null Settlement == player settlement
        {
            if (settlement != null && settlement.Faction != Faction.OfPlayer)
            {
                forPlayerSettlement = false;
                this.settlement = settlement;
            }
            else
            {
                forPlayerSettlement = true;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref settlement, "settlement");
            Scribe_Values.Look(ref forPlayerSettlement, "forPlayerSettlement");
        }
    }
}
