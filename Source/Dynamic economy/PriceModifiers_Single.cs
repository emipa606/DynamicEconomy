using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.Planet;
using System.Xml;

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
        Base,
        Dynamic,
        Event,
        All,
        Stockpile
    }

    
    // xml stuff

    public class BaseThingPriceMultipilerInfo
    {
        public string thingDefName="";
        public float buyMultiplier = 1f;
        public float sellMultiplier = 1f;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 1)
            {
                Log.Error("Biome thing price multipiler error at " + xmlRoot.OuterXml);
                return;
            }

            thingDefName = xmlRoot.Name;
            buyMultiplier = ParseHelper.ParseFloat(xmlRoot.FirstChild.Value);
            if (xmlRoot.Attributes.Count > 0)
                sellMultiplier = ParseHelper.ParseFloat(xmlRoot.Attributes["Sell"].Value);
            else sellMultiplier = buyMultiplier;
        }
    }

    public class BaseCategoryPriceMultipilerInfo
    {
        public string categoryDefName = "";
        public float buyMultiplier = 1f;
        public float sellMultiplier = 1f;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 1)
            {
                Log.Error("Biome thing price multipiler error at " + xmlRoot.OuterXml);
                return;
            }

            categoryDefName = xmlRoot.Name;
            buyMultiplier = ParseHelper.ParseFloat(xmlRoot.FirstChild.Value);
            if (xmlRoot.Attributes.Count > 0)
                sellMultiplier = ParseHelper.ParseFloat(xmlRoot.Attributes["Sell"].Value);
            else sellMultiplier = buyMultiplier;
        }
    }



    /// <summary>
    /// A simple price modifier, contains only multipilers and provides methods for them
    /// </summary>
    /// No info about what price of which thing is multipiled

    public class TradeablePriceModifier : IExposable
    {
        public const float BaseCostToDoubleFactor = 7000f;
        public static float CostToDoubleFactor => DESettings.costToDoublePriceMultipiler*BaseCostToDoubleFactor;
        public static float CostToHalveFactor => DESettings.costToHalvePriceMultipiler*BaseCostToDoubleFactor;

        public float playerSellsFactor;
        public float playerBuysFactor;

        public float playerBuysFactorEvent;                     // unaffected by trades              
        public float playerSellsFactorEvent;

        public float baseSellFactor;
        public float baseBuyFactor;
        public float baseStockpileFactor;

        public float GetPriceMultipiler(TradeAction action, ConsideredFactors factor=ConsideredFactors.All)
        {
            switch(factor)
            {

                case ConsideredFactors.Dynamic:                 
                    return action == TradeAction.PlayerBuys ? playerBuysFactor : (action == TradeAction.PlayerSells ? playerSellsFactor : 1f);

                case ConsideredFactors.Event:
                    return action == TradeAction.PlayerBuys ? playerBuysFactorEvent : (action == TradeAction.PlayerSells ? playerSellsFactorEvent : 1f);

                case ConsideredFactors.Base:
                    return action == TradeAction.PlayerBuys ? baseBuyFactor : (action == TradeAction.PlayerSells ? baseSellFactor : 1f);

                case ConsideredFactors.Stockpile:
                    return playerBuysFactorEvent * baseStockpileFactor;

                case ConsideredFactors.All:
                default:
                    return action == TradeAction.PlayerBuys ? playerBuysFactorEvent*playerBuysFactor*baseBuyFactor : (action == TradeAction.PlayerSells ? playerSellsFactorEvent*playerSellsFactor*baseSellFactor : 1f);
            }    
        }
        //public bool HasNoEffect =>playerBuysFactor == 1f && playerSellsFactor == 1f && playerBuysFactorEvent==1f && playerSellsFactorEvent==1f;


        public void ResetFactors()
        {
            playerBuysFactor = 1f;
            playerSellsFactor = 1f;

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
                playerBuysFactor += baseTotalPrice / CostToDoubleFactor;                
            }
            else
            {
                playerSellsFactor /= (float)Math.Pow(2, baseTotalPrice / CostToHalveFactor);        
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

        public ThingPriceModifier() { }

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
        public ThingCategoryPriceModifier() { }

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
    
}
