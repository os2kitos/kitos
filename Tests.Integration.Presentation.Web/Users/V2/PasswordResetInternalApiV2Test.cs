using Presentation.Web.Models.API.V2.Internal.Request;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Internal.Users;
using Xunit;

namespace Tests.Integration.Presentation.Web.Users.V2
{
    public class PasswordResetInternalApiV2Test : BaseTest
    {

        [Fact]
        public async Task Can_Request_Password_Reset()
        {
            var organization = await CreateOrganizationAsync();
            var user = await CreateUserAsync(organization.Uuid);
            var request = new RequestPasswordResetRequestDTO { Email = user.Email };
            
            var result = await PasswordResetV2Helper.PostPasswordReset(request);

            Assert.True(result.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Request_Password_Reset_With_Non_Existent_Mail_Returns_Success()
        {
            var request = A<RequestPasswordResetRequestDTO>();

            var result = await PasswordResetV2Helper.PostPasswordReset(request);
            
            Assert.True(result.IsSuccessStatusCode);
        }
    }
}
