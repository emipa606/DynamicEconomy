using HarmonyLib;
using Mlie;
using UnityEngine;
using Verse;

namespace DynamicEconomy;

internal class DynamicEconomyMod : Mod
{
    public static Harmony HarmonyInstance;
    public static string CurrentVersion;

    public DynamicEconomyMod(ModContentPack content) : base(content)
    {
        GetSettings<DESettings>().Write();
        HarmonyInstance = new Harmony("saloid.DynamicEconomy");
        CurrentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        DESettings.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "Dynamic economy";
    }
}