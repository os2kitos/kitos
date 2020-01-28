using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Options;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.System;
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
        private readonly Mock<IItSystemRepository> _systemRepository;

        public ItSystemUsageServiceTest()
        {
            _usageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _systemRepository = new Mock<IItSystemRepository>();
            _sut = new ItSystemUsageService(
                _usageRepository.Object,
                _authorizationContext.Object,
                _systemRepository.Object,
                new Mock<IItContractRepository>().Object,
                new Mock<IOptionsService<SystemRelation, RelationFrequencyType>>().Object,
                new Mock<IOrganizationalUserContext>().Object);
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
            Assert.Equal(OperationFailure.Conflict, result.Error);
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
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void Add_Returns_BadInput_If_ItSystemNotFound()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage { ItSystemId = A<int>() };
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(itSystemUsage)).Returns(true);
            _systemRepository.Setup(x => x.GetSystem(itSystemUsage.ItSystemId)).Returns(default(ItSystem));

            //Act
            var result = _sut.Add(itSystemUsage, new User());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.BadInput, result.Error);
        }

        [Fact]
        public void Add_Returns_Forbidden_If_ReadAccess_To_ItSystem_Is_Declined()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage { ItSystemId = A<int>(), OrganizationId = A<int>() };
            var itSystem = new ItSystem();
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(itSystemUsage)).Returns(true);
            _systemRepository.Setup(x => x.GetSystem(itSystemUsage.ItSystemId)).Returns(itSystem);
            _authorizationContext.Setup(x => x.AllowReads(itSystem)).Returns(false);

            //Act
            var result = _sut.Add(itSystemUsage, new User());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void Add_Returns_Forbidden_If_Usage_Of_Local_System_In_Other_Org_Is_Attempted()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage { ItSystemId = A<int>(), OrganizationId = A<int>() };
            var itSystem = new ItSystem() { OrganizationId = itSystemUsage.OrganizationId + 1, AccessModifier = AccessModifier.Local };
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(itSystemUsage)).Returns(true);
            _systemRepository.Setup(x => x.GetSystem(itSystemUsage.ItSystemId)).Returns(itSystem);
            _authorizationContext.Setup(x => x.AllowReads(itSystem)).Returns(true);

            //Act
            var result = _sut.Add(itSystemUsage, new User());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Add_Returns_Ok(bool sameOrg)
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
            var associatedItSystem = new ItSystem
            {
                OrganizationId = sameOrg ? input.OrganizationId : input.OrganizationId + 1,
                AccessModifier = AccessModifier.Public
            };
            var usageCreatedByRepo = new ItSystemUsage();
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(input)).Returns(true);
            _systemRepository.Setup(x => x.GetSystem(input.ItSystemId)).Returns(associatedItSystem);
            _authorizationContext.Setup(x => x.AllowReads(associatedItSystem)).Returns(true);
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
            Assert.Equal(OperationFailure.NotFound, result.Error);
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
            Assert.Equal(OperationFailure.Forbidden, result.Error);
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

        //TODO: Add coverage of new types
    }
}
