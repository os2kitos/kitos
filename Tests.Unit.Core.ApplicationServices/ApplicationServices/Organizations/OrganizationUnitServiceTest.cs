using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.Commands;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Infrastructure.Services.DataAccess;
using Tests.Toolkit.Patterns;
using Moq;
using Xunit;
using Core.DomainModel.Events;
using Core.DomainServices;
using Core.DomainServices.Role;

namespace Tests.Unit.Core.ApplicationServices.Organizations
{
    public class OrganizationUnitServiceTest : WithAutoFixture
    {
        private readonly OrganizationUnitService _sut;

        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IDomainEvents> _domainEvents;
        private readonly Mock<IDatabaseControl> _databaseControl;
        private readonly Mock<IGenericRepository<OrganizationUnit>> _repositoryMock;
        private readonly Mock<IGenericRepository<Organization>> _organizationRepositoryMock;
        private readonly Mock<ICommandBus> _commandBusMock;
        private readonly Mock<IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit>>
            _assignmentService;

        public OrganizationUnitServiceTest()
        {
            _organizationServiceMock = new Mock<IOrganizationService>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            var organizationRightsServiceMock = new Mock<IOrganizationRightsService>();
            var contractServiceMock = new Mock<IItContractService>();
            var usageServiceMock = new Mock<IItSystemUsageService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _domainEvents = new Mock<IDomainEvents>();
            _databaseControl = new Mock<IDatabaseControl>();
            _repositoryMock = new Mock<IGenericRepository<OrganizationUnit>>();
            _organizationRepositoryMock = new Mock<IGenericRepository<Organization>>();
            _assignmentService =
                new Mock<IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit>>();

            _commandBusMock = new Mock<ICommandBus>();
            _sut = new OrganizationUnitService(
                _organizationServiceMock.Object,
                organizationRightsServiceMock.Object,
                contractServiceMock.Object,
                usageServiceMock.Object,
                _authorizationContextMock.Object,
                _transactionManagerMock.Object,
                _domainEvents.Object,
                _databaseControl.Object,
                _repositoryMock.Object,
                _commandBusMock.Object,
                _organizationRepositoryMock.Object,
                _assignmentService.Object);
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_NotFound_When_Organization_Was_NotFound()
        {
            //Arrange
            var unitUuid = A<Guid>();
            var orgUuid = A<Guid>();

            ExpectGetOrganizationReturns(orgUuid, new OperationError(OperationFailure.NotFound));

            //Act
            var result = _sut.GetRegistrations(orgUuid, unitUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void OrganizationUnitService_Methods_Return_NotFound_When_Organization_Was_NotFound(bool isDeleteSelected, bool isTransferSelected)
        {
            //Arrange
            var unitUuid = A<Guid>();
            var orgUuid = A<Guid>();

            ExpectGetOrganizationReturns(orgUuid, new OperationError(OperationFailure.NotFound));
            _transactionManagerMock.Setup(x => x.Begin());

            //Act
            var maybeResult = Maybe<OperationError>.None;
            if (isDeleteSelected)
            {
                maybeResult = _sut.DeleteRegistrations(orgUuid, unitUuid, CreateEmptyChangeParameters());
            }
            else if (isTransferSelected)
            {
                maybeResult = _sut.TransferRegistrations(orgUuid, unitUuid, A<Guid>(), CreateEmptyChangeParameters());
            }

            //Assert
            Assert.True(maybeResult.HasValue);
            Assert.Equal(OperationFailure.NotFound, maybeResult.Value.FailureType);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void OrganizationUnitService_Methods_Return_Forbidden_When_User_Is_Not_Allowed_To_Modify_Organization(bool isDeleteSelected, bool isTransferSelected)
        {
            //Arrange
            var unitUuid = A<Guid>();
            var orgUuid = A<Guid>();
            var org = new Organization { Uuid = orgUuid };

            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectAllowModifyReturns(org, false);

            //Act
            var maybeResult = Maybe<OperationError>.None;
            if (isDeleteSelected)
            {
                maybeResult = _sut.DeleteRegistrations(orgUuid, unitUuid, CreateEmptyChangeParameters());
            }
            else if (isTransferSelected)
            {
                maybeResult = _sut.TransferRegistrations(orgUuid, unitUuid, A<Guid>(), CreateEmptyChangeParameters());
            }

            Assert.True(maybeResult.HasValue);

            //Assert
            Assert.Equal(OperationFailure.Forbidden, maybeResult.Value.FailureType);
            Assert.Equal("User is not allowed to modify the organization", maybeResult.Value.Message);
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_NotFound()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var unitUuid = A<Guid>();
            var operationError = new OperationError(OperationFailure.NotFound);

            ExpectGetOrganizationReturns(orgUuid, new Organization());

            //Act
            var result = _sut.GetRegistrations(orgUuid, unitUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(operationError.FailureType, result.Error.FailureType);
        }

        [Fact]
        public void DeleteRegistrations_Returns_NotFound_When_Unit_NotFound()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var unitUuid = A<Guid>();
            var org = new Organization()
            {
                Uuid = orgUuid
            };

            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectAllowModifyReturns(org, true);

            var operationError = new OperationError(OperationFailure.NotFound);

            //Act
            var result = _sut.DeleteRegistrations(orgUuid, unitUuid, CreateEmptyChangeParameters());

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(operationError.FailureType, result.Value.FailureType);
        }

        [Fact]
        public void DeleteRegistrations_Returns_Forbidden()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var unitUuid = A<Guid>();
            var unit = new OrganizationUnit { Uuid = unitUuid };
            var org = new Organization()
            {
                Uuid = orgUuid,
                OrgUnits = new List<OrganizationUnit>()
                {
                    unit
                }
            };

            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectAllowDeleteReturns(unit, false);

            //Act
            var result = _sut.DeleteRegistrations(orgUuid, unitUuid, CreateEmptyChangeParameters());

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Forbidden, result.Value.FailureType);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TransferSelectedOrganizationRegistrations_Returns_NotFound(bool isUnitValid)
        {
            //Arrange
            var orgUuid = A<Guid>();
            var targetUnitUuid = A<Guid>();
            var unitUuid = A<Guid>();

            var org = new Organization
            {
                Uuid = orgUuid,
                Id = A<int>()
            };

            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectAllowAdministerRegistrations(org.Id, true);
            ExpectAllowModifyReturns(org, true);

            //If unit is valid check if target unit returns NotFound
            if (isUnitValid)
            {
                var unit = new OrganizationUnit { Uuid = unitUuid };
                org.OrgUnits = new List<OrganizationUnit> { unit };
                ExpectAllowModifyReturns(unit, true);
                var transaction = new Mock<IDatabaseTransaction>();
                _transactionManagerMock.Setup(x => x.Begin()).Returns(transaction.Object);
            }

            //Act
            var result = _sut.TransferRegistrations(orgUuid, unitUuid, targetUnitUuid, CreateEmptyChangeParameters());

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.NotFound, result.Value.FailureType);
        }

        [Theory]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        public void TransferSelectedOrganizationRegistrations_Returns_Forbidden(bool isUnitValid, bool isTargetUnitValid, bool hasAdministerPermission)
        {
            //Arrange
            var orgUuid = A<Guid>();
            var unitUuid = A<Guid>();
            var targetUnitUuid = A<Guid>();
            var unit = new OrganizationUnit { Uuid = unitUuid };
            var targetUnit = new OrganizationUnit { Uuid = targetUnitUuid };
            var org = new Organization()
            {
                Uuid = orgUuid,
                OrgUnits = new List<OrganizationUnit>
                {
                    unit, targetUnit
                },
                Id = A<int>()
            };

            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectAllowAdministerRegistrations(org.Id, hasAdministerPermission);
            ExpectAllowModifyReturns(unit, isUnitValid);
            ExpectAllowModifyReturns(targetUnit, isTargetUnitValid);

            //Act
            var result = _sut.TransferRegistrations(orgUuid, unitUuid, targetUnitUuid, CreateEmptyChangeParameters());

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Forbidden, result.Value.FailureType);
        }

        [Fact]
        public void GetAccessRightsByOrganization_Returns_NotFound_When_Organization_Was_NotFound()
        {
            //Arrange
            var orgUuid = A<Guid>();

            ExpectGetOrganizationReturns(orgUuid, new OperationError(OperationFailure.NotFound));

            //Act
            var result = _sut.GetAccessRightsByOrganization(orgUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void GetAccessRightsByOrganization_Returns_AccessRights()
        {
            //Arrange
            var unit1Uuid = A<Guid>();
            var unit2Uuid = A<Guid>();
            var unit3Uuid = A<Guid>();
            var orgUuid = A<Guid>();

            var unit1 = new OrganizationUnit
            {
                Uuid = unit1Uuid,
                Parent = new OrganizationUnit(),
                Origin = OrganizationUnitOrigin.Kitos
            };
            var unit2 = new OrganizationUnit
            {
                Uuid = unit2Uuid,
                Parent = null,
                Origin = OrganizationUnitOrigin.Kitos
            };
            var unit3 = new OrganizationUnit
            {
                Uuid = unit3Uuid,
                Parent = new OrganizationUnit(),
                Origin = OrganizationUnitOrigin.STS_Organisation
            };

            var organization = new Organization() { Uuid = orgUuid, OrgUnits = new List<OrganizationUnit> { unit1, unit2, unit3 } };
            unit1.Organization = organization;

            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(unit1, true);
            ExpectAllowDeleteReturns(unit1, true);
            ExpectAllowModifyReturns(unit2, true);
            ExpectAllowDeleteReturns(unit2, true);
            ExpectAllowModifyReturns(unit3, true);
            ExpectAllowDeleteReturns(unit3, true);

            //Act
            var result = _sut.GetAccessRightsByOrganization(orgUuid);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value.ToList();

            //unit 1 - all should be true
            var unit1AccessRights = accessRights.FirstOrDefault(x => x.OrganizationUnit.Uuid == unit1Uuid);
            Assert.NotNull(unit1AccessRights);
            Assert.True(unit1AccessRights.UnitAccessRights.CanBeRead);
            Assert.True(unit1AccessRights.UnitAccessRights.CanBeModified);
            Assert.True(unit1AccessRights.UnitAccessRights.CanBeRearranged);
            Assert.True(unit1AccessRights.UnitAccessRights.CanBeRenamed);
            Assert.True(unit1AccessRights.UnitAccessRights.CanDeviceIdBeModified);
            Assert.True(unit1AccessRights.UnitAccessRights.CanEanBeModified);
            Assert.True(unit1AccessRights.UnitAccessRights.CanBeDeleted);

            //unit 2 - Parent - shouldn't be allowed to delete and rearrange
            var unit2AccessRights = accessRights.FirstOrDefault(x => x.OrganizationUnit.Uuid == unit2Uuid);
            Assert.NotNull(unit2AccessRights);
            Assert.True(unit2AccessRights.UnitAccessRights.CanBeRead);
            Assert.True(unit2AccessRights.UnitAccessRights.CanBeModified);
            Assert.False(unit2AccessRights.UnitAccessRights.CanBeRearranged);
            Assert.True(unit2AccessRights.UnitAccessRights.CanBeRenamed);
            Assert.True(unit2AccessRights.UnitAccessRights.CanDeviceIdBeModified);
            Assert.True(unit2AccessRights.UnitAccessRights.CanEanBeModified);
            Assert.False(unit2AccessRights.UnitAccessRights.CanBeDeleted);

            //unit 3 - STS Org - shouldn't be allowed to delete, rearrange and rename
            var unit3AccessRights = accessRights.FirstOrDefault(x => x.OrganizationUnit.Uuid == unit3Uuid);
            Assert.NotNull(unit3AccessRights);
            Assert.True(unit3AccessRights.UnitAccessRights.CanBeRead);
            Assert.True(unit3AccessRights.UnitAccessRights.CanBeModified);
            Assert.False(unit3AccessRights.UnitAccessRights.CanBeRearranged);
            Assert.False(unit3AccessRights.UnitAccessRights.CanBeRenamed);
            Assert.True(unit3AccessRights.UnitAccessRights.CanDeviceIdBeModified);
            Assert.True(unit3AccessRights.UnitAccessRights.CanEanBeModified);
            Assert.False(unit3AccessRights.UnitAccessRights.CanBeDeleted);
        }

        [Fact]
        public void GetAccessRights_Returns_NotFound_When_Organization_Was_NotFound()
        {
            //Arrange
            var unitUuid = A<Guid>();
            var orgUuid = A<Guid>();

            ExpectGetOrganizationReturns(orgUuid, new OperationError(OperationFailure.NotFound));

            //Act
            var result = _sut.GetAccessRights(orgUuid, unitUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void GetAccessRights_Returns_NotFound_When_OrganizationUnit_Was_NotFound()
        {
            //Arrange
            var unitUuid = A<Guid>();
            var orgUuid = A<Guid>();

            var organization = new Organization() { Uuid = orgUuid };

            ExpectGetOrganizationReturns(orgUuid, organization);

            //Act
            var result = _sut.GetAccessRights(orgUuid, unitUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void GetAccessRights_Returns_AccessRights()
        {
            //Arrange
            var unitUuid = A<Guid>();
            var orgUuid = A<Guid>();

            var unit = new OrganizationUnit
            {
                Uuid = unitUuid,
                Parent = new OrganizationUnit(),
                Origin = OrganizationUnitOrigin.Kitos
            };
            var organization = new Organization() { Uuid = orgUuid, OrgUnits = new List<OrganizationUnit> { unit } };
            unit.Organization = organization;

            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(unit, true);
            ExpectAllowDeleteReturns(unit, true);

            //Act
            var result = _sut.GetAccessRights(orgUuid, unitUuid);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;

            Assert.True(accessRights.CanBeDeleted);
            Assert.True(accessRights.CanBeRenamed);
            Assert.True(accessRights.CanEanBeModified);
            Assert.True(accessRights.CanDeviceIdBeModified);
            Assert.True(accessRights.CanBeModified);
            Assert.True(accessRights.CanBeRearranged);
            Assert.True(accessRights.CanBeRead);
        }

        [Fact]
        public void GetAccessRights_Returns_Only_ReadAllowed_When_Not_Allowed_To_Modify()
        {
            //Arrange
            var unitUuid = A<Guid>();
            var orgUuid = A<Guid>();

            var unit = new OrganizationUnit
            {
                Uuid = unitUuid,
                Origin = OrganizationUnitOrigin.Kitos
            };
            var organization = new Organization() { Uuid = orgUuid, OrgUnits = new List<OrganizationUnit> { unit } };
            unit.Organization = organization;

            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(unit, false);

            //Act
            var result = _sut.GetAccessRights(orgUuid, unitUuid);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;

            Assert.True(accessRights.CanBeRead);
            Assert.False(accessRights.CanBeRenamed);
            Assert.False(accessRights.CanEanBeModified);
            Assert.False(accessRights.CanDeviceIdBeModified);
            Assert.False(accessRights.CanBeModified);
            Assert.False(accessRights.CanBeRearranged);
            Assert.False(accessRights.CanBeDeleted);
        }

        [Fact]
        public void GetAccessRights_Returns_CorrectRights_When_Unit_Is_Not_Of_Kitos_Origin()
        {
            //Arrange
            var unitUuid = A<Guid>();
            var orgUuid = A<Guid>();

            var unit = new OrganizationUnit
            {
                Uuid = unitUuid,
                Origin = OrganizationUnitOrigin.STS_Organisation
            };
            var organization = new Organization() { Uuid = orgUuid, OrgUnits = new List<OrganizationUnit> { unit } };
            unit.Organization = organization;

            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(unit, true);

            //Act
            var result = _sut.GetAccessRights(orgUuid, unitUuid);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;

            Assert.True(accessRights.CanBeRead);
            Assert.True(accessRights.CanBeModified);
            Assert.False(accessRights.CanBeRenamed);
            Assert.True(accessRights.CanEanBeModified);
            Assert.True(accessRights.CanDeviceIdBeModified);
            Assert.False(accessRights.CanBeRearranged);
            Assert.False(accessRights.CanBeDeleted);
        }

        [Fact]
        public void GetAccessRights_Returns_CorrectRights_When_Unit_Is_Root()
        {
            //Arrange
            var unitUuid = A<Guid>();
            var orgUuid = A<Guid>();

            var unit = new OrganizationUnit
            {
                Uuid = unitUuid,
                Origin = OrganizationUnitOrigin.Kitos
            };
            var organization = new Organization() { Uuid = orgUuid, OrgUnits = new List<OrganizationUnit> { unit } };
            unit.Organization = organization;

            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(unit, true);

            //Act
            var result = _sut.GetAccessRights(orgUuid, unitUuid);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;

            Assert.True(accessRights.CanBeRead);
            Assert.True(accessRights.CanBeModified);
            Assert.True(accessRights.CanBeRenamed);
            Assert.True(accessRights.CanEanBeModified);
            Assert.True(accessRights.CanDeviceIdBeModified);
            Assert.False(accessRights.CanBeRearranged);
            Assert.False(accessRights.CanBeDeleted);
        }

        [Fact]
        public void GetAccessRights_Returns_CorrectRights_When_Unit_Is_Not_Root()
        {
            //Arrange
            var unitUuid = A<Guid>();
            var orgUuid = A<Guid>();

            var unit = new OrganizationUnit
            {
                Uuid = unitUuid,
                Parent = new OrganizationUnit(),
                Origin = OrganizationUnitOrigin.Kitos
            };
            var organization = new Organization() { Uuid = orgUuid, OrgUnits = new List<OrganizationUnit> { unit } };
            unit.Organization = organization;

            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(unit, true);

            //Act
            var result = _sut.GetAccessRights(orgUuid, unitUuid);

            //Assert
            Assert.True(result.Ok);
            var accessRights = result.Value;

            Assert.True(accessRights.CanBeRead);
            Assert.True(accessRights.CanBeModified);
            Assert.True(accessRights.CanBeRenamed);
            Assert.True(accessRights.CanEanBeModified);
            Assert.True(accessRights.CanDeviceIdBeModified);
            Assert.True(accessRights.CanBeRearranged);
            Assert.False(accessRights.CanBeDeleted);
        }

        [Fact]
        public void Can_Delete_Organization_Unit()
        {
            //Arrange
            var orgId = A<Guid>();
            var unitUuid = A<Guid>();
            var root = new OrganizationUnit { Uuid = A<Guid>() };
            var toRemove = new OrganizationUnit { Uuid = unitUuid };
            toRemove.Parent = root;
            root.Children.Add(toRemove);
            var org = new Organization
            {
                Uuid = orgId,
                OrgUnits = new List<OrganizationUnit>
                {
                    root,toRemove
                }
            };

            var transaction = ExpectBeginTransaction();
            ExpectGetOrganizationReturns(orgId, org);
            ExpectAllowModifyReturns(org, true);
            ExpectAllowModifyReturns(toRemove, true);
            ExpectAllowDeleteReturns(toRemove, true);
            ExpectDeleteUnitCommandReturns(toRemove, Maybe<OperationError>.None);

            //Act
            var deleteError = _sut.Delete(orgId, unitUuid);

            //Assert
            Assert.True(deleteError.IsNone);
            _databaseControl.Verify(x => x.SaveChanges(), Times.Once());
            transaction.Verify(x => x.Commit(), Times.Once());
            _domainEvents.Verify(x => x.Raise(It.Is<EntityBeingDeletedEvent<OrganizationUnit>>(ev => ev.Entity == toRemove)));
        }

        [Fact]
        public void Cannot_Delete_Organization_Unit_If_Delete_Command_Fails()
        {
            //Arrange
            var orgId = A<Guid>();
            var unitUuid = A<Guid>();
            var root = new OrganizationUnit { Uuid = A<Guid>() };
            var toRemove = new OrganizationUnit { Uuid = unitUuid };
            toRemove.Parent = root;
            root.Children.Add(toRemove);
            var org = new Organization
            {
                Uuid = orgId,
                OrgUnits = new List<OrganizationUnit>
                {
                    root,toRemove
                }
            };

            var transaction = ExpectBeginTransaction();
            ExpectGetOrganizationReturns(orgId, org);
            ExpectAllowModifyReturns(org, true);
            ExpectAllowDeleteReturns(toRemove, true);
            ExpectDeleteUnitCommandReturns(toRemove, A<OperationError>());

            //Act
            var deleteError = _sut.Delete(orgId, unitUuid);

            //Assert
            Assert.False(deleteError.IsNone);
            _databaseControl.Verify(x => x.SaveChanges(), Times.Never());
            transaction.Verify(x => x.Commit(), Times.Never());
            transaction.Verify(x => x.Rollback(), Times.Once());
        }

        [Fact]
        public void Cannot_Delete_Organization_Unit_If_Delete_Unit_Is_UnAuthorized()
        {
            //Arrange
            var orgId = A<Guid>();
            var unitUuid = A<Guid>();
            var root = new OrganizationUnit { Uuid = A<Guid>() };
            var toRemove = new OrganizationUnit { Uuid = unitUuid };
            toRemove.Parent = root;
            root.Children.Add(toRemove);
            var org = new Organization
            {
                Uuid = orgId,
                OrgUnits = new List<OrganizationUnit>
                {
                    root,toRemove
                }
            };

            var transaction = ExpectBeginTransaction();
            ExpectGetOrganizationReturns(orgId, org);
            ExpectAllowModifyReturns(org, true);
            ExpectAllowDeleteReturns(toRemove, false);

            //Act
            var deleteError = _sut.Delete(orgId, unitUuid);

            //Assert
            Assert.False(deleteError.IsNone);
            _databaseControl.Verify(x => x.SaveChanges(), Times.Never());
            transaction.Verify(x => x.Commit(), Times.Never());
            transaction.Verify(x => x.Rollback(), Times.Once());
        }

        [Fact]
        public void Can_Create_Organization_Unit()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var orgId = A<int>();
            var name = A<string>();
            var origin = A<OrganizationUnitOrigin>();
            var parentUuid = A<Guid>();
            var parentUnit = new OrganizationUnit { Uuid = parentUuid};

            var org = new Organization
            {
                Uuid = orgUuid,
                Id = orgId,
                OrgUnits = new 
                    List<OrganizationUnit> { parentUnit }
            };
            var newUnit = new OrganizationUnit { Name = name, Origin = origin};
            parentUnit.Organization = org;

            var transaction = ExpectBeginTransaction();
            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectGetOrganizationUnitReturns(parentUuid, parentUnit);
            ExpectAllowModifyReturns(org, true);
            ExpectWithCreateUnitAccessReturns(orgId, true);
            _repositoryMock.Setup(mock => mock.Insert(newUnit));
            _organizationRepositoryMock.Setup(mock => mock.Update(org));

            //Act
            var createResult = _sut.Create(orgUuid, parentUuid, name, origin);

            //Assert
            Assert.True(createResult.Ok);
            _databaseControl.Verify(x => x.SaveChanges(), Times.Once());
            transaction.Verify(x => x.Commit(), Times.Once());
            _domainEvents.Verify(x => x.Raise(It.Is<EntityCreatedEvent<OrganizationUnit>>(ev => ev.Entity.Name == newUnit.Name)));
        }

        [Fact]
        public void Cannnot_Create_Organization_Unit_If_Unauthorized()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var orgId = A<int>();
            var name = A<string>();
            var origin = A<OrganizationUnitOrigin>();
            var parentUuid = A<Guid>();
            var parentUnit = new OrganizationUnit { Uuid = parentUuid};

            var org = new Organization
            {
                Uuid = orgUuid,
                Id = orgId,
                OrgUnits = new 
                    List<OrganizationUnit> { parentUnit }
            };
            var newUnit = new OrganizationUnit { Name = name, Origin = origin};
            parentUnit.Organization = org;

            var transaction = ExpectBeginTransaction();
            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectGetOrganizationUnitReturns(parentUuid, parentUnit);
            ExpectAllowModifyReturns(org, true);
            ExpectWithCreateUnitAccessReturns(orgId, false);

            //Act
            var createResult = _sut.Create(orgUuid, parentUuid, name, origin);

            //Assert
            Assert.True(createResult.Failed);
            Assert.Equal(OperationFailure.Forbidden, createResult.Error.FailureType);
        }

        [Fact]
        public void Can_Get_All_SubunitRights()
        {
            //Arrange
            var nestedUnit = CreateNestedOrganizationUnit(3, 3);
            var orgUuid = A<Guid>();
            var unitUuid = A<Guid>();
            nestedUnit.Uuid = unitUuid;
            ExpectGetOrganizationUnitReturns(unitUuid, nestedUnit);
            
            //Act
            var rights = _sut.GetRightsOfUnitSubtree(orgUuid, unitUuid);

            //Assert
            Assert.True(rights.Ok);
            Assert.Equal(1 + 3 + 9 + 27, rights.Value.Count());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Create_Role_Assignment_Returns_Expected_According_To_Modification_Rights(bool canModify)
        {
            //Arrange
            var unitUuid = A<Guid>();
            var userUuid = A<Guid>();
            var roleUuid = A<Guid>();
            var unit = new OrganizationUnit { };
            var right = new OrganizationUnitRight { };
            ExpectGetOrganizationUnitReturns(unitUuid, unit);
            ExpectAllowModifyReturns(unit, canModify);
            _assignmentService.Setup(x => x.AssignRole(unit, roleUuid, userUuid)).Returns(right);

            //Act
            var createResult = _sut.CreateRoleAssignment(unitUuid, roleUuid, userUuid);

            //Assert
            Assert.Equal(createResult.Ok, canModify);

        }

        [Fact]
        public void Create_Role_Bulk_Assignment_Returns_Unit()
        {
            //Arrange
            var unitUuid = A<Guid>();
            var assignment1 = A<UserRolePair>();
            var assignment2 = A<UserRolePair>();
            var assignments = new List<UserRolePair> { assignment1, assignment2 };
            var unit = new OrganizationUnit { };

            ExpectGetOrganizationUnitReturns(unitUuid, unit);
            ExpectAllowModifyReturns(unit, true);

            _assignmentService.Setup(x => x.BatchUpdateRoles(
                        unit,
                        It.Is<IEnumerable<(Guid roleUuid, Guid user)>>(assignments =>
                            MatchExpectedAssignments(assignments, new[] { assignment1, assignment2 }.ToList()))
                    )
                )
                .Returns(Maybe<OperationError>.None);
            //Act
            var createResult = _sut.CreateBulkRoleAssignment(unitUuid, assignments);

            //Assert
            Assert.True(createResult.Ok);

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Delete_Role_Assignment_Returns_Expected_According_To_Modification_Rights(bool canModify)
        {
            //Arrange
            var unitUuid = A<Guid>();
            var userUuid = A<Guid>();
            var roleUuid = A<Guid>();
            var unit = new OrganizationUnit { };
            var right = new OrganizationUnitRight { };
            ExpectGetOrganizationUnitReturns(unitUuid, unit);
            ExpectAllowModifyReturns(unit, canModify);
            _assignmentService.Setup(x => x.RemoveRole(unit, roleUuid, userUuid)).Returns(right);

            //Act
            var createResult = _sut.DeleteRoleAssignment(unitUuid, roleUuid, userUuid);

            //Assert
            Assert.Equal(createResult.Ok, canModify);
        }

        private OrganizationUnit CreateNestedOrganizationUnit(int nestingLevel, int branchFactor)
        {
            var unit = CreateUnit();
            if (nestingLevel == 0)
            {
                return unit;
            }
            var children = new List<OrganizationUnit>();
            for (var i = 0; i < branchFactor; i++)
            {
                var childUnit = CreateNestedOrganizationUnit(nestingLevel - 1, branchFactor);
                children.Add(childUnit);
            }
            unit.Children = children;
            return unit;
        }

        private OrganizationUnit CreateUnit()
        {
            return new OrganizationUnit
            {
                Rights = new List<OrganizationUnitRight> {new OrganizationUnitRight() }
            };
        }

        private void ExpectGetOrganizationReturns(Guid uuid, Organization result)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(uuid, OrganizationDataReadAccessLevel.All)).Returns(result);
        }

        private void ExpectGetOrganizationUnitReturns(Guid unitUuid, OrganizationUnit result)
        {
            _organizationServiceMock.Setup(x => x.GetOrganizationUnit(unitUuid)).Returns(result);
        }

        private void ExpectGetOrganizationReturns(Guid uuid, OperationError result)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(uuid, OrganizationDataReadAccessLevel.All)).Returns(result);
        }

        private void ExpectAllowDeleteReturns(IEntity unit, bool result)
        {
            _authorizationContextMock.Setup(x => x.AllowDelete(unit)).Returns(result);
        }

        private void ExpectAllowModifyReturns(IEntity unit, bool result)
        {
            _authorizationContextMock.Setup(x => x.AllowModify(unit)).Returns(result);
        }

        private void ExpectAllowAdministerRegistrations(int orgId, bool result)
        {
            _authorizationContextMock.Setup(x => x.HasPermission(It.Is<BulkAdministerOrganizationUnitRegistrations>(r => r.OrganizationId == orgId))).Returns(result);
        }

        private static OrganizationUnitRegistrationChangeParameters CreateEmptyChangeParameters()
        {
            return new OrganizationUnitRegistrationChangeParameters(
                new List<int>(),
                new List<int>(),
                new List<PaymentChangeParameters>(),
                new List<int>(),
                new List<int>());
        }

        private void ExpectDeleteUnitCommandReturns(OrganizationUnit toRemove, Maybe<OperationError> result)
        {
            _commandBusMock.Setup(x =>
                    x.Execute<RemoveOrganizationUnitRegistrationsCommand, Maybe<OperationError>>(
                        It.Is<RemoveOrganizationUnitRegistrationsCommand>(unit => unit.OrganizationUnit == toRemove)))
                .Returns(result);
        }

        private void ExpectWithCreateUnitAccessReturns(int orgId,
            bool result)
        {
            _authorizationContextMock.Setup(mock => mock.AllowCreate<OrganizationUnit>(orgId)).Returns(result);
        }

        private Mock<IDatabaseTransaction> ExpectBeginTransaction()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transaction.Object);
            return transaction;
        }
        private static bool MatchExpectedAssignments(IEnumerable<(Guid roleUuid, Guid user)> actual, List<UserRolePair> expected)
        {
            return actual.SequenceEqual(expected.Select(p => (roleUuid: p.RoleUuid, user: p.UserUuid)));
        }
    }
}
