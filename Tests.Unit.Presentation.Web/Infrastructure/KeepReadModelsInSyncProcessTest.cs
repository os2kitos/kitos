using System;
using System.Collections.Generic;
using System.Threading;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Server;
using Hangfire.Storage;
using Infrastructure.Services.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Presentation.Web.Hangfire;
using Serilog;
using Xunit;

namespace Tests.Unit.Presentation.Web.Infrastructure
{
    public class KeepReadModelsInSyncProcessTest
    {
        private const string LockResource = "kitos:read-model-sync";
        private const int JobsPerCycle = 9;
        private readonly Mock<IBackgroundJobLauncher> _launcher = new();
        private readonly Mock<IStorageConnection> _connection = new();
        private readonly Mock<ILogger> _logger = new();

        [Fact]
        public void Execute_HoldsLockForEntireCycle_AndReleasesIt()
        {
            using var stopping = new CancellationTokenSource();
            var syncLock = new Mock<IDisposable>();
            _connection.Setup(x => x.AcquireDistributedLock(LockResource, It.IsAny<TimeSpan>()))
                .Callback(() => Assert.Empty(_launcher.Invocations))
                .Returns(syncLock.Object);
            syncLock.Setup(x => x.Dispose()).Callback(() =>
            {
                Assert.Equal(JobsPerCycle, _launcher.Invocations.Count);
                stopping.Cancel();
            });

            Execute(stopping.Token);

            syncLock.Verify(x => x.Dispose(), Times.Once);
            _connection.Verify(x => x.Dispose(), Times.Once);
            Assert.Empty(_logger.Invocations);
        }

        [Fact]
        public void Execute_WhenAnotherServerOwnsLock_SkipsAllJobsWithoutError()
        {
            _connection.Setup(x => x.AcquireDistributedLock(LockResource, It.IsAny<TimeSpan>()))
                .Throws(new PostgreSqlDistributedLockException(LockResource));

            Execute(CancellationToken.None);

            Assert.Empty(_launcher.Invocations);
            Assert.Empty(_logger.Invocations);
            _connection.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public void Execute_WhenJobFails_ReleasesLock_AndStopsCycle()
        {
            using var stopping = new CancellationTokenSource();
            var syncLock = new Mock<IDisposable>();
            _connection.Setup(x => x.AcquireDistributedLock(LockResource, It.IsAny<TimeSpan>()))
                .Returns(syncLock.Object);
            syncLock.Setup(x => x.Dispose()).Callback(stopping.Cancel);
            _launcher.Setup(x => x.LaunchPurgeDuplicatedReadModelUpdates(It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("Failed purge"));

            Execute(stopping.Token);

            syncLock.Verify(x => x.Dispose(), Times.Once);
            Assert.Single(_launcher.Invocations);
            Assert.Single(_logger.Invocations);
        }

        private void Execute(CancellationToken stoppingToken)
        {
            using var services = new ServiceCollection()
                .AddSingleton(_launcher.Object)
                .AddSingleton(_logger.Object)
                .BuildServiceProvider();
            var storage = new Mock<JobStorage>();
            storage.Setup(x => x.GetConnection()).Returns(_connection.Object);
            var context = new BackgroundProcessContext("test-server", storage.Object,
                new Dictionary<string, object>(), Guid.NewGuid(), stoppingToken,
                CancellationToken.None, CancellationToken.None);

            new KeepReadModelsInSyncProcess(services).Execute(context);
        }
    }
}
