using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1.Organizations;
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
        public async Task Can_GET_Organization_Snapshot_With_Filtered_Depth(uint levels)
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
                var org = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, $"StsSync_{A<Guid>():N}", AuthorizedCvr, OrganizationTypeKeys.Kommune, AccessModifier.Public);
                targetOrgUuid = org.Uuid;
            }
            var url = TestEnvironment.CreateUrl($"api/v1/organizations/{targetOrgUuid:D}/sts-organization-synchronization/snapshot?levels={levels}");

            //Act
            using var response = await HttpApi.GetWithCookieAsync(url, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var root = await response.ReadResponseBodyAsKitosApiResponseAsync<StsOrganizationOrgUnitDTO>();
            Assert.Equal(levels, CountMaxLevels(root));
            AssertOrgTree(root,new HashSet<Guid>());
        }

        private static void AssertOrgTree(StsOrganizationOrgUnitDTO unit, HashSet<Guid> seenUuids)
        {
            //check for duplicates
            Assert.DoesNotContain(seenUuids, id => id == unit.Uuid);
            seenUuids.Add(unit.Uuid);

            //Check property validity
            Assert.False(string.IsNullOrEmpty(unit.Name));
            Assert.False(string.IsNullOrEmpty(unit.UserFacingKey));
            Assert.NotEqual(Guid.Empty, unit.Uuid);
            Assert.NotNull(unit.Children);

            //Check children
            foreach (var child in unit.Children)
            {
                AssertOrgTree(child, seenUuids);
            }
        }

        private static uint CountMaxLevels(StsOrganizationOrgUnitDTO unit)
        {
            const int currentLevelContribution = 1;
            return unit
                .Children
                .Select(CountMaxLevels)
                .OrderByDescending(max => max)
                .FirstOrDefault() + currentLevelContribution;
        }
    }
}
