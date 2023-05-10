using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace DynamicEconomy;

[HarmonyPatch(typeof(Building_CommsConsole), "GetFloatMenuOptions")]
public class GetCompDEEventRollMenuOptions
{
    public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> res,
        Building_CommsConsole __instance, Pawn myPawn)
    {
        var itWorks = __instance.CanUseCommsNow;
        foreach (var op in res)
        {
            yield return op;
        }

        if (!itWorks)
        {
            yield break;
        }

        var comp = __instance.GetComp<CompDEEventRoll>();
        if (comp == null)
        {
            yield break;
        }

        foreach (var op in comp.CompFloatMenuOptions(myPawn))
        {
            yield return op;
        }
    }
}