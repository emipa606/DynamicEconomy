using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace DynamicEconomy;

public class QuestNode_EventModifier_Init : QuestNode
{
    public SlateRef<float> playerBuysFactor;
    public SlateRef<float> playerSellsFactor;
    public SlateRef<ThingCategoryDef> requestedCategoryDef;
    public SlateRef<Settlement> settlement;

    protected override void RunInt()
    {
        var slate = QuestGen.slate;
        var targetSettlement = settlement.GetValue(slate);
        var request = requestedCategoryDef.GetValue(slate);

        GameComponent_EconomyStateTracker.CurGameInstance.SetEventModifiersForSettlement(
            targetSettlement,
            request,
            playerSellsFactor.GetValue(slate),
            playerBuysFactor.GetValue(slate));
    }

    protected override bool TestRunInt(Slate slate)
    {
        var targetSettlement = settlement.GetValue(slate);
        var request = requestedCategoryDef.GetValue(slate);

        return targetSettlement != null && targetSettlement.Faction != Faction.OfPlayer && request != null;
    }
}