using System;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices;
using Core.ApplicationServices.Model.Users.Write;
using Core.ApplicationServices.Users.Write;
using Core.DomainModel;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;
using Core.DomainModel.Organization;
using System.Collections.Generic;
using Core.Abstractions.Types;

namespace Tests.Unit.Core.ApplicationServices.Users
{
    public class UserWriteServiceTest : WithAutoFixture
    {
        private readonly UserWriteService _sut;

        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IEntityIdentityResolver> _entityIdentityResolverMock;
        private readonly Mock<IOrganizationRightsService> _organizationRightsServiceMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;

        public UserWriteServiceTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _entityIdentityResolverMock = new Mock<IEntityIdentityResolver>();
            _organizationRightsServiceMock = new Mock<IOrganizationRightsService>();
            _transactionManagerMock = new Mock<ITransactionManager>();

            _sut = new UserWriteService(_userServiceMock.Object,
                _entityIdentityResolverMock.Object,
                _organizationRightsServiceMock.Object,
                _transactionManagerMock.Object);
        }

        [Fact]
        public void Can_Create_User()
        {
            //Arrange
            var createParams = SetupUserParameters();
            var orgUuid = A<Guid>();
            var orgId = A<int>();
            var transaction = ExpectTransactionBegins();

            AssertResolveIdReturns(orgUuid, orgId);
            AssertAddUserReturns(createParams.User, createParams.SendMailOnCreation, orgId);
            foreach (var organizationRole in createParams.Roles)
            {
                AssertAddRoleReturns(organizationRole, orgId, createParams.User.Id, Result<OrganizationRight, OperationFailure>.Success(It.IsAny<OrganizationRight>()));
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
            var transaction = ExpectTransactionBegins();
            var error = A<OperationFailure>();

            AssertResolveIdReturns(orgUuid, orgId);
            AssertAddUserReturns(createParams.User, createParams.SendMailOnCreation, orgId);
            foreach (var organizationRole in createParams.Roles)
            {
                AssertAddRoleReturns(organizationRole, orgId, createParams.User.Id, error);
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
        public void Create_Fails_If_OrgId_Is_Not_Found()
        {
            //Arrange
            var createParams = SetupUserParameters();
            var orgUuid = A<Guid>();

            AssertResolveIdReturns(orgUuid, Maybe<int>.None);

            //Act
            var result = _sut.Create(orgUuid, createParams);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

//[Fact]
        public void Can_Send_Notification()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var orgId = A<int>();
            var userUuid = A<Guid>();
            var user = SetupUser();
            user.Uuid = userUuid;
            AssertResolveIdReturns(orgUuid, Maybe<int>.Some(orgId));
            ExpectGetUserInOrganizationReturns(orgUuid, userUuid, user);
            //Act
            var result = _sut.SendNotification(orgUuid, userUuid);

            _userServiceMock.Verify(x => x.IssueAdvisMail(user, false, orgId), Times.Once);
            Assert.True(result.IsNone);
        }

        private void ExpectGetUserInOrganizationReturns(Guid organizationUuid, Guid userUuid, Result<User, OperationError> result)
        {
            _userServiceMock.Setup(x => x.GetUserInOrganization(organizationUuid, userUuid)).Returns(result);
        }

        private void AssertAddUserReturns(User user, bool sendMailOnCreation, int orgId)
        {
            _userServiceMock.Setup(x => x.AddUser(user, sendMailOnCreation, orgId)).Returns(user);
        }

        private void AssertResolveIdReturns(Guid uuid, Maybe<int> idResult)
        {
            _entityIdentityResolverMock.Setup(x => x.ResolveDbId<Organization>(uuid)).Returns(idResult);
        }

        private void AssertAddRoleReturns(OrganizationRole role, int organizationId, int userId, Result<OrganizationRight, OperationFailure> result)
        {
            _organizationRightsServiceMock.Setup(x => x.AssignRole(organizationId, userId, role)).Returns(result);
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