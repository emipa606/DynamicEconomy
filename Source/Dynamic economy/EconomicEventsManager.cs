using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using Verse.AI;

namespace DynamicEconomy
{
    public class EconomicEventsManager : IExposable
    {
        public float daysSinceLastEconomicEvent;
        public float failedEventRollsBonus;
        public int ticksSinceLastEventRoll;

        public bool CanRollForEconomicEvent => ticksSinceLastEventRoll>=60000;        //DEBUG replace with 1 day cd

        public float EventRollSuccessChance(Pawn pawn)
        {
            float preChance = Math.Min(daysSinceLastEconomicEvent / 60f * 0.5f, 0.5f) + Math.Min(failedEventRollsBonus, 0.5f);

            return Math.Min(preChance * pawn.GetStatValue(StatDefOf.NegotiationAbility), 1f);

        }
        public bool RollForEconomicEvent(Pawn pawn)
        {
            bool res = Rand.Chance(EventRollSuccessChance(pawn));

            if (res)
            {
                daysSinceLastEconomicEvent = 0f;
                failedEventRollsBonus = 0f;

                QuestUtility.GenerateQuestAndMakeAvailable(DynamicEconomyDefOf.HighDemandQuest, 50);

                Find.LetterStack.ReceiveLetter("High tariffs", "One of your colonists found out that one of nearby settlements requires specific supplies. This can be a good opportunity to make some profitable trades.\n\nCheck quest log for more info.", LetterDefOf.NewQuest);
            }
            else
                failedEventRollsBonus += pawn.GetStatValue(StatDefOf.NegotiationAbility) * 0.025f;

            ticksSinceLastEventRoll = 0;

            return res;
        }

        public void TickLong()
        {
            ticksSinceLastEventRoll += 2000;
            daysSinceLastEconomicEvent += 0.03333f;         //2k ticks / 60k ticks in day
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref daysSinceLastEconomicEvent, "daysSinceLastEconomicEvent", 0f);
            Scribe_Values.Look(ref failedEventRollsBonus, "failedEventRollsBonus", 0f);
            Scribe_Values.Look(ref ticksSinceLastEventRoll, "ticksSinceLastEventRoll", 60000, true);
        }
    }

    
}
