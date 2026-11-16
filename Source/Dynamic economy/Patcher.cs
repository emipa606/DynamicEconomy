using System.Reflection;
using Verse;

namespace DynamicEconomy;

[StaticConstructorOnStartup]
internal static class Patcher
{
    static Patcher()
    {
        DynamicEconomyMod.HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
    }
}

// market value should be adjusted directrly for psicoin since storyteller will always count its market value w/o modifiers