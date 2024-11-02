using Core.DomainModel.Organization;
using Core.DomainModel;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Internal.Response;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools.Extensions;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Internal;
using Tests.Toolkit.Patterns;
using Xunit;
using Core.DomainModel.ItSystem;
using System;
using System.Linq;

namespace Tests.Integration.Presentation.Web.GlobalAdminArea
{
    public class ItSystemGlobalOptionTypesInternalV2ApiTest: WithAutoFixture
    {
        private const string BusinessTypesUrlSuffix = "business-types";
        private const string ItSystemsApiPrefix = "api/v2/internal/it-systems";

        [Fact]
        public async Task Can_Get_global_Business_Types()
        {
            var expectedGlobalOption = SetupCreateGlobalBusinessTypeInDatabase();

            using var response = await GlobalOptionTypeV2Helper.GetGlobalOptionTypes(BusinessTypesUrlSuffix, ItSystemsApiPrefix);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDtos = await response.ReadResponseBodyAsAsync<IEnumerable<LocalRegularOptionResponseDTO>>();
            Assert.NotNull(responseDtos);
            var actualIncludedOption = responseDtos.First(dto => dto.Uuid == expectedGlobalOption.Uuid);
            Assert.NotNull(actualIncludedOption);
            Assert.Equal(expectedGlobalOption.Name, actualIncludedOption.Name);
        }

        private BusinessType SetupCreateGlobalBusinessTypeInDatabase()
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
