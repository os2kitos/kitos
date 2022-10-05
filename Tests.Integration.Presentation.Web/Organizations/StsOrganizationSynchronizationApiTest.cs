using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.ApplicationServices.Extensions;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.Organizations;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations
{
    [Collection(nameof(SequentialTestGroup))]
    public class StsOrganizationSynchronizationApiTest : WithAutoFixture
    {
        private const string UnAuthorizedCvr = "55133018"; //This one is Aarhus and we don't have a service agreement with them in STS Test environment
        private const string AuthorizedCvr = "58271713"; //This one is Ballerup and we have a service agreement with them in STS Test environment

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task Can_GET_Organization_Snapshot_With_Filtered_Depth(uint levels)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await GetOrCreateOrgWithCvr(token, AuthorizedCvr);
            var url = TestEnvironment.CreateUrl($"api/v1/organizations/{targetOrgUuid:D}/sts-organization-synchronization/snapshot?levels={levels}");

            //Act
            using var response = await HttpApi.GetWithCookieAsync(url, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var root = await response.ReadResponseBodyAsKitosApiResponseAsync<StsOrganizationOrgUnitDTO>();
            Assert.Equal(levels, CountMaxLevels(root));
            AssertOrgTree(root, new HashSet<Guid>());
        }

        [Theory]
        [InlineData(UnAuthorizedCvr, false, CheckConnectionError.MissingServiceAgreement)]
        [InlineData(null, false, CheckConnectionError.InvalidCvrOnOrganization)]
        [InlineData(AuthorizedCvr, true, null)]
        public async Task Can_GET_ConnectionStatus(string cvr, bool expectConnected, CheckConnectionError? expectedError)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await GetOrCreateOrgWithCvr(token, cvr);
            var url = TestEnvironment.CreateUrl($"api/v1/organizations/{targetOrgUuid:D}/sts-organization-synchronization/connection-status");

            //Act
            using var response = await HttpApi.GetWithCookieAsync(url, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var root = await response.ReadResponseBodyAsKitosApiResponseAsync<CheckStsOrganizationConnectionResponseDTO>();
            Assert.Equal(expectConnected, root.Connected);
            Assert.Equal(expectedError, root.Error);
        }

        private async Task<Guid> GetOrCreateOrgWithCvr(GetTokenResponseDTO token, string cvr)
        {
            Guid targetOrgUuid;
            //Check if we already have the authorized org before we test snapshot (so we dont have to create a new org)
            var orgWithCorrectCvr = (await OrganizationV2Helper.GetOrganizationsAsync(token.Token, cvrContent: cvr))
                .FirstOrDefault();
            if (orgWithCorrectCvr != null)
            {
                targetOrgUuid = orgWithCorrectCvr.Uuid;
            }
            else
            {
                var org = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, $"StsSync_{A<Guid>():N}", cvr, OrganizationTypeKeys.Kommune, AccessModifier.Public);
                targetOrgUuid = org.Uuid;
            }

            return targetOrgUuid;
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
