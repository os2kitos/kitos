using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Repositories.SystemUsage;
using Moq;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class ItSystemUsageServiceTest : WithAutoFixture
    {
        private readonly Mock<IItSystemUsageRepository> _repository;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly ItSystemUsageService _sut;

        public ItSystemUsageServiceTest()
        {
            _repository = new Mock<IItSystemUsageRepository>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _sut = new ItSystemUsageService(null, _repository.Object, _authorizationContext.Object);
        }

        [Fact]
        public void CanAddDataWorkerRelation_Returns_False_If_Usage_Is_Null()
        {
            //Arrange
            var usageId = A<int>();
            var organizationId = A<int>();
            ExpectGetSystemUsageReturns(usageId, default(ItSystemUsage));

            //Act
            var result = _sut.CanAddDataWorkerRelation(usageId, organizationId);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void CanAddDataWorkerRelation_Returns_False_If_Modification_Of_System_Usage_Is_UnAuthorized()
        {
            //Arrange
            var usageId = A<int>();
            var organizationId = A<int>();
            var systemUsage = new ItSystemUsage();
            ExpectGetSystemUsageReturns(usageId, systemUsage);
            ExpectAllowModifyReturns(systemUsage, false);

            //Act
            var result = _sut.CanAddDataWorkerRelation(usageId, organizationId);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void CanAddDataWorkerRelation_Returns_False_If_Creation_Of_DataWorkers_Returns_False()
        {
            //Arrange
            var usageId = A<int>();
            var organizationId = A<int>();
            var systemUsage = new ItSystemUsage();
            ExpectGetSystemUsageReturns(usageId, systemUsage);
            ExpectAllowModifyReturns(systemUsage, true);
            ExpectAllowCreateDataWorkerReturns(false);

            //Act
            var result = _sut.CanAddDataWorkerRelation(usageId, organizationId);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void CanAddDataWorkerRelation_Returns_False_If_Overlapping_Relation_Exists()
        {
            //Arrange
            var usageId = A<int>();
            var organizationId = A<int>();
            var systemUsage = new ItSystemUsage()
            {
                AssociatedDataWorkers = { new ItSystemUsageDataWorkerRelation { DataWorkerId = organizationId } }
            };
            ExpectGetSystemUsageReturns(usageId, systemUsage);
            ExpectAllowModifyReturns(systemUsage, true);
            ExpectAllowCreateDataWorkerReturns(true);

            //Act
            var result = _sut.CanAddDataWorkerRelation(usageId, organizationId);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void CanAddDataWorkerRelation_Returns_True()
        {
            //Arrange
            var usageId = A<int>();
            var organizationId = A<int>();
            var systemUsage = new ItSystemUsage()
            {
                AssociatedDataWorkers = { new ItSystemUsageDataWorkerRelation { DataWorkerId = A<int>() } }
            };
            ExpectGetSystemUsageReturns(usageId, systemUsage);
            ExpectAllowModifyReturns(systemUsage, true);
            ExpectAllowCreateDataWorkerReturns(true);

            //Act
            var result = _sut.CanAddDataWorkerRelation(usageId, organizationId);

            //Assert
            Assert.True(result);
        }

        private void ExpectAllowCreateDataWorkerReturns(bool value)
        {
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsageDataWorkerRelation>()).Returns(value);
        }

        private void ExpectAllowModifyReturns(IEntity systemUsage, bool value)
        {
            _authorizationContext.Setup(x => x.AllowModify(systemUsage)).Returns(value);
        }

        private void ExpectGetSystemUsageReturns(int usageId, ItSystemUsage itSystemUsage)
        {
            _repository.Setup(x => x.GetSystemUsage(usageId)).Returns(itSystemUsage);
        }
    }
}
