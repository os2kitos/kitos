using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Types.Organization;
using Tests.Integration.Presentation.Web.Tools;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationApiV2TestBase : BaseTest
    {
        protected async Task<ShallowOrganizationResponseDTO> CreateOrganizationAsync(OrganizationType orgType)
        {
            var organizationName = CreateName();
            var organization = await CreateOrganizationAsync(organizationName, type: orgType);
            return organization;
        }

        private string CreateName()
        {
            return $"{nameof(OrganizationApiV2Test)}æøå{A<string>()}";
        }
    }
}
