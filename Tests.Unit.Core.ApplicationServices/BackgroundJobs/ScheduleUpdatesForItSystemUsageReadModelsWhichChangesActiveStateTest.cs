using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.BackgroundJobs.Model.ReadModels;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.SystemUsage;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.BackgroundJobs
{
    public class ScheduleUpdatesForItSystemUsageReadModelsWhichChangesActiveStateTest : WithAutoFixture
    {
        private readonly ScheduleUpdatesForItSystemUsageReadModelsWhichChangesActiveState _sut;
        private readonly Mock<IItSystemUsageOverviewReadModelRepository> _readModelRepositoryMock;
        private readonly Mock<IPendingReadModelUpdateRepository> _pendingReadModelUpdatesRepository;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IDatabaseControl> _databaseControlMock;

        public ScheduleUpdatesForItSystemUsageReadModelsWhichChangesActiveStateTest()
        {
            _readModelRepositoryMock = new Mock<IItSystemUsageOverviewReadModelRepository>();
            _pendingReadModelUpdatesRepository = new Mock<IPendingReadModelUpdateRepository>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _databaseControlMock = new Mock<IDatabaseControl>();
            _sut = new ScheduleUpdatesForItSystemUsageReadModelsWhichChangesActiveState(
                _readModelRepositoryMock.Object,
                _pendingReadModelUpdatesRepository.Object,
                _transactionManagerMock.Object,
                _databaseControlMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_Does_Not_Mutate_Db_If_All_ReadModels_Have_The_Right_State()
        {
            //Arrange
            var transaction = SetupTransaction();
            SetupGetReadModelsWhichMustUpdateActiveState(Enumerable.Empty<ItSystemUsageOverviewReadModel>());

            //Act
            await _sut.ExecuteAsync();

            //Assert
            _databaseControlMock.Verify(x => x.SaveChanges(), Times.Never());
            transaction.Verify(x => x.Commit(), Times.Never());
            _pendingReadModelUpdatesRepository.Verify(x => x.AddMany(It.IsAny<IEnumerable<PendingReadModelUpdate>>()), Times.Never());
        }

        [Fact]
        public async Task ExecuteAsync_Schedules_Updates_To_System_Usage_If_The_Read_Model_Should_Update_Active_State()
        {
            //Arrange - two read models for the same system usage and one for another
            var transaction = SetupTransaction();
            //TODO: Test data helper - use it

            var readModel1ForSystemUsage1 = CreateReadModel(A<int>());
            var readModel2ForSystemUsage1 = CreateReadModel(readModel1ForSystemUsage1.SourceEntity.Id);
            var readModelForSystemUsage2 = CreateReadModel(A<int>());

            SetupGetReadModelsWhichMustUpdateActiveState(new[] { readModel1ForSystemUsage1, readModel2ForSystemUsage1, readModelForSystemUsage2 });

            //Act
            await _sut.ExecuteAsync();

            //Assert
            _databaseControlMock.Verify(x => x.SaveChanges(), Times.Once());
            transaction.Verify(x => x.Commit(), Times.Once());
            _pendingReadModelUpdatesRepository.Verify(x => x.AddMany(It.Is<IEnumerable<PendingReadModelUpdate>>(updates => VerifyExpectedUpdates(updates, new[] { readModel1ForSystemUsage1.SourceEntity.Id, readModelForSystemUsage2.SourceEntity.Id }))), Times.Once());
        }

        private static bool VerifyExpectedUpdates(IEnumerable<PendingReadModelUpdate> updates, int[] expectedIds)
        {
            var pendingReadModelUpdates = updates.ToList();
            Assert.Equal(expectedIds.Length, pendingReadModelUpdates.Count);
            foreach (var expectedId in expectedIds)
            {
                Assert.Contains(pendingReadModelUpdates, x => x.SourceId == expectedId);
            }
            Assert.All(pendingReadModelUpdates, x => Assert.Equal(PendingReadModelUpdateSourceCategory.ItSystemUsage, x.Category));
            return true;
        }

        private void SetupGetReadModelsWhichMustUpdateActiveState(IEnumerable<ItSystemUsageOverviewReadModel> result)
        {
            _readModelRepositoryMock
                .Setup(x => x.GetReadModelsMustUpdateToChangeActiveState())
                .Returns(result.AsQueryable());
        }

        private Mock<IDatabaseTransaction> SetupTransaction()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transaction.Object);
            return transaction;
        }

        public ItSystemUsageOverviewReadModel CreateReadModel(int systemUsageId)
        {
            return new ItSystemUsageOverviewReadModel
            {
                Id = A<int>(),
                SourceEntity = new ItSystemUsage
                {
                    Id = systemUsageId
                },
                SourceEntityId = systemUsageId
            };
        }
    }
}
