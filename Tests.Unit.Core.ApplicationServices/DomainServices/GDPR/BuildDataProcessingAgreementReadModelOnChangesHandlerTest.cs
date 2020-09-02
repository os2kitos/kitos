using Core.DomainModel.GDPR;
using Core.DomainServices.Model.EventHandlers;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.GDPR
{
    public class BuildDataProcessingAgreementReadModelOnChangesHandlerTest : WithAutoFixture
    {
        private readonly Mock<IDataProcessingAgreementReadModelRepository> _repository;
        private readonly BuildDataProcessingAgreementReadModelOnChangesHandler _sut;

        public BuildDataProcessingAgreementReadModelOnChangesHandlerTest()
        {
            _repository = new Mock<IDataProcessingAgreementReadModelRepository>();
            _sut = new BuildDataProcessingAgreementReadModelOnChangesHandler(_repository.Object);
        }

        [Fact]
        public void Handle_Created_Adds_New_ReadModel()
        {
            //Arrange
            var dataProcessingAgreement = new DataProcessingAgreement()
            {
                Id = A<int>(),
                Name = A<string>(),
                OrganizationId = A<int>()
            };

            //Act
            _sut.Handle(new EntityLifeCycleEvent<DataProcessingAgreement>(LifeCycleEventType.Created, dataProcessingAgreement));

            //Assert
            _repository.Verify(x => x.Add(MatchSourceData(dataProcessingAgreement)));
        }

        [Fact]
        public void Handle_Updated_Adds_New_ReadModel_If_None_Exists()
        {
            //Arrange
            var dataProcessingAgreement = new DataProcessingAgreement
            {
                Id = A<int>(),
                Name = A<string>(),
                OrganizationId = A<int>()
            };

            _repository.Setup(x => x.GetBySourceId(dataProcessingAgreement.Id)).Returns(Maybe<DataProcessingAgreementReadModel>.None);

            //Act
            _sut.Handle(new EntityLifeCycleEvent<DataProcessingAgreement>(LifeCycleEventType.Updated, dataProcessingAgreement));

            //Assert
            _repository.Verify(x => x.Add(MatchSourceData(dataProcessingAgreement)));
        }

        [Fact]
        public void Handle_Updated_Updates_Existing_Model()
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
            _repository.Verify(x => x.Update(MatchSourceData(dataProcessingAgreement)));
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
