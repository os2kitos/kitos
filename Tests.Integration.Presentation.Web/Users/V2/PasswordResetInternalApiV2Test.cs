using Core.DomainModel.Organization;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Request;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Internal.Users;
using Tests.Toolkit.Patterns;
using Xunit;
using System;
using Presentation.Web.Models.API.V2.Internal.Response.User;
using Presentation.Web.Models.API.V2.Request.User;

namespace Tests.Integration.Presentation.Web.Users.V2
{
    public class PasswordResetInternalApiV2Test : WithAutoFixture
    {

        [Fact]
        public async Task Can_Request_Password_Reset()
        {
            var user = await CreateUserAsync();
            var request = new RequestPasswordResetRequestDTO { Email = user.Email };
            
            var result = await PasswordResetV2Helper.PostPasswordReset(request);

            Assert.True(result.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Request_Password_Reset_With_Nonexistant_Mail_Returns_Success()
        {
            var request = A<RequestPasswordResetRequestDTO>();

            var result = await PasswordResetV2Helper.PostPasswordReset(request);
            
            Assert.True(result.IsSuccessStatusCode);
        }

        private async Task<UserResponseDTO> CreateUserAsync()
        {
            var orgUuid = await CreateOrganizationAsync();
            return await CreateUserInOrgAsync(orgUuid);
        }

        private async Task<UserResponseDTO> CreateUserInOrgAsync(Guid organizationUuid)
        {
            var validEmail = $"{A<string>()}@{A<string>()}.dk";
            var request = A<CreateUserRequestDTO>();
            request.Email = validEmail;
            return await UsersV2Helper.CreateUser(organizationUuid, request);
        }

        private async Task<Guid> CreateOrganizationAsync()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                A<string>(), "11224455", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            return organization.Uuid;
        }
    }
}
