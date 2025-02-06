using Core.DomainModel.Organization;
using Core.DomainModel;
using Presentation.Web.Models.API.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Internal.Response.LocalOptions;
using Tests.Integration.Presentation.Web.Tools.Extensions;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Internal;
using Tests.Toolkit.Patterns;
using Xunit;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Newtonsoft.Json;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class LocalOrganizationUnitRoleOptionTypesInternalV2ApiTest: WithAutoFixture
    {
        private const int CvrLengthLimit = 10;
        private const string OrganizationUnitRolesUrlSuffix = "organization-unit-roles";
        private const string OrganizationUnitsApiPrefix = "api/v2/internal/organization-units";

        [Fact]
        public async Task Can_Get_Local_Organization_Unit_Roles()
        {
            var organization = await CreateOrganization();
            var globalOption = SetupCreateGlobalOrganizationUnitRole();

            using var response = await LocalOptionTypeV2Helper.GetLocalOptionTypes(organization.Uuid, OrganizationUnitRolesUrlSuffix, OrganizationUnitsApiPrefix);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDtos = await response.ReadResponseBodyAsAsync<IEnumerable<LocalRoleOptionResponseDTO>>();
            Assert.NotNull(responseDtos);
            var actualOptionDto = responseDtos.First(dto => dto.Uuid == globalOption.Uuid);
            AssertOrganizationUnitRoleOptionDto(globalOption, actualOptionDto);
        }

        [Fact]
        public async Task Can_Get_Local_Organization_Unit_Role_By_Option_Id()
        {
            var organization = await CreateOrganization();
            Assert.NotNull(organization);
            var globalOption = SetupCreateGlobalOrganizationUnitRole();

            using var response = await LocalOptionTypeV2Helper.GetLocalOptionType(organization.Uuid, OrganizationUnitRolesUrlSuffix, globalOption.Uuid, OrganizationUnitsApiPrefix);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDto = await response.ReadResponseBodyAsAsync<LocalRoleOptionResponseDTO>();
            AssertOrganizationUnitRoleOptionDto(globalOption, responseDto);
        }

        [Fact]
        public async Task Can_Create_Local_Organization_Unit_Role()
        {
            var organization = await CreateOrganization();
            Assert.NotNull(organization);
            var globalOption = SetupCreateGlobalOrganizationUnitRole();
            var dto = new LocalOptionCreateRequestDTO() { OptionUuid = globalOption.Uuid };

            using var response = await LocalOptionTypeV2Helper.CreateLocalOptionType(organization.Uuid, OrganizationUnitRolesUrlSuffix, dto, OrganizationUnitsApiPrefix);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDto = await response.ReadResponseBodyAsAsync<LocalRoleOptionResponseDTO>();
            Assert.True(responseDto.IsActive);
            AssertOrganizationUnitRoleOptionDto(globalOption, responseDto);
        }

        [Fact]
        public async Task Can_Patch_Local_Organization_Unit_Role()
        {
            var organization = await CreateOrganization();
            Assert.NotNull(organization);
            var globalOption = SetupCreateGlobalOrganizationUnitRole();

            var dto = new LocalRegularOptionUpdateRequestDTO() { Description = A<string>() };

            using var response = await LocalOptionTypeV2Helper.PatchLocalOptionType(organization.Uuid, globalOption.Uuid, OrganizationUnitRolesUrlSuffix, dto, OrganizationUnitsApiPrefix);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDto = await response.ReadResponseBodyAsAsync<LocalRoleOptionResponseDTO>();
            Assert.Equal(dto.Description, responseDto.Description);
            AssertOrganizationUnitRoleOptionDto(globalOption, responseDto, true);
        }

        [Fact]
        public async Task Can_Delete_Local_Organization_Unit_Role()
        {
            var organization = await CreateOrganization();
            Assert.NotNull(organization);
            var globalOption = SetupCreateGlobalOrganizationUnitRole();
            var dto = new LocalOptionCreateRequestDTO() { OptionUuid = globalOption.Uuid };
            using var createLocalOptionResponse = await LocalOptionTypeV2Helper.CreateLocalOptionType(organization.Uuid, OrganizationUnitRolesUrlSuffix, dto, OrganizationUnitsApiPrefix);
            Assert.Equal(HttpStatusCode.OK, createLocalOptionResponse.StatusCode);
            var createContent = await createLocalOptionResponse.Content.ReadAsStringAsync();
            var createResponseDto = JsonConvert.DeserializeObject<LocalRoleOptionResponseDTO>(createContent);
            Assert.Equal(globalOption.Uuid, createResponseDto.Uuid);

            using var response = await LocalOptionTypeV2Helper.DeleteLocalOptionType(organization.Uuid, globalOption.Uuid, OrganizationUnitRolesUrlSuffix, OrganizationUnitsApiPrefix);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDto = await response.ReadResponseBodyAsAsync<LocalRoleOptionResponseDTO>();

            Assert.False(responseDto.IsActive);
        }


        private async Task<OrganizationDTO> CreateOrganization()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(A<int>(), A<string>(), A<string>().Truncate(CvrLengthLimit), OrganizationTypeKeys.Kommune, AccessModifier.Public);
            Assert.NotNull(organization);
            return organization;
        }

        private OrganizationUnitRole SetupCreateGlobalOrganizationUnitRole()
        {
            var globalOption = new OrganizationUnitRole()
            {
                Uuid = A<Guid>(),
                Id = A<int>(),
                Name = A<string>(),
                IsObligatory = false,
                HasWriteAccess = A<bool>(),
                IsEnabled = true
            };
            DatabaseAccess.MutateEntitySet<OrganizationUnitRole>(repository =>
            {
                repository.Insert(globalOption);
                repository.Save();
            });
            return globalOption;
        }
        private void AssertOrganizationUnitRoleOptionDto(OrganizationUnitRole expected,
            LocalRoleOptionResponseDTO actual, bool skipDescription = false)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Uuid, actual.Uuid);
            Assert.Equal(expected.IsObligatory, actual.IsObligatory);
            Assert.Equal(expected.HasWriteAccess, actual.WriteAccess);
            if (!skipDescription) Assert.Equal(expected.Description, actual.Description);
        }
    }
}
