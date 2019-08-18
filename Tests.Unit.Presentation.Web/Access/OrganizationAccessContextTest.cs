using Core.DomainModel.Organization;
using Moq;
using Presentation.Web.Access;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Access
{
    public class OrganizationAccessContextTest : WithAutoFixture
    {
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly OrganizationAccessContext _sut;

        public OrganizationAccessContextTest()
        {
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _sut = new OrganizationAccessContext(_userContextMock.Object);
        }

        [Fact]
        public void AllowReadsWithinOrganization_Returns_True_For_GlobalAdmins()
        {
            //Arrange
            var targetOrganization = A<int>();
            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, true);

            //Act
            var hasAccess = _sut.AllowReadsWithinOrganization(targetOrganization);

            //Assert
            Assert.True(hasAccess);
        }

        [Fact]
        public void AllowReadsWithinOrganization_Returns_True_If_Organization_Equals_ActiveOrganization()
        {
            //Arrange
            var targetOrganization = A<int>();
            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, false);
            ExpectIsActiveInOrganizationReturns(targetOrganization, true);

            //Act
            var hasAccess = _sut.AllowReadsWithinOrganization(targetOrganization);

            //Assert
            Assert.True(hasAccess);
        }

        [Fact]
        public void AllowReadsWithinOrganization_Returns_True_If_Active_User_Organization_Equals_Municipality()
        {
            //Arrange
            var targetOrganization = A<int>();
            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, false);
            ExpectIsActiveInOrganizationReturns(targetOrganization, false);
            ExpectIsActiveInOrganizationOfTypeReturns(OrganizationCategory.Municipality, false);

            //Act
            var hasAccess = _sut.AllowReadsWithinOrganization(targetOrganization);

            //Assert
            Assert.False(hasAccess);
        }

        [Fact]
        public void AllowReadsWithinOrganization_Returns_False()
        {
            //Arrange
            var targetOrganization = A<int>();
            ExpectIsActiveInOrganizationOfTypeReturns(OrganizationCategory.Municipality, true);

            //Act
            var hasAccess = _sut.AllowReadsWithinOrganization(targetOrganization);

            //Assert
            Assert.True(hasAccess);
        }

        private void ExpectIsActiveInOrganizationOfTypeReturns(OrganizationCategory organizationCategory, bool value)
        {
            _userContextMock.Setup(x => x.IsActiveInOrganizationOfType(organizationCategory)).Returns(value);
        }

        private void ExpectIsActiveInOrganizationReturns(int targetOrganization, bool value)
        {
            _userContextMock.Setup(x => x.IsActiveInOrganization(targetOrganization)).Returns(value);
        }

        private void ExpectHasRoleReturns(OrganizationRole role, bool value)
        {
            _userContextMock.Setup(x => x.HasRole(role)).Returns(value);
        }
    }
}
