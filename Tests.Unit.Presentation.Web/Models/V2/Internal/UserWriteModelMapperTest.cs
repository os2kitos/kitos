using System.Linq;
using Moq;
using Presentation.Web.Controllers.API.V2.Internal.Users.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.User;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2.Internal
{
    public class UserWriteModelMapperTest : WithAutoFixture
    {
        private readonly UserWriteModelMapper _sut;

        public UserWriteModelMapperTest()
        {
            var httpRequest = new Mock<ICurrentHttpRequest>();
            _sut = new UserWriteModelMapper(httpRequest.Object);
        }

        [Fact]
        public void Can_Map_Create_Request_To_Create_Parameters()
        {
            //Arrange
            var request = A<CreateUserRequestDTO>();

            //Act
            var parameters = _sut.FromPOST(request);

            //Assert
            Assert.Equal(request.Email, parameters.User.Email);
            Assert.Equal(request.FirstName, parameters.User.Name);
            Assert.Equal(request.LastName, parameters.User.LastName);
            Assert.Equal(request.PhoneNumber, parameters.User.PhoneNumber);
            Assert.Equal(request.HasApiAccess, parameters.User.HasApiAccess);
            Assert.Equal(request.HasStakeHolderAccess, parameters.User.HasStakeHolderAccess);
            Assert.Equal(request.DefaultUserStartPreference,
                DefaultUserStartPreferenceChoiceMapper.GetDefaultUserStartPreferenceChoice(parameters.User
                    .DefaultUserStartPreference));
            Assert.Equal(request.SendMailOnCreation, parameters.SendMailOnCreation);
            Assert.Equal(request.Roles, parameters.Roles.Select(x => x.ToOrganizationRoleChoice()));
        }
    }
}
