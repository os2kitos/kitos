using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Moq;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class ItSystemUsageServiceTest : WithAutoFixture
    {
        private readonly ItSystemUsageService _sut;
        private readonly Mock<IGenericRepository<ItSystemUsage>> _usageRepository;
        private readonly Mock<IAuthorizationContext> _authorizationContext;

        public ItSystemUsageServiceTest()
        {
            _usageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _sut = new ItSystemUsageService(_usageRepository.Object, _authorizationContext.Object);
        }

        [Fact]
        public void Add_Returns_Conflict_If_System_Already_In_Use()
        {
            //Arrange
            var organizationId = A<int>();
            var systemId = A<int>();
            var systemUsage = SetupRepositoryQueryWith(organizationId, systemId);

            //Act
            var result = _sut.Add(systemUsage, new User());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(GenericOperationFailure.Conflict, result.Error);
        }

        [Fact]
        public void Add_Returns_Forbidden_Not_Allowed_To_Create()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(itSystemUsage)).Returns(false);

            //Act
            var result = _sut.Add(itSystemUsage, new User());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(GenericOperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void Add_Returns_Ok()
        {
            //Arrange
            var objectOwner = new User();
            var input = new ItSystemUsage
            {
                ItSystemId = A<int>(),
                OrganizationId = A<int>(),
                DataLevel = A<DataSensitivityLevel>(),
                ContainsLegalInfo = A<DataOptions>(),
                AssociatedDataWorkers = new List<ItSystemUsageDataWorkerRelation> { new ItSystemUsageDataWorkerRelation(), new ItSystemUsageDataWorkerRelation() }

            };
            var usageCreatedByRepo = new ItSystemUsage();
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(input)).Returns(true);
            _usageRepository.Setup(x => x.Create()).Returns(usageCreatedByRepo);

            //Act
            var result = _sut.Add(input, objectOwner);

            //Assert
            Assert.True(result.Ok);
            var createdUsage = result.Value;
            Assert.NotSame(input, createdUsage);
            Assert.Same(usageCreatedByRepo, createdUsage);
            Assert.Same(objectOwner, createdUsage.ObjectOwner);
            Assert.Equal(input.OrganizationId, createdUsage.OrganizationId);
            Assert.Equal(input.DataLevel, createdUsage.DataLevel);
            Assert.Equal(input.ItSystemId, createdUsage.ItSystemId);
            Assert.Equal(input.ContainsLegalInfo, createdUsage.ContainsLegalInfo);
            Assert.Equal(input.AssociatedDataWorkers, createdUsage.AssociatedDataWorkers);
            _usageRepository.Verify(x => x.Insert(usageCreatedByRepo), Times.Once);
            _usageRepository.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void Delete_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            _usageRepository.Setup(x => x.GetByKey(id)).Returns(default(ItSystemUsage));

            //Act
            var result = _sut.Delete(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(GenericOperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void Delete_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var itSystemUsage = new ItSystemUsage();
            _usageRepository.Setup(x => x.GetByKey(id)).Returns(itSystemUsage);
            _authorizationContext.Setup(x => x.AllowDelete(itSystemUsage)).Returns(false);

            //Act
            var result = _sut.Delete(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(GenericOperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void Delete_Returns_Ok()
        {
            //Arrange
            var id = A<int>();
            var itSystemUsage = new ItSystemUsage();
            _usageRepository.Setup(x => x.GetByKey(id)).Returns(itSystemUsage);
            _authorizationContext.Setup(x => x.AllowDelete(itSystemUsage)).Returns(true);

            //Act
            var result = _sut.Delete(id);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itSystemUsage, result.Value);
            _usageRepository.Verify(x => x.DeleteByKeyWithReferencePreload(id), Times.Once);
            _usageRepository.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void GetByOrganizationAndSystemId_Returns_Nothing()
        {
            //Arrange
            var systemId = A<int>();
            var organizationId = A<int>();
            SetupRepositoryQueryWith(organizationId + 1, systemId + 1);

            //Act
            var result = _sut.GetByOrganizationAndSystemId(organizationId, systemId);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetByOrganizationAndSystemId_Returns()
        {
            //Arrange
            var systemId = A<int>();
            var organizationId = A<int>();
            var systemUsage = SetupRepositoryQueryWith(organizationId, systemId);

            //Act
            var result = _sut.GetByOrganizationAndSystemId(organizationId, systemId);

            //Assert
            Assert.Same(systemUsage, result);
        }

        [Fact]
        public void GetById_Returns()
        {
            //Arrange
            var id = A<int>();
            var itSystemUsage = new ItSystemUsage();
            _usageRepository.Setup(x => x.GetByKey(id)).Returns(itSystemUsage);

            //Act
            var result = _sut.GetById(id);

            //Assert
            Assert.Same(itSystemUsage, result);
        }

        private ItSystemUsage SetupRepositoryQueryWith(int organizationId, int systemId)
        {
            var itSystemUsage = new ItSystemUsage { OrganizationId = organizationId, ItSystemId = systemId };

            _usageRepository.Setup(x => x.AsQueryable())
                .Returns(new[] { itSystemUsage }.AsQueryable());

            return itSystemUsage;
        }

        private void SetupRepositoryQueryWith(IEnumerable<ItSystemUsage> response = null)
        {
            response = response ?? new ItSystemUsage[0];
            _usageRepository.Setup(x => x.AsQueryable()).Returns(response.AsQueryable());
        }
    }
}
