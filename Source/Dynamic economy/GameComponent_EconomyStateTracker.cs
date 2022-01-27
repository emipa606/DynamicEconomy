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

        private PsiCoinManager _psiCoinManager;
        public PsiCoinManager PsiCoinManager => _psiCoinManager;

        private EconomicEventsManager _eventsManager;
        public EconomicEventsManager EconomicEventsManager => _eventsManager;

        private float _recentTurnover;
        public const float BaseTurnoverToDoubleTradersCurrency = 7000f;
        public const float BaseTurnoverEffectDrop = 0.001f;
        public float TraderSilverMultipiler => 1f + _recentTurnover * DESettings.turnoverEffectOnTraderCurrencyMultipiler / BaseTurnoverToDoubleTradersCurrency;     



        public SettlementPriceModifier GetOrCreateIfNeededSettlementModifier(Settlement settlement)       // can return null for unaffected things
        {
            SettlementPriceModifier modifier;
            
            
            if (settlement != null && settlement.Faction != Faction.OfPlayer)
            {
                modifier = settlementPriceModifiers.Find(mod => mod.Settlement == settlement);
                
                if (modifier == null)
                {
                    Log.Message("Created modifier for " + settlement.Label);
                    modifier = new SettlementPriceModifier(settlement);
                    settlementPriceModifiers.Add(modifier);
                }
            }

            else
            {
                modifier = settlementPriceModifiers.Find(mod => mod.ForPlayerSettlement);

                if (modifier == null)
                {
                    modifier = new SettlementPriceModifier(null);
                    settlementPriceModifiers.Add(modifier);
                }
            }
            

            return modifier;
        }

        public float GetPriceMultipilerFor(Tradeable tradeable, Settlement settlementOfTrade=null) => GetPriceMultipilerFor(tradeable.ThingDef, tradeable.ActionToDo, settlementOfTrade);

        public float GetPriceMultipilerFor(ThingDef thingDef, TradeAction action, Settlement settlementOfTrade=null, ConsideredFactors factor=ConsideredFactors.All)
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
           
            _recentTurnover += Math.Abs(totalCost);

            modifier.RecordNewDeal(def, totalCost, action);
            settlementPriceModifiers.Add(modifier);

        }

        public void SetEventModifiersForSettlement(Settlement targetSettlement, ThingCategoryDef thingCategoryDef, float playerSellsFactor, float playerBuysFactor)
        {
            ComplexPriceModifier modifier = GetOrCreateIfNeededSettlementModifier(targetSettlement);

            if (targetSettlement==null)
            {
                Log.Warning("Setted an event modifier for null(player) settlement");
            }
            

            modifier.AddEventModifier(thingCategoryDef, playerSellsFactor, playerBuysFactor);

            /*var debugMod = GetOrCreateIfNeededSettlementModifier(targetSettlement).thingCategoryPriceModifiers.Find(mod => mod.Def == thingCategoryDef);

            if (debugMod == null)
                Log.Message("modifier was not created");
            else
                Log.Message("DEBUG sell=" + debugMod.playerSellsFactor + " buy=" + debugMod.playerBuysFactor + " sellEv=" + debugMod.playerSellsFactorEvent + " buyEv=" + debugMod.playerBuysFactorEvent + "TOTAL SELL MUL = " + debugMod.GetPriceMultipiler(TradeAction.PlayerSells));*/
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
                _recentTurnover *= (1-BaseTurnoverEffectDrop*DESettings.turnoverEffectDropRateMultipiler);
                _psiCoinManager.TickLong();
                _eventsManager.TickLong();

                foreach (var mod in settlementPriceModifiers)
                {
                    mod.TickLong();
                }
            }
        }




        public GameComponent_EconomyStateTracker(Game game)
        {
            //settlementPriceModifiers = new List<SettlementPriceModifier>();
            _eventsManager = new EconomicEventsManager();
            _psiCoinManager = new PsiCoinManager();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            _eventsManager = new EconomicEventsManager();
            _psiCoinManager = new PsiCoinManager();

            Scribe_Collections.Look(ref settlementPriceModifiers, "settlementPriceModifiers", LookMode.Deep);
            Scribe_Deep.Look(ref _eventsManager, "eventsManager");
            Scribe_Deep.Look(ref _psiCoinManager,"psiCoinManager");

            
        }


    }
}
