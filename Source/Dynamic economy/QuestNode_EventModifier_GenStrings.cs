using RimWorld.QuestGen;
using Verse;

namespace DynamicEconomy;

public class QuestNode_EventModifier_GenStrings : QuestNode
{
    public SlateRef<ThingCategoryDef> category;

    public SlateRef<float> playerBuysFactor;

    public SlateRef<float> playerSellsFactor;

    [NoTranslate] public SlateRef<string> storeBuyFactorStringAs;

    [NoTranslate] public SlateRef<string> storeCategoryStringAs;

    [NoTranslate] public SlateRef<string> storeSellFactorStringAs;


    protected override void RunInt()
    {
        var slate = QuestGen.slate;

        slate.Set(storeBuyFactorStringAs.GetValue(slate), playerBuysFactor.GetValue(slate).ToString("F2"));
        slate.Set(storeSellFactorStringAs.GetValue(slate), playerSellsFactor.GetValue(slate).ToString("F2"));

        slate.Set(storeCategoryStringAs.GetValue(slate), category.GetValue(slate).defName);
    }

    protected override bool TestRunInt(Slate slate)
    {
        var categoryDef = category.GetValue(slate);
        if (categoryDef == null)
        {
            return false;
        }

        slate.Set(storeCategoryStringAs.GetValue(slate), categoryDef);
        slate.Set(storeBuyFactorStringAs.GetValue(slate), playerBuysFactor.GetValue(slate).ToString("F2"));
        slate.Set(storeSellFactorStringAs.GetValue(slate), playerSellsFactor.GetValue(slate).ToString("F2"));

        return true;
    }
}