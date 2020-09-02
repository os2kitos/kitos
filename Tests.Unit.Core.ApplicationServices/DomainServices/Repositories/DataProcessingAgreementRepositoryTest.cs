using Core.DomainModel.GDPR;
using Core.DomainServices;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DomainEvents;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.Repositories
{
    public class DataProcessingAgreementRepositoryTest : WithAutoFixture
    {
        private readonly DataProcessingAgreementRepository _sut;
        private readonly Mock<IGenericRepository<DataProcessingAgreement>> _repository;
        private readonly Mock<IDomainEvents> _domainEvents;

        public DataProcessingAgreementRepositoryTest()
        {
            _repository = new Mock<IGenericRepository<DataProcessingAgreement>>();
            _domainEvents = new Mock<IDomainEvents>();
            _sut = new DataProcessingAgreementRepository(_repository.Object, _domainEvents.Object);
        }

        [Fact]
        public void Can_Add()
        {
            //Arrange
            var dataProcessingAgreement = new DataProcessingAgreement();
            _repository.Setup(x => x.Insert(dataProcessingAgreement)).Returns<DataProcessingAgreement>(x => x);

            //Act
            var processingAgreement = _sut.Add(dataProcessingAgreement);

            //Assert
            Assert.Same(dataProcessingAgreement, processingAgreement);
            VerifyLifeCycleEvent(dataProcessingAgreement, LifeCycleEventType.Created);
            VerifySaved();
        }

        [Fact]
        public void Delete_Returns_True_If_Found_And_Deleted()
        {
            //Arrange
            var id = A<int>();
            var dataProcessingAgreement = new DataProcessingAgreement();
            _repository.Setup(x => x.GetByKey(id)).Returns(dataProcessingAgreement);

            //Act
            var deleted = _sut.DeleteById(id);

            //Assert
            _repository.Verify(x => x.DeleteWithReferencePreload(dataProcessingAgreement), Times.Once);
            Assert.True(deleted);
            VerifyLifeCycleEvent(dataProcessingAgreement, LifeCycleEventType.Deleted);
            VerifySaved();
        }

        [Fact]
        public void Update_Saves_And_Notifies()
        {
            //Arrange
            var dataProcessingAgreement = new DataProcessingAgreement();

            //Act
            _sut.Update(dataProcessingAgreement);

            //Assert
            VerifySaved();
            VerifyLifeCycleEvent(dataProcessingAgreement, LifeCycleEventType.Updated);
        }

        private void VerifySaved()
        {
            _repository.Verify(x => x.Save(), Times.Once);
        }

        private void VerifyLifeCycleEvent(DataProcessingAgreement dataProcessingAgreement, LifeCycleEventType lifeCycleEventType)
        {
            _domainEvents.Verify(x => x.Raise(It.Is<EntityLifeCycleEvent<DataProcessingAgreement>>(args =>
                args.ChangeType == lifeCycleEventType && args.Entity == dataProcessingAgreement)));
        }
    }
}
