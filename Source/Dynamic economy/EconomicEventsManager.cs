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
        public const int EconomicEventRollCooldownTicks = 60000;
        public const int DaysTillMaxTimeBonus = 60;
        public const int XPBonus = 1250;


        public float daysSinceLastEconomicEvent;
        public float failedEventRollsBonus;
        public int ticksSinceLastEventRoll;

        public bool CanRollForEconomicEvent => ticksSinceLastEventRoll>=EconomicEventRollCooldownTicks;        

        public float EventRollSuccessChance(Pawn pawn)
        {
            float preChance = Math.Min((daysSinceLastEconomicEvent / DaysTillMaxTimeBonus) * 0.5f, 0.5f) + Math.Min(failedEventRollsBonus, 0.5f);

            return Math.Min(preChance * pawn.GetStatValue(StatDefOf.NegotiationAbility), 1f);

        }
        public bool RollForEconomicEvent(Pawn pawn)
        {
            bool res = Rand.Chance(EventRollSuccessChance(pawn));

            if (res)
            {
                daysSinceLastEconomicEvent = 0f;
                failedEventRollsBonus = 0f;

                if (Rand.Value > 0.5f)
                {
                    QuestUtility.GenerateQuestAndMakeAvailable(DynamicEconomyDefOf.HighDemandQuest, 50);
                    Find.LetterStack.ReceiveLetter("DE_HighDemandQuest_LetterLabel".Translate(), "DE_HighDemandQuest_LetterBody".Translate(), LetterDefOf.NewQuest);
                }

                else
                {
                    QuestUtility.GenerateQuestAndMakeAvailable(DynamicEconomyDefOf.HighDemandQuest, 50);
                    Find.LetterStack.ReceiveLetter("DE_HighSupplyQuest_LetterLabel".Translate(), "DE_HighSupplyQuest_LetterBody".Translate(), LetterDefOf.NewQuest);
                }
            }
            else
                failedEventRollsBonus += pawn.GetStatValue(StatDefOf.NegotiationAbility) * 0.025f;

            Log.Message("Roll");
            pawn.skills.Learn(SkillDefOf.Social, XPBonus);
            ticksSinceLastEventRoll = 0;

            return res;
        }

        public void TickLong()
        {
            ticksSinceLastEventRoll += 2000;
            if (ticksSinceLastEventRoll % 60000 == 0)
                Log.Message("Event ticks " + ticksSinceLastEventRoll);

            daysSinceLastEconomicEvent += 0.03333f;         //2k ticks / 60k ticks in day
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref daysSinceLastEconomicEvent, "daysSinceLastEconomicEvent");
            Scribe_Values.Look(ref failedEventRollsBonus, "failedEventRollsBonus");
            Scribe_Values.Look(ref ticksSinceLastEventRoll, "ticksSinceLastEventRoll");         
        }
    }

    
}
