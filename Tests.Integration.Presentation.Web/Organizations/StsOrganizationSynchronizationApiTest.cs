using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations
{
    public class StsOrganizationSynchronizationApiTest : WithAutoFixture
    {
        private const string AuthorizedCvr = "58271713"; //This one is Ballerup and we have a service agreement in both local and integration for that so that's why it is used for test

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task Can_GET_Organization_Snapshot_With_Filtered_Depth(int levels)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            Guid targetOrgUuid = Guid.Empty;

            //Check if we already have the authorized org before we test snapshot (so we dont have to create a new org)
            var orgWithCorrectCvr = (await OrganizationV2Helper.GetOrganizationsAsync(token.Token, cvrContent: AuthorizedCvr)).FirstOrDefault();
            if (orgWithCorrectCvr != null)
            {
                targetOrgUuid = orgWithCorrectCvr.Uuid;
            }
            else
            {
                var org = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, $"StsSync_" + A<Guid>().ToString("N"), AuthorizedCvr, OrganizationTypeKeys.Kommune, AccessModifier.Public);
                targetOrgUuid = org.Uuid;
            }
            var url = TestEnvironment.CreateUrl($"api/v1/organizations/{targetOrgUuid:D}/sts-organization-synchronization/snapshot?levels={levels}");

            //Act
            using var response = await HttpApi.GetWithCookieAsync(url, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            //TODO: Check the contents and verify the levels
        }
    }
}
