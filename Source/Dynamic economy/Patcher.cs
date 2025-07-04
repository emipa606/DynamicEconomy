﻿using Verse;

namespace DynamicEconomy;

[StaticConstructorOnStartup]
internal class Patcher
{
    static Patcher()
    {
        DynamicEconomyMod.HarmonyInstance.PatchAll();
    }
}

// market value should be adjusted directrly for psicoin since storyteller will always count its market value w/o modifiers