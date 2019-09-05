using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainServices.Authorization;
using Moq;
using Presentation.Web.Infrastructure.Authorization.Controller;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public class LegacyAuthorizationStrategyTest : WithAutoFixture
    {
        private readonly int _userId;
        private readonly LegacyAuthorizationStrategy _sut;
        private readonly Mock<IAuthenticationService> _authenticationService;

        public LegacyAuthorizationStrategyTest()
        {
            _userId = A<int>();
            _authenticationService = new Mock<IAuthenticationService>();
            _sut = new LegacyAuthorizationStrategy(_authenticationService.Object, () => _userId);
        }

        [Theory]
        [InlineData(true, false, CrossOrganizationDataReadAccessLevel.All)]
        [InlineData(false, true, CrossOrganizationDataReadAccessLevel.Public)]
        [InlineData(false, false, CrossOrganizationDataReadAccessLevel.None)]
        public void GetCrossOrganizationReadAccess_Returns(bool isGlobalAdmin, bool hasReadAccessOutsideOfContext, CrossOrganizationDataReadAccessLevel expectedResult)
        {
            //Arrange
            _authenticationService.Setup(x => x.IsGlobalAdmin(_userId)).Returns(isGlobalAdmin);
            ExpectHasReadAccessOutsideOfContextReturns(hasReadAccessOutsideOfContext);

            //Act
            var readAccess = _sut.GetCrossOrganizationReadAccess();

            //Assert
            Assert.Equal(expectedResult, readAccess);
        }

        [Theory]
        [InlineData(true, false, OrganizationDataReadAccessLevel.All)]
        [InlineData(false, true, OrganizationDataReadAccessLevel.All)]
        [InlineData(false, false, OrganizationDataReadAccessLevel.None)]
        public void GetOrganizationReadAccessLevel_Returns(bool sameOrg, bool readAccessOutsideOfContext, OrganizationDataReadAccessLevel expectedAccessLevel)
        {
            //Arrange
            var organizationId = A<int>();
            _authenticationService.Setup(x => x.GetCurrentOrganizationId(_userId)).Returns(sameOrg ? organizationId : A<int>());
            ExpectHasReadAccessOutsideOfContextReturns(readAccessOutsideOfContext);

            //Act
            var readAccess = _sut.GetOrganizationReadAccessLevel(organizationId);

            //Assert
            Assert.Equal(expectedAccessLevel, readAccess);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AllowReadAccess_Returns_Result_From_AuthenticationService(bool expectedResult)
        {
            //Arrange
            var entity = Mock.Of<IEntity>();
            _authenticationService.Setup(x => x.HasReadAccess(_userId, entity)).Returns(expectedResult);

            //Act
            var result = _sut.AllowRead(entity);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AllowWriteAccess_Returns_Result_From_AuthenticationService(bool expectedResult)
        {
            //Arrange
            var entity = Mock.Of<IEntity>();
            _authenticationService.Setup(x => x.HasWriteAccess(_userId, entity)).Returns(expectedResult);

            //Act
            var result = _sut.AllowModify(entity);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AllowEntityVisibilityControl_Returns_Result_From_AuthenticationService_FeatureCheck(bool expectedResult)
        {
            //Arrange
            var entity = Mock.Of<IEntity>();
            _authenticationService.Setup(x => x.CanExecute(_userId, Feature.CanSetAccessModifierToPublic)).Returns(expectedResult);

            //Act
            var result = _sut.AllowEntityVisibilityControl(entity);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        private void ExpectHasReadAccessOutsideOfContextReturns(bool hasReadAccessOutsideOfContext)
        {
            _authenticationService.Setup(x => x.HasReadAccessOutsideContext(_userId)).Returns(hasReadAccessOutsideOfContext);
        }
    }
}
