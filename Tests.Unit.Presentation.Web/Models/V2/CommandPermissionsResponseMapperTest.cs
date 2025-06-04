using Core.ApplicationServices.Authorization;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class CommandPermissionsResponseMapperTest : WithAutoFixture
    {
        private readonly CommandPermissionsResponseMapper _sut;

        public CommandPermissionsResponseMapperTest()
        {
            _sut = new CommandPermissionsResponseMapper();
        }

        [Fact]
        public void Can_Map()
        {
            //Arrange
            var permission = A<CommandPermissionResult>();

            //Act
            var result = _sut.MapCommandPermission(permission);

            //Assert
            Assert.Equal(permission.Id, result.Id);
            Assert.Equal(permission.CanExecute, result.CanExecute);
        }
    }
}
