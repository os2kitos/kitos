using System;
using Core.DomainModel.Advice;

namespace Core.ApplicationServices.Helpers
{
    public static class CronStringHelper
    {
        // ┌───────────── minute (0 - 59)
        // │ ┌───────────── hour (0 - 23)
        // │ │ ┌───────────── day of the month (1 - 31)
        // │ │ │ ┌───────────── month (1 - 12)
        // │ │ │ │ ┌───────────── day of the week (0 - 6) (Sunday to Saturday;
        // │ │ │ │ │                                   7 is also Sunday on some systems)
        // │ │ │ │ │
        // │ │ │ │ │
        // * * * * * <command to execute>

        public static string CronPerInterval(Scheduling interval, DateTime zeroTime, bool enforceLastDayInMonth = false)
        {
            var dayComponent = (zeroTime.Day == 31 || enforceLastDayInMonth) ? "L" : $"{zeroTime.Day}"; //L is extended cron syntax and results in last day of the month for all months.
            switch (interval)
            {
                case Scheduling.Hour: return $"{CronPatternDefaults.TriggerMinute} * * * *";
                case Scheduling.Day: return $"{CronPatternDefaults.TriggerMinute} {CronPatternDefaults.TriggerHourUTC} * * *";
                case Scheduling.Week: return $"{CronPatternDefaults.TriggerMinute} {CronPatternDefaults.TriggerHourUTC} * * {zeroTime.DayOfWeek:D}";
                case Scheduling.Month: return $"{CronPatternDefaults.TriggerMinute} {CronPatternDefaults.TriggerHourUTC} {dayComponent} * *";
                case Scheduling.Quarter: return $"{CronPatternDefaults.TriggerMinute} {CronPatternDefaults.TriggerHourUTC} {dayComponent} {ComputeOffsetMonth(zeroTime, 4)}-12/3 *";
                case Scheduling.Semiannual: return $"{CronPatternDefaults.TriggerMinute} {CronPatternDefaults.TriggerHourUTC} {dayComponent} {ComputeOffsetMonth(zeroTime, 2)}-12/6 *";
                case Scheduling.Year: return $"{CronPatternDefaults.TriggerMinute} {CronPatternDefaults.TriggerHourUTC} {dayComponent} {zeroTime.Month} *";
                case Scheduling.Immediate: // Fallthrough intended
                default:
                    throw new ArgumentOutOfRangeException(nameof(interval), interval, null);
            }
        }

        private static int ComputeOffsetMonth(DateTime zeroTime, int partitions)
        {
            //Months are 1-based so we adjust for that in the offset calculation
            return ((zeroTime.Month - 1) % (12/partitions)) + 1;
        }
    }
}