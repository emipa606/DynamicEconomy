using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using RimWorld.QuestGen;

namespace DynamicEconomy
{
    public class QuestNode_HighDemand_GetEventModifiers : QuestNode
    {
        public SlateRef<Settlement> settlement;
        [NoTranslate]
        public SlateRef<string> storeCategoryAs;
        [NoTranslate]
        public SlateRef<string> storePlayerSellsFactorAs;
        [NoTranslate]
        public SlateRef<string> storePlayerBuysFactorAs;


        private ThingCategoryDef GetCategoryDef(Settlement settlement)
        {
            List<ThingCategoryDef> allowedCats = new List<ThingCategoryDef>()
            {
                ThingCategoryDefOf.Medicine,
                ThingCategoryDefOf.Drugs,
                ThingCategoryDefOf.BuildingsArt,
                ThingCategoryDefOf.Weapons
            };

            Season season = GenLocalDate.Season(settlement.Tile);

            if (ModsConfig.IdeologyActive)
            {
                var primaIdeo = settlement.Faction.ideos.PrimaryIdeo;

                if (!primaIdeo.HasMeme(DefDatabase<MemeDef>.GetNamed("AnimalPersonhood")))
                {
                    allowedCats.Add(ThingCategoryDefOf.MeatRaw);
                }

                if ((season == Season.Winter || season == Season.PermanentWinter || season == Season.Spring) && !primaIdeo.HasMeme(MemeDefOf.Rancher))
                {
                    allowedCats.Add(ThingCategoryDefOf.PlantFoodRaw);
                }

                if (!primaIdeo.HasMeme(MemeDefOf.Nudism))
                {
                    allowedCats.Add(ThingCategoryDefOf.Apparel);
                }

            }
            else
            {
                if (season == Season.Winter || season == Season.PermanentWinter || season == Season.Spring)
                    allowedCats.Add(ThingCategoryDefOf.PlantFoodRaw);


                allowedCats.Add(ThingCategoryDefOf.MeatRaw);

                allowedCats.Add(ThingCategoryDefOf.Apparel);
            }


            var settlementPriceMod = GameComponent_EconomyStateTracker.CurGameInstance.GetOrCreateIfNeededSettlementModifier(settlement);
            for (int i = 0; i < allowedCats.Count; i++)
            {
                float curEventMod = settlementPriceMod.GetOrCreateIfNeededTradeablePriceModifier(allowedCats[i]).GetPriceMultipiler(TradeAction.PlayerBuys, ConsideredFactors.Event);
                if (curEventMod > 1.05f || curEventMod < 0.96f)
                {
                    allowedCats.RemoveAt(i);
                    i--;
                }
            }

            return allowedCats.RandomElement();
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            Settlement settlement = this.settlement.GetValue(slate);

            

            slate.Set(storeCategoryAs.GetValue(slate), GetCategoryDef(settlement));

            float eventFactor = 1.4f + Rand.Value * 0.6f;

            float playerBuysFactor = eventFactor;
            float playerSellsFactor = eventFactor;

            slate.Set(storePlayerSellsFactorAs.GetValue(slate), playerSellsFactor);
            slate.Set(storePlayerBuysFactorAs.GetValue(slate), playerBuysFactor);
        }

        protected override bool TestRunInt(Slate slate)
        {
            //TODO checks for empty strings
            if (settlement.GetValue(slate) == null)
                return false;

            slate.Set(storeCategoryAs.GetValue(slate), GetCategoryDef(settlement.GetValue(slate)));

            float eventFactor = 1.4f + Rand.Value * 0.6f;

            float playerBuysFactor = eventFactor;
            float playerSellsFactor = eventFactor;

            slate.Set(storePlayerSellsFactorAs.GetValue(slate), playerSellsFactor);
            slate.Set(storePlayerBuysFactorAs.GetValue(slate), playerBuysFactor);

            return true;
        }
    }

    public class QuestNode_EventModifier_Init : QuestNode
    {
        public SlateRef<Settlement> settlement;
        public SlateRef<ThingCategoryDef> requestedCategoryDef;
        public SlateRef<float> playerSellsFactor;
        public SlateRef<float> playerBuysFactor;

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            Settlement targetSettlement = settlement.GetValue(slate);
            ThingCategoryDef request = requestedCategoryDef.GetValue(slate);

            GameComponent_EconomyStateTracker.CurGameInstance.SetEventModifiersForSettlement(
                targetSettlement, 
                request, 
                playerSellsFactor.GetValue(slate), 
                playerBuysFactor.GetValue(slate));


        }
        protected override bool TestRunInt(Slate slate)
        {
            Settlement targetSettlement = settlement.GetValue(slate);
            ThingCategoryDef request = requestedCategoryDef.GetValue(slate);

            if (targetSettlement == null || request == null)
                return false;

            return true;
        }
    }

    public class QuestNode_EventModifier_GenStrings : QuestNode
    {
        public SlateRef<ThingCategoryDef> category;
        [NoTranslate]
        public SlateRef<string> storeCategoryStringAs;

        public SlateRef<float> playerBuysFactor;
        [NoTranslate]
        public SlateRef<string> storeBuyFactorStringAs;

        public SlateRef<float> playerSellsFactor;
        [NoTranslate]
        public SlateRef<string> storeSellFactorStringAs;



        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            ThingCategoryDef categoryDef = category.GetValue(slate);

            slate.Set(storeBuyFactorStringAs.GetValue(slate), playerBuysFactor.GetValue(slate).ToString("F2"));
            slate.Set(storeSellFactorStringAs.GetValue(slate), playerSellsFactor.GetValue(slate).ToString("F2"));

            slate.Set(storeCategoryStringAs.GetValue(slate), category.GetValue(slate).defName);



        }

        protected override bool TestRunInt(Slate slate)
        {
            ThingCategoryDef categoryDef = category.GetValue(slate);
            if (categoryDef == null)
                return false;

            if (categoryDef == ThingCategoryDefOf.Medicine)
                slate.Set(storeCategoryStringAs.GetValue(slate), "Medicine");

            return true;
        }
    }

    public class QuestNode_HighSupply_GetEventModifiers : QuestNode
    {
        public SlateRef<Settlement> settlement;
        [NoTranslate]
        public SlateRef<string> storeCategoryAs;
        [NoTranslate]
        public SlateRef<string> storePlayerSellsFactorAs;
        [NoTranslate]
        public SlateRef<string> storePlayerBuysFactorAs;

        private ThingCategoryDef GetCategoryDef(Settlement settlement)
        {
            List<ThingCategoryDef> allowedCats = new List<ThingCategoryDef>()
            {
                ThingCategoryDefOf.Medicine,
                ThingCategoryDefOf.Drugs,
                ThingCategoryDefOf.Apparel,
                ThingCategoryDefOf.BodyParts
            };

            Season season = GenLocalDate.Season(settlement.Tile);

            if (ModsConfig.IdeologyActive)
            {
                var primaIdeo = settlement.Faction.ideos.PrimaryIdeo;

                if (!primaIdeo.HasMeme(DefDatabase<MemeDef>.GetNamed("Raider")))
                {
                    allowedCats.Add(ThingCategoryDefOf.Weapons);        // raiders wont give away their guns and knifes, wont they?
                }

                if (season == Season.Summer || season == Season.PermanentSummer || season == Season.Fall)             // cant have a harvest during/after winter 
                {
                    if (!primaIdeo.HasMeme(DefDatabase<MemeDef>.GetNamed("AnimalPersonhood")))
                    {
                        allowedCats.Add(ThingCategoryDefOf.MeatRaw);
                    }

                    if (!primaIdeo.HasMeme(MemeDefOf.Rancher))
                    {
                        allowedCats.Add(ThingCategoryDefOf.PlantFoodRaw);
                    }
                }
            }
            else
            {
                if (season == Season.Summer || season == Season.PermanentSummer || season == Season.Fall)
                {
                    allowedCats.Add(ThingCategoryDefOf.PlantFoodRaw);
                    allowedCats.Add(ThingCategoryDefOf.MeatRaw);
                }
                allowedCats.Add(ThingCategoryDefOf.Weapons);
            }

            // it is not optimal to add and then remove things from list
            // but hey, it searches through defdatabase twice
            // it wouldnt make any difference to run through list and remove a couple of items
            var settlementPriceMod = GameComponent_EconomyStateTracker.CurGameInstance.GetOrCreateIfNeededSettlementModifier(settlement);
            for (int i = 0; i < allowedCats.Count; i++)
            {
                float curEventMod = settlementPriceMod.GetOrCreateIfNeededTradeablePriceModifier(allowedCats[i]).GetPriceMultipiler(TradeAction.PlayerBuys, ConsideredFactors.Event);
                if (curEventMod > 1.05f || curEventMod < 0.96f)
                {
                    allowedCats.RemoveAt(i);
                    i--;
                }
            }

            return allowedCats.RandomElement();
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;


            slate.Set(storeCategoryAs.GetValue(slate), GetCategoryDef(settlement.GetValue(slate)));

            float eventFactor = 0.4f + Rand.Value * 0.3f;

            float playerBuysFactor = eventFactor;           
            float playerSellsFactor = eventFactor;

            slate.Set(storePlayerSellsFactorAs.GetValue(slate), playerSellsFactor);
            slate.Set(storePlayerBuysFactorAs.GetValue(slate), playerBuysFactor);
        }

        protected override bool TestRunInt(Slate slate)
        {
            //TODO checks for empty strings

            if (settlement.GetValue(slate) == null)
                return false;

            slate.Set(storeCategoryAs.GetValue(slate), GetCategoryDef(settlement.GetValue(slate)));

            float eventFactor = 0.4f + Rand.Value*0.3f;

            float playerBuysFactor = eventFactor;
            float playerSellsFactor = eventFactor;

            slate.Set(storePlayerSellsFactorAs.GetValue(slate), playerSellsFactor);
            slate.Set(storePlayerBuysFactorAs.GetValue(slate), playerBuysFactor);

            return true;
        }
    }


}
