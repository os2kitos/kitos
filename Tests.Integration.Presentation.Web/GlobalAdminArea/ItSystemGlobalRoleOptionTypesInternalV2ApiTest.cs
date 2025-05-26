using Core.DomainModel.ItSystem;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Linq;
using Presentation.Web.Models.API.V2.Internal.Request;
using Tests.Integration.Presentation.Web.Tools.Internal;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.GlobalAdminArea
{
    public class ItSystemGlobalRoleOptionTypesInternalV2ApiTest : WithAutoFixture
    {
        [Fact]
        public async Task Can_Get_Global_Business_Types()
        {
            var expectedGlobalOption = SetupCreateGlobalItSystemRoleInDatabase();

            using var response = await GlobalOptionTypeV2Helper.GetGlobalOptionTypes(GlobalOptionTypeV2Helper.ItSystemRoles);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDtos = await response.ReadResponseBodyAsAsync<IEnumerable<GlobalRoleOptionResponseDTO>>();
            Assert.NotNull(responseDtos);
            var actualIncludedOption = responseDtos.First(dto => dto.Uuid == expectedGlobalOption.Uuid);
            Assert.NotNull(actualIncludedOption);
            Assert.Equal(expectedGlobalOption.Uuid, actualIncludedOption.Uuid);
        }

        [Fact]
        public async Task Can_Create_Global_Business_Type()
        {
            var dto = A<GlobalRoleOptionCreateRequestDTO>();

            using var response = await GlobalOptionTypeV2Helper.CreateGlobalOptionType(GlobalOptionTypeV2Helper.ItSystemRoles, dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDto = await response.ReadResponseBodyAsAsync<GlobalRoleOptionResponseDTO>();
            Assert.NotNull(responseDto);
            Assert.Equal(dto.Name, responseDto.Name);
            Assert.False(responseDto.IsEnabled);
            Assert.Equal(dto.WriteAccess, responseDto.WriteAccess);
        }

        [Fact]
        public async Task Can_Patch_Global_Business_Type()
        {
            var globalOption = SetupCreateGlobalItSystemRoleInDatabase();

            var dto = A<GlobalRoleOptionUpdateRequestDTO>();

            using var response = await GlobalOptionTypeV2Helper.PatchGlobalOptionType(globalOption.Uuid, GlobalOptionTypeV2Helper.ItSystemRoles, dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDto = await response.ReadResponseBodyAsAsync<GlobalRoleOptionResponseDTO>();
            Assert.Equal(dto.Description, responseDto.Description);
            Assert.Equal(dto.Name, responseDto.Name);
            Assert.Equal(dto.IsObligatory, responseDto.IsObligatory);
            Assert.Equal(dto.IsEnabled, responseDto.IsEnabled);
            Assert.Equal(globalOption.Uuid, responseDto.Uuid);
            Assert.Equal(dto.WriteAccess, responseDto.WriteAccess);
        }

        private ItSystemRole SetupCreateGlobalItSystemRoleInDatabase()
        {
            var globalOption = new ItSystemRole()
            {
                Uuid = A<Guid>(),
                Id = A<int>(),
                Name = A<string>(),
                IsObligatory = true,
                IsLocallyAvailable = true,
                HasWriteAccess = A<bool>()
            };
            DatabaseAccess.MutateEntitySet<ItSystemRole>(repository =>
            {
                repository.Insert(globalOption);
                repository.Save();
            });
            return globalOption;
        }
    }
}
