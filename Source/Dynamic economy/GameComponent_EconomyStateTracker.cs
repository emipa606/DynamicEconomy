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
    /// <summary>
    /// Main class that serves as pricemod container and as proxy for managers
    /// </summary>
    public class GameComponent_EconomyStateTracker : GameComponent
    {
        private static GameComponent_EconomyStateTracker _instance;
        public static GameComponent_EconomyStateTracker CurGameInstance => Current.ProgramState==ProgramState.Playing ? _instance : null;

        protected AllTraderCaravansPriceModifier traderCaravanPriceModifier;

        protected List<SettlementPriceModifier> settlementPriceModifiers;
        protected List<OrbitalTraderPriceModifier> orbitalTraderPriceModifiers;

        private PsiCoinManager _psiCoinManager;
        public PsiCoinManager PsiCoinManager => _psiCoinManager;

        private EconomicEventsManager _eventsManager;
        public EconomicEventsManager EconomicEventsManager => _eventsManager;

        private float _recentTurnover;
        public const float BaseTurnoverToDoubleTradersCurrency = 7000f;
        public const float BaseTurnoverEffectDrop = 0.001f;
        public float TraderSilverMultipiler => 1f + _recentTurnover * DESettings.turnoverEffectOnTraderCurrencyMultipiler / BaseTurnoverToDoubleTradersCurrency;     

        public void RemoveModifierForShip(TradeShip ship) => orbitalTraderPriceModifiers.RemoveAll(mod => mod.ship == ship);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trader">will return pricemod for trader caravans if trader is null</param>
        /// <returns></returns>
        public ComplexPriceModifier GetOrCreateIfNeededComplexModifier(ITrader trader)       
        {
            if (trader is Settlement settlement)
            {
                SettlementPriceModifier modifier;
                if (settlement.Faction != Faction.OfPlayer)
                {
                    modifier = settlementPriceModifiers.Find(mod => mod.settlement == settlement);

                    if (modifier == null)
                    {
                        //Log.Message("Created modifier for " + settlement.Label);
                        modifier = new SettlementPriceModifier(settlement);
                        settlementPriceModifiers.Add(modifier);
                    }

                    return modifier;
                }

                else
                {
                    throw new ArgumentException("settlement cant belong to player to set priceMod for it");
                }
            }

            else if (trader==null || trader is Pawn)
            {
                return traderCaravanPriceModifier;
            }

            else if (trader is TradeShip tradeShip)
            {
                OrbitalTraderPriceModifier modifier;
                modifier = orbitalTraderPriceModifiers.Find(mod => mod.ship == tradeShip);

                if (modifier == null)
                {
                    //Log.Message("Created modifier for " + tradeShip.name);
                    modifier = new OrbitalTraderPriceModifier(tradeShip);
                    orbitalTraderPriceModifiers.Add(modifier);
                }

                return modifier;
            }

            throw new ArgumentException("trader is not a pawn, settlement or ship");
        }

        public float GetPriceMultipilerFor(Tradeable tradeable, ITrader trader) => GetPriceMultipilerFor(tradeable.ThingDef, tradeable.ActionToDo, trader);

        public float GetPriceMultipilerFor(ThingDef thingDef, TradeAction action, ITrader trader, ConsideredFactors factor=ConsideredFactors.All)
        {
            if (action == TradeAction.None)
                return 1f;

            ComplexPriceModifier modifier = GetOrCreateIfNeededComplexModifier(trader);


            if (modifier == null) 
                return 1f;

            return modifier.GetPriceMultipilerFor(thingDef, action, factor);
        }



        public void RecordThingTransfer(ThingDef def, float totalCost, TradeAction action, ITrader trader)
        {
            var modifier = GetOrCreateIfNeededComplexModifier(trader);
           
            _recentTurnover += Math.Abs(totalCost);

            modifier.RecordNewDeal(def, totalCost, action);

        }

        public void SetEventModifiersForSettlement(Settlement targetSettlement, ThingCategoryDef thingCategoryDef, float playerSellsFactor, float playerBuysFactor)
        {

            ComplexPriceModifier modifier = GetOrCreateIfNeededComplexModifier(targetSettlement);

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
                foreach (var mod in orbitalTraderPriceModifiers)
                {
                    mod.TickLong();
                }
                traderCaravanPriceModifier.TickLong();
            }
        }




        public GameComponent_EconomyStateTracker(Game game)
        {
            // avoiding nullref
            // or attempting to do so
            settlementPriceModifiers = new List<SettlementPriceModifier>();
            orbitalTraderPriceModifiers = new List<OrbitalTraderPriceModifier>();
            _eventsManager = new EconomicEventsManager();
            _psiCoinManager = new PsiCoinManager();
            traderCaravanPriceModifier = new AllTraderCaravansPriceModifier();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            // srsly, i have no idea how to make it work properly
            // damn nullref after game loading

            Scribe_Collections.Look(ref settlementPriceModifiers, "settlementPriceModifiers", LookMode.Deep);
            Scribe_Collections.Look(ref orbitalTraderPriceModifiers, "orbitalTraderPriceModifiers", LookMode.Deep);
            Scribe_Deep.Look(ref _eventsManager, "eventsManager");
            Scribe_Deep.Look(ref traderCaravanPriceModifier, "caravanPriceMod");
            Scribe_Deep.Look(ref _psiCoinManager,"psiCoinManager");

            
        }


    }
}
