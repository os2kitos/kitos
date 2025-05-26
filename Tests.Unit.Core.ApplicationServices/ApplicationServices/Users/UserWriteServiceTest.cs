using System;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices;
using Core.ApplicationServices.Model.Users.Write;
using Core.ApplicationServices.Users.Write;
using Core.DomainModel;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;
using Core.DomainModel.Organization;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Rights;
using Core.DomainServices.Generic;
using Core.ApplicationServices.Model.Users;
using Core.DomainServices;
using Serilog;

namespace Tests.Unit.Core.ApplicationServices.Users
{
    public class UserWriteServiceTest : WithAutoFixture
    {
        private readonly UserWriteService _sut;

        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IOrganizationRightsService> _organizationRightsServiceMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IEntityIdentityResolver> _entityIdentityResolverMock;
        private readonly Mock<IUserRightsService> _userRightsServiceMock;
        private readonly Mock<IOrganizationalUserContext> _organizationalUserContextMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ILogger> _loggerMock;

        public UserWriteServiceTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _organizationRightsServiceMock = new Mock<IOrganizationRightsService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _entityIdentityResolverMock = new Mock<IEntityIdentityResolver>();
            _userRightsServiceMock = new Mock<IUserRightsService>();
            _organizationalUserContextMock = new Mock<IOrganizationalUserContext>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger>();

            _sut = new UserWriteService(_userServiceMock.Object, 
                _organizationRightsServiceMock.Object, 
                _transactionManagerMock.Object,
                _authorizationContextMock.Object,
                _organizationServiceMock.Object,
                _entityIdentityResolverMock.Object,
                _userRightsServiceMock.Object,
                _organizationalUserContextMock.Object,
                _userRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public void Can_Create_User()
        {
            //Arrange
            var createParams = SetupUserParameters();
            var orgUuid = A<Guid>();
            var orgId = A<int>();
            var org = new Organization { Id = orgId};
            var transaction = ExpectTransactionBegins();

            ExpectIsEmailInUseReturns(createParams.User.Email, false);
            ExpectHasGlobalAdminPermissionReturns(true);
            ExpectHasStakeHolderAccessReturns(true);
            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectPermissionsReturn(org, true);
            ExpectAddUserReturns(createParams.User, createParams.SendMailOnCreation, orgId);
            foreach (var organizationRole in createParams.Roles)
            {
                ExpectAddRoleReturns(organizationRole, orgId, createParams.User.Id, Result<OrganizationRight, OperationFailure>.Success(It.IsAny<OrganizationRight>()));
            }

            //Act
            var result = _sut.Create(orgUuid, createParams);

            //Assert
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Create_Fails_If_Add_Role_Fails()
        {
            //Arrange
            var createParams = SetupUserParameters();
            var orgUuid = A<Guid>();
            var orgId = A<int>();
            var org = new Organization { Id = orgId };
            var transaction = ExpectTransactionBegins();
            var error = A<OperationFailure>();

            ExpectIsEmailInUseReturns(createParams.User.Email, false);
            ExpectHasGlobalAdminPermissionReturns(true);
            ExpectHasStakeHolderAccessReturns(true);
            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectPermissionsReturn(org, true);
            ExpectAddUserReturns(createParams.User, createParams.SendMailOnCreation, orgId);
            foreach (var organizationRole in createParams.Roles)
            {
                ExpectAddRoleReturns(organizationRole, orgId, createParams.User.Id, error);
            }

            //Act
            var result = _sut.Create(orgUuid, createParams);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(error, result.Error.FailureType);
            Assert.True(result.Error.Message.HasValue);
            Assert.Contains("Failed to assign role", result.Error.Message.Value);
            transaction.Verify(x => x.Rollback(), Times.Once);
        }

        [Theory]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        public void Create_Returns_Forbidden_If_Permission_Is_Missing(bool accessPermissions, bool globalAdminPermission, bool stakeHolderPermission)
        {
            //Arrange
            var createParams = SetupUserParameters();
            var orgUuid = A<Guid>();
            var org = new Organization { Id = A<int>() };

            ExpectIsEmailInUseReturns(createParams.User.Email, false);
            ExpectHasGlobalAdminPermissionReturns(globalAdminPermission);
            ExpectHasStakeHolderAccessReturns(stakeHolderPermission);
            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectPermissionsReturn(org, accessPermissions);

            //Act
            var result = _sut.Create(orgUuid, createParams);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Create_Fails_If_GetOrganization_Fails()
        {
            //Arrange
            var createParams = SetupUserParameters();
            var orgUuid = A<Guid>();
            var error = A<OperationError>();

            ExpectIsEmailInUseReturns(createParams.User.Email, false);
            ExpectHasGlobalAdminPermissionReturns(true);
            ExpectHasStakeHolderAccessReturns(true);
            ExpectGetOrganizationReturns(orgUuid, error);

            //Act
            var result = _sut.Create(orgUuid, createParams);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(error.FailureType, result.Error.FailureType);
        }


        [Fact]
        public void Can_Send_Notification()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var orgId = A<int>();
            var userUuid = A<Guid>();
            var user = SetupUser();
            user.Uuid = userUuid;
            ExpectResolveOrgUuidReturns(orgUuid, Maybe<int>.Some(orgId));
            ExpectGetUserInOrganizationReturns(orgUuid, userUuid, user);
            //Act
            var result = _sut.SendNotification(orgUuid, userUuid);

            _userServiceMock.Verify(x => x.IssueAdvisMail(user, false, orgId), Times.Once);
            Assert.True(result.IsNone);
        }

        [Fact]
        public void Create_Fails_If_Email_Already_Exists()
        {
            //Arrange
            var createParams = SetupUserParameters();
            var orgUuid = A<Guid>();

            ExpectIsEmailInUseReturns(createParams.User.Email, true);

            //Act
            var result = _sut.Create(orgUuid, createParams);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void Can_Get_User_Permissions(bool canCreate, bool canEdit, bool canDelete)
        {
            //Arrange
            var orgUuid = A<Guid>();
            var orgId = A<int>();
            var org = new Organization { Id = orgId };

            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectCreatePermissionReturns(orgId, canCreate);
            ExpectModifyPermissionReturns(org, canEdit);
            ExpectDeletePermissionReturns(orgId, canDelete);

            //Act
            var result = _sut.GetCollectionPermissions(orgUuid);

            //Assert
            Assert.True(result.Ok);
            var permissions = result.Value;
            Assert.Equal(canCreate, permissions.Create);
            Assert.Equal(canEdit, permissions.Edit);
            Assert.Equal(canDelete, permissions.Delete);
        }

        [Fact]
        public void Can_Update_User()
        {
            //Arrange
            var user = SetupUser();
            var organization = new Organization {Id = A<int>(), Uuid = A<Guid>()};
            var defaultUnit = new OrganizationUnit {Id = A<int>(), Uuid = A<Guid>()};
            var updateParameters = A<UpdateUserParameters>();
            updateParameters.DefaultOrganizationUnitUuid = defaultUnit.Uuid.AsChangedValue();
            ExpectGetUserByUuid(user.Uuid, user);
            ExpectModifyPermissionsForUserReturns(user, true);
            ExpectGetOrganizationReturns(organization.Uuid, organization);
            ExpectResolveOrgUuidReturns(organization.Uuid, organization.Id);
            ExpectResolveOrgUnitUuidReturns(defaultUnit.Uuid, defaultUnit.Id);
            ExpectHasStakeHolderAccessReturns(true);
            ExpectAssignRolesReturn(updateParameters.Roles.NewValue, user, organization);
            ExpectRemoveRolesReturn(user.GetRolesInOrganization(organization.Uuid), user, organization);
            _organizationServiceMock.Setup(x => x.SetDefaultOrgUnit(user, organization.Id, defaultUnit.Id));

            var transaction = ExpectTransactionBegins();

            //Act
            var updatedUserResult = _sut.Update(organization.Uuid, user.Uuid, updateParameters);

            //Assert
            Assert.True(updatedUserResult.Ok);
            var updatedUser = updatedUserResult.Value;
            Assert.Equal(updateParameters.Email.NewValue, updatedUser.Email);
            Assert.Equal(updateParameters.FirstName.NewValue, updatedUser.Name);
            Assert.Equal(updateParameters.LastName.NewValue, updatedUser.LastName);
            Assert.Equal(updateParameters.PhoneNumber.NewValue, updatedUser.PhoneNumber);
            Assert.Equal(updateParameters.HasApiAccess.NewValue, updatedUser.HasApiAccess);
            Assert.Equal(updateParameters.HasStakeHolderAccess.NewValue, updatedUser.HasStakeHolderAccess);
            Assert.Equal(updateParameters.DefaultUserStartPreference.NewValue, updatedUser.DefaultUserStartPreference);
            transaction.Verify(x => x.Commit(), Times.AtLeastOnce);
        }

        [Fact]
        public void Can_Not_Update_User_If_Email_Is_Already_In_Use() 
        {
            //Arrange
            var user = SetupUser();
            var organization = new Organization { Id = A<int>(), Uuid = A<Guid>() };
            var updateParameters = A<UpdateUserParameters>();
            ExpectResolveOrgUuidReturns(organization.Uuid, A<int>());
            ExpectGetOrganizationReturns(organization.Uuid, organization);
            ExpectGetUserByUuid(user.Uuid, user);
            ExpectModifyPermissionsForUserReturns(user, true);
            ExpectHasStakeHolderAccessReturns(true);
            ExpectIsEmailInUseReturns(updateParameters.Email.NewValue, true);
            ExpectAssignRolesReturn(updateParameters.Roles.NewValue, user, organization);
            ExpectRemoveRolesReturn(user.GetRolesInOrganization(organization.Uuid), user, organization);
            ExpectTransactionBegins();

            //Act
            var updateResult = _sut.Update(organization.Uuid, user.Uuid, updateParameters);

            //Assert
            Assert.True(updateResult.Failed);

        }

        [Fact]
        public void Can_Copy_Roles()
        {
            //Arrange
            var fromUser = SetupUser();
            var toUser = SetupUser();
            var org = new Organization { Id = A<int>(), Uuid = A<Guid>() };
            var updateParameters = A<UserRightsChangeParameters>();
            ExpectGetUserInOrganizationReturns(org.Uuid, fromUser.Uuid, fromUser);
            ExpectGetUserInOrganizationReturns(org.Uuid, toUser.Uuid, toUser);
            ExpectResolveOrgUuidReturns(org.Uuid, org.Id);
            ExpectModifyPermissionsForUserReturns(toUser, true);
            _organizationServiceMock.Setup(_ => _.GetOrganization(org.Uuid, null)).Returns(org);
            _userRightsServiceMock.Setup(x => x.CopyRights(fromUser.Id, toUser.Id, org.Id, updateParameters))
                .Returns(Maybe<OperationError>.None);

            //Act
            var result = _sut.CopyUserRights(org.Uuid, fromUser.Uuid, toUser.Uuid, updateParameters);

            //Assert
            Assert.True(result.IsNone);
            _userRightsServiceMock.Verify(x => x.CopyRights(fromUser.Id, toUser.Id, org.Id, updateParameters));
        }

        [Fact]
        public void Can_Transfer_Roles()
        {
            //Arrange
            var fromUser = SetupUser();
            var toUser = SetupUser();
            var org = new Organization { Id = A<int>(), Uuid = A<Guid>() };
            var updateParameters = A<UserRightsChangeParameters>();
            ExpectGetUserInOrganizationReturns(org.Uuid, fromUser.Uuid, fromUser);
            ExpectGetUserInOrganizationReturns(org.Uuid, toUser.Uuid, toUser);
            ExpectModifyPermissionsForUserReturns(toUser, true);
            ExpectResolveOrgUuidReturns(org.Uuid, org.Id);

            //Act
            _ = _sut.TransferUserRights(org.Uuid, fromUser.Uuid, toUser.Uuid, updateParameters);

            //Assert
            _userRightsServiceMock.Verify(x => x.TransferRights(fromUser.Id, toUser.Id, org.Id, updateParameters));
        }

        [Fact]
        public void Transfer_Roles_Returns_Not_Found_If_No_Org_Id()
        {
            //Arrange
            var fromUser = SetupUser();
            var toUser = SetupUser();
            var org = new Organization { Id = A<int>(), Uuid = A<Guid>() };
            var updateParameters = A<UserRightsChangeParameters>();
            ExpectGetUserInOrganizationReturns(org.Uuid, fromUser.Uuid, fromUser);
            ExpectGetUserInOrganizationReturns(org.Uuid, toUser.Uuid, toUser);
            ExpectModifyPermissionsForUserReturns(toUser, true);
            ExpectResolveOrgUuidReturns(org.Uuid, Maybe<int>.None);

            //Act
            var result = _sut.TransferUserRights(org.Uuid, fromUser.Uuid, toUser.Uuid, updateParameters);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.NotFound, result.Value.FailureType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Delete_User(bool isScopedToOrganization)
        {
            //Arrange
            var userUuid = A<Guid>();
            var orgUuid = isScopedToOrganization ? A<Guid>() : Maybe<Guid>.None;
            int? orgId = null;

            if (isScopedToOrganization)
            {
                orgId = A<int>();
                ExpectResolveOrgUuidReturns(orgUuid.Value, orgId);
            }

            _userServiceMock.Setup(x => x.DeleteUser(userUuid, orgId)).Returns(Maybe<OperationError>.None);

            //Act
            var result = _sut.DeleteUser(userUuid, orgUuid);

            //Assert
            Assert.True(result.IsNone);
        }

        [Fact]
        public void Can_Add_Global_Admin()
        {
            var user = SetupUser();
            user.IsGlobalAdmin = false;
            ExpectGetUserByUuid(user.Uuid, user);
            ExpectIsGlobalAdminReturns(true);
            var transaction = ExpectTransactionBegins();

            var result = _sut.AddGlobalAdmin(user.Uuid);

            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.AtLeastOnce);
            Assert.True(result.Value.IsGlobalAdmin);
        }

        [Fact]
        public void Cannot_Add_Global_Admin_With_No_GlobalAdmin_Permission()
        {
            var user = SetupUser();
            ExpectGetUserByUuid(user.Uuid, user);
            ExpectIsGlobalAdminReturns(false);
            ExpectTransactionBegins();

            var result = _sut.AddGlobalAdmin(user.Uuid);

            Assert.True(result.Failed);
        }

        [Fact] 
        public void Can_Remove_Global_Admin()
        {
            var user = SetupUser();
            user.IsGlobalAdmin = true;
            ExpectGetUserByUuid(user.Uuid, user);
            ExpectIsGlobalAdminReturns(true);
            var transaction = ExpectTransactionBegins();

            var result = _sut.RemoveGlobalAdmin(user.Uuid);

            transaction.Verify(x => x.Commit(), Times.AtLeastOnce);
            Assert.True(result.IsNone);
            Assert.False(user.IsGlobalAdmin);
        }

        [Fact]
        public void Cannot_Remove_Global_Admin_With_No_GlobalAdmin_Permission()
        {
            var user = SetupUser();
            ExpectGetUserByUuid(user.Uuid, user);
            ExpectIsGlobalAdminReturns(false);
            ExpectTransactionBegins();

            var result = _sut.RemoveGlobalAdmin(user.Uuid);

            Assert.True(result.HasValue);
        }

        [Fact]
        public void Cannot_Remove_Yourself_As_Global_Admin()
        {
            var user = SetupUser();
            ExpectGetUserByUuid(user.Uuid, user);
            ExpectIsGlobalAdminReturns(true);
            ExpectTransactionBegins();
            _organizationalUserContextMock.Setup(x => x.UserId).Returns(user.Id);

            var result = _sut.RemoveGlobalAdmin(user.Uuid);

            Assert.True(result.HasValue);
        }

        [Fact]
        public void Can_Add_Local_Admin()
        {
            var user = SetupUser();
            var org = new Organization { Uuid = A<Guid>(), Id = A<int>() };
            ExpectGetUserByUuid(user.Uuid, user);
            ExpectResolveOrgUuidReturns(org.Uuid, org.Id);
            _organizationRightsServiceMock.Setup(x => x.AssignRole(org.Id, user.Id, OrganizationRole.LocalAdmin)).Returns(new OrganizationRight());
            var transaction = ExpectTransactionBegins();

            var result = _sut.AddLocalAdmin(org.Uuid, user.Uuid);

            Assert.True(result.Ok);
            _organizationRightsServiceMock.Verify(x => x.AssignRole(org.Id, user.Id, OrganizationRole.LocalAdmin), Times.Once);
            transaction.Verify(x => x.Commit(), Times.AtLeastOnce);
        }


        [Fact]
        public void Can_Remove_Local_Admin()
        {
            var user = SetupUser();
            var org = new Organization { Uuid = A<Guid>(), Id = A<int>() };
            ExpectGetUserByUuid(user.Uuid, user);
            ExpectResolveOrgUuidReturns(org.Uuid, org.Id);
            _organizationRightsServiceMock.Setup(x => x.RemoveRole(org.Id, user.Id, OrganizationRole.LocalAdmin)).Returns(new OrganizationRight());
            var transaction = ExpectTransactionBegins();

            var result = _sut.RemoveLocalAdmin(org.Uuid, user.Uuid);

            Assert.True(result.IsNone);
            _organizationRightsServiceMock.Verify(x => x.RemoveRole(org.Id, user.Id, OrganizationRole.LocalAdmin), Times.Once);
            transaction.Verify(x => x.Commit(), Times.AtLeastOnce);
        }

        [Fact]
        public void Can_Issue_Password_Reset()
        {
            var user = SetupUser();
            ExpectGetUserByEmailReturns(user.Email, user);

            _sut.RequestPasswordReset(user.Email);

            _userServiceMock.Verify(x => x.IssuePasswordReset(user, null, null), Times.Once());
        }

        [Fact]
        public void Can_Not_Issue_Password_Reset_If_Email_Doesnt_Exist()
        {
            var nonExistantMail = "test@mail.dk";
            ExpectGetUserByEmailReturns(nonExistantMail, null);

            _sut.RequestPasswordReset(nonExistantMail);

            VerifyNoPasswordResetsHasBeenIssued();
        }

        [Fact]
        public void Can_Not_Issue_Password_Reset_If_User_Cant_Authenticate()
        {
            var user = SetupUser();
            user.Deleted = true; //Make user unable to authenticate
            ExpectGetUserByEmailReturns(user.Email, user);

            _sut.RequestPasswordReset(user.Email);

            VerifyNoPasswordResetsHasBeenIssued();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_Only_Update_System_Integrator_Role_As_Global_Admin(bool isGlobalAdmin)
        {
            var user = SetupUser();
            ExpectGetUserByUuid(user.Uuid, user);
            ExpectIsGlobalAdminReturns(isGlobalAdmin);
            ExpectTransactionBegins();

            var result = _sut.UpdateSystemIntegrator(user.Uuid, true);

            if (isGlobalAdmin)
            {
                _userServiceMock.Verify(x => x.UpdateUser(user, null, null), Times.Once);
                Assert.True(result.Value.IsSystemIntegrator);
            }
            Assert.Equal(result.Ok, isGlobalAdmin);
        }

        private void VerifyNoPasswordResetsHasBeenIssued()
        {
            _userServiceMock.Verify(x => x.IssuePasswordReset(It.IsAny<User>(), null, null), Times.Never());
        }

        private void ExpectGetUserByEmailReturns(string email, User user)
        {
            _userRepositoryMock.Setup(x => x.GetByEmail(email)).Returns(user);
        }

        private void ExpectIsGlobalAdminReturns(bool expected)
        {
            _organizationalUserContextMock.Setup(x => x.IsGlobalAdmin()).Returns(expected);
        }

        private void ExpectAssignRolesReturn(IEnumerable<OrganizationRole> roles, User user, Organization org)
        {
            foreach (var role in roles)
            {
                var expectedRight = new OrganizationRight { Role = role, UserId = user.Id, Organization = org, OrganizationId = org.Id };
                _organizationRightsServiceMock.Setup(x => x.AssignRole(org.Id, user.Id, role)).Returns(expectedRight);
            }
        }

        private void ExpectRemoveRolesReturn(IEnumerable<OrganizationRole> roles, User user, Organization org)
        {
            foreach (var role in roles)
            {
                var expectedRight = new OrganizationRight { Role = role, UserId = user.Id, OrganizationId = org.Id };
                _organizationRightsServiceMock.Setup(x => x.RemoveRole(org.Id, user.Id, role)).Returns(expectedRight);
            }
        }

        private void ExpectResolveOrgUuidReturns(Guid orgUuid, Maybe<int> result)
        {
            _entityIdentityResolverMock.Setup(x => x.ResolveDbId<Organization>(orgUuid)).Returns(result);
        }
        private void ExpectResolveOrgUnitUuidReturns(Guid unitUuid, Maybe<int> result)
        {
            _entityIdentityResolverMock.Setup(x => x.ResolveDbId<OrganizationUnit>(unitUuid)).Returns(result);
        }

        private void ExpectGetUserInOrganizationReturns(Guid organizationUuid, Guid userUuid, Result<User, OperationError> result)
        {
            _userServiceMock.Setup(x => x.GetUserInOrganization(organizationUuid, userUuid)).Returns(result);
        }

        private void ExpectGetUserByUuid(Guid userUuid, Result<User, OperationError> result)
        {
            _userServiceMock.Setup(x => x.GetUserByUuid(userUuid)).Returns(result);
        }

        private void ExpectModifyPermissionsForUserReturns(User user, bool result)
        {
            _authorizationContextMock.Setup(x => x.AllowModify(user)).Returns(result);
        }

        private void ExpectAddUserReturns(User user, bool sendMailOnCreation, int orgId)
        {
            _userServiceMock.Setup(x => x.AddUser(user, sendMailOnCreation, orgId)).Returns(user);
        }
        
        private void ExpectAddRoleReturns(OrganizationRole role, int organizationId, int userId, Result<OrganizationRight, OperationFailure> result)
        {
            _organizationRightsServiceMock.Setup(x => x.AssignRole(organizationId, userId, role)).Returns(result);
        }

        private void ExpectGetOrganizationReturns(Guid organizationUuid, Result<Organization, OperationError> result)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(organizationUuid, null)).Returns(result);
        }

        private void ExpectCreatePermissionReturns(int organizationId, bool result)
        {
            _authorizationContextMock.Setup(x => x.AllowCreate<User>(organizationId)).Returns(result);
        }

        private void ExpectModifyPermissionReturns(Organization organization, bool result)
        {
            _authorizationContextMock.Setup(x => x.AllowModify(organization)).Returns(result);
        }

        private void ExpectDeletePermissionReturns(int organizationId, bool result)
        {
            _authorizationContextMock.Setup(x => x.HasPermission(It.Is<DeleteAnyUserPermission>(parameter => parameter.OptionalOrganizationScopeId.Value == organizationId))).Returns(result);
        }

        private void ExpectPermissionsReturn(Organization organization, bool result)
        {
            ExpectCreatePermissionReturns(organization.Id, result);
            ExpectModifyPermissionReturns(organization, result);
            ExpectDeletePermissionReturns(organization.Id, result);
        }

        private void ExpectIsEmailInUseReturns(string email, bool result)
        {
            _userServiceMock.Setup(x => x.IsEmailInUse(email)).Returns(result);
        }

        private void ExpectHasGlobalAdminPermissionReturns(bool result)
        {
            _authorizationContextMock.Setup(x =>
                x.HasPermission(
                    It.Is<AdministerGlobalPermission>(perm => perm.Permission == GlobalPermission.GlobalAdmin)))
                .Returns(result);
        }

        private void ExpectHasStakeHolderAccessReturns(bool result)
        {
            _authorizationContextMock.Setup(x =>
                x.HasPermission(
                    It.Is<AdministerGlobalPermission>(perm => perm.Permission == GlobalPermission.StakeHolderAccess)))
                .Returns(result);
        }

        private Mock<IDatabaseTransaction> ExpectTransactionBegins()
        {
            var transactionMock = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transactionMock.Object);
            return transactionMock;
        }

        private CreateUserParameters SetupUserParameters()
        {
            return new CreateUserParameters
            {
                Roles = A<IEnumerable<OrganizationRole>>(),
                SendMailOnCreation = A<bool>(),
                User = SetupUser()
            };
        }

        private User SetupUser()
        {
            return new User
            {
                Id = A<int>(),
                Uuid = A<Guid>(),
                Email = A<string>(),
                Name = A<string>(),
                LastName = A<string>(),
                PhoneNumber = A<string>(),
                DefaultUserStartPreference = "index",
                HasApiAccess = A<bool>(),
                HasStakeHolderAccess = true,
                IsGlobalAdmin = true
            };
        }
    }
}
