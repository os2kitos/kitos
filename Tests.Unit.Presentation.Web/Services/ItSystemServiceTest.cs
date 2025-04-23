using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.References;
using Core.ApplicationServices.System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Model;
using Core.DomainServices.Options;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.TaskRefs;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.Events;
using Core.DomainServices.Generic;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class ItSystemServiceTest : WithAutoFixture
    {
        private readonly ItSystemService _sut;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IItSystemRepository> _systemRepository;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IDatabaseTransaction> _dbTransaction;
        private readonly Mock<IReferenceService> _referenceService;
        private readonly Mock<ILogger> _logger;
        private readonly Mock<IOrganizationalUserContext> _userContext;
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly Mock<IOptionsService<ItSystem, BusinessType>> _businessTypeServiceMock;
        private readonly Mock<ITaskRefRepository> _taskRefRepositoryMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<IItInterfaceService> _interfaceServiceMock;
        private readonly Mock<IItSystemUsageService> _systemUsageServiceMock;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IEntityIdentityResolver> _entityIdentityResolver;

        public ItSystemServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _systemRepository = new Mock<IItSystemRepository>();
            _transactionManager = new Mock<ITransactionManager>();
            _dbTransaction = new Mock<IDatabaseTransaction>();
            _referenceService = new Mock<IReferenceService>();
            _logger = new Mock<ILogger>();
            _userContext = new Mock<IOrganizationalUserContext>();
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _businessTypeServiceMock = new Mock<IOptionsService<ItSystem, BusinessType>>();
            _taskRefRepositoryMock = new Mock<ITaskRefRepository>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _interfaceServiceMock = new Mock<IItInterfaceService>();
            _systemUsageServiceMock = new Mock<IItSystemUsageService>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _entityIdentityResolver = new Mock<IEntityIdentityResolver>();
            _sut = new ItSystemService(
                _systemRepository.Object,
                _authorizationContext.Object,
                _transactionManager.Object,
                _referenceService.Object,
                _taskRefRepositoryMock.Object,
                _businessTypeServiceMock.Object,
                _organizationRepositoryMock.Object,
                _logger.Object,
                _userContext.Object,
                _domainEventsMock.Object,
                Mock.Of<IOperationClock>(),
                _interfaceServiceMock.Object,
                _systemUsageServiceMock.Object,
                _organizationServiceMock.Object,
                _entityIdentityResolver.Object
                );
        }

        [Fact]
        public void GetSystem_Returns_Not_Found_If_System_Does_Not_Exist()
        {
            //Arrange
            var uuid = A<Guid>();
            ExpectGetSystemReturns(uuid, Maybe<ItSystem>.None);

            //Act
            var system = _sut.GetSystem(uuid);

            //Assert
            Assert.True(system.Failed);
            Assert.Equal(OperationFailure.NotFound, system.Error.FailureType);
        }

        [Fact]
        public void GetSystem_Returns_Forbidden_If_ReadAccessIsDeclined()
        {
            //Arrange
            var uuid = A<Guid>();
            var itSystem = CreateSystem();
            ExpectGetSystemReturns(uuid, itSystem);
            ExpectAllowReadsReturns(itSystem, false);

            //Act
            var system = _sut.GetSystem(uuid);

            //Assert
            Assert.True(system.Failed);
            Assert.Equal(OperationFailure.Forbidden, system.Error.FailureType);
        }

        [Fact]
        public void GetSystem_Returns_Ok()
        {
            //Arrange
            var uuid = A<Guid>();
            var itSystem = CreateSystem();
            ExpectGetSystemReturns(uuid, itSystem);
            ExpectAllowReadsReturns(itSystem, true);

            //Act
            var system = _sut.GetSystem(uuid);

            //Assert
            Assert.True(system.Ok);
            Assert.Same(itSystem, system.Value);
        }

        [Fact]
        public void GetAvailableSystems_With_Full_Access()
        {
            //Arrange
            var all = new List<ItSystem> { CreateSystem(), CreateSystem() };
            ExpectGetSystemsReturns(all);
            ExpectGetCrossLevelOrganizationAccessReturns(CrossOrganizationDataReadAccessLevel.All);

            //Act
            var availableSystems = _sut.GetAvailableSystems().ToList();

            //Assert
            Assert.Equal(all, availableSystems);
        }

        [Fact]
        public void GetAvailableSystems_With_Own_OrganizationOnlyAccess()
        {
            //Arrange
            var ownOrganizationId1 = A<int>();
            var ownOrganizationId2 = ownOrganizationId1 + 1;
            var otherOrganizationId = ownOrganizationId2 + 1;
            var includedSystem1 = CreateSystem(ownOrganizationId1);
            var includedSystem2 = CreateSystem(ownOrganizationId2);
            var all = new List<ItSystem> { includedSystem1, CreateSystem(otherOrganizationId), CreateSystem(otherOrganizationId), includedSystem2 };
            ExpectGetSystemsReturns(all);
            ExpectGetCrossLevelOrganizationAccessReturns(CrossOrganizationDataReadAccessLevel.None);
            ExpectGetUserOrganizationIdsReturns(ownOrganizationId1, ownOrganizationId2);

            //Act
            var availableSystems = _sut.GetAvailableSystems().ToList();

            //Assert
            Assert.Equal(new[] { includedSystem1, includedSystem2 }, availableSystems);
        }

        [Fact]
        public void GetAvailableSystems_With_Public_Shared_Access()
        {
            //Arrange
            var ownOrganizationId1 = A<int>();
            var ownOrganizationId2 = ownOrganizationId1 + 1;
            var otherOrganizationId = ownOrganizationId2 + 1;
            var includedSystem1 = CreateSystem(ownOrganizationId1);
            var includedSystem2 = CreateSystem(ownOrganizationId2);
            var includedBySharing = CreateSystem(otherOrganizationId, AccessModifier.Public);
            var all = new List<ItSystem> { includedSystem1, includedBySharing, CreateSystem(otherOrganizationId, AccessModifier.Local), includedSystem2 };
            ExpectGetSystemsReturns(all);
            ExpectGetCrossLevelOrganizationAccessReturns(CrossOrganizationDataReadAccessLevel.Public);
            ExpectGetUserOrganizationIdsReturns(ownOrganizationId1, ownOrganizationId2);

            //Act
            var availableSystems = _sut.GetAvailableSystems().ToList();

            //Assert
            Assert.Equal(new[] { includedSystem1, includedBySharing, includedSystem2 }, availableSystems);
        }

        [Fact]
        public void GetAvailableSystems_With_RightsHolders_Access()
        {
            //Arrange
            var mainOrg = A<int>();
            var rightsHoldingOrganization1 = mainOrg + 1;
            var rightsHoldingOrganization2 = rightsHoldingOrganization1 + 1;
            var otherOrganizationId = rightsHoldingOrganization2 + 1;
            var includedSystem1 = CreateSystem(mainOrg, AccessModifier.Public, rightsHoldingOrganization1);
            var includedSystem2 = CreateSystem(mainOrg, AccessModifier.Local, rightsHoldingOrganization2);
            var excludedWrongRightsHolder = CreateSystem(otherOrganizationId, AccessModifier.Public, otherOrganizationId);
            var all = new List<ItSystem> { includedSystem1, excludedWrongRightsHolder, CreateSystem(otherOrganizationId), includedSystem2 };
            ExpectGetSystemsReturns(all);
            ExpectGetCrossLevelOrganizationAccessReturns(CrossOrganizationDataReadAccessLevel.RightsHolder);
            _userContext.Setup(x => x.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess))
                .Returns(new[] { rightsHoldingOrganization1, rightsHoldingOrganization2 });

            //Act
            var availableSystems = _sut.GetAvailableSystems().ToList();

            //Assert
            Assert.Equal(new[] { includedSystem1, includedSystem2 }, availableSystems);
        }

        [Fact]
        public void GetAvailableSystems_Applies_Sub_queries()
        {
            //Arrange
            var all = new List<ItSystem> { CreateSystem(), CreateSystem() };
            ExpectGetSystemsReturns(all);
            ExpectGetCrossLevelOrganizationAccessReturns(CrossOrganizationDataReadAccessLevel.All);

            var subQuery = new Mock<IDomainQuery<ItSystem>>();
            subQuery.Setup(x => x.Apply(It.Is<IQueryable<ItSystem>>(systems => systems.ToList().SequenceEqual(all))))
                .Returns<IQueryable<ItSystem>>((input) => input.Take(1).AsQueryable());

            //Act
            var availableSystems = _sut.GetAvailableSystems(subQuery.Object).ToList();

            //Assert
            var itSystem = Assert.Single(availableSystems);
            Assert.Same(all.Take(1).Single(), itSystem);
        }

        [Fact]
        public void GetUsingOrganizations_Returns_Forbidden_If_Read_Access_To_System_Is_Unauthorized()
        {
            //Arrange
            var system = CreateSystem();
            ExpectGetSystemReturns(system.Id, system);
            ExpectAllowReadsReturns(system, false);

            //Act
            var result = _sut.GetUsingOrganizations(system.Id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void GetUsingOrganizations_Returns_NotFound_If_System_Does_Not_Exist()
        {
            //Arrange
            var system = CreateSystem();
            ExpectGetSystemReturns(system.Id, null);
            //Act
            var result = _sut.GetUsingOrganizations(system.Id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void GetUsingOrganizations_Returns_UsingOrganization_List_With_Single_Item()
        {
            //Arrange
            var system = CreateSystem();
            var organization = CreateOrganization();
            var systemUsage = CreateSystemUsage(organization);
            AddUsage(system, systemUsage);
            ExpectGetSystemReturns(system.Id, system);
            ExpectAllowReadsReturns(system, true);

            //Act
            var result = _sut.GetUsingOrganizations(system.Id);

            //Assert
            Assert.True(result.Ok);
            var usage = Assert.Single(result.Value);
            Assert.Equal(systemUsage.Uuid, usage.ItSystemUsage.Uuid);
            Assert.Equal(organization.Id, usage.Organization.Id);
            Assert.Equal(organization.Name, usage.Organization.Name);
        }

        [Fact]
        public void GetUsingOrganizations_Returns_UsingOrganization_List_With_Multiple_Items()
        {
            //Arrange
            var system = CreateSystem();
            var firstOrganization = CreateOrganization();
            var firstSystemUsage = CreateSystemUsage(firstOrganization);
            AddUsage(system, firstSystemUsage);
            var secondOrganization = CreateOrganization();
            var secondSystemUsage = CreateSystemUsage(secondOrganization);
            AddUsage(system, secondSystemUsage);
            ExpectGetSystemReturns(system.Id, system);
            ExpectAllowReadsReturns(system, true);

            //Act
            var result = _sut.GetUsingOrganizations(system.Id);

            //Assert
            Assert.True(result.Ok);
            Assert.Collection(result.Value,
                item => Assert.Equal(firstSystemUsage.Uuid, item.ItSystemUsage.Uuid),
                item => Assert.Equal(secondSystemUsage.Uuid, item.ItSystemUsage.Uuid)
                );
        }

        [Fact]
        public void Delete_Returns_NotFound()
        {
            //Arrange
            var system = CreateSystem();
            ExpectGetSystemReturns(system.Id, null);

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.NotFound, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_Forbidden()
        {
            //Arrange
            var system = CreateSystem();
            ExpectAllowDeleteReturns(system, false);
            ExpectGetSystemReturns(system.Id, system);

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.Forbidden, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_InUse_If_System_Is_In_Use()
        {
            //Arrange
            var system = CreateSystem();
            var organization = CreateOrganization();
            var systemUsage = CreateSystemUsage(organization);
            AddUsage(system, systemUsage);
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.InUse, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_Ok_If_System_Is_In_Use_And_BreakBindings_Is_True()
        {
            //Arrange
            var system = CreateSystem();
            var organization = CreateOrganization();
            var systemUsage = CreateSystemUsage(organization);
            AddUsage(system, systemUsage);
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectTransactionToBeSet();
            _systemUsageServiceMock.Setup(x => x.Delete(systemUsage.Id)).Returns(Result<ItSystemUsage, OperationError>.Success(systemUsage));
            ExpectReferenceDeletionSuccess(system);

            //Act
            var result = _sut.Delete(system.Id, true);

            //Assert
            AssertSystemDeleted(result);
        }

        [Fact]
        public void Delete_Returns_HasChildren_If_System_Has_Children()
        {
            //Arrange
            var system = CreateSystem();
            AddChild(system, CreateSystem());
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.HasChildren, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_Ok_If_If_System_Has_Children_And_BreakBindings_Are_Specified()
        {
            //Arrange
            var system = CreateSystem();
            var childSystem = CreateSystem();
            AddChild(system, childSystem);
            ExpectAllowDeleteReturns(system, true);
            ExpectAllowModifyReturns(childSystem, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectGetSystemReturns(childSystem.Id, childSystem);
            ExpectTransactionToBeSet();
            ExpectReferenceDeletionSuccess(system);

            //Act
            var result = _sut.Delete(system.Id, true);

            //Assert
            AssertSystemDeleted(result);
            Assert.Null(childSystem.Parent);
        }

        [Fact]
        public void Delete_Returns_HasExhibitInterfaces_If_System_Has_Exhibit_Interfaces()
        {
            //Arrange
            var system = CreateSystem();
            AddInterfaceExhibit(system, CreateInterfaceExhibit());
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.HasInterfaceExhibits, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_Ok_If_System_Has_Exhibit_Interfaces_And_BreakBindings_Is_True()
        {
            //Arrange
            var system = CreateSystem();
            var itInterfaceExhibit = CreateInterfaceExhibit();
            AddInterfaceExhibit(system, itInterfaceExhibit);
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectTransactionToBeSet();
            _interfaceServiceMock.Setup(x => x.UpdateExposingSystem(itInterfaceExhibit.ItInterface.Id, null)).Returns(Result<ItInterface, OperationError>.Success(itInterfaceExhibit.ItInterface));
            ExpectReferenceDeletionSuccess(system);

            //Act
            var result = _sut.Delete(system.Id, true);

            //Assert
            AssertSystemDeleted(result);
        }

        [Fact]
        public void Delete_Returns_Not_Found_If_System_Does_Not_Exist()
        {
            //Arrange
            var system = CreateSystem();
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, null);

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.NotFound, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Delete_Returns_Ok_If_System_Can_Be_Deleted()
        {
            //Arrange
            var system = CreateSystem();
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectTransactionToBeSet();
            ExpectReferenceDeletionSuccess(system);

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            AssertSystemDeleted(result);
        }

        [Fact]
        public void Delete_Returns_Ok_And_Deletes_ExternalReferences()
        {
            //Arrange
            var system = CreateSystem();
            var externalReference = CreateExternalReference();
            AddExternalReference(system, externalReference);
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectDeleteReferenceReturns(system.Id, Result<IEnumerable<ExternalReference>, OperationFailure>.Success(new ExternalReference[0]));
            ExpectTransactionToBeSet();

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.Ok, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
            _referenceService.Verify(x => x.DeleteBySystemId(system.Id), Times.Once);
        }

        [Theory]
        [InlineData(OperationFailure.Forbidden)]
        [InlineData(OperationFailure.UnknownError)]
        [InlineData(OperationFailure.NotFound)]
        public void Delete_Returns_UnknownError_And_Does_Not_Delete_ExternalReferences(OperationFailure referenceDeleteResult)
        {
            //Arrange
            var system = CreateSystem();
            var externalReference = CreateExternalReference();
            AddExternalReference(system, externalReference);
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectDeleteReferenceReturns(system.Id, Result<IEnumerable<ExternalReference>, OperationFailure>.Failure(referenceDeleteResult));
            ExpectTransactionToBeSet();

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.UnknownError, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
            _dbTransaction.Verify(x => x.Rollback(), Times.Once);
            _referenceService.Verify(x => x.DeleteBySystemId(system.Id), Times.Once);
        }

        [Fact]
        public void Delete_Returns_Ok_With_Task_Refs_Added()
        {
            //Arrange
            var system = CreateSystem();
            var taskRef = CreateTaskRef();
            AddTaskRef(system, taskRef);
            ExpectAllowDeleteReturns(system, true);
            ExpectGetSystemReturns(system.Id, system);
            ExpectTransactionToBeSet();
            _referenceService.Setup(x => x.DeleteBySystemId(system.Id)).Returns(Result<IEnumerable<ExternalReference>, OperationFailure>.Success(new ExternalReference[0]));

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.Ok, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
            _dbTransaction.Verify(x => x.Rollback(), Times.Never);
            _referenceService.Verify(x => x.DeleteBySystemId(system.Id), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateNewSystem_Returns_Ok(bool withUuid)
        {
            //Arrange
            var organizationId = A<int>();
            var uuid = withUuid ? A<Guid>() : (Guid?)null;
            var name = A<string>();
            ExpectTransactionToBeSet();
            ExpectAllowCreateReturns(organizationId, true);
            ExpectGetSystemsReturns(null, Enumerable.Empty<ItSystem>());
            if (withUuid)
                ExpectGetSystemByUuidReturns(uuid, Maybe<ItSystem>.None);

            //Act

            var result = _sut.CreateNewSystem(organizationId, name, uuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(name, result.Value.Name);
            if (withUuid)
                Assert.Equal(uuid, result.Value.Uuid);
            else
                Assert.NotEqual(Guid.Empty, result.Value.Uuid);
            Assert.Equal(organizationId, result.Value.OrganizationId);

            _dbTransaction.Verify(x => x.Commit(), Times.Once);
            _systemRepository.Verify(x => x.Add(It.Is<ItSystem>(s => s.Uuid == result.Value.Uuid)), Times.Once);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_Name_Overlaps()
        {
            //Arrange
            var organizationId = A<int>();
            var name = A<string>();
            ExpectTransactionToBeSet();
            ExpectAllowCreateReturns(organizationId, true);
            ExpectGetSystemsReturns(null,
                new[]
                {
                    new ItSystem() {Name = A<string>(), OrganizationId = organizationId},
                    new ItSystem() {Name = name, OrganizationId = organizationId} //Overlapping name
                });

            //Act
            var result = _sut.CreateNewSystem(organizationId, name);

            //Assert
            AssertUpdateFailure(result, OperationFailure.Conflict);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_Uuid_Overlaps()
        {
            //Arrange
            var organizationId = A<int>();
            var name = A<string>();
            var uuid = A<Guid>();
            ExpectTransactionToBeSet();
            ExpectAllowCreateReturns(organizationId, true);
            ExpectGetSystemsReturns(null, new List<ItSystem>());
            ExpectGetSystemReturns(uuid, new ItSystem());

            //Act

            var result = _sut.CreateNewSystem(organizationId, name, uuid);

            //Assert
            AssertUpdateFailure(result, OperationFailure.Conflict);
        }

        [Fact]
        public void UpdatePreviousName_Returns_Ok()
        {
            //Arrange
            var systemId = A<int>();
            var newValue = A<string>();
            var itSystem = new ItSystem();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);

            //Act
            var result = _sut.UpdatePreviousName(systemId, newValue);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(newValue, result.Select(x => x.PreviousName).Value);
            _systemRepository.Verify(x => x.Update(itSystem), Times.Once);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void UpdatePreviousName_Returns_Error_If_Write_Access_Is_Denied()
        {
            //Arrange
            var systemId = A<int>();
            var newValue = A<string>();
            var itSystem = new ItSystem();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, false);

            //Act
            var result = _sut.UpdatePreviousName(systemId, newValue);

            //Assert
            AssertUpdateFailure(result, OperationFailure.Forbidden);
        }

        [Fact]
        public void UpdatePreviousName_Returns_Error_If_System_Is_Not_Found()
        {
            //Arrange
            var systemId = A<int>();
            var newValue = A<string>();
            ExpectGetSystemReturns(systemId, null);

            //Act
            var result = _sut.UpdatePreviousName(systemId, newValue);

            //Assert
            AssertUpdateFailure(result, OperationFailure.NotFound);
        }

        [Fact]
        public void UpdateDescription_Returns_Ok()
        {
            //Arrange
            var systemId = A<int>();
            var newValue = A<string>();
            var itSystem = new ItSystem();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);

            //Act
            var result = _sut.UpdateDescription(systemId, newValue);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(newValue, result.Select(x => x.Description).Value);
            _systemRepository.Verify(x => x.Update(itSystem), Times.Once);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateDescription_Returns_Error_If_Write_Access_Is_Denied()
        {
            //Arrange
            var systemId = A<int>();
            var newValue = A<string>();
            var itSystem = new ItSystem();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, false);

            //Act
            var result = _sut.UpdateDescription(systemId, newValue);

            //Assert
            AssertUpdateFailure(result, OperationFailure.Forbidden);
        }

        [Fact]
        public void UpdateDescription_Returns_Error_If_System_Is_Not_Found()
        {
            //Arrange
            var systemId = A<int>();
            var newValue = A<string>();
            ExpectGetSystemReturns(systemId, null);

            //Act
            var result = _sut.UpdateDescription(systemId, newValue);

            //Assert
            AssertUpdateFailure(result, OperationFailure.NotFound);
        }

        [Fact]
        public void UpdateParentSystem_Returns_Ok()
        {
            //Arrange
            var systemId = A<int>();
            var parentSystemId = A<int>();
            var itSystem = new ItSystem();
            var parentSystem = new ItSystem { Id = parentSystemId };
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectGetSystemReturns(parentSystemId, parentSystem);
            ExpectAllowReadsReturns(parentSystem, true);

            //Act
            var result = _sut.UpdateParentSystem(systemId, parentSystemId);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(parentSystem.Id, result.Select(x => x.ParentId.GetValueOrDefault()).Value);
            _systemRepository.Verify(x => x.Update(itSystem), Times.Once);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateParentSystem_Returns_Error_If_ParentSystem_Cannot_Be_Found()
        {
            //Arrange
            var systemId = A<int>();
            var parentSystemId = A<int>();
            var itSystem = new ItSystem();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectGetSystemReturns(parentSystemId, null);

            //Act
            var result = _sut.UpdateParentSystem(systemId, parentSystemId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.BadInput);
        }

        [Fact]
        public void UpdateParentSystem_Returns_Error_If_Access_To_ParentSystem_Is_Denied()
        {
            //Arrange
            var systemId = A<int>();
            var parentSystemId = A<int>();
            var itSystem = new ItSystem();
            var parentSystem = new ItSystem { Id = parentSystemId };
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectGetSystemReturns(parentSystemId, parentSystem);
            ExpectAllowReadsReturns(parentSystem, false);

            //Act
            var result = _sut.UpdateParentSystem(systemId, parentSystemId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.Forbidden);
        }

        [Fact]
        public void UpdateParentSystem_Returns_Error_If_Write_Access_Is_Denied()
        {
            //Arrange
            var systemId = A<int>();
            var parentSystemId = A<int>();
            var itSystem = new ItSystem();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, false);

            //Act
            var result = _sut.UpdateParentSystem(systemId, parentSystemId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.Forbidden);
        }

        [Fact]
        public void UpdateParentSystem_Returns_Error_If_System_Is_Not_Found()
        {
            //Arrange
            var systemId = A<int>();
            var parentSystemId = A<int>();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, null);

            //Act
            var result = _sut.UpdateParentSystem(systemId, parentSystemId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.NotFound);

        }

        [Fact]
        public void UpdateRightsHolder_Returns_Ok()
        {
            //Arrange
            var systemId = A<int>();
            var rightsHolderId = A<Guid>();
            var itSystem = new ItSystem();
            var rightsHolder = new Organization() { Id = A<int>() };
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectGetOrganizationReturns(rightsHolderId, rightsHolder);
            ExpectAllowReadsReturns(rightsHolder, true);

            //Act
            var result = _sut.UpdateRightsHolder(systemId, rightsHolderId);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(rightsHolder.Id, result.Select(x => x.BelongsToId.GetValueOrDefault()).Value);
            _systemRepository.Verify(x => x.Update(itSystem), Times.Once);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateRightsHolder_Returns_Error_If_RightsHolder_Cannot_Be_Found()
        {
            //Arrange
            var systemId = A<int>();
            var rightsHolderId = A<Guid>();
            var itSystem = new ItSystem();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectGetOrganizationReturns(rightsHolderId, Maybe<Organization>.None);

            //Act
            var result = _sut.UpdateRightsHolder(systemId, rightsHolderId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.BadInput);
        }

        [Fact]
        public void UpdateRightsHolder_Returns_Error_If_Access_To_RightsHolder_Is_Denied()
        {
            //Arrange
            var systemId = A<int>();
            var rightsHolderId = A<Guid>();
            var itSystem = new ItSystem();
            var rightsHolder = new Organization();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectGetOrganizationReturns(rightsHolderId, rightsHolder);
            ExpectAllowReadsReturns(rightsHolder, false);

            //Act
            var result = _sut.UpdateRightsHolder(systemId, rightsHolderId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.Forbidden);
        }

        [Fact]
        public void UpdateRightsHolder_Returns_Error_If_Write_Access_Is_Denied()
        {
            //Arrange
            var systemId = A<int>();
            var rightsHolderId = A<Guid>();
            var itSystem = new ItSystem();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, false);

            //Act
            var result = _sut.UpdateRightsHolder(systemId, rightsHolderId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.Forbidden);
        }

        [Fact]
        public void UpdateRightsHolder_Returns_Error_If_System_Is_Not_Found()
        {
            //Arrange
            var systemId = A<int>();
            var rightsHolderId = A<Guid>();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, null);

            //Act
            var result = _sut.UpdateRightsHolder(systemId, rightsHolderId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.NotFound);
        }

        [Fact]
        public void UpdateBusinessType_Returns_Ok()
        {
            //Arrange
            var systemId = A<int>();
            var businessTypeId = A<Guid>();
            var organizationId = A<int>();
            var itSystem = new ItSystem { OrganizationId = organizationId };
            var businessType = new BusinessType { Id = A<int>() };
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectGetBusinessTypeOptionReturns(organizationId, businessTypeId, (businessType, true));

            //Act
            var result = _sut.UpdateBusinessType(systemId, businessTypeId);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(businessType.Id, result.Select(x => x.BusinessTypeId.GetValueOrDefault()).Value);
            _systemRepository.Verify(x => x.Update(It.IsAny<ItSystem>()), Times.Once);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateBusinessType_Fails_If_Option_Is_Not_Available_In_Organization()
        {
            //Arrange
            var systemId = A<int>();
            var businessTypeId = A<Guid>();
            var organizationId = A<int>();
            var itSystem = new ItSystem { OrganizationId = organizationId };
            var businessType = new BusinessType { Id = A<int>() };
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectGetBusinessTypeOptionReturns(organizationId, businessTypeId, (businessType, false));

            //Act
            var result = _sut.UpdateBusinessType(systemId, businessTypeId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.BadInput);
        }

        [Fact]
        public void UpdateBusinessType_Fails_If_Option_Is_Not_Found()
        {
            //Arrange
            var systemId = A<int>();
            var businessTypeId = A<Guid>();
            var organizationId = A<int>();
            var itSystem = new ItSystem { OrganizationId = organizationId };
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectGetBusinessTypeOptionReturns(organizationId, businessTypeId, Maybe<(BusinessType, bool)>.None);

            //Act
            var result = _sut.UpdateBusinessType(systemId, businessTypeId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.BadInput);
        }

        [Fact]
        public void UpdateBusinessType_Fails_If_WriteAccess_Is_Denied()
        {
            //Arrange
            var systemId = A<int>();
            var businessTypeId = A<Guid>();
            var organizationId = A<int>();
            var itSystem = new ItSystem { OrganizationId = organizationId };
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, false);
            ExpectGetBusinessTypeOptionReturns(organizationId, businessTypeId, (new BusinessType(), true));

            //Act
            var result = _sut.UpdateBusinessType(systemId, businessTypeId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.Forbidden);
        }

        [Fact]
        public void UpdateBusinessType_Fails_If_System_Is_Not_found()
        {
            //Arrange
            var systemId = A<int>();
            var businessTypeId = A<Guid>();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, null);

            //Act
            var result = _sut.UpdateBusinessType(systemId, businessTypeId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.NotFound);
        }

        [Fact]
        public void UpdateTaskRefs_Returns_Ok()
        {
            //Arrange
            var systemId = A<int>();
            var itSystem = new ItSystem() { TaskRefs = new List<TaskRef>() { new() { Id = A<int>() } } };
            var taskRefId1 = A<int>();
            var taskRefId2 = A<int>();
            var taskRefIds = new[] { taskRefId1, taskRefId2 };
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectGetTaskRefReturnsSome(taskRefId1);
            ExpectGetTaskRefReturnsSome(taskRefId2);

            //Act
            var result = _sut.UpdateTaskRefs(systemId, taskRefIds);

            //Assert - make sure that the current task refs are now only the ones provided.. existing ones were removed
            Assert.True(result.Ok);
            Assert.Equal(taskRefIds.OrderBy(id => id), result.Select(system => system.TaskRefs.Select(taskRef => taskRef.Id).OrderBy(id => id)).Value);
            _systemRepository.Verify(x => x.Update(It.IsAny<ItSystem>()), Times.Once);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateTaskRefs_Fails_If_TaskRefId_Is_Invalid()
        {
            //Arrange
            var systemId = A<int>();
            var itSystem = new ItSystem() { TaskRefs = new List<TaskRef> { new() { Id = A<int>() } } };
            var taskRefId1 = A<int>();
            var taskRefId2 = A<int>();
            var taskRefIds = new[] { taskRefId1, taskRefId2 };
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectGetTaskRefReturnsSome(taskRefId1);
            ExpectGetTaskRefReturnsNone(taskRefId2);

            //Act
            var result = _sut.UpdateTaskRefs(systemId, taskRefIds);

            //Assert - make sure that the current task refs are now only the ones provided.. existing ones were removed
            AssertUpdateFailure(result, OperationFailure.BadInput);
        }

        [Fact]
        public void UpdateTaskRefs_Fails_If_Modification_Is_Denied()
        {
            //Arrange
            var systemId = A<int>();
            var itSystem = new ItSystem();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, false);

            //Act
            var result = _sut.UpdateTaskRefs(systemId, Many<int>());

            //Assert - make sure that the current task refs are now only the ones provided.. existing ones were removed
            AssertUpdateFailure(result, OperationFailure.Forbidden);
        }

        [Fact]
        public void UpdateTaskRefs_Fails_If_System_Cannot_Be_Found()
        {
            //Arrange
            var systemId = A<int>();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, null);

            //Act
            var result = _sut.UpdateTaskRefs(systemId, Many<int>());

            //Assert - make sure that the current task refs are now only the ones provided.. existing ones were removed
            AssertUpdateFailure(result, OperationFailure.NotFound);
        }

        [Fact]
        public void UpdateName_Returns_Ok()
        {
            //Arrange
            var systemId = A<int>();
            var newName = A<string>();
            var organization1Id = A<int>();
            var organization2Id = A<int>();
            var itSystem = CreateSystem(organization1Id);
            var otherSystem = CreateSystem(organization2Id, name: newName); //Different org, same name as the new name
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectGetSystemsReturns(new List<ItSystem> { itSystem, otherSystem });

            //Act
            var result = _sut.UpdateName(systemId, newName);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(newName, result.Select(x => x.Name).Value);
            _systemRepository.Verify(x => x.Update(It.IsAny<ItSystem>()), Times.Once);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateName_Returns_Conflict_If_Name_Equals_ExistingSystem_In_Same_Org()
        {
            //Arrange
            var systemId = A<int>();
            var newName = A<string>();
            var organization1Id = A<int>();
            var itSystem = CreateSystem(organization1Id);
            var otherSystem = CreateSystem(organization1Id, name: newName);
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectGetSystemsReturns(new List<ItSystem> { itSystem, otherSystem });

            //Act
            var result = _sut.UpdateName(systemId, newName);

            //Assert
            AssertUpdateFailure(result, OperationFailure.Conflict);
        }

        [Fact]
        public void UpdateName_Returns_Conflict_If_Name_Is_Too_Short()
        {
            UpdateName_Fails_With_BadInput(string.Empty);
        }

        [Fact]
        public void UpdateName_Returns_Conflict_If_Name_Is_Too_Long()
        {
            var newName = Enumerable.Repeat("a", ItSystem.MaxNameLength + 1).Transform(x => string.Join(string.Empty, x));
            UpdateName_Fails_With_BadInput(newName);
        }

        [Fact]
        public void Deactivate_Returns_Ok()
        {
            //Arrange
            var systemId = A<int>();
            var organization1Id = A<int>();
            var itSystem = CreateSystem(organization1Id);
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);

            //Act
            var result = _sut.Deactivate(systemId);

            //Assert
            Assert.True(result.Ok);
            Assert.True(result.Select(x => x.Disabled).Value);
            _systemRepository.Verify(x => x.Update(It.IsAny<ItSystem>()), Times.Once);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Cannot_Deactivate_If_No_Access()
        {
            //Arrange
            var systemId = A<int>();
            var organization1Id = A<int>();
            var itSystem = CreateSystem(organization1Id);
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, false);

            //Act
            var result = _sut.Deactivate(systemId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.Forbidden);
        }

        [Fact]
        public void Cannot_Deactivate_If_Not_Found()
        {
            //Arrange
            var systemId = A<int>();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, null);

            //Act
            var result = _sut.Deactivate(systemId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.NotFound);
        }

        [Fact]
        public void Can_Get_Hierarchy()
        {
            //Arrange
            var (root, createdItSystems) = CreateHierarchy();

            ExpectAllowReadsReturns(root, true);
            ExpectGetSystemReturns(root.Id, root);

            //Act
            var result = _sut.GetCompleteHierarchy(root.Id);

            //Assert
            Assert.True(result.Ok);
            var hierarchy = result.Value.ToList();
            Assert.Equal(createdItSystems.Count, hierarchy.Count);

            foreach (var node in hierarchy)
            {
                var system = createdItSystems.FirstOrDefault(x => x.Id == node.Id);
                Assert.NotNull(system);
                if (node.Id == root.Id)
                {
                    Assert.Null(node.Parent);
                }
                else
                {
                    Assert.NotNull(node.Parent);
                    Assert.Equal(node.Parent.Id, system.Parent.Id);
                }
            }
        }

        [Fact]
        public void Get_Hierarchy_Returns_Forbidden_If_User_Doesnt_Have_ReadRights()
        {
            //Arrange
            var (root, _) = CreateHierarchy();

            ExpectAllowReadsReturns(root, false);
            ExpectGetSystemReturns(root.Id, root);

            //Act
            var result = _sut.GetCompleteHierarchy(root.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Get_Hierarchy_Returns_NotFound_If_System_With_Id_Doesnt_Exist()
        {
            //Arrange
            var (root, _) = CreateHierarchy();

            ExpectGetSystemReturns(root.Id, null);

            //Act
            var result = _sut.GetCompleteHierarchy(root.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Can_Get_Hierarchy_By_Uuid()
        {
            //Arrange
            var (root, createdItSystems) = CreateHierarchy();

            ExpectAllowReadsReturns(root, true);
            ExpectGetSystemReturns(root.Id, root);
            ExpectResolveIdReturns<ItSystem>(root.Uuid, root.Id);

            //Act
            var result = _sut.GetCompleteHierarchyByUuid(root.Uuid);

            //Assert
            Assert.True(result.Ok);
            var hierarchy = result.Value.ToList();
            Assert.Equal(createdItSystems.Count, hierarchy.Count);

            foreach (var node in hierarchy)
            {
                var system = createdItSystems.FirstOrDefault(x => x.Id == node.Id);
                Assert.NotNull(system);
                if (node.Id == root.Id)
                {
                    Assert.Null(node.Parent);
                }
                else
                {
                    Assert.NotNull(node.Parent);
                    Assert.Equal(node.Parent.Id, system.Parent.Id);
                }
            }
        }

        [Fact]
        public void Get_Hierarchy_By_Uuid_Return_NotFound()
        {
            //Arrange
            var (root, _) = CreateHierarchy();

            ExpectResolveIdReturns<ItSystem>(root.Uuid, Maybe<int>.None);

            //Act
            var result = _sut.GetCompleteHierarchyByUuid(root.Uuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Activate_Returns_Ok()
        {
            //Arrange
            var systemId = A<int>();
            var organization1Id = A<int>();
            var itSystem = CreateSystem(organization1Id);
            itSystem.Disabled = true;
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);

            //Act
            var result = _sut.Activate(systemId);

            //Assert
            Assert.True(result.Ok);
            Assert.False(result.Select(x => x.Disabled).Value);
            _systemRepository.Verify(x => x.Update(It.IsAny<ItSystem>()), Times.Once);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Cannot_Activate_If_No_Access()
        {
            //Arrange
            var systemId = A<int>();
            var organization1Id = A<int>();
            var itSystem = CreateSystem(organization1Id);
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, false);

            //Act
            var result = _sut.Activate(systemId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.Forbidden);
        }

        [Fact]
        public void Cannot_Activate_If_Not_Found()
        {
            //Arrange
            var systemId = A<int>();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, null);

            //Act
            var result = _sut.Activate(systemId);

            //Assert
            AssertUpdateFailure(result, OperationFailure.NotFound);
        }

        [Theory]
        [InlineData(AccessModifier.Local, AccessModifier.Public)]
        [InlineData(AccessModifier.Public, AccessModifier.Local)]
        public void UpdateAccessModifier_Returns_Ok(AccessModifier from, AccessModifier to)
        {
            //Arrange
            var systemId = A<int>();
            var organization1Id = A<int>();
            var itSystem = CreateSystem(organization1Id);
            itSystem.AccessModifier = from;
            itSystem.Disabled = true;
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectHasVisibilityControlPermission(itSystem, true);

            //Act
            var result = _sut.UpdateAccessModifier(systemId, to);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(to, itSystem.AccessModifier);
            _systemRepository.Verify(x => x.Update(It.IsAny<ItSystem>()), Times.Once);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Cannot_UpdateAccessModifier_If_No_Permission()
        {
            //Arrange
            var systemId = A<int>();
            var organization1Id = A<int>();
            var itSystem = CreateSystem(organization1Id);
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, false);
            ExpectHasVisibilityControlPermission(itSystem, false);

            //Act
            var result = _sut.UpdateAccessModifier(systemId, AccessModifier.Local);

            //Assert
            AssertUpdateFailure(result, OperationFailure.Forbidden);
        }

        [Fact]
        public void Cannot_UpdateAccessModifier_If_No_Access()
        {
            //Arrange
            var systemId = A<int>();
            var organization1Id = A<int>();
            var itSystem = CreateSystem(organization1Id);
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, false);

            //Act
            var result = _sut.UpdateAccessModifier(systemId, AccessModifier.Local);

            //Assert
            AssertUpdateFailure(result, OperationFailure.Forbidden);
        }

        [Fact]
        public void Cannot_UpdateAccessModifier_If_Not_Found()
        {
            //Arrange
            var systemId = A<int>();
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, null);

            //Act
            var result = _sut.UpdateAccessModifier(systemId, AccessModifier.Local);

            //Assert
            AssertUpdateFailure(result, OperationFailure.NotFound);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void Can_Get_Permissions(bool read, bool modify, bool delete)
        {
            //Arrange
            var uuid = A<Guid>();
            var itSystem = new ItSystem { Uuid = uuid, AccessModifier = AccessModifier.Public };
            ExpectGetSystemReturns(uuid, itSystem);
            ExpectAllowReadsReturns(itSystem, read);
            ExpectAllowModifyReturns(itSystem, modify);
            ExpectAllowDeleteReturns(itSystem, delete);
            ExpectAllowEditVisibilityReturns(itSystem, false);

            //Act
            var result = _sut.GetPermissions(uuid);

            //Assert
            Assert.True(result.Ok);
            var permissions = result.Value;
            Assert.Equivalent(new SystemPermissions(new ResourcePermissionsResult(read, modify, delete), Array.Empty<SystemDeletionConflict>(), false), permissions);
        }

        [Theory]
        [InlineData(true, true, true, true, true)]
        [InlineData(false, true, true, true, true)]
        [InlineData(true, false, false, false, true)]
        [InlineData(true, false, false, true, true)]
        [InlineData(true, false, true, false, true)]
        [InlineData(true, true, false, false, true)]
        [InlineData(true, true, true, true, false)]
        public void Can_Get_Permissions_With_Deletion_Conflicts_And_Visibility(bool allowDelete, bool withUsages, bool withChildren, bool withExposures, bool withEditVisibilityPermission)
        {
            //Arrange
            var uuid = A<Guid>();
            var itSystem = new ItSystem { Uuid = uuid, AccessModifier = AccessModifier.Public };
            ExpectGetSystemReturns(uuid, itSystem);
            ExpectAllowReadsReturns(itSystem, true);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectAllowDeleteReturns(itSystem, allowDelete);
            if (withUsages) itSystem.Usages.Add(new ItSystemUsage());
            if (withExposures) itSystem.ItInterfaceExhibits.Add(new ItInterfaceExhibit());
            if (withChildren) itSystem.Children.Add(new ItSystem());
            ExpectAllowEditVisibilityReturns(itSystem, withEditVisibilityPermission);

            //Act
            var result = _sut.GetPermissions(uuid);

            //Assert
            Assert.True(result.Ok);
            var permissions = result.Value;
            var expectedConflicts = new List<SystemDeletionConflict>();
            if (allowDelete)
            {
                if (withUsages) expectedConflicts.Add(SystemDeletionConflict.InUse);
                if (withChildren) expectedConflicts.Add(SystemDeletionConflict.HasChildren);
                if (withExposures) expectedConflicts.Add(SystemDeletionConflict.HasInterfaceExhibits);
            }
            Assert.Equivalent(new SystemPermissions(new ResourcePermissionsResult(true, true, allowDelete), expectedConflicts, withEditVisibilityPermission), permissions);
        }

        [Fact]
        public void Get_Permissions_Returns_Not_Found()
        {
            //Arrange
            var wrongUuid = A<Guid>();
            ExpectGetSystemReturns(wrongUuid, Maybe<ItSystem>.None);

            //Act
            var result = _sut.GetPermissions(wrongUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Get_CollectionPermissions(bool create)
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var organization = new Organization { Id = A<int>() };

            ExpectOrganizationServiceGetOrganizationReturns(organizationUuid, organization);
            ExpectAllowCreateReturns(organization.Id, create);

            //Act
            var result = _sut.GetCollectionPermissions(organizationUuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(create, result.Value.Create);
        }

        [Fact]
        public void Get_CollectionPermissions_Returns_OperationError_When_GetOrganization_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var error = A<OperationError>();

            ExpectOrganizationServiceGetOrganizationReturns(organizationUuid, error);

            //Act
            var result = _sut.GetCollectionPermissions(organizationUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(error.FailureType, result.Error.FailureType);
        }

        [Fact]
        public void Stakeholders_Can_Read_All_ItSystems()
        {
            var localSystem = new ItSystem { Organization = CreateOrganization(), AccessModifier = AccessModifier.Local };
            var publicSystem = new ItSystem { Organization = CreateOrganization(), AccessModifier = AccessModifier.Public };
            var systems = new List<ItSystem> { localSystem, publicSystem };
            ExpectGetSystemsReturns(systems);
            ExpectGetUserOrganizationIdsReturns(A<int>());
            ExpectUserIsStakeholder();

            var result = _sut.GetAvailableSystems();

            Assert.Equal(2, result.Count());
        }

        private (ItSystem root, IReadOnlyList<ItSystem> createdItSystems) CreateHierarchy()
        {
            var root = CreateSystem();
            var child = CreateSystem();
            var grandchild = CreateSystem();

            child.Children = new List<ItSystem> { grandchild };
            grandchild.Parent = child;

            root.Children = new List<ItSystem> { child };
            child.Parent = root;

            return (root, new List<ItSystem> { root, child, grandchild });
        }

        private void ExpectUserIsStakeholder()
        {
            ExpectGetCrossLevelOrganizationAccessReturns(CrossOrganizationDataReadAccessLevel.Public); //Access level of stakeholders
            _userContext.Setup(x => x.HasStakeHolderAccess()).Returns(true);
        }

        private void UpdateName_Fails_With_BadInput(string newName)
        {
            //Arrange
            var systemId = A<int>();
            var organization1Id = A<int>();
            var itSystem = CreateSystem(organization1Id);
            ExpectTransactionToBeSet();
            ExpectGetSystemReturns(systemId, itSystem);
            ExpectAllowModifyReturns(itSystem, true);
            ExpectGetSystemsReturns(new List<ItSystem>());

            //Act
            var result = _sut.UpdateName(systemId, newName);

            //Assert
            AssertUpdateFailure(result, OperationFailure.BadInput);
        }

        private void AssertUpdateFailure(Result<ItSystem, OperationError> result, OperationFailure expectedErrorType)
        {
            Assert.True(result.Failed);
            Assert.Equal(expectedErrorType, result.Error.FailureType);
            _systemRepository.Verify(x => x.Update(It.IsAny<ItSystem>()), Times.Never);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        private void ExpectGetBusinessTypeOptionReturns(int organizationId, Guid businessTypeId, Maybe<(BusinessType, bool)> result)
        {
            _businessTypeServiceMock.Setup(x => x.GetOptionByUuid(organizationId, businessTypeId)).Returns(result);
        }

        private void ExpectGetOrganizationReturns(Guid uuid, Maybe<Organization> response)
        {
            _organizationRepositoryMock.Setup(x => x.GetByUuid(uuid)).Returns(response);
        }

        private void ExpectOrganizationServiceGetOrganizationReturns(Guid uuid, Result<Organization, OperationError> result, OrganizationDataReadAccessLevel? organizationDataReadAccessLevel = null)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(uuid, organizationDataReadAccessLevel)).Returns(result);
        }

        private void ExpectAllowModifyReturns(ItSystem itSystem, bool value)
        {
            _authorizationContext.Setup(x => x.AllowModify(itSystem)).Returns(value);
        }

        private void ExpectGetSystemByUuidReturns(Guid? uuid, Maybe<ItSystem> value)
        {
            _systemRepository.Setup(x => x.GetSystem(uuid.Value)).Returns(value);
        }

        private void ExpectAllowCreateReturns(int organizationId, bool value)
        {
            _authorizationContext.Setup(x => x.AllowCreate<ItSystem>(organizationId)).Returns(value);
        }

        private void ExpectGetSystemsReturns(OrganizationDataQueryParameters organizationDataQueryParameters, IEnumerable<ItSystem> itSystems)
        {
            _systemRepository.Setup(x => x.GetSystems(organizationDataQueryParameters))
                .Returns(new EnumerableQuery<ItSystem>(itSystems));
        }

        private Organization CreateOrganization()
        {
            return new() { Id = A<int>(), Uuid = A<Guid>(), Name = A<string>() };
        }

        private ItSystem CreateSystem(int? organizationId = null, AccessModifier accessModifier = AccessModifier.Local, int? belongsToId = null, string name = null)
        {
            ItSystem itSystem = new()
            {
                Id = A<int>(),
                AccessModifier = accessModifier,
                Name = name ?? A<string>()
            };

            if (organizationId.HasValue)
            {
                itSystem.OrganizationId = organizationId.Value;
            }

            itSystem.BelongsToId = belongsToId;

            return itSystem;
        }

        private TaskRef CreateTaskRef()
        {
            return new() { Id = A<int>() };
        }

        private ItSystemUsage CreateSystemUsage(Organization organization)
        {
            return new() { Id = A<int>(), Organization = organization, ItSystem = new ItSystem { Name = A<string>() } };
        }

        private ItInterfaceExhibit CreateInterfaceExhibit()
        {
            return new() { Id = A<int>(), ItInterface = new ItInterface() { Id = A<int>() } };
        }

        private ExternalReference CreateExternalReference()
        {
            return new() { Id = A<int>() };
        }

        private void ExpectAllowReadsReturns<T>(T entity, bool value) where T : IEntity
        {
            _authorizationContext.Setup(x => x.AllowReads(entity)).Returns(value);
        }

        private void ExpectAllowDeleteReturns(ItSystem system, bool value)
        {
            _authorizationContext.Setup(x => x.AllowDelete(system)).Returns(value);
        }

        private void ExpectAllowEditVisibilityReturns(ItSystem system, bool value)
        {
            _authorizationContext.Setup(x => x.HasPermission(It.Is<VisibilityControlPermission>(x => x.Target.Id == system.Id)))
                .Returns(value);
        }

        private void ExpectGetSystemReturns(int id, ItSystem system)
        {
            _systemRepository.Setup(x => x.GetSystem(id)).Returns(system);
        }

        private void ExpectResolveIdReturns<T>(Guid uuid, Maybe<int> result) where T : class, IHasUuid, IHasId
        {
            _entityIdentityResolver.Setup(x => x.ResolveDbId<T>(uuid)).Returns(result);
        }

        private void ExpectGetSystemReturns(Guid id, Maybe<ItSystem> system)
        {
            _systemRepository.Setup(x => x.GetSystem(id)).Returns(system);
        }

        private void ExpectDeleteReferenceReturns(int id, Result<IEnumerable<ExternalReference>, OperationFailure> result)
        {
            _referenceService.Setup(x => x.DeleteBySystemId(id)).Returns(result);
        }

        private void ExpectTransactionToBeSet()
        {
            _transactionManager.Setup(x => x.Begin()).Returns(_dbTransaction.Object);
        }

        private static void AddUsage(ItSystem system, ItSystemUsage usage)
        {
            system.Usages.Add(usage);
        }

        private static void AddChild(ItSystem mainSystem, ItSystem childSystem)
        {
            mainSystem.Children.Add(childSystem);
        }

        private static void AddInterfaceExhibit(ItSystem system, ItInterfaceExhibit interfaceExhibit)
        {
            system.ItInterfaceExhibits.Add(interfaceExhibit);
        }

        private static void AddExternalReference(ItSystem system, ExternalReference externalReference)
        {
            system.ExternalReferences.Add(externalReference);
        }

        private static void AddTaskRef(ItSystem system, TaskRef taskRef)
        {
            system.TaskRefs.Add(taskRef);
        }

        private void ExpectGetCrossLevelOrganizationAccessReturns(CrossOrganizationDataReadAccessLevel accessLevel)
        {
            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(accessLevel);
        }

        private void ExpectGetSystemsReturns(List<ItSystem> itSystems)
        {
            _systemRepository.Setup(x => x.GetSystems(null)).Returns(new EnumerableQuery<ItSystem>(itSystems));
        }

        private void ExpectGetUserOrganizationIdsReturns(params int[] ownOrganizationIds)
        {
            _userContext.Setup(x => x.OrganizationIds).Returns(ownOrganizationIds);
        }

        private void ExpectGetTaskRefReturnsSome(int taskRefId1)
        {
            _taskRefRepositoryMock.Setup(x => x.GetTaskRef(taskRefId1)).Returns(new TaskRef() { Id = taskRefId1 });
        }

        private void ExpectGetTaskRefReturnsNone(int taskRefId1)
        {
            _taskRefRepositoryMock.Setup(x => x.GetTaskRef(taskRefId1)).Returns(Maybe<TaskRef>.None);
        }

        private void AssertSystemDeleted(SystemDeleteResult result)
        {
            Assert.Equal(SystemDeleteResult.Ok, result);
            _dbTransaction.Verify(x => x.Commit(), Times.AtLeastOnce());
        }

        private void ExpectReferenceDeletionSuccess(ItSystem system)
        {
            _referenceService.Setup(x => x.DeleteBySystemId(system.Id))
                .Returns(Result<IEnumerable<ExternalReference>, OperationFailure>.Success(new ExternalReference[0]));
        }

        private void ExpectHasVisibilityControlPermission(ItSystem itSystem, bool value)
        {
            _authorizationContext
                .Setup(x => x.HasPermission(It.Is<VisibilityControlPermission>(p => p.Target == itSystem)))
                .Returns(value);
        }
    }
}