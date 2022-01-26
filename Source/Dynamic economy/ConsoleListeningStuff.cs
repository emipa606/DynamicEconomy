using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace DynamicEconomy
{
    public class JobDriver_RollForEconomicEvent : JobDriver
    {
        private const int BaseTimeToDoTicks = 1500;

        private Building_CommsConsole Console => (Building_CommsConsole)TargetThingA;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.Reserve(Console, job, errorOnFailed: errorOnFailed))
                return false;

            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var manager = GameComponent_EconomyStateTracker.CurGameInstance.EconomicEventsManager;

            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            AddFailCondition(() => !manager.CanRollForEconomicEvent);

            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn((to => !(Console.CanUseCommsNow)));

            yield return Toils_General.WaitWith(
                TargetIndex.A, 
                (int)(BaseTimeToDoTicks * pawn.GetStatValue(StatDefOf.NegotiationAbility)),
                useProgressBar: true)
                .FailOn(to => !(Console.CanUseCommsNow));

            yield return Toils_General.Do(() => manager.RollForEconomicEvent(pawn));
        }
    }

    public class CompDEEventRoll : ThingComp
    {
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            var manager = GameComponent_EconomyStateTracker.CurGameInstance.EconomicEventsManager;

            FloatMenuOption option;

            if (!manager.CanRollForEconomicEvent)
            {
                option = new FloatMenuOption("Gather info (unavailable, " + (60000 - manager.ticksSinceLastEventRoll).ToStringTicksToPeriod() + " untill next opportunity", null);
                
                yield return option;
                yield break;
            }

            if (selPawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
            {
                option = new FloatMenuOption("Gather info (unavailable, " + selPawn.Name + " is incapable of social interactions", null);
                
                yield return option;
                yield break;
            }

            float rollChance = manager.EventRollSuccessChance(selPawn);
            Job job = JobMaker.MakeJob(DynamicEconomyDefOf.GatherInfo);
            job.targetA = parent;
            job.count = 1;

            option = new FloatMenuOption(
                "Gather info (" + rollChance.ToStringPercent() + " success chance)",
                () => selPawn.jobs.TryTakeOrderedJob(job));

            yield return option;

        }
    }

    public class CompProperties_DEEventRoll : CompProperties
    {
        public CompProperties_DEEventRoll() => compClass = typeof(CompDEEventRoll);
    }
}
