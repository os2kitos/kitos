using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainServices;
using Core.DomainServices.GDPR;
using Core.DomainServices.Model.EventHandlers;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DomainEvents;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.GDPR
{
    public class BuildDataProcessingRegistrationReadModelOnChangesHandlerTest : WithAutoFixture
    {
        private readonly Mock<IDataProcessingRegistrationReadModelRepository> _repository;
        private readonly BuildDataProcessingRegistrationReadModelOnChangesHandler _sut;
        private readonly Mock<IPendingReadModelUpdateRepository> _pendingUpdatesRepository;

        public BuildDataProcessingRegistrationReadModelOnChangesHandlerTest()
        {
            _repository = new Mock<IDataProcessingRegistrationReadModelRepository>();
            _pendingUpdatesRepository = new Mock<IPendingReadModelUpdateRepository>();
            _sut = new BuildDataProcessingRegistrationReadModelOnChangesHandler(_repository.Object,
                new DataProcessingRegistrationReadModelUpdate(
                    Mock.Of<IGenericRepository<DataProcessingRegistrationRoleAssignmentReadModel>>(),
                    new Mock<IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption>>().Object),
                _pendingUpdatesRepository.Object);
        }

        [Fact]
        public void Handle_Created_Adds_New_ReadModel()
        {
            //Arrange
            var registration = new DataProcessingRegistration
            {
                Id = A<int>(),
                Name = A<string>(),
                OrganizationId = A<int>()
            };

            //Act
            _sut.Handle(new EntityCreatedEvent<DataProcessingRegistration>(registration));

            //Assert
            _repository.Verify(x => x.Add(MatchSourceData(registration)), Times.Once);
        }

        [Fact]
        public void Handle_Updated_Schedules_Async_Update()
        {
            //Arrange
            var registration = new DataProcessingRegistration
            {
                Id = A<int>(),
                Name = A<string>(),
                OrganizationId = A<int>()
            };

            _repository.Setup(x => x.GetBySourceId(registration.Id)).Returns(new DataProcessingRegistrationReadModel());

            //Act
            _sut.Handle(new EntityUpdatedEvent<DataProcessingRegistration>(registration));

            //Assert
            _pendingUpdatesRepository.Verify(x => x.Add(It.Is<PendingReadModelUpdate>(pru =>
                pru.Category == PendingReadModelUpdateSourceCategory.DataProcessingRegistration &&
                pru.SourceId == registration.Id)));
        }

        [Fact]
        public void Handle_Deleted_Deletes_ReadModel()
        {
            //Arrange
            var registration = new DataProcessingRegistration
            {
                Id = A<int>(),
            };

            //Act
            _sut.Handle(new EntityDeletedEvent<DataProcessingRegistration>(registration));

            //Assert
            _repository.Verify(x => x.DeleteBySourceId(registration.Id), Times.Once);
        }

        private static DataProcessingRegistrationReadModel MatchSourceData(DataProcessingRegistration dataProcessingRegistration)
        {
            return It.Is<DataProcessingRegistrationReadModel>(rm =>
                rm.SourceEntityId == dataProcessingRegistration.Id &&
                rm.OrganizationId == dataProcessingRegistration.OrganizationId &&
                rm.Name == dataProcessingRegistration.Name);
        }
    }
}
