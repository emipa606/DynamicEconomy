using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace DynamicEconomy;

public class QuestNode_HighDemand_GetEventModifiers : QuestNode
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
            ThingCategoryDefOf.BuildingsArt,
            ThingCategoryDefOf.Weapons
        };

        var season = GenLocalDate.Season(settlement.Tile);

        if (ModsConfig.IdeologyActive)
        {
            var primaIdeo = settlement.Faction.ideos.PrimaryIdeo;

            if (!primaIdeo.HasMeme(DefDatabase<MemeDef>.GetNamed("AnimalPersonhood")))
            {
                allowedCats.Add(ThingCategoryDefOf.MeatRaw);
            }

            if ((season == Season.Winter || season == Season.PermanentWinter || season == Season.Spring) &&
                !primaIdeo.HasMeme(DynamicEconomyDefOf.Rancher))
            {
                allowedCats.Add(ThingCategoryDefOf.PlantFoodRaw);
            }

            if (!primaIdeo.HasMeme(DynamicEconomyDefOf.Nudism))
            {
                allowedCats.Add(ThingCategoryDefOf.Apparel);
            }
        }
        else
        {
            if (season == Season.Winter || season == Season.PermanentWinter || season == Season.Spring)
            {
                allowedCats.Add(ThingCategoryDefOf.PlantFoodRaw);
            }


            allowedCats.Add(ThingCategoryDefOf.MeatRaw);

            allowedCats.Add(ThingCategoryDefOf.Apparel);
        }


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
        var value = settlement.GetValue(slate);


        slate.Set(storeCategoryAs.GetValue(slate), GetCategoryDef(value));

        var eventFactor = 1.6f + (Rand.Value * 0.6f);

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

        var eventFactor = 1.6f + (Rand.Value * 0.6f);

        slate.Set(storePlayerSellsFactorAs.GetValue(slate), eventFactor);
        slate.Set(storePlayerBuysFactorAs.GetValue(slate), eventFactor);

        return true;
    }
}