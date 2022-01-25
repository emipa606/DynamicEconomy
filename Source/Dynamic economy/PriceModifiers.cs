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

    public enum ConsideredFactors
    {
        Dynamic,
        Event,
        All
    }

    

    public class TradeablePriceModifier : IExposable
    {
        public float playerSellsFactor;
        public float playerBuysFactor;

        public float playerBuysFactorEvent;
        public float playerSellsFactorEvent;

        public float baseSellFactor;
        public float baseBuyFactor;

        public float GetPriceMultipiler(TradeAction action, ConsideredFactors factor=ConsideredFactors.All)
        {
            switch(factor)
            {
                case ConsideredFactors.Dynamic:
                    return action == TradeAction.PlayerBuys ? playerBuysFactor : (action == TradeAction.PlayerSells ? playerSellsFactor : 1f);

                case ConsideredFactors.Event:
                    return action == TradeAction.PlayerBuys ? playerBuysFactorEvent : (action == TradeAction.PlayerSells ? playerSellsFactorEvent : 1f);

                case ConsideredFactors.All:
                default:
                    return action == TradeAction.PlayerBuys ? playerBuysFactorEvent*playerBuysFactor : (action == TradeAction.PlayerSells ? playerSellsFactorEvent*playerSellsFactor : 1f);
            }    
        }
        public bool HasNoEffect =>playerBuysFactor == 1f && playerSellsFactor == 1f && playerBuysFactorEvent==1f && playerSellsFactorEvent==1f;


        public void ResetFactors()
        {
            playerBuysFactor = baseBuyFactor;
            playerSellsFactor = baseSellFactor;

            playerBuysFactorEvent = 1f;
            playerSellsFactorEvent = 1f;
        }


        protected void Init(float baseSellFactor = 1f, float baseBuyFactor = 1f)
        {
            this.baseBuyFactor = baseBuyFactor;
            this.baseSellFactor = baseSellFactor;

            ResetFactors();
        }

        public void TickLongUpdate()
        {
            playerSellsFactor += DESettings.sellingPriceFactorGrowthRate;
            if (playerSellsFactor > 1)
                playerSellsFactor = 1;

            playerBuysFactor *= (1 - DESettings.buyingPriceFactorDropRate);
            if (playerBuysFactor < 1)
                playerBuysFactor = 1;


            if (playerSellsFactorEvent < 1f)
            {
                playerSellsFactorEvent += DESettings.sellingPriceFactorGrowthRate;
                if (playerSellsFactorEvent > 1f)
                    playerSellsFactorEvent = 1;

            }
            else if (playerSellsFactorEvent > 1f)
            {
                playerSellsFactorEvent *= 1 - DESettings.buyingPriceFactorDropRate;
                if (playerSellsFactorEvent < 1f)
                    playerSellsFactorEvent = 1;
            }

            if (playerBuysFactorEvent < 1f)
            {
                playerBuysFactorEvent += DESettings.sellingPriceFactorGrowthRate;
                if (playerBuysFactorEvent > 1f)
                    playerBuysFactorEvent = 1;

            }
            else if (playerBuysFactorEvent > 1f)
            {
                playerBuysFactorEvent *= 1 - DESettings.buyingPriceFactorDropRate;
                if (playerBuysFactorEvent < 1f)
                    playerBuysFactorEvent = 1;
            }
        }

        public void RecordNewDeal(TradeAction action, float baseTotalPrice)
        {
            baseTotalPrice = Math.Abs(baseTotalPrice);          //just in case

            if (action == TradeAction.None)
                return;

            if (action == TradeAction.PlayerBuys)
            {
                playerBuysFactor += baseTotalPrice / 7000f;                //TODO replace 7k with external var
            }
            else
            {
                playerSellsFactor /= (1f + baseTotalPrice / 7000f);        //TODO replace 7k with external var
            }
        }

        public void SetBaseFactors(float playerSellsBase, float playerBuysBase)
        {
            baseBuyFactor = playerBuysBase;
            baseSellFactor = playerSellsBase;

        }

        public void ForceSetFactors(float colonySellsFactor, float colonyBuysFactor)
        {
            this.playerBuysFactor = colonyBuysFactor;
            this.playerSellsFactor = colonySellsFactor;
        }

        public void SetEventFactors(float colonySellsFactor, float colonyBuysFactor)
        {
            this.playerSellsFactorEvent = colonySellsFactor;
            this.playerBuysFactorEvent = colonyBuysFactor;
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref playerSellsFactor, "colonySellsF");
            Scribe_Values.Look(ref playerBuysFactor, "colonyBuysF");

            Scribe_Values.Look(ref playerSellsFactorEvent, "colonySellsFEvent");
            Scribe_Values.Look(ref playerBuysFactorEvent, "colonyBuysFEvent");

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
                else
                {
                    Log.Message("Found modifier for " + thingDef.defName);
                }
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

        /*private static List<ThingCategoryDef> groupableThingsCategories = new List<ThingCategoryDef>()                  //apparel and other stuff  that should be grouped
        {
            ThingCategoryDefOf.Apparel,
            ThingCategoryDefOf.Weapons,
            ThingCategoryDefOf.BuildingsArt,
            ThingCategoryDefOf.Medicine
        };

        private static List<ThingCategoryDef> standaloneThingsCategories = new List<ThingCategoryDef>() {               //steel, wood, components etc. 
            ThingCategoryDefOf.ResourcesRaw,
            ThingCategoryDefOf.Manufactured
        };                 */

        public ComplexPriceModifier()
        {
            //this.faction = faction;
            thingPriceModifiers = new List<ThingPriceModifier>();
            thingCategoryPriceModifiers = new List<ThingCategoryPriceModifier>();

        }

        public virtual float GetPriceMultipilerFor(ThingDef thingDef, TradeAction action, ConsideredFactors factor = ConsideredFactors.All)
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
                modifier.RecordNewDeal(action, totalCost);          // mb divide totalCost by current multipiler?
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

        

        public static ModifierCategory GetModifierCategoryFor(ThingDef thingDef, out ThingCategoryDef definingCategory)              
        {
            if (thingDef==null || thingDef.thingCategories.Count==0)
            {
                definingCategory = null;
                return ModifierCategory.None;
            }

            var thingSpecificMod = thingDef.GetModExtension<PriceModifierCategoryDefExtension>();
            if (thingSpecificMod != null)
            {
                definingCategory = null;
                return thingSpecificMod.category;
            }

            var node = thingDef.thingCategories[0];                           // It appears that almost every thing has only one category
                                                                              

            /*ThingCategoryDef node = thingDef.thingCategories.Last();        
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
            }*/

            while(node!=null)
            {
                var extension = node.GetModExtension<PriceModifierCategoryDefExtension>();
                if (extension != null && extension.category != ModifierCategory.None)
                {
                    definingCategory = node;
                    return extension.category;
                }
                node = node.parent;
            }

            definingCategory = null;
            return ModifierCategory.None;
        }

        public static ModifierCategory GetModifierCategoryFor(ThingCategoryDef thingCategoryDef)
        {
            if (thingCategoryDef == null)
                return ModifierCategory.None;


            var extension = thingCategoryDef.GetModExtension<PriceModifierCategoryDefExtension>();
            if (extension != null && extension.category != ModifierCategory.None)
                return extension.category;

            return ModifierCategory.None;
        }

        public void SetBaseModifier(ThingDef def, float baseSellFactor, float baseBuyFactor)
        {
            var modifier = GetOrCreateIfNeededTradeablePriceModifier(def);
            if (modifier==null)
            {
                Log.Error("tried to set base multipilers for None- or constant-category thing");    //TODO make const category available for setting base mods
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
