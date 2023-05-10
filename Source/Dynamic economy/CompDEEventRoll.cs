using System.Collections.Generic;
using RimWorld;
using Verse;

namespace DynamicEconomy;

public class CompDEEventRoll : ThingComp
{
    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        var manager = GameComponent_EconomyStateTracker.CurGameInstance.EconomicEventsManager;

        FloatMenuOption option;

        if (!manager.CanRollForEconomicEvent)
        {
            option = new FloatMenuOption(
                "DE_GatherInfo_UnavailableCooldown".Translate((60000 - manager.ticksSinceLastEventRoll)
                    .ToStringTicksToPeriod()), null);

            yield return option;
            yield break;
        }

        if (selPawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
        {
            option = new FloatMenuOption("DE_GatherInfo_UnavailablePawnIsIncapable".Translate(selPawn.ToString()),
                null);

            yield return option;
            yield break;
        }

        var rollChance = manager.EventRollSuccessChance(selPawn);
        var job = JobMaker.MakeJob(DynamicEconomyDefOf.GatherInfo);
        job.targetA = parent;
        job.count = 1;

        option = new FloatMenuOption(
            "DE_GatherInfo".Translate(rollChance.ToStringPercent()),
            () => selPawn.jobs.TryTakeOrderedJob(job));

        yield return option;
    }
}