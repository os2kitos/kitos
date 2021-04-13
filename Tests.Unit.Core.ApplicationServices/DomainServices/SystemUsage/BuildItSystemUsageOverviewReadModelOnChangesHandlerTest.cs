using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices.Model;
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
        private readonly Mock<IReadModelUpdate<ItSystemUsage, ItSystemUsageOverviewReadModel>> _readModelUpdate;

        public BuildItSystemUsageOverviewReadModelOnChangesHandlerTest()
        {
            _repository = new Mock<IItSystemUsageOverviewReadModelRepository>();
            _pendingUpdatesRepository = new Mock<IPendingReadModelUpdateRepository>();
            _readModelUpdate = new Mock<IReadModelUpdate<ItSystemUsage, ItSystemUsageOverviewReadModel>>();
            _sut = new BuildItSystemUsageOverviewReadModelOnChangesHandler(_pendingUpdatesRepository.Object,
                _repository.Object,
                _readModelUpdate.Object);
        }

        [Fact]
        public void Handle_Created_Adds_New_ReadModel()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();

            //Act
            _sut.Handle(new EntityCreatedEvent<ItSystemUsage>(itSystemUsage));

            //Assert
            _readModelUpdate.Verify(x => x.Apply(itSystemUsage, It.IsAny<ItSystemUsageOverviewReadModel>()), Times.Once);
            _repository.Verify(x => x.Add(It.IsAny<ItSystemUsageOverviewReadModel>()), Times.Once);
        }

        [Fact]
        public void Handle_Updated_Schedules_Async_Update()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage
            {
                Id = A<int>()
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
            var itSystemUsage = new ItSystemUsage
            {
                Id = A<int>()
            };

            //Act
            _sut.Handle(new EntityDeletedEvent<ItSystemUsage>(itSystemUsage));

            //Assert
            _repository.Verify(x => x.DeleteBySourceId(itSystemUsage.Id), Times.Once);
        }
    }
}
