using System;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Rights;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Authorization;
using Core.DomainServices.Generic;
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
        private readonly Mock<IRoleAssignmentService<ItContractRight, ItContractRole, ItContract>> _contractRightsServiceMock;
        private readonly Mock<IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage>> _systemRightsServiceMock;
        private readonly Mock<IRoleAssignmentService<ItProjectRight, ItProjectRole, ItProject>> _projectRightsServiceMock;
        private readonly Mock<IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit>> _organizationUnitRightsServiceMock;
        private readonly Mock<IRoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, DataProcessingRegistration>> _dprRightsServiceMock;
        private readonly Mock<ITransactionManager> _transactionMgrMock;
        private readonly Mock<IDatabaseControl> _dbControlMock;
        private readonly Mock<IEntityIdentityResolver> _identityResolverMock;

        public UserRightsServiceTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _authServiceMock = new Mock<IAuthorizationContext>();
            _orgRightsServiceMock = new Mock<IOrganizationRightsService>();
            _contractRightsServiceMock = new Mock<IRoleAssignmentService<ItContractRight, ItContractRole, ItContract>>();
            _systemRightsServiceMock = new Mock<IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage>>();
            _projectRightsServiceMock = new Mock<IRoleAssignmentService<ItProjectRight, ItProjectRole, ItProject>>();
            _organizationUnitRightsServiceMock = new Mock<IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit>>();
            _dprRightsServiceMock = new Mock<IRoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, DataProcessingRegistration>>();
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

        [Theory]
        [InlineData(CrossOrganizationDataReadAccessLevel.None)]
        [InlineData(CrossOrganizationDataReadAccessLevel.RightsHolder)]
        [InlineData(CrossOrganizationDataReadAccessLevel.Public)]
        public void GetUsersWithRoleAssignment_Returns_Forbidden_If_User_Not_Full_Cross_Level_Access(CrossOrganizationDataReadAccessLevel crossOrganizationDataReadAccess)
        {
            //Arrange
            ExpectUserHasCrossLevelAccess(crossOrganizationDataReadAccess);

            //Act
            var result = _sut.GetUsersWithRoleAssignment(A<OrganizationRole>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
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
        public void GetUserRights_Returns_Error_If_Read_Access_Level_Too_Low(OrganizationDataReadAccessLevel accessLevel)
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
            Assert.Equal(Enumerable.Empty<DataProcessingRegistrationRight>(), userRightsAssignments.DataProcessingRegistrationRights);
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
            ExpectReadAccessLevel(OrganizationDataReadAccessLevel.All, organizationId);
            ExpectResolveUuidReturns<Organization>(organizationId, orgUuid);
            ExpectResolveUuidReturns<User>(userId, userUuid);
            ExpectGetUserInOrganization(orgUuid, userUuid, user);
            var userRolesInOrg = EnumRange.All<OrganizationRole>().ToArray();


            user.OrganizationRights = CreateUserOrganizationRights(orgUuid, userRolesInOrg)
                .Concat(CreateUserOrganizationRights(A<Guid>(), OrganizationRole.LocalAdmin))
                .ToList();


            //Act
            var result = _sut.GetUserRights(userId, organizationId);

            //Assert
            Assert.True(result.Ok);
            var userRightsAssignments = result.Value;
            var expectedRoles = userRolesInOrg.Intersect(GetLocalAdminRoles()).OrderBy(x => x).ToList();//only the local admin roles are expected - not user (non admin) and global admin (global role)
            Assert.Equal(expectedRoles, userRightsAssignments.LocalAdministrativeAccessRoles.OrderBy(x => x).ToList()); 
            Assert.Equal(Enumerable.Empty<OrganizationUnitRight>(), userRightsAssignments.OrganizationUnitRights);
            Assert.Equal(Enumerable.Empty<ItContractRight>(), userRightsAssignments.ContractRights);
            Assert.Equal(Enumerable.Empty<ItProjectRight>(), userRightsAssignments.ProjectRights);
            Assert.Equal(Enumerable.Empty<ItSystemRight>(), userRightsAssignments.SystemRights);
            Assert.Equal(Enumerable.Empty<DataProcessingRegistrationRight>(), userRightsAssignments.DataProcessingRegistrationRights);
        }
        
        [Fact]
        public void RemoveAllRights_Fails_If_Get_Organization_Fails()
        {

            throw new NotImplementedException();
        }

        [Fact]
        public void RemoveAllRights_Fails_If_No_Modification_Access_To_Organization()
        {

            throw new NotImplementedException();
        }

        [Fact]
        public void RemoveAllRights_Fails_If_Get_Users_In_Org_Fails()
        {

            throw new NotImplementedException();
        }

        [Fact]
        public void RemoveAllRights_Fails_If_User_Not_In_Org()
        {
            throw new NotImplementedException();

        }

        [Fact]
        public void RemoveAllRights_Fails_If_Removal_Of_A_Right_Fails()
        {
            throw new NotImplementedException();

        }

        [Fact]
        public void RemoveAllRights_RemovesAllRightsInOrganization()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void RemoveRights_Fails_If_Get_Organization_Fails()
        {

            throw new NotImplementedException();
        }

        [Fact]
        public void RemoveRights_Fails_If_No_Modification_Access_To_Organization()
        {

            throw new NotImplementedException();
        }

        [Fact]
        public void RemoveRights_Fails_If_Get_Users_In_Org_Fails()
        {

            throw new NotImplementedException();
        }

        [Fact]
        public void RemoveRights_Fails_If_User_Not_In_Org()
        {
            throw new NotImplementedException();

        }

        [Fact]
        public void RemoveRights_Fails_If_Removal_Of_A_Right_Fails()
        {
            throw new NotImplementedException();

        }

        [Fact]
        public void RemoveRights_RemovesAllRightsInOrganization()
        {
            throw new NotImplementedException();
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

        private static IEnumerable<OrganizationRight> CreateUserOrganizationRights(Guid organizationId, params OrganizationRole[] roles)
        {
            return roles.Select(role =>
                new OrganizationRight
                {
                    Organization = new Organization { Uuid = organizationId},
                    Role = role,
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

        private User CreateUserWithRole(int orgId, OrganizationRole role)
        {
            return new()
            {
                Id = A<int>(),
                OrganizationRights = new List<OrganizationRight>()
                {
                    new()
                    {
                        OrganizationId = orgId,
                        Role = role
                    }
                }
            };
        }
        private void ExpectReadAccessLevel(OrganizationDataReadAccessLevel accessLevel, int organizationId)
        {
            _authServiceMock.Setup(x => x.GetOrganizationReadAccessLevel(organizationId)).Returns(accessLevel);
        }

        private void ExpectResolveUuidReturns<T>(int organizationId, Maybe<Guid> value) where T : class, IHasUuid, IHasId
        {
            _identityResolverMock.Setup(x => x.ResolveUuid<T>(organizationId)).Returns(value);
        }

        private void ExpectGetUserInOrganization(Guid orgUuid, Guid userUuid, Result<User, OperationError> result)
        {
            _userServiceMock.Setup(x => x.GetUserInOrganization(orgUuid, userUuid)).Returns(result);
        }
    }
}
