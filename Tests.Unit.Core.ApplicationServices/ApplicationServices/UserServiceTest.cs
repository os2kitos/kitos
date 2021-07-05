using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Infrastructure.Services.Cryptography;
using Infrastructure.Services.DomainEvents;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Organizations;
using Core.DomainServices.Queries;
using Tests.Toolkit.Patterns;
using Xunit;

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
        private readonly Mock<IOrganizationService> _organizationServiceMock;

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
                _organizationServiceMock.Object);
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

        [Theory]
        [InlineData(OrganizationDataReadAccessLevel.None)]
        [InlineData(OrganizationDataReadAccessLevel.Public)]
        [InlineData(OrganizationDataReadAccessLevel.RightsHolder)]
        public void GetUsersInOrganization_Returns_Forbidden_If_Anything_But_Full_Access_In_Organization(OrganizationDataReadAccessLevel accessLevel)
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var organization = new Organization { Id = A<int>(), Uuid = organizationUuid };
            var queryMock = new Mock<IDomainQuery<User>>();
            ExpectGetOrganizationReturns(organizationUuid, organization);
            ExpectGetOrganizationAccessReturns(organization.Id, accessLevel);

            //Act
            var result = _sut.GetUsersInOrganization(organizationUuid, queryMock.Object);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
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
            var expectedUser = new User(){Uuid = userUuid};
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
            var allUsers = new[] { new User(),  new User() }.AsQueryable();
            ExpectGetOrganizationReturns(organizationUuid, organization);
            ExpectGetOrganizationAccessReturns(organization.Id, OrganizationDataReadAccessLevel.All);
            _repositoryMock.Setup(x => x.GetUsersInOrganization(organization.Id)).Returns(allUsers);

            //Act
            var result = _sut.GetUserInOrganization(organizationUuid, userUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        private void ExpectGetOrganizationAccessReturns(int organizationId, OrganizationDataReadAccessLevel organizationDataReadAccessLevel)
        {
            _authorizationContextMock.Setup(x => x.GetOrganizationReadAccessLevel(organizationId))
                .Returns(organizationDataReadAccessLevel);
        }

        private void ExpectGetOrganizationReturns(Guid organizationId, Result<Organization, OperationError> organization)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(organizationId,OrganizationDataReadAccessLevel.All)).Returns(organization);
        }
    }
}
