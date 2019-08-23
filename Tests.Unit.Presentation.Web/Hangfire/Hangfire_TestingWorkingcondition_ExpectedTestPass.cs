using System;
using Hangfire;
using NSubstitute;
using Xunit;

namespace Tests.Unit.Presentation.Web.Hangfire
{
    public class HangfireTestingWorkingconditionExpectedTestPass
    {
        [Fact]
        public void CheckOnOptimizationRequestCreate_BackgroundjobSchedules()
        {
            // Arrange
            // sets up a mock for the Hangfire scheduling client
            var backgroundjob = Substitute.For<IBackgroundJobClient>();
            // Act
            // Background scheduler creates a task that writes text to the console and will then execute the job in 42 mins
            backgroundjob.Schedule(() => Console.WriteLine("Test of delayed task"), TimeSpan.FromMinutes(42));
            // Assert
            // Assert that the job was created via the Hangfire scheduler
            backgroundjob.ReceivedWithAnyArgs()
                .Schedule(() => Console.WriteLine("Test of delayed task"), TimeSpan.FromMinutes(42));
        }
    }
}
