using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.BackgroundJobs.Model.ReadModels;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.SystemUsage;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.BackgroundJobs
{
    public class RebuildItSystemUsageOverviewReadModelsBatchJobTest : WithAutoFixture
    {
        private readonly RebuildItSystemUsageOverviewReadModelsBatchJob _sut;
        private readonly Mock<IPendingReadModelUpdateRepository> _pendingUpdates;
        private readonly Mock<IItSystemUsageBatchLoadRepository> _sources;
        private readonly Mock<IItSystemUsageOverviewReadModelRepository> _readModels;
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewReadModel>> _readModelRepository;
        private readonly Mock<IGenericRepository<PendingReadModelUpdate>> _pendingRepository;
        private readonly Mock<IReadModelUpdate<ItSystemUsage, ItSystemUsageOverviewReadModel>> _updater;
        private readonly Mock<IDatabaseControl> _databaseControl;
        private readonly Mock<IDatabaseTransaction> _transaction;

        public RebuildItSystemUsageOverviewReadModelsBatchJobTest()
        {
            _pendingUpdates = new Mock<IPendingReadModelUpdateRepository>();
            _sources = new Mock<IItSystemUsageBatchLoadRepository>();
            _readModels = new Mock<IItSystemUsageOverviewReadModelRepository>();
            _readModelRepository = new Mock<IGenericRepository<ItSystemUsageOverviewReadModel>>();
            _pendingRepository = new Mock<IGenericRepository<PendingReadModelUpdate>>();
            _updater = new Mock<IReadModelUpdate<ItSystemUsage, ItSystemUsageOverviewReadModel>>();
            _databaseControl = new Mock<IDatabaseControl>();
            _transaction = new Mock<IDatabaseTransaction>();
            var transactionManager = new Mock<ITransactionManager>();
            transactionManager.Setup(manager => manager.Begin()).Returns(_transaction.Object);
            _sut = new RebuildItSystemUsageOverviewReadModelsBatchJob(
                Mock.Of<ILogger>(), _pendingUpdates.Object, _updater.Object, _readModels.Object,
                Mock.Of<IItSystemUsageRepository>(), _sources.Object, transactionManager.Object,
                _readModelRepository.Object, _pendingRepository.Object, _databaseControl.Object);
        }

        [Fact]
        public async Task ExecuteAsync_Repeated_Updates_For_New_Usage_Inserts_Only_One_Read_Model()
        {
            var source = new ItSystemUsage { Id = A<int>() };
            var pendingUpdates = SetupRepeatedUpdates(source.Id, source);
            _readModels.Setup(repository => repository.GetBySourceIds(It.IsAny<IEnumerable<int>>()))
                .Returns(new Dictionary<int, ItSystemUsageOverviewReadModel>());
            var insertedReadModels = new List<ItSystemUsageOverviewReadModel>();
            _readModelRepository.Setup(repository => repository.Insert(It.IsAny<ItSystemUsageOverviewReadModel>()))
                .Callback<ItSystemUsageOverviewReadModel>(model => insertedReadModels.Add(model))
                .Returns((ItSystemUsageOverviewReadModel model) => model);

            var result = await _sut.ExecuteAsync();

            Assert.True(result.Ok);
            var insertedReadModel = Assert.Single(insertedReadModels);
            _updater.Verify(updater => updater.Apply(source, insertedReadModel), Times.Exactly(pendingUpdates.Length));
            VerifyUpdatesCompleted(pendingUpdates);
        }

        [Fact]
        public async Task ExecuteAsync_Repeated_Updates_For_Existing_Usage_Reuses_Read_Model()
        {
            var source = new ItSystemUsage { Id = A<int>() };
            var pendingUpdates = SetupRepeatedUpdates(source.Id, source);
            var readModel = new ItSystemUsageOverviewReadModel { SourceEntityId = source.Id };
            _readModels.Setup(repository => repository.GetBySourceIds(It.IsAny<IEnumerable<int>>()))
                .Returns(new Dictionary<int, ItSystemUsageOverviewReadModel> { { source.Id, readModel } });

            var result = await _sut.ExecuteAsync();

            Assert.True(result.Ok);
            _readModelRepository.Verify(repository => repository.Insert(It.IsAny<ItSystemUsageOverviewReadModel>()), Times.Never);
            _updater.Verify(updater => updater.Apply(source, readModel), Times.Exactly(pendingUpdates.Length));
            VerifyUpdatesCompleted(pendingUpdates);
        }

        [Fact]
        public async Task ExecuteAsync_Repeated_Updates_For_Deleted_Usage_Deletes_Read_Model_Once()
        {
            var sourceId = A<int>();
            var pendingUpdates = SetupRepeatedUpdates(sourceId, null);
            var readModel = new ItSystemUsageOverviewReadModel { SourceEntityId = sourceId };
            _readModels.Setup(repository => repository.GetBySourceIds(It.IsAny<IEnumerable<int>>()))
                .Returns(new Dictionary<int, ItSystemUsageOverviewReadModel> { { sourceId, readModel } });

            var result = await _sut.ExecuteAsync();

            Assert.True(result.Ok);
            _readModelRepository.Verify(repository => repository.Delete(readModel), Times.Once);
            _readModelRepository.Verify(repository => repository.Insert(It.IsAny<ItSystemUsageOverviewReadModel>()), Times.Never);
            VerifyUpdatesCompleted(pendingUpdates);
        }

        [Fact]
        public async Task ExecuteAsync_Save_Fails_Does_Not_Commit_Or_Process_Next_Update()
        {
            var source = new ItSystemUsage { Id = A<int>() };
            var pendingUpdates = SetupRepeatedUpdates(source.Id, source);
            _readModels.Setup(repository => repository.GetBySourceIds(It.IsAny<IEnumerable<int>>()))
                .Returns(new Dictionary<int, ItSystemUsageOverviewReadModel>());
            _databaseControl.Setup(control => control.SaveChanges()).Throws(new InvalidOperationException(A<string>()));

            var result = await _sut.ExecuteAsync();

            Assert.True(result.Failed);
            _transaction.Verify(transaction => transaction.Commit(), Times.Never);
            _transaction.Verify(transaction => transaction.Dispose(), Times.Once);
            _pendingRepository.Verify(repository => repository.Delete(pendingUpdates.Last()), Times.Never);
            _readModelRepository.Verify(repository => repository.Insert(It.IsAny<ItSystemUsageOverviewReadModel>()), Times.Once);
        }

        private PendingReadModelUpdate[] SetupRepeatedUpdates(int sourceId, ItSystemUsage? source)
        {
            var pendingUpdates = new[]
            {
                PendingReadModelUpdate.Create(sourceId, PendingReadModelUpdateSourceCategory.ItSystemUsage),
                PendingReadModelUpdate.Create(sourceId, PendingReadModelUpdateSourceCategory.ItSystemUsage)
            };
            _pendingUpdates.Setup(repository => repository.GetMany(PendingReadModelUpdateSourceCategory.ItSystemUsage, It.IsAny<int>()))
                .Returns(pendingUpdates.AsQueryable());
            _sources.Setup(repository => repository.GetForReadModelRebuild(It.IsAny<IEnumerable<int>>()))
                .Returns(source == null
                    ? new Dictionary<int, ItSystemUsage>()
                    : new Dictionary<int, ItSystemUsage> { { sourceId, source } });
            return pendingUpdates;
        }

        private void VerifyUpdatesCompleted(PendingReadModelUpdate[] pendingUpdates)
        {
            foreach (var pendingUpdate in pendingUpdates)
            {
                _pendingRepository.Verify(repository => repository.Delete(pendingUpdate), Times.Once);
            }
            _databaseControl.Verify(control => control.SaveChanges(), Times.Exactly(pendingUpdates.Length));
            _transaction.Verify(transaction => transaction.Commit(), Times.Exactly(pendingUpdates.Length));
        }
    }
}
