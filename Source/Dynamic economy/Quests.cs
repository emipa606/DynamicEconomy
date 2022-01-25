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
        [NoTranslate]
        public SlateRef<string> storeCategoryAs;
        [NoTranslate]
        public SlateRef<string> storePlayerSellsFactorAs;
        [NoTranslate]
        public SlateRef<string> storePlayerBuysFactorAs;


        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;

            ThingCategoryDef requestedCategory = ThingCategoryDefOf.Medicine;       //Placeholder, redo with random
            slate.Set(storeCategoryAs.GetValue(slate), requestedCategory);

            float playerBuysFactor = 2f;            //Also tmp placeholder
            float playerSellsFactor = 2f;

            slate.Set(storePlayerSellsFactorAs.GetValue(slate), playerSellsFactor);
            slate.Set(storePlayerBuysFactorAs.GetValue(slate), playerBuysFactor);
        }

        protected override bool TestRunInt(Slate slate)
        {
            //TODO checks for empty strings

            ThingCategoryDef requestedCategory = ThingCategoryDefOf.Medicine;       //Placeholder, redo with rand
            slate.Set(storeCategoryAs.GetValue(slate), requestedCategory);

            float playerBuysFactor = 2f;            //Also tmp placeholder
            float playerSellsFactor = 2f;

            slate.Set(storePlayerSellsFactorAs.GetValue(slate), playerSellsFactor);
            slate.Set(storePlayerBuysFactorAs.GetValue(slate), playerBuysFactor);

            return true;
        }
    }

    public class QuestNode_HighDemand_Init : QuestNode
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
        public SlateRef<string> storeCategoryStringAs;

        public SlateRef<float> playerBuysFactor;
        public SlateRef<string> storeBuyFactorStringAs;

        public SlateRef<float> playerSellsFactor;
        public SlateRef<string> storeSellFactorStringAs;



        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            ThingCategoryDef categoryDef = category.GetValue(slate);

            slate.Set(storeBuyFactorStringAs.GetValue(slate), playerBuysFactor.GetValue(slate).ToString("F2"));
            slate.Set(storeSellFactorStringAs.GetValue(slate), playerSellsFactor.GetValue(slate).ToString("F2"));


            if (categoryDef == ThingCategoryDefOf.Medicine)
                slate.Set(storeCategoryStringAs.GetValue(slate), "Medicine");



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


}
