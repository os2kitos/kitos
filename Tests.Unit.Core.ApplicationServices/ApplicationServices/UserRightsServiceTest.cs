using System;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Rights;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices;
using Core.ApplicationServices.Model.Users;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Authorization;
using Core.DomainServices.Generic;
using Core.DomainServices.Queries;
using Core.DomainServices.Role;
using Infrastructure.Services.DataAccess;
using Serilog;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class UserRightsServiceTest : WithAutoFixture
    {
        private readonly UserRightsService _sut;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IAuthorizationContext> _authServiceMock;
        private readonly Mock<IOrganizationRightsService> _orgRightsServiceMock;

        private readonly Mock<IRoleAssignmentService<ItContractRight, ItContractRole, ItContract>>
            _contractRightsServiceMock;

        private readonly Mock<IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage>>
            _systemRightsServiceMock;

        private readonly Mock<IRoleAssignmentService<ItProjectRight, ItProjectRole, ItProject>>
            _projectRightsServiceMock;

        private readonly Mock<IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit>>
            _organizationUnitRightsServiceMock;

        private readonly
            Mock<IRoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole,
                DataProcessingRegistration>> _dprRightsServiceMock;

        private readonly Mock<ITransactionManager> _transactionMgrMock;
        private readonly Mock<IDatabaseControl> _dbControlMock;
        private readonly Mock<IEntityIdentityResolver> _identityResolverMock;

        public UserRightsServiceTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _authServiceMock = new Mock<IAuthorizationContext>();
            _orgRightsServiceMock = new Mock<IOrganizationRightsService>();
            _contractRightsServiceMock =
                new Mock<IRoleAssignmentService<ItContractRight, ItContractRole, ItContract>>();
            _systemRightsServiceMock = new Mock<IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage>>();
            _projectRightsServiceMock = new Mock<IRoleAssignmentService<ItProjectRight, ItProjectRole, ItProject>>();
            _organizationUnitRightsServiceMock =
                new Mock<IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit>>();
            _dprRightsServiceMock =
                new Mock<IRoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole,
                    DataProcessingRegistration>>();
            _transactionMgrMock = new Mock<ITransactionManager>();
            _dbControlMock = new Mock<IDatabaseControl>();
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _sut = new UserRightsService(
                _userServiceMock.Object,
                _organizationServiceMock.Object,
                _authServiceMock.Object,
                _identityResolverMock.Object,
                _orgRightsServiceMock.Object,
                _contractRightsServiceMock.Object,
                _systemRightsServiceMock.Object,
                _projectRightsServiceMock.Object,
                _organizationUnitRightsServiceMock.Object,
                _dprRightsServiceMock.Object,
                _transactionMgrMock.Object,
                _dbControlMock.Object,
                Mock.Of<ILogger>()
            );
        }

        [Fact]
        public void GetUsersWithRoleAssignment_Returns_Users_And_Organization_Relations()
        {
            //Arrange
            var role = A<OrganizationRole>();
            var orgId1 = A<int>();
            var orgId2 = A<int>();
            var user1 = CreateUserWithRole(orgId1, role);
            var user2 = CreateUserWithRole(orgId2, role);
            var users = new List<User> { user1, user2 };

            var org1 = new Organization { Id = orgId1 };
            var org2 = new Organization { Id = orgId2 };

            ExpectUserHasCrossLevelAccess(CrossOrganizationDataReadAccessLevel.All);

            SetupUserService(users, role);
            SetupOrganizationService(new List<Organization> { org1, org2 });

            //Act
            var result = _sut.GetUsersWithRoleAssignment(role);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(2, result.Value.Count());

            Assert.Same(user1, result.Value.First(x => x.User.Id == user1.Id).User);
            Assert.Same(org1, result.Value.First(x => x.User.Id == user1.Id).Organization);

            Assert.Same(user2, result.Value.First(x => x.User.Id == user2.Id).User);
            Assert.Same(org2, result.Value.First(x => x.User.Id == user2.Id).Organization);
        }

        [Fact]
        public void GetUsersWithRoleAssignment_Returns_Multiple_If_User_Is_RightsHolder_In_Multiple_Orgs()
        {
            //Arrange
            var role = A<OrganizationRole>();
            var orgId1 = A<int>();
            var orgId2 = A<int>();
            var user = CreateUserWithRole(orgId1, role);
            user.OrganizationRights.Add(new OrganizationRight() { OrganizationId = orgId2, Role = role });

            var users = new List<User>() { user };
            var org1 = new Organization() { Id = orgId1 };
            var org2 = new Organization() { Id = orgId2 };

            ExpectUserHasCrossLevelAccess(CrossOrganizationDataReadAccessLevel.All);

            SetupUserService(users, role);
            SetupOrganizationService(new List<Organization>() { org1, org2 });

            //Act
            var result = _sut.GetUsersWithRoleAssignment(role);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(2, result.Value.Count());

            Assert.Same(user, result.Value.First(x => x.Organization.Id == orgId1).User);
            Assert.Same(org1, result.Value.First(x => x.Organization.Id == orgId1).Organization);

            Assert.Same(user, result.Value.First(x => x.Organization.Id == orgId2).User);
            Assert.Same(org2, result.Value.First(x => x.Organization.Id == orgId2).Organization);
        }

        [Fact]
        public void GetUsersWithRoleAssignment_Returns_Error_If_GetUsersWithRole_Fails()
        {
            //Arrange
            var role = A<OrganizationRole>();
            var operationError = A<OperationError>();

            ExpectUserHasCrossLevelAccess(CrossOrganizationDataReadAccessLevel.All);
            _userServiceMock
                .Setup(x => x.GetUsersWithRoleAssignedInAnyOrganization(role))
                .Returns(Result<IQueryable<User>, OperationError>.Failure(operationError));

            //Act
            var result = _sut.GetUsersWithRoleAssignment(role);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        [Fact]
        public void GetUsersWithRoleAssignment_Returns_Error_If_GetOrganizations_Fails()
        {
            //Arrange
            var role = A<OrganizationRole>();
            var orgId = A<int>();
            var user = CreateUserWithRole(orgId, role);
            var users = new List<User>() { user };

            var operationError = A<OperationError>();

            ExpectUserHasCrossLevelAccess(CrossOrganizationDataReadAccessLevel.All);

            SetupUserService(users, role);

            _organizationServiceMock
                .Setup(x => x.GetAllOrganizations())
                .Returns(Result<IQueryable<Organization>, OperationError>.Failure(operationError));

            //Act
            var result = _sut.GetUsersWithRoleAssignment(role);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        [Theory]
        [InlineData(OrganizationDataReadAccessLevel.None)]
        [InlineData(OrganizationDataReadAccessLevel.Public)]
        [InlineData(OrganizationDataReadAccessLevel.RightsHolder)]
        public void GetUserRights_Returns_Error_If_Read_Access_Level_Too_Low(
            OrganizationDataReadAccessLevel accessLevel)
        {
            //Arrange
            var userId = A<int>();
            var organizationId = A<int>();
            ExpectReadAccessLevel(accessLevel, organizationId);

            //Act
            var result = _sut.GetUserRights(userId, organizationId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void GetUserRights_Returns_Error_If_Org_If_Is_Invalid()
        {
            //Arrange
            var userId = A<int>();
            var organizationId = A<int>();
            ExpectReadAccessLevel(OrganizationDataReadAccessLevel.All, organizationId);
            ExpectResolveUuidReturns<Organization>(organizationId, Maybe<Guid>.None);

            //Act
            var result = _sut.GetUserRights(userId, organizationId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void GetUserRights_Returns_Error_If_User_Id_Invalid()
        {
            //Arrange
            var userId = A<int>();
            var organizationId = A<int>();
            var orgUuid = A<Guid>();
            ExpectReadAccessLevel(OrganizationDataReadAccessLevel.All, organizationId);
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectResolveUuidReturns<User>(userId, Maybe<Guid>.None);

            //Act
            var result = _sut.GetUserRights(userId, organizationId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void GetUserRights_Returns_Error_If_GetUserInOrganization_Fails()
        {
            //Arrange
            var userId = A<int>();
            var organizationId = A<int>();
            var orgUuid = A<Guid>();
            var userUuid = A<Guid>();
            var operationError = A<OperationError>();
            ExpectReadAccessLevel(OrganizationDataReadAccessLevel.All, organizationId);
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectResolveUuidReturns<User>(userId, userUuid);
            ExpectGetUserInOrganization(orgUuid, userUuid, operationError);

            //Act
            var result = _sut.GetUserRights(userId, organizationId);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        [Fact]
        public void GetUserRights_With_No_Rights_Returns_Empty_Rights_In_Organization()
        {
            //Arrange
            var userId = A<int>();
            var organizationId = A<int>();
            var orgUuid = A<Guid>();
            var userUuid = A<Guid>();
            var user = new User();
            ExpectReadAccessLevel(OrganizationDataReadAccessLevel.All, organizationId);
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectResolveUuidReturns<User>(userId, userUuid);
            ExpectGetUserInOrganization(orgUuid, userUuid, user);

            //Act
            var result = _sut.GetUserRights(userId, organizationId);

            //Assert
            Assert.True(result.Ok);
            var userRightsAssignments = result.Value;
            Assert.Equal(Enumerable.Empty<OrganizationRole>(), userRightsAssignments.LocalAdministrativeAccessRoles);
            Assert.Equal(Enumerable.Empty<OrganizationUnitRight>(), userRightsAssignments.OrganizationUnitRights);
            Assert.Equal(Enumerable.Empty<ItContractRight>(), userRightsAssignments.ContractRights);
            Assert.Equal(Enumerable.Empty<ItProjectRight>(), userRightsAssignments.ProjectRights);
            Assert.Equal(Enumerable.Empty<ItSystemRight>(), userRightsAssignments.SystemRights);
            Assert.Equal(Enumerable.Empty<DataProcessingRegistrationRight>(),
                userRightsAssignments.DataProcessingRegistrationRights);
        }

        [Fact]
        public void GetUserRights_With_All_Rights_Returns_Rights_In_Organization()
        {
            //Arrange - add rights and roles int different organizations to verify that only the ones in the organization are returned
            var userId = A<int>();
            var organizationId = A<int>();
            var orgUuid = A<Guid>();
            var userUuid = A<Guid>();
            var user = new User();
            var dprRightIds = Many<int>().ToArray();
            var contractRightIds = Many<int>().ToArray();
            var projectRightIds = Many<int>().ToArray();
            var systemRightIds = Many<int>().ToArray();
            var orgUnitRightIds = Many<int>().ToArray();


            ExpectReadAccessLevel(OrganizationDataReadAccessLevel.All, organizationId);
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectResolveUuidReturns<User>(userId, userUuid);
            ExpectGetUserInOrganization(orgUuid, userUuid, user);
            var userRolesInOrg = EnumRange.All<OrganizationRole>().ToArray();


            user.OrganizationRights = CreateUserOrganizationRights(orgUuid, userRolesInOrg)
                .Concat(CreateUserOrganizationRights(A<Guid>(), OrganizationRole.LocalAdmin))
                .ToList();

            user.DataProcessingRegistrationRights = CreateDprRights(orgUuid, dprRightIds)
                .Concat(CreateDprRights(A<Guid>(), Many<int>().ToArray()))
                .ToList();

            user.ItSystemRights = CreateSystemRights(orgUuid, systemRightIds)
                .Concat(CreateSystemRights(A<Guid>(), Many<int>().ToArray()))
                .ToList();

            user.ItContractRights = CreateContractRights(orgUuid, contractRightIds)
                .Concat(CreateContractRights(A<Guid>(), Many<int>().ToArray()))
                .ToList();

            user.ItProjectRights = CreateProjectRights(orgUuid, projectRightIds)
                .Concat(CreateProjectRights(A<Guid>(), Many<int>().ToArray()))
                .ToList();

            user.OrganizationUnitRights = CreateOrgUnitRights(orgUuid, orgUnitRightIds)
                .Concat(CreateOrgUnitRights(A<Guid>(), Many<int>().ToArray()))
                .ToList();


            //Act
            var result = _sut.GetUserRights(userId, organizationId);

            //Assert
            Assert.True(result.Ok);
            var userRightsAssignments = result.Value;
            var expectedRoles =
                userRolesInOrg.Intersect(GetLocalAdminRoles()).OrderBy(x => x)
                    .ToList(); //only the local admin roles are expected - not user (non admin) and global admin (global role)
            Assert.Equal(expectedRoles, userRightsAssignments.LocalAdministrativeAccessRoles.OrderBy(x => x).ToList());
            Assert.Equal(dprRightIds, userRightsAssignments.DataProcessingRegistrationRights.Select(x => x.Id));
            Assert.Equal(systemRightIds, userRightsAssignments.SystemRights.Select(x => x.Id));
            Assert.Equal(contractRightIds, userRightsAssignments.ContractRights.Select(x => x.Id));
            Assert.Equal(projectRightIds, userRightsAssignments.ProjectRights.Select(x => x.Id));
            Assert.Equal(orgUnitRightIds, userRightsAssignments.OrganizationUnitRights.Select(x => x.Id));
        }

        [Fact]
        public void RemoveAllRights_Fails_If_Get_Organization_Fails()
        {
            //Arrange
            var userId = A<int>();
            var organizationId = A<int>();
            var orgUuid = A<Guid>();
            var getOrganizationError = A<OperationError>();
            var transaction = ExpectBeginTransaction();
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, getOrganizationError);

            //Act
            var error = _sut.RemoveAllRights(userId, organizationId);

            //Assert
            Assert.True(error.HasValue);
            Assert.Same(getOrganizationError, error.Value);
            VerifyTransactionFailed(transaction);
        }

        [Fact]
        public void RemoveAllRights_Fails_If_No_Modification_Access_To_Organization()
        {
            //Arrange
            var userId = A<int>();
            var organizationId = A<int>();
            var orgUuid = A<Guid>();
            var organization = new Organization() { Id = A<int>() };
            var transaction = ExpectBeginTransaction();
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(organization, false);

            //Act
            var error = _sut.RemoveAllRights(userId, organizationId);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Forbidden, error.Value.FailureType);
            VerifyTransactionFailed(transaction);
        }

        [Fact]
        public void RemoveAllRights_Fails_If_Get_Users_In_Org_Fails()
        {
            //Arrange
            var userId = A<int>();
            var organizationId = A<int>();
            var orgUuid = A<Guid>();
            var organization = new Organization() { Id = A<int>(), Uuid = orgUuid };
            var transaction = ExpectBeginTransaction();
            var getUserError = A<OperationError>();
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(organization, true);
            ExpectGetUsersInOrgReturns(orgUuid, getUserError);

            //Act
            var error = _sut.RemoveAllRights(userId, organizationId);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(getUserError.FailureType, error.Value.FailureType);
            VerifyTransactionFailed(transaction);
        }

        [Fact]
        public void RemoveAllRights_Fails_If_User_Not_In_Org()
        {
            //Arrange
            var userId = A<int>();
            var organizationId = A<int>();
            var orgUuid = A<Guid>();
            var organization = new Organization() { Id = A<int>(), Uuid = orgUuid };
            var transaction = ExpectBeginTransaction();
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(organization, true);
            ExpectGetUsersInOrgReturns(orgUuid,
                Result<IQueryable<User>, OperationError>.Success(Enumerable.Empty<User>().AsQueryable()));

            //Act
            var error = _sut.RemoveAllRights(userId, organizationId);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
            VerifyTransactionFailed(transaction);

        }

        [Fact]
        public void RemoveAllRights_Fails_If_Removal_Of_A_Right_Fails()
        {
            //Arrange
            var organizationRole = GetLocalAdminRoles().RandomItem();
            var orgUuid = A<Guid>();
            var organizationId = A<int>();
            var user = CreateUserWithRole(organizationId, organizationRole, orgUuid);
            var organization = new Organization() { Id = organizationId, Uuid = orgUuid };
            var transaction = ExpectBeginTransaction();
            var removeRightError = A<OperationFailure>();
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(organization, true);
            ExpectGetUsersInOrgReturns(orgUuid,
                Result<IQueryable<User>, OperationError>.Success(new[] { user }.AsQueryable()));
            ExpectRemoveOrgRoleReturns(organizationId, user.Id, organizationRole, removeRightError);

            //Act
            var error = _sut.RemoveAllRights(user.Id, organizationId);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(removeRightError, error.Value.FailureType);
            VerifyTransactionFailed(transaction);
        }

        [Fact]
        public void RemoveAllRights_RemovesAllRightsInOrganization()
        {
            //Arrange
            var organizationRole = GetLocalAdminRoles().RandomItem();
            var orgUuid = A<Guid>();
            var organizationId = A<int>();
            var user = CreateUserWithRole(organizationId, organizationRole, orgUuid);

            var dprRights = CreateDprRights(orgUuid, Many<int>().ToArray()).ToList();
            var itSystemRights = CreateSystemRights(orgUuid, Many<int>().ToArray()).ToList();
            var organizationUnitRights = CreateOrgUnitRights(orgUuid, Many<int>().ToArray()).ToList();
            var itProjectRights = CreateProjectRights(orgUuid, Many<int>().ToArray()).ToList();
            var itContractRights = CreateContractRights(orgUuid, Many<int>().ToArray()).ToList();

            user.DataProcessingRegistrationRights = dprRights.Concat(CreateDprRights(A<Guid>(), Many<int>().ToArray())).ToList();
            user.ItSystemRights = itSystemRights.Concat(CreateSystemRights(A<Guid>(), Many<int>().ToArray())).ToList();
            user.OrganizationUnitRights = organizationUnitRights.Concat(CreateOrgUnitRights(A<Guid>(), Many<int>().ToArray())).ToList();
            user.ItProjectRights = itProjectRights.Concat(CreateProjectRights(A<Guid>(), Many<int>().ToArray())).ToList();
            user.ItContractRights = itContractRights.Concat(CreateContractRights(A<Guid>(), Many<int>().ToArray())).ToList();

            var organization = new Organization() { Id = organizationId, Uuid = orgUuid };
            var transaction = ExpectBeginTransaction();
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(organization, true);
            ExpectGetUsersInOrgReturns(orgUuid, Result<IQueryable<User>, OperationError>.Success(new[] { user }.AsQueryable()));
            ExpectRemoveOrgRoleReturns(organizationId, user.Id, organizationRole, Result<OrganizationRight, OperationFailure>.Success(new OrganizationRight()));
            ExpectSuccessfulRemovalOf(dprRights, user, _dprRightsServiceMock);
            ExpectSuccessfulRemovalOf(itSystemRights, user, _systemRightsServiceMock);
            ExpectSuccessfulRemovalOf(itProjectRights, user, _projectRightsServiceMock);
            ExpectSuccessfulRemovalOf(itContractRights, user, _contractRightsServiceMock);
            ExpectSuccessfulRemovalOf(organizationUnitRights, user, _organizationUnitRightsServiceMock);

            //Act
            var error = _sut.RemoveAllRights(user.Id, organizationId);

            //Assert
            Assert.True(error.IsNone);
            VerifyTransactionSucceeded(transaction);
            VerifyThatRemovalWasOnlyCalledForExpectedRights(user.DataProcessingRegistrationRights.ToList(), dprRights, user, _dprRightsServiceMock);
            VerifyThatRemovalWasOnlyCalledForExpectedRights(user.ItSystemRights.ToList(), itSystemRights, user, _systemRightsServiceMock);
            VerifyThatRemovalWasOnlyCalledForExpectedRights(user.ItProjectRights.ToList(), itProjectRights, user, _projectRightsServiceMock);
            VerifyThatRemovalWasOnlyCalledForExpectedRights(user.ItContractRights.ToList(), itContractRights, user, _contractRightsServiceMock);
            VerifyThatRemovalWasOnlyCalledForExpectedRights(user.OrganizationUnitRights.ToList(), organizationUnitRights, user, _organizationUnitRightsServiceMock);
        }

        [Fact]
        public void RemoveRights_Fails_If_Get_Organization_Fails()
        {
            //Arrange
            var userId = A<int>();
            var organizationId = A<int>();
            var orgUuid = A<Guid>();
            var getOrganizationError = A<OperationError>();
            var transaction = ExpectBeginTransaction();
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, getOrganizationError);

            //Act
            var error = _sut.RemoveRights(userId, organizationId, A<UserRightsChangeParameters>());

            //Assert
            Assert.True(error.HasValue);
            Assert.Same(getOrganizationError, error.Value);
            VerifyTransactionFailed(transaction);
        }

        [Fact]
        public void RemoveRights_Fails_If_No_Modification_Access_To_Organization()
        {

            //Arrange
            var userId = A<int>();
            var organizationId = A<int>();
            var orgUuid = A<Guid>();
            var organization = new Organization() { Id = A<int>() };
            var transaction = ExpectBeginTransaction();
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(organization, false);

            //Act
            var error = _sut.RemoveRights(userId, organizationId, A<UserRightsChangeParameters>());

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Forbidden, error.Value.FailureType);
            VerifyTransactionFailed(transaction);
        }

        [Fact]
        public void RemoveRights_Fails_If_Get_Users_In_Org_Fails()
        {

            //Arrange
            var userId = A<int>();
            var organizationId = A<int>();
            var orgUuid = A<Guid>();
            var organization = new Organization() { Id = A<int>(), Uuid = orgUuid };
            var transaction = ExpectBeginTransaction();
            var getUserError = A<OperationError>();
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(organization, true);
            ExpectGetUsersInOrgReturns(orgUuid, getUserError);

            //Act
            var error = _sut.RemoveRights(userId, organizationId, A<UserRightsChangeParameters>());

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(getUserError.FailureType, error.Value.FailureType);
            VerifyTransactionFailed(transaction);
        }

        [Fact]
        public void RemoveRights_Fails_If_User_Not_In_Org()
        {
            //Arrange
            var userId = A<int>();
            var organizationId = A<int>();
            var orgUuid = A<Guid>();
            var organization = new Organization() { Id = A<int>(), Uuid = orgUuid };
            var transaction = ExpectBeginTransaction();
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(organization, true);
            ExpectGetUsersInOrgReturns(orgUuid,
                Result<IQueryable<User>, OperationError>.Success(Enumerable.Empty<User>().AsQueryable()));

            //Act
            var error = _sut.RemoveRights(userId, organizationId, A<UserRightsChangeParameters>());

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
            VerifyTransactionFailed(transaction);
        }

        [Fact]
        public void RemoveRights_Fails_If_Removal_Of_A_Right_Fails()
        {
            //Arrange
            var organizationRole = GetLocalAdminRoles().RandomItem();
            var orgUuid = A<Guid>();
            var organizationId = A<int>();
            var user = CreateUserWithRole(organizationId, organizationRole, orgUuid);
            var organization = new Organization() { Id = organizationId, Uuid = orgUuid };
            var transaction = ExpectBeginTransaction();
            var removeRightError = A<OperationFailure>();
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(organization, true);
            ExpectGetUsersInOrgReturns(orgUuid,
                Result<IQueryable<User>, OperationError>.Success(new[] { user }.AsQueryable()));
            ExpectRemoveOrgRoleReturns(organizationId, user.Id, organizationRole, removeRightError);

            //Act
            var error = _sut.RemoveRights(user.Id, organizationId,
                new UserRightsChangeParameters(organizationRole.WrapAsEnumerable(), Enumerable.Empty<int>(),
                    Enumerable.Empty<int>(), Enumerable.Empty<int>(), Enumerable.Empty<int>(),
                    Enumerable.Empty<int>()));

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(removeRightError, error.Value.FailureType);
            VerifyTransactionFailed(transaction);

        }

        [Fact]
        public void RemoveRights_RemovesAllProvidedRights()
        {
            //Arrange
            var organizationRole = GetLocalAdminRoles().RandomItem();
            var orgUuid = A<Guid>();
            var organizationId = A<int>();
            var user = CreateUserWithRole(organizationId, organizationRole, orgUuid);

            var dprRights = CreateDprRights(orgUuid, Many<int>().ToArray()).ToList();
            var itSystemRights = CreateSystemRights(orgUuid, Many<int>().ToArray()).ToList();
            var organizationUnitRights = CreateOrgUnitRights(orgUuid, Many<int>().ToArray()).ToList();
            var itProjectRights = CreateProjectRights(orgUuid, Many<int>().ToArray()).ToList();
            var itContractRights = CreateContractRights(orgUuid, Many<int>().ToArray()).ToList();

            user.DataProcessingRegistrationRights = dprRights.Concat(CreateDprRights(A<Guid>(), Many<int>().ToArray())).ToList();
            user.ItSystemRights = itSystemRights.Concat(CreateSystemRights(A<Guid>(), Many<int>().ToArray())).ToList();
            user.OrganizationUnitRights = organizationUnitRights.Concat(CreateOrgUnitRights(A<Guid>(), Many<int>().ToArray())).ToList();
            user.ItProjectRights = itProjectRights.Concat(CreateProjectRights(A<Guid>(), Many<int>().ToArray())).ToList();
            user.ItContractRights = itContractRights.Concat(CreateContractRights(A<Guid>(), Many<int>().ToArray())).ToList();

            var dprRightsWhichAreRemoved = dprRights.RandomItems(2).ToList();
            var systemRightsWhichAreRemoved = itSystemRights.RandomItems(2).ToList();
            var contractRightsWhichAreRemoved = itContractRights.RandomItems(2).ToList();
            var projectRightsWhichAreRemoved = itProjectRights.RandomItems(2).ToList();
            var orgUnitRightsWhichAreRemoved = organizationUnitRights.RandomItems(2).ToList();
            var parameters = new UserRightsChangeParameters
            (
                organizationRole.WrapAsEnumerable(),
                dprRightsWhichAreRemoved.Select(x => x.Id).ToList(),
                systemRightsWhichAreRemoved.Select(x => x.Id).ToList(),
                contractRightsWhichAreRemoved.Select(x => x.Id).ToList(),
                projectRightsWhichAreRemoved.Select(x => x.Id).ToList(),
                orgUnitRightsWhichAreRemoved.Select(x => x.Id).ToList()
            );

            var organization = new Organization() { Id = organizationId, Uuid = orgUuid };
            var transaction = ExpectBeginTransaction();
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, organization);
            ExpectAllowModifyReturns(organization, true);
            ExpectGetUsersInOrgReturns(orgUuid, Result<IQueryable<User>, OperationError>.Success(new[] { user }.AsQueryable()));
            ExpectRemoveOrgRoleReturns(organizationId, user.Id, organizationRole, Result<OrganizationRight, OperationFailure>.Success(new OrganizationRight()));
            ExpectSuccessfulRemovalOf(dprRightsWhichAreRemoved, user, _dprRightsServiceMock);
            ExpectSuccessfulRemovalOf(systemRightsWhichAreRemoved, user, _systemRightsServiceMock);
            ExpectSuccessfulRemovalOf(projectRightsWhichAreRemoved, user, _projectRightsServiceMock);
            ExpectSuccessfulRemovalOf(contractRightsWhichAreRemoved, user, _contractRightsServiceMock);
            ExpectSuccessfulRemovalOf(orgUnitRightsWhichAreRemoved, user, _organizationUnitRightsServiceMock);

            //Act
            var error = _sut.RemoveRights(user.Id, organizationId, parameters);

            //Assert
            Assert.True(error.IsNone);
            VerifyTransactionSucceeded(transaction);
            VerifyThatRemovalWasOnlyCalledForExpectedRights(user.DataProcessingRegistrationRights.ToList(), dprRightsWhichAreRemoved, user, _dprRightsServiceMock);
            VerifyThatRemovalWasOnlyCalledForExpectedRights(user.ItSystemRights.ToList(), systemRightsWhichAreRemoved, user, _systemRightsServiceMock);
            VerifyThatRemovalWasOnlyCalledForExpectedRights(user.ItProjectRights.ToList(), projectRightsWhichAreRemoved, user, _projectRightsServiceMock);
            VerifyThatRemovalWasOnlyCalledForExpectedRights(user.ItContractRights.ToList(), contractRightsWhichAreRemoved, user, _contractRightsServiceMock);
            VerifyThatRemovalWasOnlyCalledForExpectedRights(user.OrganizationUnitRights.ToList(), orgUnitRightsWhichAreRemoved, user, _organizationUnitRightsServiceMock);
        }



        /*
       *
      TODO Maybe<OperationError> TransferRights(int fromUserId, int toUserId, int organizationId, UserRightsChangeParameters parameters);
       *
       */

        private static IEnumerable<OrganizationRole> GetLocalAdminRoles() => new[]
        {
            OrganizationRole.ContractModuleAdmin,
            OrganizationRole.LocalAdmin,
            OrganizationRole.OrganizationModuleAdmin,
            OrganizationRole.RightsHolderAccess,
            OrganizationRole.SystemModuleAdmin,
            OrganizationRole.ProjectModuleAdmin
        };

        private IEnumerable<OrganizationRight> CreateUserOrganizationRights(Guid organizationId,
            params OrganizationRole[] roles)
        {
            return roles.Select(role =>
                new OrganizationRight
                {
                    Id = A<int>(),
                    Organization = new Organization { Uuid = organizationId },
                    Role = role,
                }).ToList();
        }

        private IEnumerable<DataProcessingRegistrationRight> CreateDprRights(Guid organizationId, params int[] rightIds)
        {
            return rightIds.Select(id =>
                new DataProcessingRegistrationRight
                {
                    Id = id,
                    Object = new() { Organization = new() { Uuid = organizationId } },
                    RoleId = A<int>()
                }).ToList();
        }

        private IEnumerable<ItSystemRight> CreateSystemRights(Guid organizationId, params int[] rightIds)
        {
            return rightIds.Select(id =>
                new ItSystemRight
                {
                    Id = id,
                    Object = new() { Organization = new() { Uuid = organizationId } },
                    RoleId = A<int>()
                }).ToList();
        }

        private IEnumerable<ItContractRight> CreateContractRights(Guid organizationId, params int[] rightIds)
        {
            return rightIds.Select(id =>
                new ItContractRight
                {
                    Id = id,
                    Object = new() { Organization = new() { Uuid = organizationId } },
                    RoleId = A<int>()
                }).ToList();
        }

        private IEnumerable<ItProjectRight> CreateProjectRights(Guid organizationId, params int[] rightIds)
        {
            return rightIds.Select(id =>
                new ItProjectRight
                {
                    Id = id,
                    Object = new() { Organization = new() { Uuid = organizationId } },
                    RoleId = A<int>()
                }).ToList();
        }

        private IEnumerable<OrganizationUnitRight> CreateOrgUnitRights(Guid organizationId, params int[] rightIds)
        {
            return rightIds.Select(id =>
                new OrganizationUnitRight
                {
                    Id = id,
                    Object = new() { Organization = new() { Uuid = organizationId } },
                    RoleId = A<int>()
                }).ToList();
        }

        private void ExpectUserHasCrossLevelAccess(CrossOrganizationDataReadAccessLevel value)
        {
            _authServiceMock.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(value);
        }

        private void SetupUserService(IEnumerable<User> users, OrganizationRole role)
        {
            _userServiceMock
                .Setup(x => x.GetUsersWithRoleAssignedInAnyOrganization(role))
                .Returns(Result<IQueryable<User>, OperationError>.Success(users.AsQueryable()));
        }

        private void SetupOrganizationService(List<Organization> orgs)
        {
            _organizationServiceMock
                .Setup(x => x.GetAllOrganizations())
                .Returns(Result<IQueryable<Organization>, OperationError>.Success(orgs.AsQueryable()));
        }

        private User CreateUserWithRole(int orgId, OrganizationRole role = OrganizationRole.User, Guid? orgUuid = null)
        {
            return new()
            {
                Id = A<int>(),
                OrganizationRights = new List<OrganizationRight>
                {
                    new()
                    {
                        Id = A<int>(),
                        OrganizationId = orgId,
                        Organization = new Organization
                        {
                            Id = orgId,
                            Uuid = orgUuid.GetValueOrDefault(A<Guid>())
                        },
                        Role = role
                    }
                }
            };
        }

        private void ExpectReadAccessLevel(OrganizationDataReadAccessLevel accessLevel, int organizationId)
        {
            _authServiceMock.Setup(x => x.GetOrganizationReadAccessLevel(organizationId)).Returns(accessLevel);
        }

        private void ExpectResolveUuidReturns<T>(int organizationId, Maybe<Guid> value)
            where T : class, IHasUuid, IHasId
        {
            _identityResolverMock.Setup(x => x.ResolveUuid<T>(organizationId)).Returns(value);
        }

        private void ExpectGetUserInOrganization(Guid orgUuid, Guid userUuid, Result<User, OperationError> result)
        {
            _userServiceMock.Setup(x => x.GetUserInOrganization(orgUuid, userUuid)).Returns(result);
        }

        private void ExpectGetOrganizationReturns(Guid orgUuid, Result<Organization, OperationError> result)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(orgUuid, OrganizationDataReadAccessLevel.All))
                .Returns(result);
        }

        private void VerifyTransactionFailed(Mock<IDatabaseTransaction> transaction)
        {
            transaction.Verify(x => x.Commit(), Times.Never());
            transaction.Verify(x => x.Rollback(), Times.Once());
            _dbControlMock.Verify(x => x.SaveChanges(), Times.Never());
        }

        private void VerifyTransactionSucceeded(Mock<IDatabaseTransaction> transaction)
        {
            transaction.Verify(x => x.Commit(), Times.Once());
            transaction.Verify(x => x.Rollback(), Times.Never());
            _dbControlMock.Verify(x => x.SaveChanges(), Times.Once());
        }

        private void ExpectAllowModifyReturns(IEntity subject, bool allow)
        {
            _authServiceMock.Setup(x => x.AllowModify(subject)).Returns(allow);
        }

        private Mock<IDatabaseTransaction> ExpectBeginTransaction()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionMgrMock.Setup(x => x.Begin()).Returns(transaction.Object);
            return transaction;
        }

        private void ExpectGetUsersInOrgReturns(Guid orgUuid, Result<IQueryable<User>, OperationError> result)
        {
            _userServiceMock.Setup(x => x.GetUsersInOrganization(orgUuid,
                    It.Is<IDomainQuery<User>[]>(q => q.OfType<QueryById<User>>().FirstOrDefault() != null)))
                .Returns(result);
        }

        private void ExpectRemoveOrgRoleReturns(int organizationId, int userId, OrganizationRole organizationRole,
            Result<OrganizationRight, OperationFailure> result)
        {
            _orgRightsServiceMock.Setup(x => x.RemoveRole(organizationId, userId, organizationRole)).Returns(result);
        }

        private static void ExpectSuccessfulRemovalOf<TRight, TRole, TModel>(List<TRight> rights, User user,
            Mock<IRoleAssignmentService<TRight, TRole, TModel>> assignmentService)
            where TModel : HasRightsEntity<TModel, TRight, TRole>, IOwnedByOrganization
            where TRole : OptionEntity<TRight>, IRoleEntity, IOptionReference<TRight>
            where TRight : Entity, IRight<TModel, TRight, TRole>, new()
        {
            foreach (var right in rights)
            {
                assignmentService.Setup(x => x.RemoveRole(right.Object, right.RoleId, user.Id)).Returns(
                    Result<TRight, OperationError>.Success(new TRight()));
            }
        }

        private static void VerifyThatRemovalWasOnlyCalledForExpectedRights<TRight, TRole, TModel>(List<TRight> allRights,
            List<TRight> rightsExpectedToBeRemoved, User user,
            Mock<IRoleAssignmentService<TRight, TRole, TModel>> assignmentService)
            where TModel : HasRightsEntity<TModel, TRight, TRole>, IOwnedByOrganization
            where TRole : OptionEntity<TRight>, IRoleEntity, IOptionReference<TRight>
            where TRight : Entity, IRight<TModel, TRight, TRole>, new()
        {
            foreach (var right in allRights)
            {
                var times = rightsExpectedToBeRemoved.Contains(right) ? Times.Once() : Times.Never();
                assignmentService.Verify(x => x.RemoveRole(right.Object, right.RoleId, user.Id), times);
            }
        }
    }
}