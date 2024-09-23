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
            var org = new Organization { Id = A<int>() };
            var transaction = ExpectTransactionBegins();

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
            var org = new Organization { Id = A<int>() };
            var transaction = ExpectTransactionBegins();
            var error = A<OperationFailure>();

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

        [Fact]
        public void Create_Fails_If_No_Create_Permission()
        {
            //Arrange
            var createParams = SetupUserParameters();
            var orgUuid = A<Guid>();
            var org = new Organization { Id = A<int>() };

            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectPermissionsReturn(org, false);

            //Act
            var result = _sut.Create(orgUuid, createParams);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
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
                HasStakeHolderAccess = A<bool>(),
            };
        }
    }
}
