using System;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.Internal.Users;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem.V2
{
    public class LegalPropertiesV2ApiTest : BaseItSystemsApiV2Test
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_Only_Change_Properties_As_System_Integrator(bool isSystemIntegrator)
        {
            var (_, systemUuid, organizationUuid) = await CreatePrerequisites();
            var (userUuid, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organizationUuid, true);
            var request = A<LegalPropertiesUpdateRequestDTO>();
            await UsersV2Helper.UpdateSystemIntegrator(userUuid, isSystemIntegrator);

            var response =
                await LegalItSystemPropertiesV2Helper.PatchSystem(systemUuid, request, token);

            Assert.Equal(isSystemIntegrator, response.IsSuccessStatusCode);
            if (isSystemIntegrator)
            {
                var systemAfter = await ItSystemV2Helper.GetSingleAsync(token, systemUuid);
                Assert.Equal(request.SystemName, systemAfter.LegalName);
                Assert.Equal(request.DataProcessorName, systemAfter.LegalDataProcessorName);
            }
        }

        private async Task<(string, Guid, Guid)> CreatePrerequisites()
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var org = await CreateOrganizationAsync();
            var systemUuid = await CreateSystemAsync(org.Uuid, AccessModifier.Public);
            return (token.Token, systemUuid, org.Uuid);
        }
    }
}
