﻿using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Infrastructure.Services.Cryptography;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using Core.Abstractions.Types;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Rights;
using Core.DomainModel.Events;
using Core.DomainModel.Organization.DomainEvents;
using Core.DomainServices.Queries;
using Infrastructure.Services.DataAccess;
using Tests.Toolkit.Patterns;
using Xunit;
using Core.DomainModel.Users;
using Core.DomainServices.Extensions;

namespace Tests.Unit.Core.ApplicationServices
{
    public class UserServiceTest : WithAutoFixture
    {
        private readonly UserService _sut;

        private readonly Mock<IUserRepository> _repositoryMock;
        private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
        private readonly Mock<IGenericRepository<Organization>> _orgRepositoryMock;
        private readonly Mock<IGenericRepository<PasswordResetRequest>> _passwordResetRequestRepositoryMock;
        private readonly Mock<IMailClient> _mailClientMock;
        private readonly Mock<ICryptoService> _cryptoServiceMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IOrganizationalUserContext> _organizationalUserContextMock;
        private readonly Mock<IUserRightsService> _userRightsService;

        public UserServiceTest()
        {
            _repositoryMock = new Mock<IUserRepository>();
            _userRepositoryMock = new Mock<IGenericRepository<User>>();
            _orgRepositoryMock = new Mock<IGenericRepository<Organization>>();
            _passwordResetRequestRepositoryMock = new Mock<IGenericRepository<PasswordResetRequest>>();
            _mailClientMock = new Mock<IMailClient>();
            _cryptoServiceMock = new Mock<ICryptoService>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _organizationalUserContextMock = new Mock<IOrganizationalUserContext>();

            _organizationServiceMock = new Mock<IOrganizationService>();
            _sut = new UserService(
                TimeSpan.Zero,
                "",
                "",
                "TestPassword",
                true,
                _userRepositoryMock.Object,
                _orgRepositoryMock.Object,
                _passwordResetRequestRepositoryMock.Object,
                _mailClientMock.Object,
                _cryptoServiceMock.Object,
                _authorizationContextMock.Object,
                _domainEventsMock.Object,
                _repositoryMock.Object,
                _organizationServiceMock.Object,
                _transactionManagerMock.Object,
                _organizationalUserContextMock.Object);
        }

        [Fact]
        public void GetUsersWithCrossOrganizationPermissions_Returns_Users()
        {
            //Arrange
            var expectedUser1 = new User() { Id = A<int>(), HasStakeHolderAccess = true };
            var expectedUser2 = new User() { Id = A<int>(), HasApiAccess = true };
            var expectedUser3 = new User() { Id = A<int>(), HasStakeHolderAccess = true, HasApiAccess = true };
            _repositoryMock.Setup(x => x.GetUsersWithCrossOrganizationPermissions()).Returns(new List<User>() { expectedUser1, expectedUser2, expectedUser3 }.AsQueryable());
            _authorizationContextMock.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(CrossOrganizationDataReadAccessLevel.All);

            //Act
            var result = _sut.GetUsersWithCrossOrganizationPermissions();

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(3, result.Value.Count());
            Assert.Same(expectedUser1, result.Value.First(x => x.Id == expectedUser1.Id));
            Assert.Same(expectedUser2, result.Value.First(x => x.Id == expectedUser2.Id));
            Assert.Same(expectedUser3, result.Value.First(x => x.Id == expectedUser3.Id));
        }

        [Theory]
        [InlineData(CrossOrganizationDataReadAccessLevel.None)]
        [InlineData(CrossOrganizationDataReadAccessLevel.Public)]
        [InlineData(CrossOrganizationDataReadAccessLevel.RightsHolder)]
        public void GetUsersWithCrossOrganizationPermissions_Returns_Forbidden_If_Not_CrossOrganizationDataReadAccessLevel_All(CrossOrganizationDataReadAccessLevel accessLevel)
        {
            //Arrange
            _repositoryMock.Setup(x => x.GetUsers()).Returns(new List<User>().AsQueryable());
            _authorizationContextMock.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(accessLevel);

            //Act
            var result = _sut.GetUsersWithCrossOrganizationPermissions();

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void GetUsersWithRoleAssignedInAnyOrganization_Returns_Users()
        {
            //Arrange
            var role = A<OrganizationRole>();
            var expectedUser = new User()
            {
                Id = A<int>(),
                OrganizationRights = new List<OrganizationRight>(){
                    new(){ Role = role }
                }
            };
            _repositoryMock.Setup(x => x.GetUsersWithRoleAssignment(role)).Returns(new List<User>() { expectedUser }.AsQueryable());
            _authorizationContextMock.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(CrossOrganizationDataReadAccessLevel.All);

            //Act
            var result = _sut.GetUsersWithRoleAssignedInAnyOrganization(role);

            //Assert
            Assert.True(result.Ok);
            var userResult = Assert.Single(result.Value);
            Assert.Same(expectedUser, userResult);
        }

        [Theory]
        [InlineData(CrossOrganizationDataReadAccessLevel.None)]
        [InlineData(CrossOrganizationDataReadAccessLevel.Public)]
        [InlineData(CrossOrganizationDataReadAccessLevel.RightsHolder)]
        public void GetUsersWithRoleAssignedInAnyOrganization_Returns_Forbidden_If_Not_CrossOrganizationDataReadAccessLevel_All(CrossOrganizationDataReadAccessLevel accessLevel)
        {
            //Arrange
            _repositoryMock.Setup(x => x.GetUsers()).Returns(new List<User>().AsQueryable());
            _authorizationContextMock.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(accessLevel);

            //Act
            var result = _sut.GetUsersWithRoleAssignedInAnyOrganization(A<OrganizationRole>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void GetUsersInOrganization_Returns_Ok()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var organization = new Organization { Id = A<int>(), Uuid = organizationUuid };
            var allUsers = new[] { new User(), new User(), new User() }.AsQueryable();
            var filteredUsers = allUsers.Skip(1);
            var queryMock = new Mock<IDomainQuery<User>>();
            ExpectGetOrganizationReturns(organizationUuid, organization);
            ExpectGetOrganizationAccessReturns(organization.Id, OrganizationDataReadAccessLevel.All);
            _repositoryMock.Setup(x => x.GetUsersInOrganization(organization.Id)).Returns(allUsers);
            queryMock.Setup(x => x.Apply(allUsers)).Returns(filteredUsers);

            //Act
            var result = _sut.GetUsersInOrganization(organizationUuid, queryMock.Object);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(filteredUsers, result.Value);
        }

        [Fact]
        public void GetUsersInOrganization_Returns_Error_From_OrganizationService()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var queryMock = new Mock<IDomainQuery<User>>();
            var operationError = A<OperationError>();
            ExpectGetOrganizationReturns(organizationUuid, operationError);

            //Act
            var result = _sut.GetUsersInOrganization(organizationUuid, queryMock.Object);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        [Fact]
        public void GetUserInOrganization_Returns_Ok()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var userUuid = A<Guid>();
            var organization = new Organization { Id = A<int>(), Uuid = organizationUuid };
            var expectedUser = new User() { Uuid = userUuid };
            var allUsers = new[] { new User(), expectedUser, new User() }.AsQueryable();
            ExpectGetOrganizationReturns(organizationUuid, organization);
            ExpectGetOrganizationAccessReturns(organization.Id, OrganizationDataReadAccessLevel.All);
            _repositoryMock.Setup(x => x.GetUsersInOrganization(organization.Id)).Returns(allUsers);

            //Act
            var result = _sut.GetUserInOrganization(organizationUuid, userUuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(expectedUser, result.Value);
        }

        [Fact]
        public void GetUserInOrganization_Returns_Error_From_OrganizationService()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var userUuid = A<Guid>();
            var operationError = A<OperationError>();
            ExpectGetOrganizationReturns(organizationUuid, operationError);

            //Act
            var result = _sut.GetUserInOrganization(organizationUuid, userUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        [Fact]
        public void GetUserInOrganization_Returns_NotFound()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var userUuid = A<Guid>();
            var organization = new Organization { Id = A<int>(), Uuid = organizationUuid };
            var allUsers = new[] { new User(), new User() }.AsQueryable();
            ExpectGetOrganizationReturns(organizationUuid, organization);
            ExpectGetOrganizationAccessReturns(organization.Id, OrganizationDataReadAccessLevel.All);
            _repositoryMock.Setup(x => x.GetUsersInOrganization(organization.Id)).Returns(allUsers);

            //Act
            var result = _sut.GetUserInOrganization(organizationUuid, userUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }


        [Fact]
        public void DeleteUser_Raises_EntityBeingDeletedEvent_And_AccessRights_Changed()
        {
            //Arrange
            var userId = A<int>();
            var userUuid = A<Guid>();
            var isDeleteAllowed = true;

            var transaction = ExpectTransactionBeginReturns();
            var user = ExpectUserRepositoryByUuidReturns(userId, userUuid);
            ExpectAuthorizationAllowDeleteReturns(user, isDeleteAllowed);
            ExpectIsGlobalReturns(true);

            //Act
            var result = _sut.DeleteUser(userUuid);

            //Assert
            Assert.True(result.IsNone);
            _domainEventsMock.Verify(x => x.Raise(It.Is<EntityBeingDeletedEvent<User>>(deleteEvent => deleteEvent.Entity.Id == user.Id)), Times.Once);
            _domainEventsMock.Verify(x => x.Raise(It.Is<AdministrativeAccessRightsChanged>(changedEvent => changedEvent.UserId == user.Id)), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, false, true, false)]
        [InlineData(OrganizationRole.LocalAdmin, false, true, false)]
        [InlineData(OrganizationRole.GlobalAdmin, false, false, false)]
        [InlineData(OrganizationRole.LocalAdmin, false, false, false)]
        public void DeleteUser_Raises_EntityBeingDeletedEvent(
            OrganizationRole adminType,
            bool isUserForDeletionGlobalAdmin,
            bool isInSameOrganization,
            bool hasManyOrganizations)
        {
            //Arrange
            var userId = A<int>();
            var userUuid = A<Guid>();
            var organizationId = A<int>();
            var user = ExpectUserRepositoryByUuidReturns(userId, userUuid);
            var organization = ExpectOrgRepositoryByUuidReturns(organizationId);
            user.IsGlobalAdmin = isUserForDeletionGlobalAdmin;
            user.OrganizationRights = new List<OrganizationRight> {new() {OrganizationId = organizationId}};

            if (hasManyOrganizations)
            {
                AddNewOrganizationToUser(user, A<int>());
            }

            var transaction = ExpectTransactionBeginReturns();
            ExpectAuthorizationAllowModifyReturns(organization, true);
            ExpectHasRoleInSameOrganizationAsReturns(user, isInSameOrganization);
            ExpectIsGlobalReturns(adminType == OrganizationRole.GlobalAdmin);
            ExpectHasRoleReturns(organizationId, OrganizationRole.LocalAdmin, adminType == OrganizationRole.LocalAdmin);

            //Act
            var result = _sut.DeleteUser(user.Uuid, organizationId);

            //Assert
            Assert.True(result.IsNone);
            _domainEventsMock.Verify(x => x.Raise(It.Is<EntityBeingDeletedEvent<User>>(deleteEvent => deleteEvent.Entity.Id == user.Id)), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, false, true, true)]
        [InlineData(OrganizationRole.LocalAdmin, false, true, true)]
        [InlineData(OrganizationRole.GlobalAdmin, true, true, false)]
        [InlineData(OrganizationRole.LocalAdmin, true, true, false)]
        public void DeleteUser_Raises_EntityBeingRemovedFromOrganizationEvent(
            OrganizationRole adminType,
            bool isUserForDeletionGlobalAdmin,
            bool isInSameOrganization,
            bool hasManyOrganizations)
        {
            //Arrange
            var userId = A<int>();
            var userUuid = A<Guid>();
            var organizationId = A<int>();
            var user = ExpectUserRepositoryByUuidReturns(userId, userUuid);
            user.IsGlobalAdmin = isUserForDeletionGlobalAdmin;
            user.OrganizationRights = new List<OrganizationRight> {new() {OrganizationId = organizationId}};

            if (hasManyOrganizations)
            {
                AddNewOrganizationToUser(user, A<int>());
            }

            ExpectHasRoleInSameOrganizationAsReturns(user, isInSameOrganization);
            ExpectIsGlobalReturns(adminType == OrganizationRole.GlobalAdmin);
            ExpectHasRoleReturns(organizationId, OrganizationRole.LocalAdmin, adminType == OrganizationRole.LocalAdmin);

            //Act
            var result = _sut.DeleteUser(user.Uuid, organizationId);

            //Assert
            Assert.True(result.IsNone);
            _domainEventsMock.Verify(x => x.Raise(It.Is<UserBeingRemovedFromOrganizationEvent<User>>(deleteEvent => deleteEvent.Entity.Id == user.Id)), Times.Once);
        }

        [Fact]
        public void DeleteUser_Returns_BadInput()
        {
            //Arrange
            var userId = A<int>();
            var userUuid = A<Guid>();
            var organizationId1 = A<int>();
            var organizationId2 = A<int>();
            var user = ExpectUserRepositoryByUuidReturns(userId, userUuid);

            user.OrganizationRights = new List<OrganizationRight>{new(){OrganizationId = organizationId2 } };

            ExpectIsGlobalReturns(true);

            //Act
            var result = _sut.DeleteUser(user.Uuid, organizationId1);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
        }

        [Fact]
        public void DeleteUser_Returns_BadState()
        {
            //Arrange
            var userId = A<int>();
            var userUuid = A<Guid>();
            var organizationId = A<int>();
            var user = ExpectUserRepositoryByUuidReturns(userId, userUuid);
            user.Deleted = true;

            ExpectIsGlobalReturns(true);

            //Act
            var result = _sut.DeleteUser(user.Uuid, organizationId);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.BadState, result.Value.FailureType);
        }

        [Fact]
        public void DeleteUser_Returns_Forbidden()
        {
            //Arrange
            var userId = A<int>();
            var userUuid = A<Guid>();
            var organizationId = A<int>();
            var user = ExpectUserRepositoryByUuidReturns(userId, userUuid);
            user.OrganizationRights = new List<OrganizationRight> { new() { OrganizationId = organizationId } };
            
            ExpectHasRoleInSameOrganizationAsReturns(user, true);
            ExpectIsGlobalReturns(false);
            ExpectHasRoleReturns(organizationId, OrganizationRole.LocalAdmin, false);

            //Act
            var result = _sut.DeleteUser(user.Uuid, organizationId);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Forbidden,result.Value);
        }

        private void ExpectGetOrganizationAccessReturns(int organizationId, OrganizationDataReadAccessLevel organizationDataReadAccessLevel)
        {
            _authorizationContextMock.Setup(x => x.GetOrganizationReadAccessLevel(organizationId))
                .Returns(organizationDataReadAccessLevel);
        }

        private void ExpectGetOrganizationReturns(Guid organizationId, Result<Organization, OperationError> organization)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(organizationId, OrganizationDataReadAccessLevel.All)).Returns(organization);
        }

        private Mock<IDatabaseTransaction> ExpectTransactionBeginReturns()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transaction.Object);
            return transaction;
        }

        private User ExpectUserRepositoryByUuidReturns(int userId, Guid uuid)
        {
            var user = new User()
            {
                Id = userId,
                Uuid = uuid
            };
            _userRepositoryMock.Setup(x => x.AsQueryable()).Returns(new List<User> { user }.AsQueryable);
            return user;
        }

        private void ExpectAuthorizationAllowDeleteReturns(IEntity entity, bool result)
        {
            _authorizationContextMock.Setup(x => x.AllowDelete(entity)).Returns(result);
        }

        private void ExpectAuthorizationAllowModifyReturns(IEntity entity, bool result)
        {
            _authorizationContextMock.Setup(x => x.AllowModify(entity)).Returns(result);
        }

        private Organization ExpectOrgRepositoryByUuidReturns(int organizationId)
        {
            var organization = new Organization
            {
                Id = organizationId
            };
            _orgRepositoryMock.Setup(x => x.AsQueryable()).Returns(new List<Organization>{organization}.AsQueryable);
            return organization;
        }

        private void ExpectHasRoleInSameOrganizationAsReturns(IEntity entity, bool value)
        {
            _organizationalUserContextMock.Setup(x => x.HasRoleInSameOrganizationAs(entity)).Returns(value);
        }

        private void ExpectIsGlobalReturns(bool value)
        {
            _organizationalUserContextMock.Setup(x => x.IsGlobalAdmin()).Returns(value);
        }

        private void ExpectHasRoleReturns(int organizationId, OrganizationRole role, bool value)
        {
            _organizationalUserContextMock.Setup(x => x.HasRole(organizationId, role)).Returns(value);
        }

        private static void AddNewOrganizationToUser(User user, int organizationId)
        {
            user.OrganizationRights.Add(new OrganizationRight() { OrganizationId = organizationId });
        }
    }
}
