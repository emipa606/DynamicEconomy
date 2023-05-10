using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace DynamicEconomy;

public class JobDriver_RollForEconomicEvent : JobDriver
{
    private const int BaseTimeToDoTicks = 1500;

    private Building_CommsConsole Console => (Building_CommsConsole)TargetThingA;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Console, job, errorOnFailed: errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var manager = GameComponent_EconomyStateTracker.CurGameInstance.EconomicEventsManager;

        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        AddFailCondition(() => !manager.CanRollForEconomicEvent);

        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell)
            .FailOn(_ => !Console.CanUseCommsNow);

        yield return Toils_General.WaitWith(
                TargetIndex.A,
                (int)(BaseTimeToDoTicks * pawn.GetStatValue(StatDefOf.NegotiationAbility)),
                true)
            .FailOn(_ => !Console.CanUseCommsNow);

        yield return Toils_General.Do(() => manager.RollForEconomicEvent(pawn));
    }
}