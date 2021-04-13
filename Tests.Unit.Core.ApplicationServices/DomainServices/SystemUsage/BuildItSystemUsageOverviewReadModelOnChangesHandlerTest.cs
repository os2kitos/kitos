using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.SystemUsage;
using Core.DomainServices.SystemUsage;
using Infrastructure.Services.DomainEvents;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.SystemUsage
{
    public class BuildItSystemUsageOverviewReadModelOnChangesHandlerTest : WithAutoFixture
    {
        private readonly Mock<IItSystemUsageOverviewReadModelRepository> _repository;
        private readonly BuildItSystemUsageOverviewReadModelOnChangesHandler _sut;
        private readonly Mock<IPendingReadModelUpdateRepository> _pendingUpdatesRepository;

        public BuildItSystemUsageOverviewReadModelOnChangesHandlerTest()
        {
            _repository = new Mock<IItSystemUsageOverviewReadModelRepository>();
            _pendingUpdatesRepository = new Mock<IPendingReadModelUpdateRepository>();
            _sut = new BuildItSystemUsageOverviewReadModelOnChangesHandler(_pendingUpdatesRepository.Object,
                _repository.Object,
                new ItSystemUsageOverviewReadModelUpdate());
        }

        [Fact]
        public void Handle_Created_Adds_New_ReadModel()
        {
            //Arrange
            var itSystem = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>(),
                Disabled = A<bool>()
            };
            var itSystemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                ItSystemId = itSystem.Id,
                ItSystem = itSystem
            };

            //Act
            _sut.Handle(new EntityCreatedEvent<ItSystemUsage>(itSystemUsage));

            //Assert
            _repository.Verify(x => x.Add(MatchSourceData(itSystemUsage, itSystem)), Times.Once);
        }

        [Fact]
        public void Handle_Updated_Schedules_Async_Update()
        {
            //Arrange
            var itSystem = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>(),
                Disabled = A<bool>()
            };
            var itSystemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                ItSystemId = itSystem.Id
            };

            _repository.Setup(x => x.GetBySourceId(itSystemUsage.Id)).Returns(new ItSystemUsageOverviewReadModel());

            //Act
            _sut.Handle(new EntityUpdatedEvent<ItSystemUsage>(itSystemUsage));

            //Assert
            _pendingUpdatesRepository.Verify(x => x.Add(It.Is<PendingReadModelUpdate>(pru =>
                pru.Category == PendingReadModelUpdateSourceCategory.ItSystemUsage &&
                pru.SourceId == itSystemUsage.Id)));
        }

        [Fact]
        public void Handle_Deleted_Deletes_ReadModel()
        {
            //Arrange
            var itSystem = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>(),
                Disabled = A<bool>()
            };
            var itSystemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                ItSystemId = itSystem.Id
            };

            //Act
            _sut.Handle(new EntityDeletedEvent<ItSystemUsage>(itSystemUsage));

            //Assert
            _repository.Verify(x => x.DeleteBySourceId(itSystemUsage.Id), Times.Once);
        }

        private static ItSystemUsageOverviewReadModel MatchSourceData(ItSystemUsage itSystemUsage, ItSystem itSystem)
        {
            return It.Is<ItSystemUsageOverviewReadModel>(rm =>
                rm.SourceEntityId == itSystemUsage.Id &&
                rm.OrganizationId == itSystemUsage.OrganizationId &&
                rm.Name == itSystem.Name &&
                rm.ItSystemDisabled == itSystem.Disabled);
        }
    }
}
