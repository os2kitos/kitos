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
using Presentation.Web.Models.API.V2.Internal.Response;

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
            Assert.NotNull(actualOptionDto);
            Assert.Equal(globalOption.Name, actualOptionDto.Name);
            Assert.Equal(globalOption.HasWriteAccess, actualOptionDto.WriteAccess);
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
            Assert.NotNull(responseDto);
            Assert.Equal(globalOption.Uuid, responseDto.Uuid);
            Assert.Equal(globalOption.HasWriteAccess, responseDto.WriteAccess);
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
            Assert.NotNull(responseDto);
            Assert.Equal(globalOption.Uuid, responseDto.Uuid);
            Assert.Equal(globalOption.HasWriteAccess, responseDto.WriteAccess);
        }

        [Fact]
        public async Task Can_Patch_Local_Business_Type()
        {
            var organization = await CreateOrganization();
            Assert.NotNull(organization);
            var globalOption = SetupCreateGlobalOrganizationUnitRole();

            var dto = new LocalRegularOptionUpdateRequestDTO() { Description = A<string>() };

            using var response = await LocalOptionTypeV2Helper.PatchLocalOptionType(organization.Uuid, globalOption.Uuid, OrganizationUnitRolesUrlSuffix, dto, OrganizationUnitsApiPrefix);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDto = await response.ReadResponseBodyAsAsync<LocalRegularOptionResponseDTO>();
            Assert.Equal(dto.Description, responseDto.Description);
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
                HasWriteAccess = A<bool>()
            };
            DatabaseAccess.MutateEntitySet<OrganizationUnitRole>(repository =>
            {
                repository.Insert(globalOption);
                repository.Save();
            });
            return globalOption;
        }
    }
}
