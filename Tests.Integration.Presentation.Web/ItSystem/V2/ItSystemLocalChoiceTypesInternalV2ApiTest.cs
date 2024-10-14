

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
    public class ItSystemLocalChoiceTypesInternalV2ApiTest: WithAutoFixture
    {
        private readonly int CvrLenghtLimit = 10;
        [Fact]
        public async Task Can_Get_Local_Business_Types()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(A<int>(), A<string>(), A<string>().Truncate(CvrLenghtLimit), OrganizationTypeKeys.Kommune, AccessModifier.Public);
            Assert.NotNull(organization);

            var response = await ItSystemInternalV2Helper.GetLocalChoiceTypes(organization.Uuid, "business-types");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        } 

    }
}
