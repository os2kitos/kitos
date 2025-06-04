using Core.Abstractions.Types;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainServices;
using Core.DomainServices.GDPR;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.GDPR;
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
        private readonly Mock<IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption>> _basisForTransferOptionService;
        private readonly Mock<IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption>> _dataResponsibleOptionService;

        public BuildDataProcessingRegistrationReadModelOnChangesHandlerTest()
        {

            _repository = new Mock<IDataProcessingRegistrationReadModelRepository>();
            _pendingUpdatesRepository = new Mock<IPendingReadModelUpdateRepository>();
            _basisForTransferOptionService =
                new Mock<IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption>>();
            _dataResponsibleOptionService =
                new Mock<IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption>>();

            _sut = new BuildDataProcessingRegistrationReadModelOnChangesHandler(_repository.Object,
                new DataProcessingRegistrationReadModelUpdate(
                    Mock.Of<IGenericRepository<DataProcessingRegistrationRoleAssignmentReadModel>>(),
                    _basisForTransferOptionService.Object,
                    _dataResponsibleOptionService.Object,
                    new Mock<IOptionsService<DataProcessingRegistration, DataProcessingOversightOption>>().Object),
                _pendingUpdatesRepository.Object);
        }

        [Fact]
        public void Handle_Created_Adds_New_ReadModel()
        {
            var dataResponsible = new DataProcessingDataResponsibleOption
            {
                Name = A<string>()
            };
            var basisForTransfer = new DataProcessingBasisForTransferOption
            {
                Name = A<string>()
            };
            //Arrange
            var registration = new DataProcessingRegistration
            {
                Id = A<int>(),
                Name = A<string>(),
                OrganizationId = A<int>(),
                DataResponsible = dataResponsible,
                BasisForTransfer = basisForTransfer
            };
            _basisForTransferOptionService.Setup(_ => _.GetOption(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Maybe<(DataProcessingBasisForTransferOption, bool)>.Some((basisForTransfer, true)));
            _dataResponsibleOptionService.Setup(_ => _.GetOption(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Maybe<(DataProcessingDataResponsibleOption, bool)>.Some((dataResponsible, true)));

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
            _sut.Handle(new EntityBeingDeletedEvent<DataProcessingRegistration>(registration));

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
