
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Internal;
using Tests.Toolkit.Patterns;
using Xunit;
using Core.DomainModel.ItSystem;
using System;
using System.Linq;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;

namespace Tests.Integration.Presentation.Web.GlobalAdminArea
{
    public class ItSystemGlobalBusinessTypesInternalV2ApiTest : WithAutoFixture
    {

        [Fact]
        public async Task Can_Get_Global_Business_Types()
        {
            var expectedGlobalOption = SetupCreateGlobalBusinessTypeInDatabase();

            using var response = await GlobalOptionTypeV2Helper.GetGlobalOptionTypes(GlobalOptionTypeV2Helper.BusinessTypes);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDtos = await response.ReadResponseBodyAsAsync<IEnumerable<GlobalRegularOptionResponseDTO>>();
            Assert.NotNull(responseDtos);
            var actualIncludedOption = responseDtos.First(dto => dto.Uuid == expectedGlobalOption.Uuid);
            Assert.NotNull(actualIncludedOption);
            Assert.Equal(expectedGlobalOption.Uuid, actualIncludedOption.Uuid);
        }

        [Fact]
        public async Task Can_Create_Global_Business_Type()
        {
            var dto = A<GlobalRegularOptionCreateRequestDTO>();

            using var response = await GlobalOptionTypeV2Helper.CreateGlobalOptionType(GlobalOptionTypeV2Helper.BusinessTypes, dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDto = await response.ReadResponseBodyAsAsync<GlobalRegularOptionResponseDTO>();
            Assert.NotNull(responseDto);
            Assert.Equal(dto.Name, responseDto.Name);
            Assert.False(responseDto.IsEnabled);
        }

        [Fact]
        public async Task Can_Patch_Global_Business_Type()
        {
            var globalOption = SetupCreateGlobalBusinessTypeInDatabase();

            var dto = A<GlobalRegularOptionUpdateRequestDTO>();

            using var response = await GlobalOptionTypeV2Helper.PatchGlobalOptionType(globalOption.Uuid, GlobalOptionTypeV2Helper.BusinessTypes, dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDto = await response.ReadResponseBodyAsAsync<GlobalRegularOptionResponseDTO>();
            Assert.Equal(dto.Description, responseDto.Description);
            Assert.Equal(dto.Name, responseDto.Name);
            Assert.Equal(dto.IsObligatory, responseDto.IsObligatory);
            Assert.Equal(dto.IsEnabled, responseDto.IsEnabled);
            Assert.Equal(globalOption.Uuid, responseDto.Uuid);
        }

        private BusinessType SetupCreateGlobalBusinessTypeInDatabase()
        {
            var globalOption = new BusinessType()
            {
                Uuid = A<Guid>(),
                Id = A<int>(),
                Name = A<string>(),
                IsObligatory = true,
                IsLocallyAvailable = true
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
