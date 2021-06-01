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
                case Scheduling.Hour: return "0 * * * *";
                case Scheduling.Day: return "0 8 * * *";
                case Scheduling.Week: return $"0 8 * * {zeroTime.DayOfWeek:D}";
                case Scheduling.Month: return $"0 8 {dayComponent} * *";
                case Scheduling.Quarter: return $"0 8 {dayComponent} {zeroTime.Month % 3}-12/3 *";
                case Scheduling.Semiannual: return $"0 8 {dayComponent} {zeroTime.Month % 6}-12/6 *";
                case Scheduling.Year: return $"0 8 {dayComponent} {zeroTime.Month} *";
                case Scheduling.Immediate: // Fallthrough intended
                default:
                    throw new ArgumentOutOfRangeException(nameof(interval), interval, null);
            }
        }
    }
}