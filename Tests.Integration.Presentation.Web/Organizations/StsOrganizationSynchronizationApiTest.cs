using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model.StsOrganization;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.Organizations;
using Presentation.Web.Models.API.V2.Request.Contract;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations
{
    [Collection(nameof(SequentialTestGroup))]
    public class StsOrganizationSynchronizationApiTest : WithAutoFixture
    {
        private const string UnAuthorizedCvr = "55133018"; //This one is Aarhus and we don't have a service agreement with them in STS Test environment
        private const string AuthorizedCvr = "58271713"; //This one is Ballerup and we have a service agreement with them in STS Test environment

        public StsOrganizationSynchronizationApiTest()
        {
            //Reset the id mapping state to prevent constraint issues (one org owning the cvr)
            DatabaseAccess.MutateEntitySet<StsOrganizationIdentity>(repo =>
            {
                var existingMapping = repo.AsQueryable().Where(x => x.Organization.Cvr == AuthorizedCvr).ToList();
                foreach (var stsOrganizationIdentity in existingMapping)
                    repo.Delete(stsOrganizationIdentity);
            });
            DatabaseAccess.MutateEntitySet<StsOrganizationConnection>(repo =>
            {
                var existingMapping = repo.AsQueryable().Where(x => x.Organization.Cvr == AuthorizedCvr).ToList();
                foreach (var stsOrganizationIdentity in existingMapping)
                    repo.Delete(stsOrganizationIdentity);
            });
            DatabaseAccess.MutateEntitySet<Organization>(repo =>
            {
                var existingMapping = repo.AsQueryable().Where(x => x.Cvr == AuthorizedCvr || x.Cvr == UnAuthorizedCvr).ToList();
                foreach (var organization in existingMapping)
                {
                    organization.Cvr = null;
                }
            });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task Can_GET_Organization_Snapshot_With_Filtered_Depth(int levels)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await GetOrCreateOrgWithCvr(token, AuthorizedCvr);

            //Act
            using var response = await SendGetSnapshotAsync(levels, targetOrgUuid, cookie);

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

            //Act
            using var response = await SendGetConnectionStatusAsync(targetOrgUuid, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var root = await response.ReadResponseBodyAsKitosApiResponseAsync<StsOrganizationSynchronizationDetailsResponseDTO>();
            Assert.Equal(expectConnected, root.AccessStatus.AccessGranted);
            Assert.Equal(expectedError, root.AccessStatus.Error);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_POST_Create_Connection(bool subscribe)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int levels = 2;
            using var getResponse = await SendGetSnapshotAsync(levels, targetOrgUuid, cookie).WithExpectedResponseCode(HttpStatusCode.OK);
            var expectedImport = await getResponse.ReadResponseBodyAsKitosApiResponseAsync<StsOrganizationOrgUnitDTO>();

            //Act
            using var response = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, levels, subscribe);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            DatabaseAccess.MapFromEntitySet<Organization, bool>(orgs =>
            {
                var organization = orgs.AsQueryable().ByUuid(targetOrgUuid);
                var dbRoot = organization.GetRoot();
                Assert.NotNull(organization.StsOrganizationConnection);
                Assert.True(organization.StsOrganizationConnection.Connected);
                Assert.Equal(levels, organization.StsOrganizationConnection.SynchronizationDepth);
                Assert.Equal(subscribe, organization.StsOrganizationConnection.SubscribeToUpdates);
                AssertImportedTree(expectedImport, dbRoot, OrganizationUnitOrigin.STS_Organisation);
                return true;
            });
        }

        [Fact]
        public async Task Can_DELETE_Connection()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int levels = 2;
            var connectionUrl = TestEnvironment.CreateUrl($"api/v1/organizations/{targetOrgUuid:D}/sts-organization-synchronization/connection");
            var getUrl = TestEnvironment.CreateUrl($"api/v1/organizations/{targetOrgUuid:D}/sts-organization-synchronization/snapshot?levels={levels}");
            using var getResponse = await HttpApi.GetWithCookieAsync(getUrl, cookie);
            var expectedImport = await getResponse.ReadResponseBodyAsKitosApiResponseAsync<StsOrganizationOrgUnitDTO>();
            using var response = await HttpApi.PostWithCookieAsync(connectionUrl, cookie, new ConnectToStsOrganizationRequestDTO
            {
                SynchronizationDepth = levels
            });
            //Act
            using var deleteResponse = await HttpApi.DeleteWithCookieAsync(connectionUrl, cookie);

            //Assert
            DatabaseAccess.MapFromEntitySet<Organization, bool>(orgs =>
            {
                var organization = orgs.AsQueryable().ByUuid(targetOrgUuid);
                var dbRoot = organization.GetRoot();
                Assert.NotNull(organization.StsOrganizationConnection);
                Assert.False(organization.StsOrganizationConnection.Connected);
                Assert.Null(organization.StsOrganizationConnection.SynchronizationDepth);

                //Assert that the imported stuff is till there - just converted to kitos units
                AssertImportedTree(expectedImport, dbRoot, OrganizationUnitOrigin.Kitos);
                return true;
            });

        }

        [Fact, Description("If no changes between imports of the same levels, we expect an empty changeset.")]
        public async Task Can_GET_UPDATE_Consequences_With_No_Consequences()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int levels = 2;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, levels);

            //Act
            using var getResponse = await SendGetUpdateConsequencesAsync(targetOrgUuid, levels, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var consequences = await getResponse.ReadResponseBodyAsKitosApiResponseAsync<ConnectionUpdateConsequencesResponseDTO>();
            Assert.Empty(consequences.Consequences);
        }

        [Fact]
        public async Task Can_GET_UPDATE_Consequences_With_Addition_Consequences()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int firstRequestLevels = 2;
            const int secondRequestLevels = 3;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, firstRequestLevels);

            //Act
            using var consequencesResponse = await SendGetUpdateConsequencesAsync(targetOrgUuid, secondRequestLevels, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, consequencesResponse.StatusCode);
            var consequences = await consequencesResponse.ReadResponseBodyAsKitosApiResponseAsync<ConnectionUpdateConsequencesResponseDTO>();
            Assert.NotEmpty(consequences.Consequences);
            Assert.All(consequences.Consequences, c => Assert.Equal(ConnectionUpdateOrganizationUnitChangeCategory.Added, c.Category));
        }

        [Fact]
        public async Task Can_GET_UPDATE_Consequences_With_Removal_Consequences()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int firstRequestLevels = 3;
            const int secondRequestLevels = 2;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, firstRequestLevels);

            //Act
            using var consequencesResponse = await SendGetUpdateConsequencesAsync(targetOrgUuid, secondRequestLevels, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, consequencesResponse.StatusCode);
            var consequences = await consequencesResponse.ReadResponseBodyAsKitosApiResponseAsync<ConnectionUpdateConsequencesResponseDTO>();
            Assert.NotEmpty(consequences.Consequences);
            Assert.All(consequences.Consequences, c => Assert.Equal(ConnectionUpdateOrganizationUnitChangeCategory.Deleted, c.Category));
        }

        [Fact]
        public async Task Can_GET_UPDATE_Consequences_With_Rename_Consequences()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int firstRequestLevels = 2;
            const int secondRequestLevels = 2;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, firstRequestLevels);
            var uuidsOfRenamedUnits = new List<Guid>();
            DatabaseAccess.MutateEntitySet<OrganizationUnit>(repo =>
            {
                var renamedUnits = repo
                    .AsQueryable()
                    .Where(x => x.Organization.Uuid == targetOrgUuid && x.Origin == OrganizationUnitOrigin.STS_Organisation)
                    .ToList()
                    .RandomItems(2)
                    .ToList();

                foreach (var organizationUnit in renamedUnits)
                {
                    organizationUnit.Name += "_rn1";
                    uuidsOfRenamedUnits.Add(organizationUnit.ExternalOriginUuid.GetValueOrDefault());
                }
            });

            //Act
            using var consequencesResponse = await SendGetUpdateConsequencesAsync(targetOrgUuid, secondRequestLevels, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, consequencesResponse.StatusCode);
            var consequences = await consequencesResponse.ReadResponseBodyAsKitosApiResponseAsync<ConnectionUpdateConsequencesResponseDTO>();
            Assert.NotEmpty(consequences.Consequences);
            Assert.All(consequences.Consequences, c => Assert.Equal(ConnectionUpdateOrganizationUnitChangeCategory.Renamed, c.Category));
            Assert.Equal(consequences.Consequences.Select(x => x.Uuid).OrderBy(x => x), uuidsOfRenamedUnits.OrderBy(x => x));
        }

        [Fact]
        public async Task Can_GET_UPDATE_Consequences_With_Moval_Consequences()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int firstRequestLevels = 2;
            const int secondRequestLevels = 2;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, firstRequestLevels);
            DatabaseAccess.MutateEntitySet<OrganizationUnit>(repo =>
            {
                var twoLeafs = repo
                    .AsQueryable()
                    .Where(x => x.Organization.Uuid == targetOrgUuid && x.Origin == OrganizationUnitOrigin.STS_Organisation && !x.Children.Any())
                    .ToList()
                    .RandomItems(2)
                    .ToList();

                var firstLeaf = twoLeafs.First();
                var secondLeaf = twoLeafs.Last();

                //Make first leaf parent of second leaf
                secondLeaf.ParentId = firstLeaf.Id;
            });

            //Act
            using var consequencesResponse = await SendGetUpdateConsequencesAsync(targetOrgUuid, secondRequestLevels, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, consequencesResponse.StatusCode);
            var consequences = await consequencesResponse.ReadResponseBodyAsKitosApiResponseAsync<ConnectionUpdateConsequencesResponseDTO>();
            Assert.NotEmpty(consequences.Consequences);
            Assert.All(consequences.Consequences, c => Assert.Equal(ConnectionUpdateOrganizationUnitChangeCategory.Moved, c.Category));
        }

        [Fact]
        public async Task Can_GET_UPDATE_Consequences_With_Conversion_Consequences()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int firstRequestLevels = 2;
            const int secondRequestLevels = 1;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, firstRequestLevels);
            var expectedConvertedUnit = Guid.Empty;
            DatabaseAccess.MutateEntitySet<OrganizationUnit>(repo =>
            {
                var leaf = repo
                    .AsQueryable()
                    .Where(x => x.Organization.Uuid == targetOrgUuid && x.Origin == OrganizationUnitOrigin.STS_Organisation && !x.Children.Any())
                    .ToList()
                    .RandomItem();

                expectedConvertedUnit = leaf.ExternalOriginUuid.GetValueOrDefault();

                //Add a kitos-unit as a child to a
                var newUnit = new OrganizationUnit
                {
                    OrganizationId = leaf.OrganizationId,
                    ParentId = leaf.Id,
                    ObjectOwnerId = leaf.ObjectOwnerId,
                    Name = "Test",
                    LastChangedByUserId = leaf.LastChangedByUserId
                };
                repo.Insert(newUnit);
            });

            //Act
            using var consequencesResponse = await SendGetUpdateConsequencesAsync(targetOrgUuid, secondRequestLevels, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, consequencesResponse.StatusCode);
            var consequences = await consequencesResponse.ReadResponseBodyAsKitosApiResponseAsync<ConnectionUpdateConsequencesResponseDTO>();
            Assert.NotEmpty(consequences.Consequences);
            var conversion = Assert.Single(consequences.Consequences.Where(x => x.Category == ConnectionUpdateOrganizationUnitChangeCategory.Converted));
            Assert.Equal(expectedConvertedUnit, conversion.Uuid);
        }

        [Fact]
        public async Task Can_PUT_UPDATE_Consequences_With_Addition_Consequences()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int firstRequestLevels = 2;
            const int secondRequestLevels = 3;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, firstRequestLevels);

            using var consequencesBeforePutResponse = await SendGetUpdateConsequencesAsync(targetOrgUuid, secondRequestLevels, cookie);
            Assert.Equal(HttpStatusCode.OK, consequencesBeforePutResponse.StatusCode);
            var consequencesBeforePut = await consequencesBeforePutResponse.ReadResponseBodyAsKitosApiResponseAsync<ConnectionUpdateConsequencesResponseDTO>();
            var expectedAdded = consequencesBeforePut.Consequences.ToList().Where(x => x.Category == ConnectionUpdateOrganizationUnitChangeCategory.Added).Select(x => x.Uuid).ToHashSet();
            Assert.NotEmpty(expectedAdded);

            //Act
            using var putResponse = await SendPutUpdateConsequencesAsync(targetOrgUuid, secondRequestLevels, cookie);
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

            var externalUnits = DatabaseAccess.MapFromEntitySet<OrganizationUnit, IEnumerable<Guid?>>(repository =>
                repository
                    .AsQueryable()
                    .Where(unit => unit.Organization.Uuid == targetOrgUuid)
                    .Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation)
                    .Select(x => x.ExternalOriginUuid)
                    .ToList())
                .Select(x => x.GetValueOrDefault())
                .ToList();
            Assert.All(expectedAdded, expected => Assert.Contains(expected, externalUnits));
        }

        [Fact]
        public async Task Can_PUT_UPDATE_Consequences_With_Removal_Consequences()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int firstRequestLevels = 3;
            const int secondRequestLevels = 2;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, firstRequestLevels);

            var expectedUuidsLeft = DatabaseAccess.MapFromEntitySet<OrganizationUnit, IEnumerable<Guid?>>(repository =>
                    repository
                        .AsQueryable()
                        .Where(unit => unit.Organization.Uuid == targetOrgUuid)
                        .Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation && x.Parent.Parent == null) //we expect leafs at level 3 to be removed, so leave those out of the expected
                        .Select(x => x.ExternalOriginUuid)
                        .ToList())
                .Select(x => x.GetValueOrDefault())
                .ToList();

            //Act
            using var putResponse = await SendPutUpdateConsequencesAsync(targetOrgUuid, secondRequestLevels, cookie);
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

            var actualUuidsLeft = DatabaseAccess.MapFromEntitySet<OrganizationUnit, IEnumerable<Guid?>>(repository =>
                    repository
                        .AsQueryable()
                        .Where(unit => unit.Organization.Uuid == targetOrgUuid)
                        .Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation)
                        .Select(x => x.ExternalOriginUuid)
                        .ToList())
                .Select(x => x.GetValueOrDefault())
                .ToList();

            Assert.Equal(expectedUuidsLeft.Count, actualUuidsLeft.Count);
            Assert.All(expectedUuidsLeft, expected => Assert.Contains(expected, actualUuidsLeft));
        }

        [Fact]
        public async Task Can_PUT_UPDATE_Consequences_With_Rename_Consequences()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int levels = 2;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, levels);
            var uuidsAndNewNamesOfRenamedUnits = new List<(Guid uuid, string name)>();
            DatabaseAccess.MutateEntitySet<OrganizationUnit>(repo =>
            {
                var renamedUnits = repo
                    .AsQueryable()
                    .Where(x => x.Organization.Uuid == targetOrgUuid && x.Origin == OrganizationUnitOrigin.STS_Organisation)
                    .ToList()
                    .RandomItems(2)
                    .ToList();

                foreach (var organizationUnit in renamedUnits)
                {
                    var originalName = organizationUnit.Name;
                    organizationUnit.Name += "_rn1"; //change name so we expect an update to restore the old names
                    uuidsAndNewNamesOfRenamedUnits.Add((organizationUnit.Uuid, originalName));
                }
            });

            //Act
            using var putResponse = await SendPutUpdateConsequencesAsync(targetOrgUuid, levels, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
            var uuidToName = DatabaseAccess.MapFromEntitySet<OrganizationUnit, Dictionary<Guid, string>>(repo =>
            {
                return repo
                    .AsQueryable()
                    .Where(unit => unit.Organization.Uuid == targetOrgUuid)
                    .ToList()
                    .ToDictionary(x => x.Uuid, x => x.Name);
            });
            Assert.All(uuidsAndNewNamesOfRenamedUnits, expected => Assert.Equal(expected.name, uuidToName[expected.uuid]));
        }

        [Fact]
        public async Task Can_PUT_UPDATE_Consequences_With_Relocation_Consequences()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int levels = 2;
            var uuidOfExpectedMoval = Guid.NewGuid();
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, levels);
            (Guid expectedMoval, Guid expectedParent) expectedMoval = (Guid.Empty, Guid.Empty);
            DatabaseAccess.MutateEntitySet<OrganizationUnit>(repo =>
            {
                var twoLeafs = repo
                    .AsQueryable()
                    .Where(x => x.Organization.Uuid == targetOrgUuid && x.Origin == OrganizationUnitOrigin.STS_Organisation && !x.Children.Any())
                    .ToList()
                    .RandomItems(2)
                    .ToList();

                var firstLeaf = twoLeafs.First();
                var secondLeaf = twoLeafs.Last();

                expectedMoval = (secondLeaf.Uuid, secondLeaf.Parent.Uuid); //Save original parent that we expect to be restored

                //Make first leaf parent of second leaf
                secondLeaf.ParentId = firstLeaf.Id;

                //Add a native units so we can check that it is moved along with the moved unit
                var organization = secondLeaf.Organization;
                organization.AddOrganizationUnit(new OrganizationUnit { Organization = organization, Name = "Native test unit", Uuid = uuidOfExpectedMoval, ObjectOwner = secondLeaf.ObjectOwner, LastChangedByUser = secondLeaf.LastChangedByUser }, secondLeaf);
            });

            //Act
            using var putResponse = await SendPutUpdateConsequencesAsync(targetOrgUuid, levels, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
            var actualParentUuid = DatabaseAccess.MapFromEntitySet<OrganizationUnit, Guid>(x => x.AsQueryable().ByUuid(expectedMoval.expectedMoval).Parent.Uuid);
            Assert.Equal(expectedMoval.expectedParent, actualParentUuid);
            var movedItemChildrenUuids = DatabaseAccess.MapFromEntitySet<OrganizationUnit, IEnumerable<Guid>>(x => x.AsQueryable().ByUuid(expectedMoval.expectedMoval).Children.Select(x => x.Uuid).ToList());
            Assert.Contains(uuidOfExpectedMoval, movedItemChildrenUuids);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_PUT_UPDATE_Consequences_With_SubscriptionChanges(bool initiallySubscribe)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int levels = 1;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, levels, initiallySubscribe);

            //Act
            using var putResponse = await SendPutUpdateConsequencesAsync(targetOrgUuid, levels, cookie, !initiallySubscribe);

            //Assert
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
            using var getResponse = await SendGetConnectionStatusAsync(targetOrgUuid, cookie).WithExpectedResponseCode(HttpStatusCode.OK);
            var dto = await getResponse.ReadResponseBodyAsKitosApiResponseAsync<StsOrganizationSynchronizationDetailsResponseDTO>();
            Assert.Equal(!initiallySubscribe, dto.SubscribesToUpdates);

        }

        [Fact]
        public async Task Can_PUT_UPDATE_Consequences_With_Conversion_Consequences()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var globalAdminToken = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int levels = 2;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, levels);
            var expectedConversionUuid = Guid.NewGuid();
            DatabaseAccess.MutateEntitySet<OrganizationUnit>(repo =>
            {
                expectedConversionUuid = repo
                    .AsQueryable()
                    .Where(x => x.Organization.Uuid == targetOrgUuid &&
                                x.Origin == OrganizationUnitOrigin.STS_Organisation && !x.Children.Any())
                    .ToList()
                    .RandomItem()
                    .Uuid;
            });

            //Make sure it is in use so it will not be deleted, but converted
            await ItContractV2Helper.PostContractAsync(globalAdminToken.Token,
                new CreateNewContractRequestDTO()
                {
                    Name = A<string>(),
                    OrganizationUuid = targetOrgUuid,
                    Responsible = new() { OrganizationUnitUuid = expectedConversionUuid }
                });

            //Act
            using var putResponse = await SendPutUpdateConsequencesAsync(targetOrgUuid, levels - 1, cookie);

            //Assert
            var convertedUnit = DatabaseAccess.MapFromEntitySet<OrganizationUnit, OrganizationUnit>(repo => repo.AsQueryable().ByUuid(expectedConversionUuid));
            Assert.Equal(OrganizationUnitOrigin.Kitos, convertedUnit.Origin);
            Assert.Null(convertedUnit.ExternalOriginUuid);
        }

        [Fact]
        public async Task Can_GET_LOGS()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int firstRequestLevels = 2;
            const int secondRequestLevels = 3;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, firstRequestLevels);

            //Addition consequences
            using var additionConsequencesResponse = await SendGetUpdateConsequencesAsync(targetOrgUuid, secondRequestLevels, cookie);
            Assert.Equal(HttpStatusCode.OK, additionConsequencesResponse.StatusCode);
            var additionConsequencesBody = await additionConsequencesResponse.ReadResponseBodyAsKitosApiResponseAsync<ConnectionUpdateConsequencesResponseDTO>();
            var additionConsequences = additionConsequencesBody.Consequences.ToList();

            //Update consequences in order to log addition consequences
            using var additionPutResponse = await SendPutUpdateConsequencesAsync(targetOrgUuid, secondRequestLevels, cookie);
            Assert.Equal(HttpStatusCode.OK, additionPutResponse.StatusCode);

            //Rename consequences
            var renamedUnit = new OrganizationUnit();
            DatabaseAccess.MutateEntitySet<OrganizationUnit>(repo =>
            {
                renamedUnit = repo
                    .AsQueryable()
                    .FirstOrDefault(x => x.Organization.Uuid == targetOrgUuid
                                          && x.Origin == OrganizationUnitOrigin.STS_Organisation
                                          && x.Parent != null
                                          && x.Children.Any());

                Assert.NotNull(renamedUnit);
                renamedUnit.Name += "_rn1"; //change name so we expect an update to restore the old names
            });

            //Conversion consequences
            var globalAdminToken = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var expectedConversionUuid = Guid.NewGuid();
            DatabaseAccess.MutateEntitySet<OrganizationUnit>(repo =>
            {
                expectedConversionUuid = repo
                    .AsQueryable()
                    .Where(x => x.Organization.Uuid == targetOrgUuid &&
                                x.Origin == OrganizationUnitOrigin.STS_Organisation 
                                && x.Parent != null
                                && !x.Children.Any())
                    .ToList()
                    .RandomItem()
                    .Uuid;
            });

            //Make sure it is in use so it will not be deleted, but converted
            await ItContractV2Helper.PostContractAsync(globalAdminToken.Token,
                new CreateNewContractRequestDTO()
                {
                    Name = A<string>(),
                    OrganizationUuid = targetOrgUuid,
                    Responsible = new ContractResponsibleDataWriteRequestDTO { OrganizationUnitUuid = expectedConversionUuid }
                });

            //Relocation consequences
            DatabaseAccess.MutateEntitySet<OrganizationUnit>(repo =>
            {
                var parentLeaf = repo
                    .AsQueryable()
                    .Where(x => x.Organization.Uuid == targetOrgUuid
                                && x.Parent != null
                                && x.Children.Any()
                                && x.Uuid != renamedUnit.Uuid
                                && x.Uuid != expectedConversionUuid)
                    .RandomItem();

                var secondLeaf = repo
                    .AsQueryable()
                    .Where(x => x.Organization.Uuid == targetOrgUuid 
                                && x.Origin == OrganizationUnitOrigin.STS_Organisation
                                && x.Parent != null
                                && x.Children.Any()
                                && x.Uuid != parentLeaf.Uuid
                                && x.Uuid != renamedUnit.Uuid
                                && x.Uuid != expectedConversionUuid)
                    .RandomItem();
                
                secondLeaf.ParentId = parentLeaf.Id;
            }); 
            
            using var otherConsequencesResponse = await SendGetUpdateConsequencesAsync(targetOrgUuid, firstRequestLevels, cookie);
            Assert.Equal(HttpStatusCode.OK, otherConsequencesResponse.StatusCode);
            var otherConsequencesBody = await otherConsequencesResponse.ReadResponseBodyAsKitosApiResponseAsync<ConnectionUpdateConsequencesResponseDTO>();
            var otherConsequences = otherConsequencesBody.Consequences.ToList();

            //Log deletion, renaming, conversion and relocation changes
            using var putResponse = await SendPutUpdateConsequencesAsync(targetOrgUuid, firstRequestLevels, cookie);
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
            
            //Act
            using var logsResponse = await SendGetLogsAsync(targetOrgUuid, 5, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, logsResponse.StatusCode);
            var deserializedLogs = await logsResponse.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<StsOrganizationChangeLogResponseDTO>>();
            var logsList = deserializedLogs.OrderBy(x => x.LogTime).ToList();

            //2 updates + create
            Assert.Equal(3, logsList.Count);

            //Addition consequences
            var additionLogs = logsList[1];
            Assert.NotNull(additionLogs);
            var additionLogsConsequences = additionLogs.Consequences.ToList();

            Assert.Equal(additionConsequences.Count, additionLogsConsequences.Count);
            AssertConsequenceLogs(additionConsequences, additionLogs);

            //Get second item in the list
            var otherLogs = logsList.Last();
            Assert.NotNull(otherLogs);
            var otherLogsConsequences = otherLogs.Consequences.ToList();

            Assert.Equal(otherConsequences.Count, otherLogsConsequences.Count);
            var consequenceCategories = otherLogsConsequences.Select(x => x.Category).ToList();
            Assert.Contains(ConnectionUpdateOrganizationUnitChangeCategory.Deleted, consequenceCategories);
            Assert.Contains(ConnectionUpdateOrganizationUnitChangeCategory.Renamed, consequenceCategories);
            Assert.Contains(ConnectionUpdateOrganizationUnitChangeCategory.Converted, consequenceCategories);
            Assert.Contains(ConnectionUpdateOrganizationUnitChangeCategory.Moved, consequenceCategories);

            AssertConsequenceLogs(otherConsequences, otherLogs);
        }

        private static void AssertImportedTree(StsOrganizationOrgUnitDTO treeToImport, OrganizationUnit importedTree, OrganizationUnitOrigin expectedOrganizationUnitOrigin = OrganizationUnitOrigin.STS_Organisation, int? remainingLevelsToImport = null)
        {
            Assert.Equal(treeToImport.Name, importedTree.Name);
            Assert.Equal(expectedOrganizationUnitOrigin, importedTree.Origin);
            Assert.Equal(expectedOrganizationUnitOrigin == OrganizationUnitOrigin.Kitos ? null : treeToImport.Uuid, importedTree.ExternalOriginUuid);

            remainingLevelsToImport -= 1;

            if (remainingLevelsToImport is < 1)
            {
                Assert.Empty(importedTree.Children); //if no more remaining levels were expected the imported subtree must be empty
            }
            else
            {
                var childrenToImport = treeToImport.Children.OrderBy(x => x.Name).ThenBy(x => x.Uuid.ToString()).ToList();
                var importedUnits = importedTree.Children.OrderBy(x => x.Name).ThenBy(x => x.ExternalOriginUuid.GetValueOrDefault().ToString()).ToList();
                Assert.Equal(childrenToImport.Count, importedUnits.Count);
                for (var i = 0; i < childrenToImport.Count; i++)
                {
                    AssertImportedTree(childrenToImport[i], importedUnits[i], expectedOrganizationUnitOrigin, remainingLevelsToImport);
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

        private static void AssertConsequenceLogs(
            IEnumerable<ConnectionUpdateOrganizationUnitConsequenceDTO> consequences,
            StsOrganizationChangeLogResponseDTO logs)
        {
            var consequencesList = consequences.ToList();
            Assert.Equal(consequencesList.Count, logs.Consequences.Count());
            foreach (var consequence in consequencesList)
            {
                var logConsequence = logs.Consequences.FirstOrDefault(x => x.Uuid == consequence.Uuid && x.Category == consequence.Category);
                Assert.NotNull(logConsequence);

                Assert.Equal(consequence.Uuid, logConsequence.Uuid);
                Assert.Equal(consequence.Category, logConsequence.Category);
                Assert.Equal(consequence.Name, logConsequence.Name);
                Assert.Equal(consequence.Description, logConsequence.Description);
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

        private static async Task<HttpResponseMessage> SendGetSnapshotAsync(int levels, Guid targetOrgUuid, Cookie cookie)
        {
            var url = TestEnvironment.CreateUrl(
                $"api/v1/organizations/{targetOrgUuid:D}/sts-organization-synchronization/snapshot?levels={levels}");
            return await HttpApi.GetWithCookieAsync(url, cookie);
        }

        private static async Task<HttpResponseMessage> SendGetConnectionStatusAsync(Guid targetOrgUuid, Cookie cookie)
        {
            var url = TestEnvironment.CreateUrl(
                $"api/v1/organizations/{targetOrgUuid:D}/sts-organization-synchronization/connection-status");
            return await HttpApi.GetWithCookieAsync(url, cookie);
        }

        private static async Task<HttpResponseMessage> SendPostCreateConnectionAsync(Guid targetOrgUuid, Cookie cookie, int levels, bool subscribe = false)
        {
            var postUrl =
                TestEnvironment.CreateUrl(
                    $"api/v1/organizations/{targetOrgUuid:D}/sts-organization-synchronization/connection");
            return await HttpApi.PostWithCookieAsync(postUrl, cookie, new ConnectToStsOrganizationRequestDTO
            {
                SynchronizationDepth = levels,
                SubscribeToUpdates = subscribe
            });
        }

        private static async Task<HttpResponseMessage> SendGetUpdateConsequencesAsync(Guid targetOrgUuid, int levels, Cookie cookie)
        {
            var getUrl =
                TestEnvironment.CreateUrl(
                    $"api/v1/organizations/{targetOrgUuid:D}/sts-organization-synchronization/connection/update?synchronizationDepth={levels}");
            return await HttpApi.GetWithCookieAsync(getUrl, cookie);
        }

        private static async Task<HttpResponseMessage> SendPutUpdateConsequencesAsync(Guid targetOrgUuid, int levels, Cookie cookie, bool subscribe = false)
        {
            var postUrl =
                TestEnvironment.CreateUrl(
                    $"api/v1/organizations/{targetOrgUuid:D}/sts-organization-synchronization/connection");
            return await HttpApi.PutWithCookieAsync(postUrl, cookie, new ConnectToStsOrganizationRequestDTO
            {
                SynchronizationDepth = levels,
                SubscribeToUpdates = subscribe
            });
        }

        private static async Task<HttpResponseMessage> SendGetLogsAsync(Guid targetOrgUuid, int numberOfChangeLogs, Cookie cookie)
        {
            var postUrl =
                TestEnvironment.CreateUrl(
                    $"api/v1/organizations/{targetOrgUuid:D}/sts-organization-synchronization/connection/change-log?numberOfChangeLogs={numberOfChangeLogs}");
            return await HttpApi.GetWithCookieAsync(postUrl, cookie);
        }
    }
}
