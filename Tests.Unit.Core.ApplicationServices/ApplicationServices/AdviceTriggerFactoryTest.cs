using System;
using System.Linq;
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
            var expectedPartitions = Enumerable.Range(0,12).ToList();

            //Act
            var adviceTriggers = AdviceTriggerFactory.CreateFrom(alarmDate, Scheduling.Month).ToList();

            //Assert
            Assert.Equal(12, adviceTriggers.Count);
            Assert.Equal(expectedPartitions.ToList(),adviceTriggers.Select(x=>x.PartitionId.Value).ToList());
        }

        [Theory]
        [InlineData(5,30)]
        [InlineData(5,29)]
        [InlineData(8, 30)]
        [InlineData(8, 29)]
        [InlineData(11, 30)]
        [InlineData(11, 29)]
        public void CreateFromAlarmDateAndScheduleReturns_4_Triggers_For_QuarterlyTrigger_Trigger_With_Day_Which_Conflicts_With_February(int month, int day)
        {
            //Arrange
            var alarmDate = new DateTime(2000, month, day);
            var expectedPartitions = Enumerable.Range(0, 4).ToList();

            //Act
            var adviceTriggers = AdviceTriggerFactory.CreateFrom(alarmDate, Scheduling.Quarter).ToList();

            //Assert
            Assert.Equal(4, adviceTriggers.Count);
            Assert.Equal(expectedPartitions.ToList(), adviceTriggers.Select(x => x.PartitionId.Value).ToList());
        }

        [Theory]
        [InlineData(8, 30)]
        [InlineData(8, 29)]
        public void CreateFromAlarmDateAndScheduleReturns_2_Triggers_For_SemiAnnualTrigger_Trigger_With_Day_Which_Conflicts_With_February(int month, int day)
        {
            //Arrange
            var alarmDate = new DateTime(2000, month, day);
            var expectedPartitions = Enumerable.Range(0, 2).ToList();

            //Act
            var adviceTriggers = AdviceTriggerFactory.CreateFrom(alarmDate, Scheduling.Semiannual).ToList();

            //Assert
            Assert.Equal(2, adviceTriggers.Count);
            Assert.Equal(expectedPartitions.ToList(), adviceTriggers.Select(x => x.PartitionId.Value).ToList());
        }
    }
}
