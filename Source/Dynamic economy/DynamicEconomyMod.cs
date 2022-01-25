using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

namespace DynamicEconomy
{
    class DynamicEconomyMod : Mod
    {
        public static Harmony harmonyInstance;
        public DynamicEconomyMod(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("saloid.DynamicEconomy");
        }
    }
}
