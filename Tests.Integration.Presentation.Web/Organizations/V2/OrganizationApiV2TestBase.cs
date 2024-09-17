using Core.DomainModel.Organization;
using Core.DomainModel;
using Presentation.Web.Models.API.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationApiV2TestBase: WithAutoFixture
    {
        protected async Task<OrganizationDTO> CreateOrganizationAsync(OrganizationTypeKeys orgType)
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, "V2" + string.Join("", Many<int>(8).Select(x => Math.Abs(x) % 9)), orgType, AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return $"{nameof(OrganizationApiV2Test)}æøå{A<string>()}";
        }
    }
}
