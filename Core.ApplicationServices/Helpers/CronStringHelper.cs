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

        public static string CronPerInterval(Scheduling interval, DateTime zeroTime)
        {
            switch (interval)
            {
                case Scheduling.Hour: return "0 * * * *"; 
                case Scheduling.Day: return "0 8 * * *"; 
                case Scheduling.Week: return $"0 8 * * {zeroTime.DayOfWeek:D}";
                case Scheduling.Month: return $"0 8 {zeroTime.Day} * *";
                case Scheduling.Quarter: return $"0 8 {zeroTime.Day} */3 *";
                case Scheduling.Semiannual: return $"0 8 {zeroTime.Day} */6 *";
                case Scheduling.Year: return $"0 8 {zeroTime.Day} {zeroTime.Month} *";
                case Scheduling.Immediate: // Fallthrough intended
                default:
                    throw new ArgumentOutOfRangeException(nameof(interval), interval, null);
            }
        }
    }
}