

using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
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

        [Fact]
        public async Task Can_Get_Local_Business_Types()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(A<int>(), A<string>(), A<string>().Truncate(CvrLengthLimit), OrganizationTypeKeys.Kommune, AccessModifier.Public);
            Assert.NotNull(organization);

            var response = await ItSystemInternalV2Helper.GetLocalOptionTypes(organization.Uuid, "business-types");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Can_Get_Local_Business_Type_By_Option_Id()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(A<int>(), A<string>(), A<string>().Truncate(CvrLengthLimit), OrganizationTypeKeys.Kommune, AccessModifier.Public);
            Assert.NotNull(organization);
            var optionId = 1;

            var response = await ItSystemInternalV2Helper.GetLocalOptionTypeByOptionId(organization.Uuid, "business-types", optionId);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }
}
