using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace DynamicEconomy
{

    
    public class PsiCoinManager : IExposable
    {
        public float psiCoinPrice=DynamicEconomyDefOf.PsiCoin.BaseMarketValue;
        public const float PsiCoinDefaultRandomOffset = 0.003f;
        public void TickLong()
        {
            psiCoinPrice *= 1f+Rand.Sign*Rand.Value*PsiCoinDefaultRandomOffset*DESettings.randyCoinRandomOfsettMultipiler;               
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref psiCoinPrice, "psiCoinPrice", DynamicEconomyDefOf.PsiCoin.BaseMarketValue);
        }
    }




    public class HediffComp_PsiCoinMining : HediffComp
    {
        public HediffCompProperties_PsiCoinMining Props => props as HediffCompProperties_PsiCoinMining;
        private int _ticksTillNextPsiCoinInLightMode;
        public int psiCoinsToBeDropped;         // noone wants to click command every minute. If coins are mined too quick, more will be dropped with greater cd.
        public int TicksTillNextPsiCoin => HardMiningOn ? (int)(_ticksTillNextPsiCoinInLightMode / Props.hardMiningMultipiler) : _ticksTillNextPsiCoinInLightMode;
        public bool psiCoinReady;
        public bool HardMiningOn => parent.Severity>0.5f;
        //public int ApproxTicksTillPsiCoinOnLightMode => (int)((GameComponent_EconomyStateTracker.CurGameInstance.PsiCoinManager.psiCoinPrice / Props.silverPerDayLight) * 60000);
        private int PsiCoinDropCooldown => (int)((GameComponent_EconomyStateTracker.CurGameInstance.PsiCoinManager.psiCoinPrice / Props.silverPerDayLight) * 60000);


        private const string LightMiningTexturePath = "UI/Commands/PsiCoinMining_Light";
        private const string HardMiningTexturePath = "UI/Commands/PsiCoinMining_Hard";
        private const string ExtractTexturePath = "UI/Commands/ExtractPsiCoin";

        private void StartMiningNewCoin()
        {
            psiCoinReady = false;
            _ticksTillNextPsiCoinInLightMode = PsiCoinDropCooldown;
            psiCoinsToBeDropped = 480000 / _ticksTillNextPsiCoinInLightMode + 1;        //if cd >8 days works as normal, if less - more coins will be dropped and cd will be at least 8 days
            _ticksTillNextPsiCoinInLightMode *= psiCoinsToBeDropped;
        }

        public override void CompPostMake()
        {
            base.CompPostMake();

            StartMiningNewCoin();
        }


        private Command_Toggle GenToggleCommand()
        {
            Command_Toggle toggleMiningModeCommand = new Command_Toggle();
            toggleMiningModeCommand.isActive = () => HardMiningOn;      // is on if in hard mode



            toggleMiningModeCommand.icon = HardMiningOn ? ContentFinder<Texture2D>.Get(HardMiningTexturePath) : ContentFinder<Texture2D>.Get(LightMiningTexturePath);
            toggleMiningModeCommand.defaultLabel = "DE_ToggleMiningModeCommand_Label".Translate();
            toggleMiningModeCommand.defaultDesc = "DE_ToggleMiningModeCommand_Desc".Translate();


            toggleMiningModeCommand.toggleAction = () =>
            {
                parent.Severity = HardMiningOn ? 0.25f : 0.75f;
            };

            return toggleMiningModeCommand;
        }

        public Command_Action GenExtractCommand()
        {
            Command_Action extractCommand = new Command_Action();
            extractCommand.defaultLabel = "DE_ExtractCoinCommand_Label".Translate();
            extractCommand.defaultDesc = "DE_ExtractCoinCommand_Desc".Translate();
            
            extractCommand.icon = ContentFinder<Texture2D>.Get(ExtractTexturePath);
            extractCommand.action = () =>
            {
                var coin = ThingMaker.MakeThing(DynamicEconomyDefOf.PsiCoin);
                coin.stackCount = psiCoinsToBeDropped;
                Pawn poorSoul = parent.pawn;
                GenPlace.TryPlaceThing(coin, poorSoul.Position, poorSoul.Map, ThingPlaceMode.Near);

                StartMiningNewCoin();
            };

            return extractCommand;
        }


        public HediffComp_PsiCoinMining()
        {
            psiCoinReady = false;
        }



        public override void CompPostTick(ref float severityAdjustment)
        {

            if (psiCoinReady)
                return;


            _ticksTillNextPsiCoinInLightMode-=HardMiningOn ? Props.hardMiningMultipiler : 1;            // psicoin price shift correction? TODO. If got time. And a will.
            if (_ticksTillNextPsiCoinInLightMode<=0)
            {
                psiCoinReady = true;
            }

        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            yield return GenToggleCommand();
            
            var extractCommand = GenExtractCommand();
            if (!psiCoinReady)
                extractCommand.Disable("DE_ExtractCoinCommand_Unavailable_TillNextCoin".Translate(TicksTillNextPsiCoin.ToStringTicksToDays()));
            yield return extractCommand;


            if (Prefs.DevMode)
            {
                Command_Action debug_setCoinsReady = new Command_Action();
                debug_setCoinsReady.action = () => _ticksTillNextPsiCoinInLightMode = 0;
                debug_setCoinsReady.defaultLabel = "Set psicoin ready";
                yield return debug_setCoinsReady;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref _ticksTillNextPsiCoinInLightMode, "ticksTillPsiCoin");
            Scribe_Values.Look(ref psiCoinReady, "psiCoinReady");
            Scribe_Values.Look(ref psiCoinsToBeDropped, "psiCoinsToBeDropped");
        }

    }

    public class HediffCompProperties_PsiCoinMining : HediffCompProperties
    {
        public float silverPerDayLight;
        public int hardMiningMultipiler;

        public HediffCompProperties_PsiCoinMining() => compClass = typeof(HediffComp_PsiCoinMining);
    }




    public class Alert_PsiCoinReady : Alert
    {

        public Alert_PsiCoinReady()
        {
            defaultLabel = "DE_PsiCoinReadyAlert_Label".Translate();
            defaultExplanation = "DE_PsiCoinReadyAlert_Explanation".Translate();
        }

        public override AlertReport GetReport()
        {

            List<Pawn> pawnsWithCoinReady = new List<Pawn>();
            foreach (Pawn pawn in Find.World.PlayerPawnsForStoryteller)
            {
                var hediff = pawn.health.hediffSet.hediffs.Find(h => h.def == DynamicEconomyDefOf.PsiCoinMining);

                if (hediff != null)
                {
                    var miningComp = hediff.TryGetComp<HediffComp_PsiCoinMining>();
                    if (miningComp!=null && miningComp.psiCoinReady)
                        pawnsWithCoinReady.Add(pawn);
                }
            }

            if (pawnsWithCoinReady.Count == 0)
                return (AlertReport)false;

            return AlertReport.CulpritsAre(pawnsWithCoinReady);
            
        }
    }



    // mb combine those workers? less polls => less lags, but will have to write common desc and stages for both thoughts


    public class ThoughtWorker_LightPsiCoinMining : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            int stage = -1;
            foreach (var pawn in Find.World.PlayerPawnsForStoryteller)
            {
                if (pawn != p && pawn.IsFreeNonSlaveColonist)
                {
                    var miningHediff = pawn.health.hediffSet.GetFirstHediffOfDef(DynamicEconomyDefOf.PsiCoinMining);
                    if (miningHediff != null)
                    {
                        if (miningHediff.Severity < 0.5f)
                            stage++;
                    }
                }
            }

            return stage < 0 ? false : ThoughtState.ActiveAtStage(Math.Min(stage, 3));
        }
    }

    public class ThoughtWorker_HardPsiCoinMining : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            int stage = -1;
            foreach (var pawn in Find.World.PlayerPawnsForStoryteller)
            {
                if (pawn != p && pawn.IsFreeNonSlaveColonist)
                {
                    var miningHediff = pawn.health.hediffSet.GetFirstHediffOfDef(DynamicEconomyDefOf.PsiCoinMining);
                    if (miningHediff != null)
                    {
                        if (miningHediff.Severity > 0.5f)
                            stage++;
                    }
                }
            }

            return stage < 0 ? false : ThoughtState.ActiveAtStage(Math.Min(stage, 2));
        }
    }
}
