using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace DynamicEconomy;

public class HediffComp_PsiCoinMining : HediffComp
{
    private const string LightMiningTexturePath = "UI/Commands/PsiCoinMining_Light";
    private const string HardMiningTexturePath = "UI/Commands/PsiCoinMining_Hard";
    private const string ExtractTexturePath = "UI/Commands/ExtractPsiCoin";
    private int _ticksTillNextPsiCoinInLightMode;
    public bool psiCoinReady;

    public int
        psiCoinsToBeDropped; // noone wants to click command every minute. If coins are mined too quick, more will be dropped with greater cd.


    public HediffComp_PsiCoinMining()
    {
        psiCoinReady = false;
    }

    public HediffCompProperties_PsiCoinMining Props => props as HediffCompProperties_PsiCoinMining;

    public int TicksTillNextPsiCoin => HardMiningOn
        ? _ticksTillNextPsiCoinInLightMode / Props.hardMiningMultipiler
        : _ticksTillNextPsiCoinInLightMode;

    public bool HardMiningOn => parent.Severity > 0.5f;

    //public int ApproxTicksTillPsiCoinOnLightMode => (int)((GameComponent_EconomyStateTracker.CurGameInstance.PsiCoinManager.psiCoinPrice / Props.silverPerDayLight) * 60000);
    private int PsiCoinDropCooldown =>
        (int)(GameComponent_EconomyStateTracker.CurGameInstance.PsiCoinManager.psiCoinPrice / Props.silverPerDayLight *
              60000);

    private void StartMiningNewCoin()
    {
        psiCoinReady = false;
        _ticksTillNextPsiCoinInLightMode = PsiCoinDropCooldown;
        psiCoinsToBeDropped =
            (480000 / _ticksTillNextPsiCoinInLightMode) +
            1; //if cd >8 days works as normal, if less - more coins will be dropped and cd will be at least 8 days
        _ticksTillNextPsiCoinInLightMode *= psiCoinsToBeDropped;
    }

    public override void CompPostMake()
    {
        base.CompPostMake();

        StartMiningNewCoin();
    }


    private Command_Toggle GenToggleCommand()
    {
        var toggleMiningModeCommand = new Command_Toggle
        {
            isActive = () => HardMiningOn, // is on if in hard mode
            icon = HardMiningOn
                ? ContentFinder<Texture2D>.Get(HardMiningTexturePath)
                : ContentFinder<Texture2D>.Get(LightMiningTexturePath),
            defaultLabel = "DE_ToggleMiningModeCommand_Label".Translate(),
            defaultDesc = "DE_ToggleMiningModeCommand_Desc".Translate(),
            toggleAction = () => { parent.Severity = HardMiningOn ? 0.25f : 0.75f; }
        };


        return toggleMiningModeCommand;
    }

    public Command_Action GenExtractCommand()
    {
        var extractCommand = new Command_Action
        {
            defaultLabel = "DE_ExtractCoinCommand_Label".Translate(),
            defaultDesc = "DE_ExtractCoinCommand_Desc".Translate(),
            icon = ContentFinder<Texture2D>.Get(ExtractTexturePath),
            action = () =>
            {
                var coin = ThingMaker.MakeThing(DynamicEconomyDefOf.PsiCoin);
                coin.stackCount = psiCoinsToBeDropped;
                var poorSoul = parent.pawn;
                GenPlace.TryPlaceThing(coin, poorSoul.Position, poorSoul.Map, ThingPlaceMode.Near);

                StartMiningNewCoin();
            }
        };

        return extractCommand;
    }


    public override void CompPostTick(ref float severityAdjustment)
    {
        if (psiCoinReady)
        {
            return;
        }


        _ticksTillNextPsiCoinInLightMode -=
            HardMiningOn
                ? Props.hardMiningMultipiler
                : 1; // psicoin price shift correction? TODO. If got time. And a will.
        if (_ticksTillNextPsiCoinInLightMode <= 0)
        {
            psiCoinReady = true;
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmos()
    {
        yield return GenToggleCommand();

        var extractCommand = GenExtractCommand();
        if (!psiCoinReady)
        {
            extractCommand.Disable(
                "DE_ExtractCoinCommand_Unavailable_TillNextCoin".Translate(TicksTillNextPsiCoin.ToStringTicksToDays()));
        }

        yield return extractCommand;


        if (!Prefs.DevMode)
        {
            yield break;
        }

        var debug_setCoinsReady = new Command_Action
        {
            action = () => _ticksTillNextPsiCoinInLightMode = 0,
            defaultLabel = "Set psicoin ready"
        };
        yield return debug_setCoinsReady;
    }

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Values.Look(ref _ticksTillNextPsiCoinInLightMode, "ticksTillPsiCoin");
        Scribe_Values.Look(ref psiCoinReady, "psiCoinReady");
        Scribe_Values.Look(ref psiCoinsToBeDropped, "psiCoinsToBeDropped");
    }
}