using Core.DomainModel;
using Core.DomainModel.ItSystem;
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

        [Theory]
        [InlineData(true, false, false, true)]
        [InlineData(false, true, false, true)]
        [InlineData(false, false, true, true)]
        [InlineData(false, false, false, false)]
        public void AllowReadsWithinOrganization_Returns(bool isGlobalAdmin, bool isActiveInOrganization, bool isMunicipality, bool expectedResult)
        {
            //Arrange
            var targetOrganization = A<int>();
            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectIsActiveInOrganizationReturns(targetOrganization, isActiveInOrganization);
            ExpectIsActiveInOrganizationOfTypeReturns(OrganizationCategory.Municipality, isMunicipality);

            //Act
            var hasAccess = _sut.AllowReadsWithinOrganization(targetOrganization);

            //Assert
            Assert.Equal(expectedResult, hasAccess);
        }

        [Theory]
        [InlineData(true, false, false, false, AccessModifier.Local, true)]
        [InlineData(false, true, false, false, AccessModifier.Local, true)]
        [InlineData(false, false, true, false, AccessModifier.Local, true)]
        [InlineData(false, false, false, true, AccessModifier.Public, true)]
        [InlineData(false, false, false, true, AccessModifier.Local, false)]
        [InlineData(false, false, false, false, AccessModifier.Public, false)]
        public void AllowReads_For_Context_Dependent_Object_Returns(bool isGlobalAdmin, bool inputIsActiveUser, bool isInSameOrg, bool isUserActiveInMunicipality, AccessModifier accessModifier, bool expectedResult)
        {
            //Arrange
            var activeUser = CreateTestUser();
            var entity = inputIsActiveUser ? (IEntity)activeUser : CreateTestItSystem(accessModifier);

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectGetUserReturns(activeUser);
            ExpectIsActiveInSameOrganizationAsReturns((IContextAware)entity, isInSameOrg);
            ExpectIsActiveInOrganizationOfTypeReturns(OrganizationCategory.Municipality, isUserActiveInMunicipality);

            //Act
            var result = _sut.AllowReads(entity);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void AllowReads_For_Context_Independent_Object_Returns(bool isGlobalAdmin, bool inputIsActiveUser, bool expectedResult)
        {
            //Arrange
            var activeUser = CreateTestUser();
            var inputEntity = inputIsActiveUser ? activeUser : CreateTestUser();

            ExpectHasRoleReturns(OrganizationRole.GlobalAdmin, isGlobalAdmin);
            ExpectGetUserReturns(activeUser);

            //Act
            var result = _sut.AllowReads(inputEntity);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        private static ItSystem CreateTestItSystem(AccessModifier accessModifier)
        {
            return new ItSystem { AccessModifier = accessModifier };
        }

        private void ExpectIsActiveInSameOrganizationAsReturns(IContextAware entity, bool value)
        {
            _userContextMock.Setup(x => x.IsActiveInSameOrganizationAs(entity)).Returns(value);
        }

        private void ExpectGetUserReturns(User entity)
        {
            _userContextMock.Setup(x => x.User).Returns(entity);
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

        private User CreateTestUser()
        {
            return new User
            {
                Id = A<int>()
            };
        }
    }
}
