using Core.DomainModel;
using Moq;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Infrastructure.Authorization.Controller.General;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public class RootEntityCrudAuthorizationTest : WithAutoFixture
    {
        private readonly Mock<IControllerAuthorizationStrategy> _authorizationStrategy;
        private readonly RootEntityCrudAuthorization _sut;

        public RootEntityCrudAuthorizationTest()
        {
            _authorizationStrategy = new Mock<IControllerAuthorizationStrategy>();
            _sut = new RootEntityCrudAuthorization(_authorizationStrategy.Object);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Read_Delegates_To_Strategy(bool strategyResponse)
        {
            //Arrange
            var input = Mock.Of<IEntity>();
            _authorizationStrategy.Setup(x => x.AllowRead(input)).Returns(strategyResponse);

            //Act
            var allowRead = _sut.AllowRead(input);

            //Assert
            Assert.Equal(strategyResponse, allowRead);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Modify_Delegates_To_Strategy(bool strategyResponse)
        {
            //Arrange
            var input = Mock.Of<IEntity>();
            _authorizationStrategy.Setup(x => x.AllowModify(input)).Returns(strategyResponse);

            //Act
            var allowRead = _sut.AllowModify(input);

            //Assert
            Assert.Equal(strategyResponse, allowRead);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Delete_Delegates_To_Strategy(bool strategyResponse)
        {
            //Arrange
            var input = Mock.Of<IEntity>();
            _authorizationStrategy.Setup(x => x.AllowDelete(input)).Returns(strategyResponse);

            //Act
            var allowRead = _sut.AllowDelete(input);

            //Assert
            Assert.Equal(strategyResponse, allowRead);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Create_Delegates_To_Strategy(bool strategyResponse)
        {
            //Arrange
            var input = Mock.Of<IEntity>();
            var organizationId = A<int>();
            _authorizationStrategy.Setup(x => x.AllowCreate<IEntity>(organizationId, input)).Returns(strategyResponse);

            //Act
            var allowRead = _sut.AllowCreate<IEntity>(organizationId, input);

            //Assert
            Assert.Equal(strategyResponse, allowRead);
        }
    }
}
