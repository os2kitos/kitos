using System;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Organization;
using Moq;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class OrganizationServiceTest : WithAutoFixture
    {
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly OrganizationService _sut;

        public OrganizationServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _sut = new OrganizationService(null, null, _authorizationContext.Object);
        }

        [Fact]
        public void CanCreateOrganizationOfType_With_Null_Org_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.CanCreateOrganizationOfType(null, A<OrganizationTypeKeys>()));
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        public void CanCreateOrganizationOfType_Returns(bool allowModify, bool allowChangeOrgType, bool expectedResult)
        {
            //Arrange
            var organization = new Organization();
            var organizationTypeKeys = A<OrganizationTypeKeys>();
            _authorizationContext.Setup(x => x.AllowModify(organization)).Returns(allowModify);
            _authorizationContext.Setup(x => x.AllowChangeOrganizationType(organizationTypeKeys)).Returns(allowChangeOrgType);

            //Act
            var result = _sut.CanCreateOrganizationOfType(organization, organizationTypeKeys);

            //Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
