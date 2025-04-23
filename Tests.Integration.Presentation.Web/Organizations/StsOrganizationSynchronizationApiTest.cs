using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.Extensions;
using Core.DomainModel.Organization;
using Core.DomainModel.Tracking;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model.StsOrganization;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
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
            var root = await response.ReadResponseBodyAsAsync<StsOrganizationOrgUnitDTO>();
            Assert.Equal(levels, CountMaxLevels(root));
            AssertOrgTree(root, new HashSet<Guid>());
        }

        [Theory]
        [InlineData(UnAuthorizedCvr, false, CheckConnectionError.FailedToLookupOrganizationCompany)]
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
            var root = await response.ReadResponseBodyAsAsync<StsOrganizationSynchronizationDetailsResponseDTO>();
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
            var expectedImport = await getResponse.ReadResponseBodyAsAsync<StsOrganizationOrgUnitDTO>();

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_DELETE_Connection(bool purge)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int levels = 2;
            var connectionUrl = TestEnvironment.CreateUrl($"{GetBaseConnectionString(targetOrgUuid)}/connection");
            var getUrl = TestEnvironment.CreateUrl($"{GetBaseConnectionString(targetOrgUuid)}/snapshot?levels={levels}");
            using var getResponse = await HttpApi.GetWithCookieAsync(getUrl, cookie);
            var expectedStructureAfterDisconnect = await getResponse.ReadResponseBodyAsAsync<StsOrganizationOrgUnitDTO>();
            if (purge)
            {
                //We expect all of the external sub units to have been removed
                expectedStructureAfterDisconnect.Children = Array.Empty<StsOrganizationOrgUnitDTO>();
            }
            using var response = await HttpApi.PostWithCookieAsync(connectionUrl, cookie, new ConnectToStsOrganizationRequestDTO
            {
                SynchronizationDepth = levels
            });
            //Act
            using var deleteResponse = await HttpApi.DeleteWithCookieAsync(connectionUrl, cookie, new DisconnectFromStsOrganizationRequestDTO()
            {
                PurgeUnusedExternalUnits = purge
            });

            //Assert
            DatabaseAccess.MapFromEntitySet<Organization, bool>(orgs =>
            {
                var organization = orgs.AsQueryable().ByUuid(targetOrgUuid);
                var dbRoot = organization.GetRoot();
                Assert.NotNull(organization.StsOrganizationConnection);
                Assert.False(organization.StsOrganizationConnection.Connected);
                Assert.Null(organization.StsOrganizationConnection.SynchronizationDepth);

                //Assert that the imported stuff is till there - just converted to kitos units
                AssertImportedTree(expectedStructureAfterDisconnect, dbRoot, OrganizationUnitOrigin.Kitos);
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
            var consequences = await getResponse.ReadResponseBodyAsAsync<ConnectionUpdateConsequencesResponseDTO>();
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
            var consequences = await consequencesResponse.ReadResponseBodyAsAsync<ConnectionUpdateConsequencesResponseDTO>();
            Assert.NotEmpty(consequences.Consequences);
            Assert.All(consequences.Consequences, c => Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Added, c.Category));
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
            var consequences = await consequencesResponse.ReadResponseBodyAsAsync<ConnectionUpdateConsequencesResponseDTO>();
            Assert.NotEmpty(consequences.Consequences);
            Assert.All(consequences.Consequences, c => Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Deleted, c.Category));
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
            var consequences = await consequencesResponse.ReadResponseBodyAsAsync<ConnectionUpdateConsequencesResponseDTO>();
            Assert.NotEmpty(consequences.Consequences);
            Assert.All(consequences.Consequences, c => Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Renamed, c.Category));
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
            var consequences = await consequencesResponse.ReadResponseBodyAsAsync<ConnectionUpdateConsequencesResponseDTO>();
            Assert.NotEmpty(consequences.Consequences);
            Assert.All(consequences.Consequences, c => Assert.Equal(ConnectionUpdateOrganizationUnitChangeType.Moved, c.Category));
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
            var consequences = await consequencesResponse.ReadResponseBodyAsAsync<ConnectionUpdateConsequencesResponseDTO>();
            Assert.NotEmpty(consequences.Consequences);
            var conversion = Assert.Single(consequences.Consequences.Where(x => x.Category == ConnectionUpdateOrganizationUnitChangeType.Converted));
            Assert.Equal(expectedConvertedUnit, conversion.Uuid);
        }

        [Fact]
        public async Task Can_PUT_UPDATE_With_Addition_Consequences()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int firstRequestLevels = 2;
            const int secondRequestLevels = 3;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, firstRequestLevels);

            using var consequencesBeforePutResponse = await SendGetUpdateConsequencesAsync(targetOrgUuid, secondRequestLevels, cookie);
            Assert.Equal(HttpStatusCode.OK, consequencesBeforePutResponse.StatusCode);
            var consequencesBeforePut = await consequencesBeforePutResponse.ReadResponseBodyAsAsync<ConnectionUpdateConsequencesResponseDTO>();
            var expectedAdded = consequencesBeforePut.Consequences.ToList().Where(x => x.Category == ConnectionUpdateOrganizationUnitChangeType.Added).Select(x => x.Uuid).ToHashSet();
            Assert.NotEmpty(expectedAdded);

            //Act
            using var putResponse = await SendPutUpdateStsOrganizationConnectionAsync(targetOrgUuid, secondRequestLevels, cookie);
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
        public async Task Can_PUT_UPDATE_With_Removal_Consequences()
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

            var expectedRemovedUuids = DatabaseAccess.MapFromEntitySet<OrganizationUnit, IEnumerable<Guid>>(repository =>
                    repository
                        .AsQueryable()
                        .Where(unit => unit.Organization.Uuid == targetOrgUuid)
                        .Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation)
                        .Where(x => !expectedUuidsLeft.Contains(x.ExternalOriginUuid.Value))
                        .Select(x => x.Uuid) //selecting the kitos uuid
                        .ToList())
                .ToList();

            //Act
            using var putResponse = await SendPutUpdateStsOrganizationConnectionAsync(targetOrgUuid, secondRequestLevels, cookie);
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

            //Ensure that deletions were tracked
            var trackedDeletions = DatabaseAccess.MapFromEntitySet<LifeCycleTrackingEvent, IEnumerable<Guid>>(repository =>
                    repository
                        .AsQueryable()
                        .Where(unit => expectedRemovedUuids.Contains(unit.EntityUuid))
                        .Select(x => x.EntityUuid)
                        .ToList())
                .ToList();
            Assert.Equal(expectedRemovedUuids.Count, trackedDeletions.Count);
            Assert.Contains(trackedDeletions, uuid => expectedRemovedUuids.Contains(uuid));
        }

        [Fact]
        public async Task Can_PUT_UPDATE_With_Rename_Consequences()
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
            using var putResponse = await SendPutUpdateStsOrganizationConnectionAsync(targetOrgUuid, levels, cookie);

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
        public async Task Can_PUT_UPDATE_With_Relocation_Consequences()
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
            using var putResponse = await SendPutUpdateStsOrganizationConnectionAsync(targetOrgUuid, levels, cookie);

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
        public async Task Can_PUT_UPDATE_With_SubscriptionChanges(bool initiallySubscribe)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int levels = 1;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, levels, initiallySubscribe);

            //Act
            using var putResponse = await SendPutUpdateStsOrganizationConnectionAsync(targetOrgUuid, levels, cookie, !initiallySubscribe);

            //Assert
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
            using var getResponse = await SendGetConnectionStatusAsync(targetOrgUuid, cookie).WithExpectedResponseCode(HttpStatusCode.OK);
            var dto = await getResponse.ReadResponseBodyAsAsync<StsOrganizationSynchronizationDetailsResponseDTO>();
            Assert.Equal(!initiallySubscribe, dto.SubscribesToUpdates);
        }

        [Fact]
        public async Task Can_DELETE_Subscription()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr, true, true);

            //Act
            using var putResponse = await SendDeleteSubscriptionAsync(targetOrgUuid, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
            var subscriptionRemoved = DatabaseAccess.MapFromEntitySet<Organization, bool>(r => r.AsQueryable().ByUuid(targetOrgUuid).StsOrganizationConnection?.SubscribeToUpdates == false);
            Assert.True(subscriptionRemoved);
        }

        [Fact]
        public async Task Can_PUT_UPDATE_With_Conversion_Consequences()
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
            using var putResponse = await SendPutUpdateStsOrganizationConnectionAsync(targetOrgUuid, levels - 1, cookie);

            //Assert
            var convertedUnit = DatabaseAccess.MapFromEntitySet<OrganizationUnit, OrganizationUnit>(repo => repo.AsQueryable().ByUuid(expectedConversionUuid));
            Assert.Equal(OrganizationUnitOrigin.Kitos, convertedUnit.Origin);
            Assert.Null(convertedUnit.ExternalOriginUuid);
        }

        [Fact]
        public async Task Can_PUT_UPDATE_With_Hierarchy_Replacement_Consequences()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int firstRequestLevels = 2;
            const int secondRequestLevels = 3;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, firstRequestLevels);

            DatabaseAccess.MutateEntitySet<OrganizationUnit>(units =>
            {
                var externalUnits = units.AsQueryable().Where(x => x.Organization.Uuid == targetOrgUuid && x.Origin == OrganizationUnitOrigin.STS_Organisation).ToList();
                //Change uuid of all of them to provoke a swap
                foreach (var organizationUnit in externalUnits)
                {
                    organizationUnit.ExternalOriginUuid = Guid.NewGuid();
                }
            });

            //Act
            using var putResponse = await SendPutUpdateStsOrganizationConnectionAsync(targetOrgUuid, secondRequestLevels, cookie);
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        }

        [Fact]
        public async Task Can_PUT_UPDATE_With_Hierarchy_Swap()
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var targetOrgUuid = await CreateOrgWithCvr(AuthorizedCvr);
            const int firstRequestLevels = 2;
            const int secondRequestLevels = 3;
            using var postResponse = await SendPostCreateConnectionAsync(targetOrgUuid, cookie, firstRequestLevels);

            //Swap root and random leaf in hierarchy
            DatabaseAccess.MutateEntitySet<OrganizationUnit>(units =>
            {

                var externalUnits = units.AsQueryable().Where(x => x.Organization.Uuid == targetOrgUuid && x.Origin == OrganizationUnitOrigin.STS_Organisation).ToList();
                var root = externalUnits.Single(x => x.Parent == null);
                var leaf = externalUnits.Where(x => x.IsLeaf()).RandomItem();

                var rootUuid = root.ExternalOriginUuid;
                var leafUuid = leaf.ExternalOriginUuid;
                root.ExternalOriginUuid = leafUuid;
                leaf.ExternalOriginUuid = rootUuid;
            });

            //Act
            using var putResponse = await SendPutUpdateStsOrganizationConnectionAsync(targetOrgUuid, secondRequestLevels, cookie);
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
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
            var additionConsequencesBody = await additionConsequencesResponse.ReadResponseBodyAsAsync<ConnectionUpdateConsequencesResponseDTO>();
            var additionConsequences = additionConsequencesBody.Consequences.ToList();

            //Update consequences in order to log addition consequences
            using var additionPutResponse = await SendPutUpdateStsOrganizationConnectionAsync(targetOrgUuid, secondRequestLevels, cookie);
            Assert.Equal(HttpStatusCode.OK, additionPutResponse.StatusCode);

            //Setup other consequences
            var expectedConversionUuid = Guid.NewGuid();
            DatabaseAccess.MutateEntitySet<OrganizationUnit>(repo =>
            {
                var availableUnits = repo
                    .AsQueryable()
                    .Where(x => x.Organization.Uuid == targetOrgUuid
                                && x.Origin == OrganizationUnitOrigin.STS_Organisation
                                && x.Parent != null
                                && x.Children.Any())
                    .ToList();

                var unitToRename = availableUnits.FirstOrDefault();
                Assert.NotNull(unitToRename);
                availableUnits.Remove(unitToRename);

                var unitToMove = availableUnits.FirstOrDefault();
                Assert.NotNull(unitToMove);
                availableUnits.Remove(unitToMove);

                var targetUnit = availableUnits.FirstOrDefault(x => x.Id != unitToMove.ParentId);
                Assert.NotNull(targetUnit);

                //Since the unitToRename won't be moved and all of it's children are meant for deletion select a unit to convert from there
                var unitToConvert = unitToRename.Children.FirstOrDefault();
                Assert.NotNull(unitToConvert);
                expectedConversionUuid = unitToConvert.Uuid;

                unitToRename.Name += "_rn1"; //change name so we expect an update to restore the old names
                unitToMove.ParentId = targetUnit.Id;
            });

            var globalAdminToken = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            await ItContractV2Helper.PostContractAsync(globalAdminToken.Token,
                new CreateNewContractRequestDTO
                {
                    Name = A<string>(),
                    OrganizationUuid = targetOrgUuid,
                    Responsible = new ContractResponsibleDataWriteRequestDTO { OrganizationUnitUuid = expectedConversionUuid }
                });

            using var otherConsequencesResponse = await SendGetUpdateConsequencesAsync(targetOrgUuid, firstRequestLevels, cookie);
            Assert.Equal(HttpStatusCode.OK, otherConsequencesResponse.StatusCode);
            var otherConsequencesBody = await otherConsequencesResponse.ReadResponseBodyAsAsync<ConnectionUpdateConsequencesResponseDTO>();
            var otherConsequences = otherConsequencesBody.Consequences.ToList();

            //Log deletion, renaming, conversion and relocation changes
            using var putResponse = await SendPutUpdateStsOrganizationConnectionAsync(targetOrgUuid, firstRequestLevels, cookie);
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

            //Act
            using var logsResponse = await SendGetLogsAsync(targetOrgUuid, 5, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.OK, logsResponse.StatusCode);
            var deserializedLogs = await logsResponse.ReadResponseBodyAsAsync<IEnumerable<StsOrganizationChangeLogResponseDTO>>();
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
            Assert.Contains(ConnectionUpdateOrganizationUnitChangeType.Deleted, consequenceCategories);
            Assert.Contains(ConnectionUpdateOrganizationUnitChangeType.Renamed, consequenceCategories);
            Assert.Contains(ConnectionUpdateOrganizationUnitChangeType.Converted, consequenceCategories);
            Assert.Contains(ConnectionUpdateOrganizationUnitChangeType.Moved, consequenceCategories);

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

        private async Task<Guid> CreateOrgWithCvr(string cvr, bool fakeInitialConnection = false, bool fakeInitialSubscription = false)
        {
            var org = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, $"StsSync_{A<Guid>():N}", cvr, OrganizationTypeKeys.Kommune, AccessModifier.Public);
            if (fakeInitialConnection)
            {
                DatabaseAccess.MutateEntitySet<Organization>(repo =>
                {
                    var organization = repo.AsQueryable().ByUuid(org.Uuid);
                    organization.ConnectToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, new ExternalOrganizationUnit(Guid.NewGuid(), "FAKE ROOT", new Dictionary<string, string>(), new List<ExternalOrganizationUnit>()), Maybe<int>.Some(1), fakeInitialSubscription);
                });
            }
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
                $"{GetBaseConnectionString(targetOrgUuid)}/snapshot?levels={levels}");
            return await HttpApi.GetWithCookieAsync(url, cookie);
        }

        private static async Task<HttpResponseMessage> SendGetConnectionStatusAsync(Guid targetOrgUuid, Cookie cookie)
        {
            var url = TestEnvironment.CreateUrl(
                $"{GetBaseConnectionString(targetOrgUuid)}/connection-status");
            return await HttpApi.GetWithCookieAsync(url, cookie);
        }

        private static async Task<HttpResponseMessage> SendPostCreateConnectionAsync(Guid targetOrgUuid, Cookie cookie, int levels, bool subscribe = false)
        {
            var postUrl =
                TestEnvironment.CreateUrl(
                    $"{GetBaseConnectionString(targetOrgUuid)}/connection");
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
                    $"{GetBaseConnectionString(targetOrgUuid)}/connection/update?synchronizationDepth={levels}");
            return await HttpApi.GetWithCookieAsync(getUrl, cookie);
        }

        private static async Task<HttpResponseMessage> SendPutUpdateStsOrganizationConnectionAsync(Guid targetOrgUuid, int levels, Cookie cookie, bool subscribe = false)
        {
            var postUrl =
                TestEnvironment.CreateUrl(
                    $"{GetBaseConnectionString(targetOrgUuid)}/connection");
            return await HttpApi.PutWithCookieAsync(postUrl, cookie, new ConnectToStsOrganizationRequestDTO
            {
                SynchronizationDepth = levels,
                SubscribeToUpdates = subscribe
            });
        }

        private static async Task<HttpResponseMessage> SendDeleteSubscriptionAsync(Guid targetOrgUuid, Cookie cookie)
        {
            var postUrl = TestEnvironment.CreateUrl($"{GetBaseConnectionString(targetOrgUuid)}/connection/subscription");
            return await HttpApi.DeleteWithCookieAsync(postUrl, cookie);
        }

        private static async Task<HttpResponseMessage> SendGetLogsAsync(Guid targetOrgUuid, int numberOfChangeLogs, Cookie cookie)
        {
            var postUrl =
                TestEnvironment.CreateUrl(
                    $"{GetBaseConnectionString(targetOrgUuid)}/connection/change-log?numberOfChangeLogs={numberOfChangeLogs}");
            return await HttpApi.GetWithCookieAsync(postUrl, cookie);
        }

        private static string GetBaseConnectionString(Guid targetOrgUuid)
        {
            return $"api/v2/internal/organizations/{targetOrgUuid:D}/sts-organization-synchronization";
        }
    }
}
