

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Newtonsoft.Json;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Presentation.Web.Models.API.V2.Internal.Response;
using Presentation.Web.Models.API.V2.Response.Options;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Extensions;
using Tests.Integration.Presentation.Web.Tools.Internal.ItSystem;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem.V2
{
    public class ItSystemLocalOptionTypesInternalV2ApiTest: WithAutoFixture
    {
        private const int CvrLengthLimit = 10;
        private const string BusinessTypesUrlSuffix = "business-types";

        [Fact]
        public async Task Can_Get_Local_Business_Types()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(A<int>(), A<string>(), A<string>().Truncate(CvrLengthLimit), OrganizationTypeKeys.Kommune, AccessModifier.Public);
            Assert.NotNull(organization);

            using var response = await ItSystemInternalV2Helper.GetLocalOptionTypes(organization.Uuid, BusinessTypesUrlSuffix);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Can_Get_Local_Business_Type_By_Option_Id()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(A<int>(), A<string>(), A<string>().Truncate(CvrLengthLimit), OrganizationTypeKeys.Kommune, AccessModifier.Public);
            Assert.NotNull(organization);
            var optionId = 1;

            using var response = await ItSystemInternalV2Helper.GetLocalOptionTypeByOptionId(organization.Uuid, BusinessTypesUrlSuffix, optionId);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Can_Create_Local_Option_Type()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(A<int>(), A<string>(), A<string>().Truncate(CvrLengthLimit), OrganizationTypeKeys.Kommune, AccessModifier.Public);
            Assert.NotNull(organization);
            var optionId = A<int>();
            var dto = new LocalRegularOptionCreateRequestDTO() { OptionId = optionId };

            using var response = await ItSystemInternalV2Helper.CreateLocalOptionType(organization.Uuid, BusinessTypesUrlSuffix, dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Can_Patch_Local_Option_Type()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(A<int>(), A<string>(), A<string>().Truncate(CvrLengthLimit), OrganizationTypeKeys.Kommune, AccessModifier.Public);
            Assert.NotNull(organization);
            var optionId = 1;

            var dto = new LocalRegularOptionUpdateRequestDTO() { Description = A<string>() };

            using var response = await ItSystemInternalV2Helper.PatchLocalOptionType(organization.Uuid, optionId, BusinessTypesUrlSuffix, dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<LocalRegularOptionResponseDTO>(content);
            Assert.Equal(dto.Description, responseDto.Description);
        }

        [Fact]
        public async Task Can_Delete_Local_Option_Type()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(A<int>(), A<string>(), A<string>().Truncate(CvrLengthLimit), OrganizationTypeKeys.Kommune, AccessModifier.Public);
            Assert.NotNull(organization);
            var optionId = 1;
            var createDto = new LocalRegularOptionCreateRequestDTO() { OptionId = optionId };
            using var createResponse = await ItSystemInternalV2Helper.CreateLocalOptionType(organization.Uuid, BusinessTypesUrlSuffix, createDto);
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

            using var response = await ItSystemInternalV2Helper.DeleteLocalOptionType(organization.Uuid, optionId, BusinessTypesUrlSuffix);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<LocalRegularOptionResponseDTO>(content);

            Assert.False(responseDto.IsActive);
        }
    }
}
