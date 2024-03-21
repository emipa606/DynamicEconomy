using Verse;

namespace DynamicEconomy;

public class PsiCoinManager : IExposable
{
    public const float PsiCoinDefaultRandomOffset = 0.003f;
    public float psiCoinPrice = DynamicEconomyDefOf.PsiCoin.BaseMarketValue;

    public void ExposeData()
    {
        Scribe_Values.Look(ref psiCoinPrice, "psiCoinPrice", DynamicEconomyDefOf.PsiCoin.BaseMarketValue);
    }

    public void TickLong()
    {
        psiCoinPrice *= 1f + (Rand.Sign * Rand.Value * PsiCoinDefaultRandomOffset *
                              DESettings.randyCoinRandomOfsettMultipiler);
    }
}

// mb combine those workers? fewer polls => fewer lags, but will have to write common desc and stages for both thoughts