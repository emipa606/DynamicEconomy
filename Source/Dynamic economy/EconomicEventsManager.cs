using System;
using RimWorld;
using Verse;

namespace DynamicEconomy;

public class EconomicEventsManager : IExposable
{
    private const int EconomicEventRollCooldownTicks = 60000;
    private const int DaysTillMaxTimeBonus = 60;
    private const int XPBonus = 1250;


    private float daysSinceLastEconomicEvent;
    private float failedEventRollsBonus;
    public int ticksSinceLastEventRoll;

    public bool CanRollForEconomicEvent => ticksSinceLastEventRoll >= EconomicEventRollCooldownTicks;

    public void ExposeData()
    {
        Scribe_Values.Look(ref daysSinceLastEconomicEvent, "daysSinceLastEconomicEvent");
        Scribe_Values.Look(ref failedEventRollsBonus, "failedEventRollsBonus");
        Scribe_Values.Look(ref ticksSinceLastEventRoll, "ticksSinceLastEventRoll");
    }

    public float EventRollSuccessChance(Pawn pawn)
    {
        var preChance = Math.Min(daysSinceLastEconomicEvent / DaysTillMaxTimeBonus * 0.5f, 0.5f) +
                        Math.Min(failedEventRollsBonus, 0.5f);

        return Math.Min(preChance * pawn.GetStatValue(StatDefOf.NegotiationAbility), 1f);
    }

    public bool RollForEconomicEvent(Pawn pawn)
    {
        var res = Rand.Chance(EventRollSuccessChance(pawn));

        if (res)
        {
            daysSinceLastEconomicEvent = 0f;
            failedEventRollsBonus = 0f;

            if (Rand.Value > 0.5f)
            {
                QuestUtility.GenerateQuestAndMakeAvailable(DynamicEconomyDefOf.HighDemandQuest, 50);
                Find.LetterStack.ReceiveLetter("DE_HighDemandQuest_LetterLabel".Translate(),
                    "DE_HighDemandQuest_LetterBody".Translate(), DynamicEconomyDefOf.NewQuest);
            }

            else
            {
                QuestUtility.GenerateQuestAndMakeAvailable(DynamicEconomyDefOf.HighDemandQuest, 50);
                Find.LetterStack.ReceiveLetter("DE_HighSupplyQuest_LetterLabel".Translate(),
                    "DE_HighSupplyQuest_LetterBody".Translate(), DynamicEconomyDefOf.NewQuest);
            }
        }
        else
        {
            failedEventRollsBonus += pawn.GetStatValue(StatDefOf.NegotiationAbility) * 0.025f;
        }

        //Log.Message("Roll");
        pawn.skills.Learn(SkillDefOf.Social, XPBonus);
        ticksSinceLastEventRoll = 0;

        return res;
    }

    public void TickLong()
    {
        if (ticksSinceLastEventRoll <= EconomicEventRollCooldownTicks)
        {
            ticksSinceLastEventRoll += 2000;
        }

        daysSinceLastEconomicEvent += 0.03333f; //2k ticks / 60k ticks in day
    }
}