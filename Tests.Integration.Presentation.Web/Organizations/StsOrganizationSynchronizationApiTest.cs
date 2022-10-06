using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
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
        public async Task Can_GET_Organization_Snapshot_With_Filtered_Depth(int levels)
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
            var root = await response.ReadResponseBodyAsKitosApiResponseAsync<StsOrganizationSynchronizationDetailsResponseDTO>();
            Assert.Equal(expectConnected, root.AccessStatus.AccessGranted);
            Assert.Equal(expectedError, root.AccessStatus.Error);
        }

        [Fact]
        public async Task Can_POST_Create_Connection()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int levels = 2;
            DatabaseAccess.MutateEntitySet<StsOrganizationIdentity>(repo =>
            {
                var existingMapping = repo.AsQueryable().FirstOrDefault(x => x.Organization.Cvr == AuthorizedCvr);
                if (existingMapping != null)
                {
                    repo.Delete(existingMapping);
                }
                repo.Save();
            });
            var postUrl = TestEnvironment.CreateUrl($"api/v1/organizations/{targetOrgUuid:D}/sts-organization-synchronization/connection");
            var getUrl = TestEnvironment.CreateUrl($"api/v1/organizations/{targetOrgUuid:D}/sts-organization-synchronization/snapshot?levels={levels}");
            using var getResponse = await HttpApi.GetWithCookieAsync(getUrl, cookie);
            var expectedImport = await getResponse.ReadResponseBodyAsKitosApiResponseAsync<StsOrganizationOrgUnitDTO>();

            //Act
            using var response = await HttpApi.PostWithCookieAsync(postUrl, cookie, new ConnectToStsOrganizationRequestDTO
            {
                SynchronizationDepth = levels
            });

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            DatabaseAccess.MapFromEntitySet<Organization, bool>(orgs =>
            {
                var organization = orgs.AsQueryable().ByUuid(targetOrgUuid);
                var dbRoot = organization.GetRoot();
                Assert.NotNull(organization.StsOrganizationConnection);
                Assert.True(organization.StsOrganizationConnection.Connected);
                Assert.Equal(levels, organization.StsOrganizationConnection.SynchronizationDepth);
                AssertImportedTree(expectedImport, dbRoot);
                return true;
            });

        }

        private static void AssertImportedTree(StsOrganizationOrgUnitDTO treeToImport, OrganizationUnit importedTree, int? remainingLevelsToImport = null)
        {
            Assert.Equal(treeToImport.Name, importedTree.Name);
            Assert.Equal(treeToImport.Uuid, importedTree.ExternalOriginUuid);
            Assert.Equal(OrganizationUnitOrigin.STS_Organisation, importedTree.Origin);

            remainingLevelsToImport -= 1;

            if (remainingLevelsToImport is < 1)
            {
                Assert.Empty(importedTree.Children); //if no more remaining levels were expected the imported subtree must be empty
            }
            else
            {
                var childrenToImport = treeToImport.Children.ToList();
                var importedUnits = importedTree.Children.ToList();
                Assert.Equal(childrenToImport.Count, importedUnits.Count);
                for (var i = 0; i < childrenToImport.Count; i++)
                {
                    AssertImportedTree(childrenToImport[i], importedUnits[i], remainingLevelsToImport);
                }
            }
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
                targetOrgUuid = await CreateOrgWithCvr(cvr);
            }

            return targetOrgUuid;
        }

        private async Task<Guid> CreateOrgWithCvr(string cvr)
        {
            var org = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, $"StsSync_{A<Guid>():N}", cvr, OrganizationTypeKeys.Kommune, AccessModifier.Public);
            return org.Uuid;
        }

        private static void AssertOrgTree(StsOrganizationOrgUnitDTO unit, HashSet<Guid> seenUuids)
        {
            //check for duplicates
            Assert.DoesNotContain(seenUuids, id => id == unit.Uuid);
            seenUuids.Add(unit.Uuid);

            //Check property validity
            Assert.False(string.IsNullOrEmpty(unit.Name));
            Assert.NotEqual(Guid.Empty, unit.Uuid);
            Assert.NotNull(unit.Children);

            //Check children
            foreach (var child in unit.Children)
            {
                AssertOrgTree(child, seenUuids);
            }
        }

        private static int CountMaxLevels(StsOrganizationOrgUnitDTO unit)
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
