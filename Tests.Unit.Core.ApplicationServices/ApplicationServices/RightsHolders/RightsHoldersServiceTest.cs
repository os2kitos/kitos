using System;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.RightsHolders;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.RightsHolders
{
    public class RightsHoldersServiceTest : WithAutoFixture
    {
        private readonly RightsHoldersService _sut;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly Mock<IGenericRepository<Organization>> _organizationRepositoryMock;
        private readonly Mock<IItSystemService> _itSystemServiceMock;

        public RightsHoldersServiceTest()
        {
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _organizationRepositoryMock = new Mock<IGenericRepository<Organization>>();
            _itSystemServiceMock = new Mock<IItSystemService>();
            _sut = new RightsHoldersService(_userContextMock.Object, _organizationRepositoryMock.Object, _itSystemServiceMock.Object);
        }

        [Fact]
        public void ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess_Returns_Only_Organizations_Where_User_Has_RightsHolderAccessRole()
        {
            //Arrange
            Organization expectedOrg1 = new() { Id = A<int>() };
            Organization expectedOrg2 = new() { Id = expectedOrg1.Id + 1 };
            Organization noMatchOrg = new() { Id = expectedOrg2.Id + 1 };

            _userContextMock.Setup(x => x.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess))
                .Returns(new[] { expectedOrg1.Id, expectedOrg2.Id });
            _organizationRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(new EnumerableQuery<Organization>(new[] { noMatchOrg, expectedOrg1, expectedOrg2 }));

            //Act
            var organizations = _sut.ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess().ToList();

            //Assert
            Assert.Equal(2, organizations.Count);
            Assert.Same(expectedOrg1, organizations.First());
            Assert.Same(expectedOrg2, organizations.Last());
        }

        [Fact]
        public void GetAvailableSystems_Returns_Forbidden_If_User_Does_Not_Have_RightsHoldersAccess()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(false);

            //Act
            var result = _sut.GetSystemsWhereAuthenticatedUserHasRightsHolderAccess();

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_GetAvailableSystems_If_User_Has_RightsHoldersAccess()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(true);
            var expectedResponse = Mock.Of<IQueryable<ItSystem>>();
            _itSystemServiceMock.Setup(x => x.GetAvailableSystems()).Returns(expectedResponse);

            //Act
            var result = _sut.GetSystemsWhereAuthenticatedUserHasRightsHolderAccess();

            //Assert
            Assert.True(result.Ok);
            Assert.Same(expectedResponse, result.Value);
        }

        [Fact]
        public void GetSystem_Returns_Forbidden_If_User_Does_Not_Have_RightsHoldersAccess()
        {
            //Arrange
            ExpectUserHasRightsHolderAccessReturns(false);

            //Act
            var result = _sut.GetSystemAsRightsHolder(A<Guid>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_GetSystem_If_User_Has_RightsHoldersAccess()
        {
            //Arrange
            var systemUuid = A<Guid>();
            ExpectUserHasRightsHolderAccessReturns(true);
            var itSystem = new ItSystem();

            _itSystemServiceMock.Setup(x => x.GetSystem(systemUuid)).Returns(itSystem);

            //Act
            var result = _sut.GetSystemAsRightsHolder(systemUuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itSystem,result.Value);
        }

        private void ExpectUserHasRightsHolderAccessReturns(bool value)
        {
            _userContextMock.Setup(x => x.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess)).Returns(value);
        }
    }
}
