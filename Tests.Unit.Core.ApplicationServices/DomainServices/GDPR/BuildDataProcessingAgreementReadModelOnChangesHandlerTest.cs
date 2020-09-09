using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainServices;
using Core.DomainServices.GDPR;
using Core.DomainServices.Model.EventHandlers;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.GDPR
{
    public class BuildDataProcessingAgreementReadModelOnChangesHandlerTest : WithAutoFixture
    {
        private readonly Mock<IDataProcessingAgreementReadModelRepository> _repository;
        private readonly BuildDataProcessingAgreementReadModelOnChangesHandler _sut;
        private readonly Mock<IPendingReadModelUpdateRepository> _pendingUpdatesRepository;

        public BuildDataProcessingAgreementReadModelOnChangesHandlerTest()
        {
            _repository = new Mock<IDataProcessingAgreementReadModelRepository>();
            _pendingUpdatesRepository = new Mock<IPendingReadModelUpdateRepository>();
            _sut = new BuildDataProcessingAgreementReadModelOnChangesHandler(_repository.Object,
                new DataProcessingAgreementReadModelUpdate(
                    Mock.Of<IGenericRepository<DataProcessingAgreementRoleAssignmentReadModel>>()),
                _pendingUpdatesRepository.Object, Mock.Of<ITransactionManager>());
        }

        [Fact]
        public void Handle_Created_Adds_New_ReadModel()
        {
            //Arrange
            var dataProcessingAgreement = new DataProcessingAgreement
            {
                Id = A<int>(),
                Name = A<string>(),
                OrganizationId = A<int>()
            };

            //Act
            _sut.Handle(new EntityLifeCycleEvent<DataProcessingAgreement>(LifeCycleEventType.Created, dataProcessingAgreement));

            //Assert
            _repository.Verify(x => x.Add(MatchSourceData(dataProcessingAgreement)), Times.Once);
        }

        [Fact]
        public void Handle_Updated_Schedules_Async_Update()
        {
            //Arrange
            var dataProcessingAgreement = new DataProcessingAgreement
            {
                Id = A<int>(),
                Name = A<string>(),
                OrganizationId = A<int>()
            };

            _repository.Setup(x => x.GetBySourceId(dataProcessingAgreement.Id)).Returns(new DataProcessingAgreementReadModel());

            //Act
            _sut.Handle(new EntityLifeCycleEvent<DataProcessingAgreement>(LifeCycleEventType.Updated, dataProcessingAgreement));

            //Assert
            _pendingUpdatesRepository.Verify(x => x.AddIfNotPresent(It.Is<PendingReadModelUpdate>(pru =>
                pru.Category == PendingReadModelUpdateSourceCategory.DataProcessingAgreement &&
                pru.SourceId == dataProcessingAgreement.Id)));
        }

        [Fact]
        public void Handle_Deleted_Deletes_ReadModel()
        {
            //Arrange
            var dataProcessingAgreement = new DataProcessingAgreement
            {
                Id = A<int>(),
            };

            //Act
            _sut.Handle(new EntityLifeCycleEvent<DataProcessingAgreement>(LifeCycleEventType.Deleted, dataProcessingAgreement));

            //Assert
            _repository.Verify(x => x.DeleteBySourceId(dataProcessingAgreement.Id), Times.Once);
        }

        private static DataProcessingAgreementReadModel MatchSourceData(DataProcessingAgreement dataProcessingAgreement)
        {
            return It.Is<DataProcessingAgreementReadModel>(rm =>
                rm.SourceEntityId == dataProcessingAgreement.Id &&
                rm.OrganizationId == dataProcessingAgreement.OrganizationId &&
                rm.Name == dataProcessingAgreement.Name);
        }
    }
}
