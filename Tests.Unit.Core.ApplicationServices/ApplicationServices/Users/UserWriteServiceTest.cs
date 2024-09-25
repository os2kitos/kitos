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

        public UserWriteServiceTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _organizationRightsServiceMock = new Mock<IOrganizationRightsService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _organizationServiceMock = new Mock<IOrganizationService>();

            _sut = new UserWriteService(_userServiceMock.Object, 
                _organizationRightsServiceMock.Object, 
                _transactionManagerMock.Object,
                _authorizationContextMock.Object,
                _organizationServiceMock.Object);
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
            Assert.True(result.Error.Message.Value.Contains("Failed to assign role"));
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
            var initialUser = SetupUser();
            var orgUuid = A<Guid>();
            _sut.Create(in)
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
