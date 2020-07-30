using System.ComponentModel;
using Core.DomainModel;
using Moq;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public interface IRoot : IEntity { }
    public class ChildEntityCrudAuthorizationTest : WithAutoFixture
    {
        private readonly IEntity _inputEntity;
        private readonly IRoot _rootEntity;
        private readonly ChildEntityCrudAuthorization<IEntity, IRoot> _sut;
        private readonly Mock<IControllerCrudAuthorization> _nextAuth;

        public ChildEntityCrudAuthorizationTest()
        {
            _inputEntity = Mock.Of<IEntity>();
            _rootEntity = Mock.Of<IRoot>();
            _nextAuth = new Mock<IControllerCrudAuthorization>();
            _sut = new ChildEntityCrudAuthorization<IEntity, IRoot>((input) =>
            {
                Assert.Same(_inputEntity, input); //Make sure the expected input is passed
                return _rootEntity;
            }, _nextAuth.Object);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Read_Delegates_To_AllowRead_On_Root(bool strategyResponse)
        {
            //Arrange
            _nextAuth.Setup(x => x.AllowRead(_rootEntity)).Returns(strategyResponse);

            //Act
            var allowRead = _sut.AllowRead(_inputEntity);

            //Assert
            Assert.Equal(strategyResponse, allowRead);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Modify_Delegates_To_AllowModify_On_Root(bool strategyResponse)
        {
            //Arrange
            _nextAuth.Setup(x => x.AllowModify(_rootEntity)).Returns(strategyResponse);

            //Act
            var allowRead = _sut.AllowModify(_inputEntity);

            //Assert
            Assert.Equal(strategyResponse, allowRead);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Delete_Delegates_To_AllowModify_On_Root(bool strategyResponse)
        {
            //Arrange
            _nextAuth.Setup(x => x.AllowModify(_rootEntity)).Returns(strategyResponse);

            //Act
            var allowRead = _sut.AllowDelete(_inputEntity);

            //Assert
            Assert.Equal(strategyResponse, allowRead);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Allow_Create_Delegates_To_AllowModify_On_Root(bool strategyResponse)
        {
            //Arrange
            _nextAuth.Setup(x => x.AllowModify(_rootEntity)).Returns(strategyResponse);

            //Act
            var allowRead = _sut.AllowCreate<IEntity>(A<int>(), _inputEntity);

            //Assert
            Assert.Equal(strategyResponse, allowRead);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [Description("Make sure calling it with the root still works and methods still translate to the right auth")]
        public void Allow_Read_Called_With_Root_Arg_Delegates_To_AllowRead_On_Root(bool strategyResponse)
        {
            //Arrange
            _nextAuth.Setup(x => x.AllowRead(_rootEntity)).Returns(strategyResponse);

            //Act
            var allowRead = _sut.AllowRead(_rootEntity);

            //Assert
            Assert.Equal(strategyResponse, allowRead);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [Description("Make sure calling it with the root still works and methods still translate to the right auth")]
        public void Allow_Read_Called_With_Root_Arg_Modify_Delegates_To_AllowModify_On_Root(bool strategyResponse)
        {
            //Arrange
            _nextAuth.Setup(x => x.AllowModify(_rootEntity)).Returns(strategyResponse);

            //Act
            var allowRead = _sut.AllowModify(_rootEntity);

            //Assert
            Assert.Equal(strategyResponse, allowRead);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [Description("Make sure calling it with the root still works and methods still translate to the right auth")]
        public void Allow_Read_Called_With_Root_Arg_Delete_Delegates_To_AllowModify_On_Root(bool strategyResponse)
        {
            //Arrange
            _nextAuth.Setup(x => x.AllowModify(_rootEntity)).Returns(strategyResponse);

            //Act
            var allowRead = _sut.AllowDelete(_rootEntity);

            //Assert
            Assert.Equal(strategyResponse, allowRead);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [Description("Make sure calling it with the root still works and methods still translate to the right auth")]
        public void Allow_Read_Called_With_Root_Arg_Create_Delegates_To_AllowModify_On_Root(bool strategyResponse)
        {
            //Arrange
            _nextAuth.Setup(x => x.AllowModify(_rootEntity)).Returns(strategyResponse);

            //Act
            var allowRead = _sut.AllowCreate<IEntity>(A<int>(), _rootEntity);

            //Assert
            Assert.Equal(strategyResponse, allowRead);
        }
    }
}
