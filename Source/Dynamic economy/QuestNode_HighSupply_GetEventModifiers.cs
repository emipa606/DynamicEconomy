using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace DynamicEconomy;

public class QuestNode_HighSupply_GetEventModifiers : QuestNode
{
    public SlateRef<Settlement> settlement;

    [NoTranslate] public SlateRef<string> storeCategoryAs;

    [NoTranslate] public SlateRef<string> storePlayerBuysFactorAs;

    [NoTranslate] public SlateRef<string> storePlayerSellsFactorAs;

    private ThingCategoryDef GetCategoryDef(Settlement settlement)
    {
        var allowedCats = new List<ThingCategoryDef>
        {
            ThingCategoryDefOf.Medicine,
            ThingCategoryDefOf.Drugs,
            ThingCategoryDefOf.Apparel,
            ThingCategoryDefOf.BodyParts
        };

        var season = GenLocalDate.Season(settlement.Tile);

        if (ModsConfig.IdeologyActive)
        {
            var primaIdeo = settlement.Faction.ideos.PrimaryIdeo;

            if (!primaIdeo.HasMeme(DefDatabase<MemeDef>.GetNamed("Raider")))
            {
                allowedCats.Add(ThingCategoryDefOf
                    .Weapons); // raiders won't give away their guns and knifes, won't they?
            }

            if (season == Season.Summer || season == Season.PermanentSummer ||
                season == Season.Fall) // cant have a harvest during/after winter 
            {
                if (!primaIdeo.HasMeme(DefDatabase<MemeDef>.GetNamed("AnimalPersonhood")))
                {
                    allowedCats.Add(ThingCategoryDefOf.MeatRaw);
                }

                if (!primaIdeo.HasMeme(DynamicEconomyDefOf.Rancher))
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
        // it wouldn't make any difference to run through list and remove a couple of items
        var settlementPriceMod =
            GameComponent_EconomyStateTracker.CurGameInstance.GetOrCreateIfNeededComplexModifier(settlement);
        for (var i = 0; i < allowedCats.Count; i++)
        {
            var curEventMod = settlementPriceMod.GetOrCreateIfNeededTradeablePriceModifier(allowedCats[i])
                .GetPriceMultipiler(TradeAction.PlayerBuys, ConsideredFactors.Event);
            if (!(curEventMod > 1.05f) && !(curEventMod < 0.96f))
            {
                continue;
            }

            allowedCats.RemoveAt(i);
            i--;
        }

        return allowedCats.RandomElement();
    }

    protected override void RunInt()
    {
        var slate = QuestGen.slate;


        slate.Set(storeCategoryAs.GetValue(slate), GetCategoryDef(settlement.GetValue(slate)));

        var eventFactor = 0.3f + (Rand.Value * 0.3f);

        slate.Set(storePlayerSellsFactorAs.GetValue(slate), eventFactor);
        slate.Set(storePlayerBuysFactorAs.GetValue(slate), eventFactor);
    }

    protected override bool TestRunInt(Slate slate)
    {
        //TODO checks for empty strings

        if (settlement.GetValue(slate) == null)
        {
            return false;
        }

        slate.Set(storeCategoryAs.GetValue(slate), GetCategoryDef(settlement.GetValue(slate)));

        var eventFactor = 0.3f + (Rand.Value * 0.3f);

        slate.Set(storePlayerSellsFactorAs.GetValue(slate), eventFactor);
        slate.Set(storePlayerBuysFactorAs.GetValue(slate), eventFactor);

        return true;
    }
}