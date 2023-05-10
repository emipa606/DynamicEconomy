using Verse;

namespace DynamicEconomy;

public class HediffCompProperties_PsiCoinMining : HediffCompProperties
{
    public int hardMiningMultipiler;
    public float silverPerDayLight;

    public HediffCompProperties_PsiCoinMining()
    {
        compClass = typeof(HediffComp_PsiCoinMining);
    }
}