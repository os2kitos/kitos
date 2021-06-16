using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.RightsHolders;
using Core.DomainModel.Organization;
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
        private readonly Mock<IItInterfaceService> _interfaceService;

        public RightsHoldersServiceTest()
        {
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _organizationRepositoryMock = new Mock<IGenericRepository<Organization>>();
            _interfaceService = new Mock<IItInterfaceService>();
            _sut = new RightsHoldersService(_userContextMock.Object, _organizationRepositoryMock.Object, _interfaceService.Object);
        }

        [Fact]
        public void ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess_Returns_Only_Organizations_Where_User_Has_RightsHolderAccessRole()
        {
            //Arrange
            Organization expectedOrg1 = new() { Id = A<int>() };
            Organization expectedOrg2 = new() {Id = expectedOrg1.Id + 1};
            Organization noMatchOrg = new() {Id = expectedOrg2.Id + 1};

            _userContextMock.Setup(x => x.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess))
                .Returns(new[] {expectedOrg1.Id, expectedOrg2.Id});
            _organizationRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(new EnumerableQuery<Organization>(new[] {noMatchOrg, expectedOrg1, expectedOrg2}));

            //Act
            var organizations = _sut.ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess().ToList();

            //Assert
            Assert.Equal(2, organizations.Count);
            Assert.Same(expectedOrg1, organizations.First());
            Assert.Same(expectedOrg2, organizations.Last());
        }
    }
}
