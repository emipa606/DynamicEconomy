using System.Collections.Generic;
using RimWorld;
using Verse;

namespace DynamicEconomy;

public class Alert_PsiCoinReady : Alert
{
    public Alert_PsiCoinReady()
    {
        defaultLabel = "DE_PsiCoinReadyAlert_Label".Translate();
        defaultExplanation = "DE_PsiCoinReadyAlert_Explanation".Translate();
    }

    public override AlertReport GetReport()
    {
        var pawnsWithCoinReady = new List<Pawn>();
        foreach (var pawn in Find.World.PlayerPawnsForStoryteller)
        {
            var hediff = pawn.health.hediffSet.hediffs.Find(h => h.def == DynamicEconomyDefOf.PsiCoinMining);

            var miningComp = hediff?.TryGetComp<HediffComp_PsiCoinMining>();
            if (miningComp is { psiCoinReady: true })
            {
                pawnsWithCoinReady.Add(pawn);
            }
        }

        return pawnsWithCoinReady.Count == 0 ? false : AlertReport.CulpritsAre(pawnsWithCoinReady);
    }
}