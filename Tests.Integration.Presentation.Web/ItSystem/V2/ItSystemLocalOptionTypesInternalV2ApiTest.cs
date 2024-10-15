

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Newtonsoft.Json;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Presentation.Web.Models.API.V2.Internal.Response;
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
            var globalOption = SetupCreateGlobalBusinessType();


            using var response = await ItSystemInternalV2Helper.GetLocalOptionTypes(organization.Uuid, BusinessTypesUrlSuffix);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDtos = JsonConvert.DeserializeObject<IEnumerable<LocalRegularOptionResponseDTO>>(content);
            Assert.NotNull(responseDtos);
            var expectedIncludedOption = responseDtos.First(dto => dto.Uuid == globalOption.Uuid);
            Assert.NotNull(expectedIncludedOption);
            Assert.Equal(globalOption.Name, expectedIncludedOption.Name);
        }

        [Fact]
        public async Task Can_Get_Local_Business_Type_By_Option_Id()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(A<int>(), A<string>(), A<string>().Truncate(CvrLengthLimit), OrganizationTypeKeys.Kommune, AccessModifier.Public);
            Assert.NotNull(organization);
            var globalOption = SetupCreateGlobalBusinessType();

            using var response = await ItSystemInternalV2Helper.GetLocalOptionTypeByOptionId(organization.Uuid, BusinessTypesUrlSuffix, globalOption.Uuid);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<LocalRegularOptionResponseDTO>(content);
            Assert.NotNull(responseDto);
            Assert.Equal(globalOption.Uuid, responseDto.Uuid);
        }

        [Fact]
        public async Task Can_Create_Local_Business_Type()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(A<int>(), A<string>(), A<string>().Truncate(CvrLengthLimit), OrganizationTypeKeys.Kommune, AccessModifier.Public);
            Assert.NotNull(organization);
            var globalOption = SetupCreateGlobalBusinessType();
            var dto = new LocalRegularOptionCreateRequestDTO() { OptionUuid = globalOption.Uuid };

            using var response = await ItSystemInternalV2Helper.CreateLocalOptionType(organization.Uuid, BusinessTypesUrlSuffix, dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<LocalRegularOptionResponseDTO>(content);
            Assert.NotNull(responseDto);
            Assert.Equal(globalOption.Uuid, responseDto.Uuid);
        }

        
        [Fact]
        public async Task Can_Patch_Local_Business_Type()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(A<int>(), A<string>(), A<string>().Truncate(CvrLengthLimit), OrganizationTypeKeys.Kommune, AccessModifier.Public);
            Assert.NotNull(organization);
            var globalOption = SetupCreateGlobalBusinessType();

            var dto = new LocalRegularOptionUpdateRequestDTO() { Description = A<string>() };

            using var response = await ItSystemInternalV2Helper.PatchLocalOptionType(organization.Uuid, globalOption.Uuid, BusinessTypesUrlSuffix, dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<LocalRegularOptionResponseDTO>(content);
            Assert.Equal(dto.Description, responseDto.Description);
        }

        [Fact]
        public async Task Can_Delete_Local_Business_Type()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(A<int>(), A<string>(), A<string>().Truncate(CvrLengthLimit), OrganizationTypeKeys.Kommune, AccessModifier.Public);
            Assert.NotNull(organization);
            var globalOption = SetupCreateGlobalBusinessType();
            var dto = new LocalRegularOptionCreateRequestDTO() { OptionUuid = globalOption.Uuid };
            using var createLocalOptionResponse = await ItSystemInternalV2Helper.CreateLocalOptionType(organization.Uuid, BusinessTypesUrlSuffix, dto);
            Assert.Equal(HttpStatusCode.OK, createLocalOptionResponse.StatusCode);
            var createContent = await createLocalOptionResponse.Content.ReadAsStringAsync();
            var createResponseDto = JsonConvert.DeserializeObject<LocalRegularOptionResponseDTO>(createContent);
            Assert.Equal(globalOption.Uuid, createResponseDto.Uuid);

            using var response = await ItSystemInternalV2Helper.DeleteLocalOptionType(organization.Uuid, globalOption.Uuid, BusinessTypesUrlSuffix);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<LocalRegularOptionResponseDTO>(content);

            Assert.False(responseDto.IsActive);
        }

        private BusinessType SetupCreateGlobalBusinessType()
        {
            var globalOption = new BusinessType()
            {
                Uuid = A<Guid>(),
                Id = A<int>(),
                Name = A<string>(),
                IsObligatory = false
            };
            DatabaseAccess.MutateEntitySet<BusinessType>(repository =>
            {
                repository.Insert(globalOption);
                repository.Save();
            });
            return globalOption;
        }
    }
}
