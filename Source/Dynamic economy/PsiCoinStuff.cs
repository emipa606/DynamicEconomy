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
        public float PsiCoinDefaultRandomOffset = 0.02f;
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
        public int TicksTillNextPsiCoin => HardMiningOn ? (int)(_ticksTillNextPsiCoinInLightMode / Props.hardMiningMultipiler) : _ticksTillNextPsiCoinInLightMode;
        public bool psiCoinReady;
        public bool HardMiningOn => parent.Severity>0.5f;
        public int ApproxTicksTillPsiCoinOnLightMode => (int)((GameComponent_EconomyStateTracker.CurGameInstance.PsiCoinManager.psiCoinPrice / Props.silverPerDayLight) * 60000);


        private Texture2D lightMiningTexture = ContentFinder<Texture2D>.Get("UI/Commands/PsiCoinMining_Light");
        private Texture2D hardMiningTexture = ContentFinder<Texture2D>.Get("UI/Commands/PsiCoinMining_Hard");
        private Texture2D extractTexture = ContentFinder<Texture2D>.Get("UI/Commands/ExtractPsiCoin");


        public override void CompPostMake()
        {
            base.CompPostMake();

            _ticksTillNextPsiCoinInLightMode = PsiCoinDropCooldown;
        }


        private Command_Toggle GenToggleCommand()
        {
            Command_Toggle toggleMiningModeCommand = new Command_Toggle();
            toggleMiningModeCommand.isActive = () => HardMiningOn;      // is on if in hard mode

            toggleMiningModeCommand.icon = HardMiningOn ? hardMiningTexture : lightMiningTexture;
            toggleMiningModeCommand.defaultLabel = "Toggle mining mode";


            toggleMiningModeCommand.toggleAction = () =>
            {
                parent.Severity = HardMiningOn ? 0.25f : 0.75f;
            };

            return toggleMiningModeCommand;
        }

        public Command_Action GenExtractCommand()
        {
            Command_Action extractCommand = new Command_Action();
            extractCommand.defaultLabel = "Extract coin";
            
            extractCommand.icon = extractTexture;
            extractCommand.action = () =>
            {
                var coin = ThingMaker.MakeThing(DynamicEconomyDefOf.PsiCoin);
                Pawn poorSoul = parent.pawn;
                GenPlace.TryPlaceThing(coin, poorSoul.Position, poorSoul.Map, ThingPlaceMode.Near);
                psiCoinReady = false;
            };

            return extractCommand;
        }


        public HediffComp_PsiCoinMining()
        {
            psiCoinReady = false;
        }

        private int PsiCoinDropCooldown =>(int)((GameComponent_EconomyStateTracker.CurGameInstance.PsiCoinManager.psiCoinPrice/Props.silverPerDayLight)*60000);


        public override void CompPostTick(ref float severityAdjustment)
        {

            if (psiCoinReady)
                return;


            _ticksTillNextPsiCoinInLightMode-=HardMiningOn ? Props.hardMiningMultipiler : 1;
            if (_ticksTillNextPsiCoinInLightMode<=0)
            {
                psiCoinReady = true;
                _ticksTillNextPsiCoinInLightMode = PsiCoinDropCooldown;
            }

        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            yield return GenToggleCommand();
            
            var extractCommand = GenExtractCommand();
            if (!psiCoinReady)
                extractCommand.Disable(TicksTillNextPsiCoin.ToStringTicksToDays() + " till next psicoin");

            yield return extractCommand;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref _ticksTillNextPsiCoinInLightMode, "ticksTillPsiCoin");
            Scribe_Values.Look(ref psiCoinReady, "psiCoinReady");
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
            defaultLabel = "PsiCoin ready for extraction";
            defaultExplanation = "explanation placeholder TODO";
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


}
