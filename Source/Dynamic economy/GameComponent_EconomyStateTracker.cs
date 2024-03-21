using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace DynamicEconomy;

/// <summary>
///     Main class that serves as pricemod container and as proxy for managers
/// </summary>
public class GameComponent_EconomyStateTracker : GameComponent
{
    public const float BaseTurnoverToDoubleTradersCurrency = 7000f;
    public const float BaseTurnoverEffectDrop = 0.001f;
    private static GameComponent_EconomyStateTracker _instance;

    private EconomicEventsManager _eventsManager;

    private PsiCoinManager _psiCoinManager;

    private float _recentTurnover;
    protected List<OrbitalTraderPriceModifier> orbitalTraderPriceModifiers;
    protected List<SettlementPriceModifier> settlementPriceModifiers;

    protected List<TraderCaravansPriceModifier> traderCaravanPriceModifiers;


    public GameComponent_EconomyStateTracker(Game game)
    {
        // avoiding nullref
        // or attempting to do so
        settlementPriceModifiers = [];
        orbitalTraderPriceModifiers = [];
        traderCaravanPriceModifiers = [];
        _eventsManager = new EconomicEventsManager();
        _psiCoinManager = new PsiCoinManager();
    }

    public static GameComponent_EconomyStateTracker CurGameInstance =>
        Current.ProgramState == ProgramState.Playing ? _instance : null;

    public PsiCoinManager PsiCoinManager => _psiCoinManager;
    public EconomicEventsManager EconomicEventsManager => _eventsManager;

    public float TraderSilverMultipiler => 1f + (_recentTurnover * DESettings.turnoverEffectOnTraderCurrencyMultipiler /
                                                 BaseTurnoverToDoubleTradersCurrency);

    public void RemoveModifierForShip(TradeShip ship)
    {
        orbitalTraderPriceModifiers.RemoveAll(mod => mod.ship == ship);
    }

    /// <summary>
    /// </summary>
    /// <param name="trader">will return pricemod for trader caravans if trader is null</param>
    /// <returns></returns>
    public ComplexPriceModifier GetOrCreateIfNeededComplexModifier(ITrader trader)
    {
        if (trader == null)
        {
            return GetModifierForCaravan(null);
        }

        if (trader is Pawn pawn)
        {
            // if on non-home map, use that settlement's modifier if possible
            if (pawn.MapHeld is { IsPlayerHome: false, Tile: > 0 } &&
                Find.WorldObjects.AnySettlementAt(pawn.MapHeld.Tile))
            {
                trader = Find.WorldObjects.SettlementAt(pawn.MapHeld.Tile);
            }
            else
            {
                return GetModifierForCaravan(trader.Faction);
            }
        }

        if (trader is Settlement settlement)
        {
            if (settlement.Faction == Faction.OfPlayer)
            {
                throw new ArgumentException("settlement cant belong to player to set priceMod for it");
            }

            var modifier = settlementPriceModifiers.Find(mod => mod.settlement == settlement);

            if (modifier != null)
            {
                return modifier;
            }

            //Log.Message("Created modifier for " + settlement.Label);
            modifier = new SettlementPriceModifier(settlement);
            settlementPriceModifiers.Add(modifier);

            return modifier;
        }

        if (trader is TradeShip tradeShip)
        {
            var modifier = orbitalTraderPriceModifiers.Find(mod => mod.ship == tradeShip);

            if (modifier != null)
            {
                return modifier;
            }

            //Log.Message("Created modifier for " + tradeShip.name);
            modifier = new OrbitalTraderPriceModifier(tradeShip);
            orbitalTraderPriceModifiers.Add(modifier);

            return modifier;
        }


        Log.Warning("trader is not null, pawn, settlement or ship");
        return null;
    }

    public TraderCaravansPriceModifier GetModifierForCaravan(Faction faction)
    {
        var modifier = traderCaravanPriceModifiers.Find(mod => mod.faction == faction);

        if (modifier != null)
        {
            return modifier;
        }

        modifier = new TraderCaravansPriceModifier(faction);
        traderCaravanPriceModifiers.Add(modifier);

        return modifier;
    }

    public float GetPriceMultipilerFor(Tradeable tradeable, ITrader trader)
    {
        return GetPriceMultipilerFor(tradeable.ThingDef, tradeable.ActionToDo, trader);
    }

    public float GetPriceMultipilerFor(ThingDef thingDef, TradeAction action, ITrader trader,
        ConsideredFactors factor = ConsideredFactors.All)
    {
        if (action == TradeAction.None)
        {
            return 1f;
        }

        var modifier = GetOrCreateIfNeededComplexModifier(trader);


        return modifier?.GetPriceMultipilerFor(thingDef, action, factor) ?? 1f;
    }


    public void RecordThingTransfer(ThingDef def, float totalCost, TradeAction action, ITrader trader)
    {
        var modifier = GetOrCreateIfNeededComplexModifier(trader);

        _recentTurnover += Math.Abs(totalCost);

        modifier.RecordNewDeal(def, totalCost, action);
    }

    public void SetEventModifiersForSettlement(Settlement targetSettlement, ThingCategoryDef thingCategoryDef,
        float playerSellsFactor, float playerBuysFactor)
    {
        var modifier = GetOrCreateIfNeededComplexModifier(targetSettlement);

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

        if (Find.TickManager.TicksGame % 2000 != 0)
        {
            return;
        }

        _recentTurnover *= 1 - (BaseTurnoverEffectDrop * DESettings.turnoverEffectDropRateMultipiler);
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

        foreach (var mod in traderCaravanPriceModifiers)
        {
            mod.TickLong();
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();

        // srsly, I have no idea how to make it work properly
        // damn nullref after game loading

        Scribe_Collections.Look(ref settlementPriceModifiers, "settlementPriceModifiers", LookMode.Deep);
        Scribe_Collections.Look(ref orbitalTraderPriceModifiers, "orbitalTraderPriceModifiers", LookMode.Deep);
        Scribe_Collections.Look(ref traderCaravanPriceModifiers, "caravanPriceModifiers", LookMode.Deep);
        Scribe_Deep.Look(ref _eventsManager, "eventsManager");
        Scribe_Deep.Look(ref _psiCoinManager, "psiCoinManager");

        orbitalTraderPriceModifiers ??= [];
        settlementPriceModifiers ??= [];
        traderCaravanPriceModifiers ??= [];
    }
}