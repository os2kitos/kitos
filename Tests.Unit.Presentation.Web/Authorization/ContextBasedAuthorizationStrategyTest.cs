using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainServices.Authorization;
using Moq;
using Presentation.Web.Infrastructure.Authorization.Controller;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public class ContextBasedAuthorizationStrategyTest : WithAutoFixture
    {
        private readonly Mock<IAuthorizationContext> _authContext;
        private readonly ContextBasedAuthorizationStrategy _sut;

        public ContextBasedAuthorizationStrategyTest()
        {
            _authContext = new Mock<IAuthorizationContext>();
            _sut = new ContextBasedAuthorizationStrategy(_authContext.Object);
        }

        [Fact]
        public void GetCrossOrganizationReadAccess_Returns_Result_From_Context()
        {
            //Arrange
            var expectedResult = A<CrossOrganizationReadAccess>();
            _authContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(expectedResult);

            //Act
            var result = _sut.GetCrossOrganizationReadAccess();

            //Assert
            Assert.Equal(expectedResult, result);
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AllowOrganizationAccess_Returns_Response_From_Context(bool expectedResult)
        {
            //Arrange
            var organizationId = A<int>();
            _authContext.Setup(x => x.AllowReadsWithinOrganization(organizationId)).Returns(expectedResult);

            //Act
            var result = _sut.AllowOrganizationReadAccess(organizationId);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AllowReadAccess_Returns_Response_From_Context(bool expectedResult)
        {
            //Arrange
            var entity = Mock.Of<IEntity>();
            _authContext.Setup(x => x.AllowReads(entity)).Returns(expectedResult);

            //Act
            var result = _sut.AllowRead(entity);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AllowWriteAccess_Returns_Response_From_Context(bool expectedResult)
        {
            //Arrange
            var entity = Mock.Of<IEntity>();
            _authContext.Setup(x => x.AllowModify(entity)).Returns(expectedResult);

            //Act
            var result = _sut.AllowModify(entity);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AllowEntityVisibilityControl_Returns_Response_From_Context(bool expectedResult)
        {
            //Arrange
            var entity = Mock.Of<IEntity>();
            _authContext.Setup(x => x.AllowEntityVisibilityControl(entity)).Returns(expectedResult);

            //Act
            var result = _sut.AllowEntityVisibilityControl(entity);

            //Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
