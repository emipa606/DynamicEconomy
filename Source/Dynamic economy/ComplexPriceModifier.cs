using System.Collections.Generic;
using RimWorld;
using Verse;

namespace DynamicEconomy;

/// <summary>
///     Contains all the info and provides methods needed to get price multipiler for anything
/// </summary>
public class ComplexPriceModifier : IExposable
{
    private List<ThingCategoryPriceModifier> thingCategoryPriceModifiers;
    private List<ThingPriceModifier> thingPriceModifiers;

    protected ComplexPriceModifier()
    {
        //this.faction = faction;
        thingPriceModifiers = [];
        thingCategoryPriceModifiers = [];
    }

    public virtual void ExposeData()
    {
        Scribe_Collections.Look(ref thingCategoryPriceModifiers, "thingCategoryMods", LookMode.Deep);
        Scribe_Collections.Look(ref thingPriceModifiers, "thingsMods", LookMode.Deep);
    }

    public static ModifierCategory GetModifierCategoryFor(ThingCategoryDef thingCategoryDef)
    {
        var extension = thingCategoryDef?.GetModExtension<PriceModifierCategoryDefExtension>();
        if (extension != null && extension.category != ModifierCategory.None)
        {
            return extension.category;
        }

        return ModifierCategory.None;
    }

    public static ModifierCategory GetModifierCategoryFor(ThingDef thingDef, out ThingCategoryDef definingCategory)
    {
        if (thingDef == null)
        {
            definingCategory = null;
            return ModifierCategory.None;
        }


        var thingSpecificMod = thingDef.GetModExtension<PriceModifierCategoryDefExtension>();
        if (thingSpecificMod != null)
        {
            definingCategory = null;
            return thingSpecificMod.category;
        }

        if (thingDef.thingCategories == null || thingDef.thingCategories.Count == 0)
        {
            definingCategory = null;
            return ModifierCategory.None;
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < thingDef.thingCategories.Count; i++)
        {
            var node = thingDef.thingCategories[i];

            while (node != null)
            {
                var extension = node.GetModExtension<PriceModifierCategoryDefExtension>();
                if (extension != null && extension.category != ModifierCategory.None)
                {
                    definingCategory = node;
                    return extension.category;
                }

                node = node.parent;
            }
        }

        definingCategory = null;
        return ModifierCategory.None;
    }


    protected virtual TradeablePriceModifier
        GetOrCreateIfNeededTradeablePriceModifier(ThingDef thingDef) //returns null for ModifierCategory.None
    {
        if (thingDef == null)
        {
            return null;
        }

        var modCategory = GetModifierCategoryFor(thingDef, out var thingCategory);
        TradeablePriceModifier modifier = null;
        switch (modCategory)
        {
            case ModifierCategory.Standalone:
            {
                modifier = thingPriceModifiers.Find(mod => mod.Def == thingDef);
                if (modifier != null)
                {
                    return modifier;
                }

                modifier = new ThingPriceModifier(thingDef);
                thingPriceModifiers.Add((ThingPriceModifier)modifier);
                //Log.Message("Found modifier for " + thingDef.defName);
                break;
            }
            case ModifierCategory.Group:
            {
                modifier = thingCategoryPriceModifiers.Find(mod => mod.Def == thingCategory);
                if (modifier == null)
                {
                    modifier = new ThingCategoryPriceModifier(thingCategory);
                    thingCategoryPriceModifiers.Add((ThingCategoryPriceModifier)modifier);
                }

                //Log.Message("Found modifier for " + thingDef.defName);
                break;
            }
        }


        return modifier;
    }

    public virtual ThingCategoryPriceModifier
        GetOrCreateIfNeededTradeablePriceModifier(
            ThingCategoryDef thingCategoryDef) //returns null for ModifierCategory != Group
    {
        if (thingCategoryDef == null)
        {
            return null;
        }

        //var modCategory = GetModifierCategoryFor(thingCategoryDef);
        //ThingCategoryPriceModifier modifier;

        //if (modCategory == ModifierCategory.Group)
        //{
        var modifier = thingCategoryPriceModifiers.Find(mod => mod.Def == thingCategoryDef);
        if (modifier != null)
        {
            return modifier;
        }

        modifier = new ThingCategoryPriceModifier(thingCategoryDef);
        thingCategoryPriceModifiers.Add(modifier);

        return modifier;
        //}
        //else
        //{
        //    Log.Error("Cant get modifier for " + modCategory.ToString() + "-type thing category defName=" + thingCategoryDef.defName);
        //    return null;
        //}
    }

    public virtual float GetPriceMultipilerFor(ThingDef thingDef, TradeAction action,
        ConsideredFactors factor = ConsideredFactors.All)
    {
        // Changed so both modifiers applies

        var result = 1f;
        result *= thingPriceModifiers.Find(priceMod => priceMod.Def == thingDef)?.GetPriceMultiplier(action, factor) ??
                  1f;

        if (thingDef.thingCategories == null)
        {
            return result;
        }

        foreach (var thingCat in thingDef.thingCategories)
        {
            var current = thingCat;
            while (current != null)
            {
                var modifier = thingCategoryPriceModifiers.Find(priceMod => priceMod.Def == current);
                if (modifier != null)
                {
                    return result * modifier.GetPriceMultiplier(action, factor);
                }

                current = current.parent;
            }
        }

        return result;
    }

    public virtual void RecordNewDeal(ThingDef thingDef, float totalCost, TradeAction action)
    {
        if (action == TradeAction.None)
        {
            return;
        }

        // The original code here won't correctly record for category modifiers
        var modifier = GetOrCreateIfNeededTradeablePriceModifier(thingDef);
        modifier?.RecordNewDeal(action, totalCost); // mb divide totalCost by current multipiler?
    }

    public virtual void TickLong()
    {
        thingPriceModifiers.RemoveAll(mod =>
            mod == null); //I have no idea where those null mods are coming from. TODO, but for now let it be. Its O(n) after all 

        foreach (var thingPriceModifier in thingPriceModifiers)
        {
            thingPriceModifier.TickLongUpdate();
        }

        thingCategoryPriceModifiers.RemoveAll(mod => mod == null);

        foreach (var thingCategoryPriceModifier in thingCategoryPriceModifiers)
        {
            thingCategoryPriceModifier.TickLongUpdate();
        }
    }

    public void SetBaseModifier(ThingDef def, float baseSellFactor, float baseBuyFactor)
    {
        var modifier = GetOrCreateIfNeededTradeablePriceModifier(def);
        if (modifier == null)
        {
            Log.Error(
                "tried to set base multipilers for None- or constant-category thing"); //TODO make const category available for setting base mods
            return;
        }

        modifier.SetBaseFactors(baseSellFactor, baseBuyFactor);
    }

    protected void RegisterThingModifiers(List<BaseThingPriceMultipilerInfo> thingPriceMultipilers)
    {
        if (thingPriceMultipilers == null)
        {
            return;
        }

        foreach (var multiplier in thingPriceMultipilers)
        {
            var modifier =
                GetOrCreateIfNeededTradeablePriceModifier(DefDatabase<ThingDef>.GetNamed(multiplier.thingDefName));
            if (modifier == null)
            {
                continue;
            }

            modifier.baseBuyFactor *= multiplier.buyMultiplier;
            modifier.baseSellFactor *= multiplier.sellMultiplier;
        }
    }

    protected void RegisterCategoryModifiers(List<BaseCategoryPriceMultipilerInfo> categoryPriceMultipilers)
    {
        if (categoryPriceMultipilers == null)
        {
            return;
        }

        foreach (var multiplier in categoryPriceMultipilers)
        {
            var modifier =
                GetOrCreateIfNeededTradeablePriceModifier(
                    DefDatabase<ThingCategoryDef>.GetNamed(multiplier.categoryDefName));
            if (modifier == null)
            {
                continue;
            }

            modifier.baseBuyFactor *= multiplier.buyMultiplier;
            modifier.baseSellFactor *= multiplier.sellMultiplier;
        }
    }

    public void AddEventModifier(ThingCategoryDef def, float playerSellsFactor, float playerBuysFactor)
    {
        var modifier = GetOrCreateIfNeededTradeablePriceModifier(def);
        modifier.SetEventFactors(playerSellsFactor, playerBuysFactor);
    }
}