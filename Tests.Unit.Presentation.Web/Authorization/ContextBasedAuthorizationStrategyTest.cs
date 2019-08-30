using Core.DomainModel;
using Moq;
using Presentation.Web.Infrastructure.Authorization.Context;
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

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void ApplyBaseQueryPostProcessing_Returns_True_If_Global_Read_Access_Is_False(bool allowGlobalRead, bool expectedResult)
        {
            _authContext.Setup(x => x.AllowGlobalReadAccess()).Returns(allowGlobalRead);

            Assert.Equal(expectedResult, _sut.ApplyBaseQueryPostProcessing);
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
