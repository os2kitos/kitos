using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.References;
using Core.ApplicationServices.System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Core.DomainServices.Model;
using Core.DomainServices.Options;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.TaskRefs;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
using Moq;
using Moq.Language.Flow;
using Serilog;
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

        public ItSystemServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _systemRepository = new Mock<IItSystemRepository>();
            _transactionManager = new Mock<ITransactionManager>();
            _dbTransaction = new Mock<IDatabaseTransaction>();
            _referenceService = new Mock<IReferenceService>();
            _logger = new Mock<ILogger>();
            _userContext = new Mock<IOrganizationalUserContext>();
            _sut = new ItSystemService(
                _systemRepository.Object,
                _authorizationContext.Object,
                _transactionManager.Object,
                _referenceService.Object,
                Mock.Of<ITaskRefRepository>(),
                Mock.Of<IOptionsService<ItSystem, BusinessType>>(),
                Mock.Of<IOrganizationRepository>(),
                _logger.Object,
                _userContext.Object,
                Mock.Of<IDomainEvents>(),
                Mock.Of<IOperationClock>()
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
            Assert.Equal(systemUsage.Id, usage.ItSystemUsageId);
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
                item => Assert.Equal(firstSystemUsage.Id, item.ItSystemUsageId),
                item => Assert.Equal(secondSystemUsage.Id, item.ItSystemUsageId)
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
            _referenceService.Setup(x => x.DeleteBySystemId(system.Id)).Returns(Result<IEnumerable<ExternalReference>, OperationFailure>.Success(new ExternalReference[0]));

            //Act
            var result = _sut.Delete(system.Id);

            //Assert
            Assert.Equal(SystemDeleteResult.Ok, result);
            _dbTransaction.Verify(x => x.Commit(), Times.Once);
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
            var taskRef = createTaskRef();
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
            ExpectTransactionToBeSet(IsolationLevel.ReadCommitted);
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
            ExpectTransactionToBeSet(IsolationLevel.ReadCommitted);
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
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Conflict, result.Error.FailureType);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void CreateNewSystem_Returns_Error_If_Uuid_Overlaps()
        {
            //Arrange
            var organizationId = A<int>();
            var name = A<string>();
            var uuid = A<Guid>();
            ExpectTransactionToBeSet(IsolationLevel.ReadCommitted);
            ExpectAllowCreateReturns(organizationId, true);
            ExpectGetSystemsReturns(null, new List<ItSystem>());
            ExpectGetSystemReturns(uuid,new ItSystem());

            //Act

            var result = _sut.CreateNewSystem(organizationId, name, uuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Conflict, result.Error.FailureType);
            _dbTransaction.Verify(x => x.Commit(), Times.Never);
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

        /*
         * Result<ItSystem, OperationError> CreateNewSystem(int organizationId, string name, Guid? uuid = null);
        Result<ItSystem, OperationError> UpdatePreviousName(int systemId, string newValue);
        Result<ItSystem, OperationError> UpdateDescription(int systemId, string newValue);
        Result<ItSystem, OperationError> UpdateMainUrlReference(int systemId, string urlReference);
        Result<ItSystem, OperationError> UpdateTaskRefs(int systemId, IEnumerable<int> taskRefIds);
        Result<ItSystem, OperationError> UpdateBusinessType(int systemId, Guid? businessTypeUuid);
        Result<ItSystem, OperationError> UpdateRightsHolder(int systemId, Guid? rightsHolderUuid);
        Result<ItSystem, OperationError> UpdateParentSystem(int systemId, int? parentSystemId = null);
         *
         */

        private Organization CreateOrganization()
        {
            return new() { Id = A<int>(), Name = A<string>() };
        }

        private ItSystem CreateSystem(int? organizationId = null, AccessModifier accessModifier = AccessModifier.Local, int? belongsToId = null)
        {
            ItSystem itSystem = new()
            {
                Id = A<int>(),
                AccessModifier = accessModifier
            };

            if (organizationId.HasValue)
            {
                itSystem.OrganizationId = organizationId.Value;
            }

            itSystem.BelongsToId = belongsToId;

            return itSystem;
        }

        private TaskRef createTaskRef()
        {
            return new TaskRef { Id = A<int>() };
        }

        private ItSystemUsage CreateSystemUsage(Organization organization)
        {
            return new ItSystemUsage { Id = A<int>(), Organization = organization };
        }

        private ItInterfaceExhibit CreateInterfaceExhibit()
        {
            return new ItInterfaceExhibit() { Id = A<int>() };
        }

        private ExternalReference CreateExternalReference()
        {
            return new ExternalReference() { Id = A<int>() };
        }

        private void ExpectAllowReadsReturns(ItSystem system, bool value)
        {
            _authorizationContext.Setup(x => x.AllowReads(system)).Returns(value);
        }

        private void ExpectAllowDeleteReturns(ItSystem system, bool value)
        {
            _authorizationContext.Setup(x => x.AllowDelete(system)).Returns(value);
        }

        private void ExpectGetSystemReturns(int id, ItSystem system)
        {
            _systemRepository.Setup(x => x.GetSystem(id)).Returns(system);
        }
        private void ExpectGetSystemReturns(Guid id, Maybe<ItSystem> system)
        {
            _systemRepository.Setup(x => x.GetSystem(id)).Returns(system);
        }

        private void ExpectDeleteReferenceReturns(int id, Result<IEnumerable<ExternalReference>, OperationFailure> result)
        {
            _referenceService.Setup(x => x.DeleteBySystemId(id)).Returns(result);
        }

        private void ExpectTransactionToBeSet(IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            _transactionManager.Setup(x => x.Begin(isolationLevel)).Returns(_dbTransaction.Object);
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

        private IReturnsResult<IAuthorizationContext> ExpectGetCrossLevelOrganizationAccessReturns(CrossOrganizationDataReadAccessLevel accessLevel)
        {
            return _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(accessLevel);
        }

        private IReturnsResult<IItSystemRepository> ExpectGetSystemsReturns(List<ItSystem> itSystems)
        {
            return _systemRepository.Setup(x => x.GetSystems(null)).Returns(new EnumerableQuery<ItSystem>(itSystems));
        }

        private void ExpectGetUserOrganizationIdsReturns(params int[] ownOrganizationIds)
        {
            _userContext.Setup(x => x.OrganizationIds).Returns(ownOrganizationIds);
        }
    }
}