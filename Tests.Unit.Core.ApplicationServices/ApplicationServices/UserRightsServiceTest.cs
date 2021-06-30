using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Rights;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Core.DomainServices.Authorization;
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

        public UserRightsServiceTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _authServiceMock = new Mock<IAuthorizationContext>();
            _sut = new UserRightsService(_userServiceMock.Object,_organizationServiceMock.Object, _authServiceMock.Object);
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

            SetupUserService(users,role);
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

            SetupUserService(users,role);
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

            SetupUserService(users,role); 
            
            _organizationServiceMock
                .Setup(x => x.GetAllOrganizations())
                .Returns(Result<IQueryable<Organization>, OperationError>.Failure(operationError));

            //Act
            var result = _sut.GetUsersWithRoleAssignment(role);

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        private void ExpectUserHasCrossLevelAccess(CrossOrganizationDataReadAccessLevel value)
        {
            _authServiceMock.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(value);
        }

        private void SetupUserService(IEnumerable<User> users,OrganizationRole role)
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
    }
}
