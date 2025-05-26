using System.Linq;
using Moq;
using Presentation.Web.Controllers.API.V2.Internal.Users.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.User;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2.Internal
{
    public class UserWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly UserWriteModelMapper _sut;
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;

        public UserWriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<UpdateUserRequestDTO>());
            _sut = new UserWriteModelMapper(_currentHttpRequestMock.Object);
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
            Assert.Equal(request.SendMail, parameters.SendMailOnCreation);
            Assert.Equal(request.Roles, parameters.Roles.Select(x => x.ToOrganizationRoleChoice()));
        }

        [Fact]
        public void Can_Map_Update_Request_To_Update_Parameters()
        {
            var request = A<UpdateUserRequestDTO>();

            var parameters = _sut.FromPATCH(request);

            Assert.Equal(request.Email, AssertPropertyContainsDataChange(parameters.Email));
            Assert.Equal(request.FirstName, AssertPropertyContainsDataChange(parameters.FirstName));
            Assert.Equal(request.LastName, AssertPropertyContainsDataChange(parameters.LastName));
            Assert.Equal(request.PhoneNumber, AssertPropertyContainsDataChange(parameters.PhoneNumber));
            Assert.Equal(request.DefaultUserStartPreference, DefaultUserStartPreferenceChoiceMapper.GetDefaultUserStartPreferenceChoice(AssertPropertyContainsDataChange(parameters.DefaultUserStartPreference)));
            Assert.Equal(request.HasApiAccess, AssertPropertyContainsDataChange(parameters.HasApiAccess));
            Assert.Equal(request.HasStakeHolderAccess, AssertPropertyContainsDataChange(parameters.HasStakeHolderAccess));
            Assert.Equal(request.Roles, AssertPropertyContainsDataChange(parameters.Roles).Select(x => x.ToOrganizationRoleChoice()));
            Assert.Equal(request.SendMail, parameters.SendMailOnUpdate);
        }
    }
}
