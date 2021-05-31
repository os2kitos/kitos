using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Helpers;
using Core.ApplicationServices.Triggers;
using Core.DomainModel.Advice;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class AdviceTriggerFactoryTest : WithAutoFixture
    {
        [Theory]
        [InlineData(30)]
        [InlineData(29)]
        public void CreateFromAlarmDateAndScheduleReturns_12_Triggers_For_Monthly_Trigger_With_Day(int day)
        {
            //Arrange
            var alarmDate = new DateTime(2000, 3, day);
            var expectedPartitions = Enumerable.Range(0, 12).ToList();
            var expectedCronJobs = new[]
            {
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 1, day)),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 2, 28),true),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 3, day)),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 4, day)),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 5, day)),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 6, day)),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 7, day)),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 8, day)),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 9, day)),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 10, day)),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 11, day)),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 12, day))
            };

            //Act
            var adviceTriggers = AdviceTriggerFactory.CreateFrom(alarmDate, Scheduling.Month).ToList();

            //Assert
            AssertCreatedJobs(adviceTriggers, expectedPartitions, expectedCronJobs);
        }

        [Theory]
        [InlineData(5, 30)]
        [InlineData(5, 29)]
        [InlineData(8, 30)]
        [InlineData(8, 29)]
        [InlineData(11, 30)]
        [InlineData(11, 29)]
        public void CreateFromAlarmDateAndScheduleReturns_4_Triggers_For_QuarterlyTrigger_Trigger_With_Day_Which_Conflicts_With_February(int month, int day)
        {
            //Arrange
            var alarmDate = new DateTime(2000, month, day);
            var expectedPartitions = Enumerable.Range(0, 4).ToList();
            var expectedCronJobs = new[]
            {
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 2, 28),true),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 5, day)),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 8, day)),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 11, day))
            };

            //Act
            var adviceTriggers = AdviceTriggerFactory.CreateFrom(alarmDate, Scheduling.Quarter).ToList();

            //Assert
            AssertCreatedJobs(adviceTriggers, expectedPartitions, expectedCronJobs);
        }

        [Theory]
        [InlineData(8, 30)]
        [InlineData(8, 29)]
        public void CreateFromAlarmDateAndScheduleReturns_2_Triggers_For_SemiAnnualTrigger_Trigger_With_Day_Which_Conflicts_With_February(int month, int day)
        {
            //Arrange
            var alarmDate = new DateTime(2000, month, day);
            var expectedPartitions = Enumerable.Range(0, 2).ToList();
            var expectedCronJobs = new[]
            {
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 2, 28),true),
                CronStringHelper.CronPerInterval(Scheduling.Year, new DateTime(2000, 8, day))
            };

            //Act
            var adviceTriggers = AdviceTriggerFactory.CreateFrom(alarmDate, Scheduling.Semiannual).ToList();

            //Assert
            AssertCreatedJobs(adviceTriggers, expectedPartitions, expectedCronJobs);
        }

        [Theory]
        [InlineData(Scheduling.Month, 1, 31)]
        [InlineData(Scheduling.Month, 3, 31)]
        [InlineData(Scheduling.Month, 5, 31)]
        [InlineData(Scheduling.Month, 7, 31)]
        [InlineData(Scheduling.Month, 8, 31)]
        [InlineData(Scheduling.Month, 10, 31)]
        [InlineData(Scheduling.Month, 12, 31)]
        [InlineData(Scheduling.Quarter, 1, 31)]
        [InlineData(Scheduling.Quarter, 3, 31)]
        [InlineData(Scheduling.Quarter, 5, 31)]
        [InlineData(Scheduling.Quarter, 8, 31)]
        [InlineData(Scheduling.Quarter, 12, 31)]
        [InlineData(Scheduling.Semiannual, 8, 31)]
        public void CreateFromAlarmDateAndScheduleReturns_1_Trigger_With_LastDayInMonth_If_31_Is_Selected(Scheduling scheduling, int month, int day)
        {
            //Arrange
            var alarmDate = new DateTime(2000, month, day);
            var expectedInterval = CronStringHelper.CronPerInterval(scheduling, new DateTime(2000, month, day),true);

            //Act
            var adviceTriggers = AdviceTriggerFactory.CreateFrom(alarmDate, scheduling).ToList();

            //Assert
            var adviceTrigger = Assert.Single(adviceTriggers);
            Assert.False(adviceTrigger.PartitionId.HasValue);
            Assert.Equal(expectedInterval,adviceTrigger.Cron);
        }

        private static void AssertCreatedJobs(List<AdviceTrigger> adviceTriggers, List<int> expectedPartitions, string[] expectedCronJobs)
        {
            Assert.Equal(expectedPartitions.Count, adviceTriggers.Count);
            Assert.Equal(expectedPartitions.ToList(), adviceTriggers.Select(x => x.PartitionId.Value).ToList());
            Assert.Equal(expectedCronJobs.OrderBy(x => x), adviceTriggers.Select(x => x.Cron).OrderBy(x => x));
        }
    }
}
