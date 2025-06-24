using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace DynamicEconomy;

public class QuestNode_HighSupply_GetEventModifiers : QuestNode
{
    private SlateRef<Settlement> settlement;

    [NoTranslate] private SlateRef<string> storeCategoryAs;

    [NoTranslate] private SlateRef<string> storePlayerBuysFactorAs;

    [NoTranslate] private SlateRef<string> storePlayerSellsFactorAs;

    private static ThingCategoryDef getCategoryDef(Settlement settlement)
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

            if (season is Season.Summer or Season.PermanentSummer
                or Season.Fall) // cant have a harvest during/after winter 
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
            if (season is Season.Summer or Season.PermanentSummer or Season.Fall)
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
                .GetPriceMultiplier(TradeAction.PlayerBuys, ConsideredFactors.Event);
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


        slate.Set(storeCategoryAs.GetValue(slate), getCategoryDef(settlement.GetValue(slate)));

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

        slate.Set(storeCategoryAs.GetValue(slate), getCategoryDef(settlement.GetValue(slate)));

        var eventFactor = 0.3f + (Rand.Value * 0.3f);

        slate.Set(storePlayerSellsFactorAs.GetValue(slate), eventFactor);
        slate.Set(storePlayerBuysFactorAs.GetValue(slate), eventFactor);

        return true;
    }
}