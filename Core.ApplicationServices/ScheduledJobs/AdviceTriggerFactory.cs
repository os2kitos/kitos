using System;
using System.Collections.Generic;
using Core.ApplicationServices.Helpers;
using Core.DomainModel.Advice;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.ScheduledJobs
{
    public static class AdviceTriggerFactory
    {
        public static IEnumerable<AdviceTrigger> CreateFrom(DateTime alarmDate, Scheduling scheduling)
        {
            if (MayRequirePartitioning(alarmDate))
            {
                switch (scheduling)
                {
                    case Scheduling.Month:
                        //12 yearly triggers
                        foreach (var adviceTrigger in ScheduleAdviceInPartsOf(alarmDate, 12))
                            yield return adviceTrigger;
                        break;
                    case Scheduling.Quarter when MatchFebruaryInQuarterlyInterval(alarmDate):
                        //Create 4 yearly triggers
                        foreach (var adviceTrigger in ScheduleAdviceInPartsOf(alarmDate, 4))
                            yield return adviceTrigger;
                        break;
                    case Scheduling.Semiannual when MatchFebruaryInSemiAnnualInterval(alarmDate):
                        //Create 2 yearly triggers
                        foreach (var adviceTrigger in ScheduleAdviceInPartsOf(alarmDate, 2))
                            yield return adviceTrigger;
                        break;
                    default:
                        yield return SetupAdviceTrigger(scheduling, alarmDate, Maybe<int>.None);
                        break;
                }
            }
            else
            {
                yield return SetupAdviceTrigger(scheduling, alarmDate, Maybe<int>.None);
            }
        }

        private static IEnumerable<AdviceTrigger> ScheduleAdviceInPartsOf(DateTime alarmDate, int partitions)
        {
            for (var i = 0; i < partitions; i++)
            {
                var offsetMonth = (((alarmDate.Month - 1) + (i * (12 / partitions))) % 12) + 1;
                var isFebruary = offsetMonth == 2;
                var triggerDate = new DateTime(alarmDate.Year, offsetMonth, isFebruary ? 28 : alarmDate.Day); //We set it to 28 for the sake of DataTime but we enforce the interval to be "Last day in month"

                //February in this interval will ALWAYS be last day in month to cater for leap and non leap year
                yield return SetupAdviceTrigger(Scheduling.Year, triggerDate, Maybe<int>.Some(i), isFebruary);
            }
        }

        private static AdviceTrigger SetupAdviceTrigger(Scheduling schedule, DateTime alarmDate, Maybe<int> partitionId, bool enforceLastDayInMonth = false)
        {
            return new AdviceTrigger(CronStringHelper.CronPerInterval(schedule, alarmDate, enforceLastDayInMonth), partitionId);
        }

        private static bool MayRequirePartitioning(DateTime alarmDate)
        {
            return alarmDate.Day > 28 && alarmDate.Day < 31;
        }

        private static bool MatchFebruaryInSemiAnnualInterval(DateTime alarmDate)
        {
            return alarmDate.Month % 6 == 2;
        }

        private static bool MatchFebruaryInQuarterlyInterval(DateTime alarmDate)
        {
            return alarmDate.Month % 3 == 2;
        }
    }
}
