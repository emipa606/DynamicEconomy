using System;
using RimWorld;
using Verse;

namespace DynamicEconomy;

public class ThoughtWorker_HardPsiCoinMining : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        var stage = -1;
        foreach (var pawn in Find.World.PlayerPawnsForStoryteller)
        {
            if (pawn == p || !pawn.IsFreeNonSlaveColonist)
            {
                continue;
            }

            var miningHediff = pawn.health.hediffSet.GetFirstHediffOfDef(DynamicEconomyDefOf.PsiCoinMining);
            if (miningHediff is { Severity: > 0.5f })
            {
                stage++;
            }
        }

        return stage < 0 ? false : ThoughtState.ActiveAtStage(Math.Min(stage, 2));
    }
}