using HarmonyLib;
using Mlie;
using UnityEngine;
using Verse;

namespace DynamicEconomy;

internal class DynamicEconomyMod : Mod
{
    public static Harmony harmonyInstance;
    public static string currentVersion;

    public DynamicEconomyMod(ModContentPack content) : base(content)
    {
        GetSettings<DESettings>().Write();
        harmonyInstance = new Harmony("saloid.DynamicEconomy");
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
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