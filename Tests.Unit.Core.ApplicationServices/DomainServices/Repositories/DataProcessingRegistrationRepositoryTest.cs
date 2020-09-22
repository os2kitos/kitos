using Core.DomainModel.GDPR;
using Core.DomainServices;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DomainEvents;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.Repositories
{
    public class DataProcessingRegistrationRepositoryTest : WithAutoFixture
    {
        private readonly DataProcessingRegistrationRepository _sut;
        private readonly Mock<IGenericRepository<DataProcessingRegistration>> _repository;
        private readonly Mock<IDomainEvents> _domainEvents;

        public DataProcessingRegistrationRepositoryTest()
        {
            _repository = new Mock<IGenericRepository<DataProcessingRegistration>>();
            _domainEvents = new Mock<IDomainEvents>();
            _sut = new DataProcessingRegistrationRepository(_repository.Object, _domainEvents.Object);
        }

        [Fact]
        public void Can_Add()
        {
            //Arrange
            var registration = new DataProcessingRegistration();
            _repository.Setup(x => x.Insert(registration)).Returns<DataProcessingRegistration>(x => x);

            //Act
            var processingAgreement = _sut.Add(registration);

            //Assert
            Assert.Same(registration, processingAgreement);
            VerifyLifeCycleEvent(registration, LifeCycleEventType.Created);
            VerifySaved();
        }

        [Fact]
        public void Delete_Returns_True_If_Found_And_Deleted()
        {
            //Arrange
            var id = A<int>();
            var registration = new DataProcessingRegistration();
            _repository.Setup(x => x.GetByKey(id)).Returns(registration);

            //Act
            var deleted = _sut.DeleteById(id);

            //Assert
            _repository.Verify(x => x.DeleteWithReferencePreload(registration), Times.Once);
            Assert.True(deleted);
            VerifyLifeCycleEvent(registration, LifeCycleEventType.Deleted);
            VerifySaved();
        }

        [Fact]
        public void Update_Saves_And_Notifies()
        {
            //Arrange
            var registration = new DataProcessingRegistration();

            //Act
            _sut.Update(registration);

            //Assert
            VerifySaved();
            VerifyLifeCycleEvent(registration, LifeCycleEventType.Updated);
        }

        private void VerifySaved()
        {
            _repository.Verify(x => x.Save(), Times.Once);
        }

        private void VerifyLifeCycleEvent(DataProcessingRegistration dataProcessingRegistration, LifeCycleEventType lifeCycleEventType)
        {
            _domainEvents.Verify(x => x.Raise(It.Is<EntityLifeCycleEvent<DataProcessingRegistration>>(args =>
                args.ChangeType == lifeCycleEventType && args.Entity == dataProcessingRegistration)));
        }
    }
}
