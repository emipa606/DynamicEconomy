using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace DynamicEconomy
{
    class DynamicEconomyMod : Mod
    {
        public static Harmony harmonyInstance;
        public DynamicEconomyMod(ModContentPack content) : base(content)
        {
            GetSettings<DESettings>().Write();
            harmonyInstance = new Harmony("saloid.DynamicEconomy");
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            DESettings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "Dynamic economy";
    }
}
