using System;
using Core.DomainModel.Advice;
using Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Controllers
{
    public class CronStringHelperTests
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

        [Theory]
        [InlineData(Scheduling.Quarter, 2000, 1, 1, "0 8 1 */3 *")]
        [InlineData(Scheduling.Quarter, 1999, 11, 29, "0 8 29 */3 *")]
        [InlineData(Scheduling.Semiannual, 2000, 1, 1, "0 8 1 */6 *")]
        [InlineData(Scheduling.Year, 2000, 1, 1, "0 8 1 1 *")]
        public void CronPerInterval_GivenIntervalAndTime_ReturnsCorrectCronString(Scheduling schedule, int year, int month, int day, string expected)
        {
            var dateToTest = new DateTime(year, month, day);
            var result = CronStringHelper.CronPerInterval(schedule, dateToTest);
            Assert.Equal(expected, result);
        }
    }
}
