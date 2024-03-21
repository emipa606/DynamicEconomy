using RimWorld;
using Verse;

namespace DynamicEconomy;

[DefOf]
public static class DynamicEconomyDefOf
{
    public static QuestScriptDef HighDemandQuest;
    public static QuestScriptDef HighSupplyQuest;

    public static JobDef GatherInfo;

    public static ThingDef PsiCoin;

    public static HediffDef PsiCoinMining;


    public static ConstantPriceModsDef Hillness_Flat;
    public static ConstantPriceModsDef Hillness_SmallHills;
    public static ConstantPriceModsDef Hillness_LargeHills;
    public static ConstantPriceModsDef Hillness_Mountainous;

    public static LetterDef NewQuest;
    public static MemeDef Rancher;
    public static MemeDef Nudism;
}