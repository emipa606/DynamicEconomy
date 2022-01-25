using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld.Planet;
using RimWorld;


namespace DynamicEconomy
{
    public class GameComponent_EconomyStateTracker : GameComponent
    {
        private static GameComponent_EconomyStateTracker _instance;
        public static GameComponent_EconomyStateTracker CurGameInstance => Current.ProgramState==ProgramState.Playing ? _instance : null;

        protected List<SettlementPriceModifier> settlementPriceModifiers;


        protected float daysSinceLastEconomicEvent;
        protected int failedEventRolls;
        protected int ticksSinceLastEventRoll;

        public bool CanRollForEconomicEvent => ticksSinceLastEventRoll >= 60000;

        public float RumorsListeningSuccessChance(Pawn pawn)
        {
            float preChance = Math.Min(daysSinceLastEconomicEvent / 40f * 0.5f, 0.5f) + Math.Min(failedEventRolls * 0.025f, 0.5f);


            return Math.Min(preChance * pawn.GetStatValue(StatDefOf.NegotiationAbility), 1f);

        }
        public bool RollForEconomicEvent(Pawn pawn)
        {
            bool res = Rand.Chance(RumorsListeningSuccessChance(pawn));

            if (res)
            {
                daysSinceLastEconomicEvent = 0f;
                failedEventRolls = 0;
            }
            else
                failedEventRolls++;

            ticksSinceLastEventRoll = 0;

            return res;
        }



        protected SettlementPriceModifier GetOrCreateIfNeededSettlementModifier(Settlement settlement)
        {
            SettlementPriceModifier modifier;
            if (settlement != null && settlement.Faction != Faction.OfPlayer)
            {
                modifier = settlementPriceModifiers.Find(mod => mod.Settlement == settlement);

                if (modifier == null)
                    modifier = new SettlementPriceModifier(settlement);

            }

            else
            {
                modifier = settlementPriceModifiers.Find(mod => mod.ForPlayerSettlement);

                if (modifier == null)
                    modifier = new SettlementPriceModifier(null);

            }

            return modifier;
        }

        public float GetPriceMultipilerFor(Tradeable tradeable, Settlement settlementOfTrade=null) => GetPriceMultipilerFor(tradeable.ThingDef, tradeable.ActionToDo, settlementOfTrade);

        public float GetPriceMultipilerFor(ThingDef thingDef, TradeAction action, Settlement settlementOfTrade=null, Factor factor=Factor.All)
        {
            if (action == TradeAction.None)
                return 1f;

            ComplexPriceModifier modifier = GetOrCreateIfNeededSettlementModifier(settlementOfTrade);


            if (modifier == null) 
                return 1f;

            return modifier.GetPriceMultipilerFor(thingDef, action, factor);
        }



        public void RecordThingTransfer(ThingDef def, float totalCost, TradeAction action, Settlement settlementOfTrade=null)
        {
            var modifier = GetOrCreateIfNeededSettlementModifier(settlementOfTrade);

            modifier.RecordNewDeal(def, totalCost, action);
            settlementPriceModifiers.Add(modifier);

        }

        public void SetEventModifiersForSettlement(Settlement targetSettlement, ThingCategoryDef thingCategoryDef, float playerSellsFactor, float playerBuysFactor)
        {
            ComplexPriceModifier modifier = GetOrCreateIfNeededSettlementModifier(targetSettlement);

            

            modifier.AddEventModifier(thingCategoryDef, playerSellsFactor, playerBuysFactor);

            var debugMod = modifier.thingCategoryPriceModifiers.Find(mod => mod.Def == thingCategoryDef);

            if (debugMod == null)
                Log.Message("modifier was not created");
            else
                Log.Message("DEBUG sell=" + debugMod.colonySellsFactor + " buy=" + debugMod.colonyBuysFactor + " sellEv=" + debugMod.colonySellsFactorEvent + " buyEv=" + debugMod.colonyBuysFactorEvent);
        }




        public override void StartedNewGame()
        {
            base.StartedNewGame();
            _instance = this;
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            _instance = this;
        }






        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame%2000==0)
            {
                ticksSinceLastEventRoll += 2000;
                daysSinceLastEconomicEvent += 0.03333f;         //2k ticks / 60k ticks in day

                foreach (var mod in settlementPriceModifiers)
                {
                    mod.TickLong();
                }
            }
        }




        public GameComponent_EconomyStateTracker(Game game)
        {
            settlementPriceModifiers = new List<SettlementPriceModifier>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref settlementPriceModifiers, "settlementPriceModifiers", LookMode.Deep);
        }


    }
}
