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
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class UserRightsServiceTest : WithAutoFixture
    {
        private readonly UserRightsService _sut;

        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;

        public UserRightsServiceTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _sut = new UserRightsService(_userServiceMock.Object, _organizationServiceMock.Object, _userContextMock.Object);
        }

        [Fact]
        public void GetUsersAndOrganizationsWhereUserHasRightsholderAccess_Returns_Users_And_Organization_Tuples()
        {
            //Arrange
            var orgId1 = A<int>();
            var orgId2 = A<int>();
            var user1 = CreateUserWithRighsholderAccess(orgId1);
            var user2 = CreateUserWithRighsholderAccess(orgId2);
            var users = new List<User>() { user1, user2 };

            var org1 = new Organization() { Id = orgId1 };
            var org2 = new Organization() { Id = orgId2 };

            _userContextMock.Setup(x => x.IsGlobalAdmin()).Returns(true);

            SetupUserService(users);
            SetupOrganizationService(new List<int>() { orgId1 }, new List<Organization>() { org1 });
            SetupOrganizationService(new List<int>() { orgId2 }, new List<Organization>() { org2 });

            //Act
            var result = _sut.GetUsersAndOrganizationsWhereUserHasRightsholderAccess();

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(2, result.Value.Count());

            Assert.Same(user1, result.Value.First(x => x.Item1.Id == user1.Id).Item1);
            Assert.Same(org1, result.Value.First(x => x.Item1.Id == user1.Id).Item2);

            Assert.Same(user2, result.Value.First(x => x.Item1.Id == user2.Id).Item1);
            Assert.Same(org2, result.Value.First(x => x.Item1.Id == user2.Id).Item2);
        }

        [Fact]
        public void GetUsersAndOrganizationsWhereUserHasRightsholderAccess_Returns_Tuples_If_User_Is_Rightsholder_In_Multiple_Orgs()
        {
            //Arrange
            var orgId1 = A<int>();
            var orgId2 = A<int>();
            var user = CreateUserWithRighsholderAccess(orgId1);
            user.OrganizationRights.Add(new OrganizationRight() { OrganizationId = orgId2, Role = OrganizationRole.RightsHolderAccess });

            var users = new List<User>() { user };
            var org1 = new Organization() { Id = orgId1 };
            var org2 = new Organization() { Id = orgId2 };

            _userContextMock.Setup(x => x.IsGlobalAdmin()).Returns(true);

            SetupUserService(users);
            SetupOrganizationService(new List<int>() { orgId1, orgId2 }, new List<Organization>() { org1, org2 });

            //Act
            var result = _sut.GetUsersAndOrganizationsWhereUserHasRightsholderAccess();

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(2, result.Value.Count());

            Assert.Same(user, result.Value.First(x => x.Item2.Id == orgId1).Item1);
            Assert.Same(org1, result.Value.First(x => x.Item2.Id == orgId1).Item2);

            Assert.Same(user, result.Value.First(x => x.Item2.Id == orgId2).Item1);
            Assert.Same(org2, result.Value.First(x => x.Item2.Id == orgId2).Item2);
        }

        [Fact]
        public void GetUsersAndOrganizationsWhereUserHasRightsholderAccess_Returns_Forbidden_If_User_Not_GlobalAdmin()
        {
            //Arrange
            _userContextMock.Setup(x => x.IsGlobalAdmin()).Returns(false);

            //Act
            var result = _sut.GetUsersAndOrganizationsWhereUserHasRightsholderAccess();

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void GetUsersAndOrganizationsWhereUserHasRightsholderAccess_Returns_Error_If_GetUsersWithRightsHolderAccess_Fails()
        {
            //Arrange
            var operationError = A<OperationError>();

            _userContextMock.Setup(x => x.IsGlobalAdmin()).Returns(true);
            _userServiceMock
                .Setup(x => x.GetUsersWithRightsHolderAccess())
                .Returns(Result<IQueryable<User>, OperationError>.Failure(operationError));

            //Act
            var result = _sut.GetUsersAndOrganizationsWhereUserHasRightsholderAccess();

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        [Fact]
        public void GetUsersAndOrganizationsWhereUserHasRightsholderAccess_Returns_Error_If_GetOrganizations_Fails()
        {
            //Arrange
            var orgId = A<int>();
            var user = CreateUserWithRighsholderAccess(orgId);
            var users = new List<User>() { user };

            var orgIds = new List<int>() { orgId };

            var operationError = A<OperationError>();

            _userContextMock.Setup(x => x.IsGlobalAdmin()).Returns(true);

            SetupUserService(users); 
            
            _organizationServiceMock
                .Setup(x => x.GetOrganizations(orgIds))
                .Returns(Result<IQueryable<Organization>, OperationError>.Failure(operationError));

            //Act
            var result = _sut.GetUsersAndOrganizationsWhereUserHasRightsholderAccess();

            //Assert
            Assert.True(result.Failed);
            Assert.Same(operationError, result.Error);
        }

        private void SetupUserService(IEnumerable<User> users)
        {
            _userServiceMock
                .Setup(x => x.GetUsersWithRightsHolderAccess())
                .Returns(Result<IQueryable<User>, OperationError>.Success(users.AsQueryable()));
        }

        private void SetupOrganizationService(List<int> orgIds, List<Organization> orgs)
        {
            _organizationServiceMock
                            .Setup(x => x.GetOrganizations(orgIds))
                            .Returns(Result<IQueryable<Organization>, OperationError>.Success(orgs.AsQueryable()));
        }

        private User CreateUserWithRighsholderAccess(int orgId)
        {
            return new User()
            {
                Id = A<int>(),
                OrganizationRights = new List<OrganizationRight>()
                {
                    new OrganizationRight()
                    {
                        OrganizationId = orgId,
                        Role = OrganizationRole.RightsHolderAccess
                    }
                }
            };
        }
    }
}
