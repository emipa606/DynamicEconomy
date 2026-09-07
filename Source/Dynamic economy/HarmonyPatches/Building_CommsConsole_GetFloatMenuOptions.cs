using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace DynamicEconomy;

[HarmonyPatch(typeof(Building_CommsConsole), nameof(Building_CommsConsole.GetFloatMenuOptions))]
public static class Building_CommsConsole_GetFloatMenuOptions
{
    public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> res,
        Building_CommsConsole __instance, Pawn myPawn)
    {
        var existingOptions = res.ToList();
        var existingLabels = new HashSet<string>(existingOptions.Select(op => op.Label));

        foreach (var op in existingOptions)
        {
            yield return op;
        }

        if (!__instance.CanUseCommsNow)
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
            if (!existingLabels.Add(op.Label))
            {
                continue;
            }

            yield return op;
        }
    }
}