using System.Linq;
using Core.DomainModel;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Xunit;
using System.Net;
using Core.DomainServices.Extensions;
using Core.DomainModel.Organization;
using System;
using Core.Abstractions.Extensions;
using Presentation.Web.Models.API.V2.Response.Organization;
using Tests.Integration.Presentation.Web.Tools.Internal;

namespace Tests.Integration.Presentation.Web.ItSystem.V2
{
    [Collection(nameof(SequentialTestGroup))]
    public class ItSystemsInternalApiV2Test : BaseItSystemsApiV2Test
    {
        [Fact]
        public async Task GET_Many_Internal_Without_Filters()
        {
            //Arrange
            var (cookie, organization) = await CreateCookieStakeHolderUserInNewOrganizationAsync();
            await CreateSystemAsync(organization.Uuid, AccessModifier.Local);
            await CreateSystemAsync(DefaultOrgUuid, AccessModifier.Public);

            //Act
            var systems = await ItSystemV2Helper.GetManyInternalAsync(cookie, pageSize: 2);

            //Assert
            Assert.Equal(2, systems.Count());
        }

        [Fact]
        public async Task GET_Many_Internal_With_RightsHolderFilter()
        {
            //Arrange
            var (cookie, organization) = await CreateCookieStakeHolderUserInNewOrganizationAsync();
            var otherOrganization = await CreateOrganizationAsync();

            var expected1 = await CreateSystemAsync(DefaultOrgUuid, AccessModifier.Local);
            var expected2 = await CreateSystemAsync(DefaultOrgUuid, AccessModifier.Public);
            var expected3 = await CreateSystemAsync(otherOrganization.Uuid, AccessModifier.Local);


            using var resp1 = await ItSystemV2Helper.PatchRightsHolderAsync(expected1, otherOrganization.Uuid);
            using var resp2 = await ItSystemV2Helper.PatchRightsHolderAsync(expected2, otherOrganization.Uuid);
            using var resp3 = await ItSystemV2Helper.PatchRightsHolderAsync(expected3, otherOrganization.Uuid);

            Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp3.StatusCode);

            //Act
            var systems = (await ItSystemV2Helper.GetManyInternalAsync(cookie, rightsHolderId: otherOrganization.Uuid)).ToList();

            Assert.Equal(3, systems.Count);
            Assert.Contains(systems, dto => dto.Uuid == expected1);
            Assert.Contains(systems, dto => dto.Uuid == expected2);
            Assert.Contains(systems, dto => dto.Uuid == expected3);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GET_Many_Internal_As_StakeHolder_Depends_On_IncludeDeactivated(bool shouldIncludeDeactivated)
        {
            //Arrange
            var (cookie, _) = await CreateCookieStakeHolderUserInNewOrganizationAsync();
            var rightsHolder = await CreateOrganizationAsync();

            var inactive = await CreateSystemAsync(DefaultOrgUuid, AccessModifier.Public);
            var active = await CreateSystemAsync(DefaultOrgUuid, AccessModifier.Public);

            using var resp1 = await ItSystemV2Helper.PatchRightsHolderAsync(inactive, rightsHolder.Uuid);
            using var resp2 = await ItSystemV2Helper.PatchRightsHolderAsync(active, rightsHolder.Uuid);

            Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);

            DatabaseAccess.MutateDatabase(db =>
            {
                var dbSystem = db.ItSystems.AsQueryable().ByUuid(inactive);
                dbSystem.Disabled = true;
                db.SaveChanges();
            });

            //Act
            var systems = (await ItSystemV2Helper.GetManyInternalAsync(cookie, rightsHolderId: rightsHolder.Uuid, includeDeactivated: shouldIncludeDeactivated)).ToList(); // Limit to only take systems in rightsholder org

            //Assert
            if (shouldIncludeDeactivated)
            {
                Assert.Equal(2, systems.Count);
                var activeSystemDTO = systems.First(x => x.Uuid.Equals(active));
                Assert.False(activeSystemDTO.Deactivated);
                var inactiveSystemDTO = systems.First(x => x.Uuid.Equals(inactive));
                Assert.True(inactiveSystemDTO.Deactivated);
            }
            else
            {
                var systemResult = Assert.Single(systems);
                Assert.Equal(systemResult.Uuid, active);
                Assert.False(systemResult.Deactivated);
            }
        }

        [Fact]
        public async Task GET_Many_Internal_As_StakeHolder_With_BusinessTypeFilter()
        {
            //Arrange
            var (cookie, _) = await CreateCookieStakeHolderUserInNewOrganizationAsync();
            var businessType1 = A<string>();
            var businessType2 = A<string>();

            var correctBusinessType = await GlobalOptionTypeV2Helper.CreateAndActivateGlobalOption(GlobalOptionTypeV2Helper.BusinessTypes, businessType1);
            var incorrectBusinessType = await GlobalOptionTypeV2Helper.CreateAndActivateGlobalOption(GlobalOptionTypeV2Helper.BusinessTypes, businessType2);

            var unexpectedWrongBusinessType = await CreateSystemAsync(DefaultOrgUuid, AccessModifier.Public);
            var expected = await CreateSystemAsync(DefaultOrgUuid, AccessModifier.Public);

            using var setBt1 = await ItSystemV2Helper.SendPatchBusinessTypeAsync(await GetGlobalToken(), expected, correctBusinessType.Uuid);
            using var setBt2 = await ItSystemV2Helper.SendPatchBusinessTypeAsync(await GetGlobalToken(), unexpectedWrongBusinessType, incorrectBusinessType.Uuid);
            Assert.Equal(HttpStatusCode.OK, setBt1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, setBt2.StatusCode);

            //Act
            var systems = (await ItSystemV2Helper.GetManyInternalAsync(cookie, businessTypeId: correctBusinessType.Uuid)).ToList();

            //Assert
            var dto = Assert.Single(systems);
            Assert.Equal(dto.Uuid, expected);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GET_Many_Internal_As_StakeHolder_With_KLE_Filter(bool useKeyAsFilter)
        {
            //Arrange
            var (cookie, _) = await CreateCookieStakeHolderUserInNewOrganizationAsync();
            var rand = new Random(DateTime.UtcNow.Millisecond);

            var correctRef = new
            {
                uuid = A<Guid>(),
                key = rand.Next().ToString("D"),
            };
            var incorrectRef = new
            {
                uuid = A<Guid>(),
                key = rand.Next().ToString("D"),
            };

            CreateTaskRefInDatabase(correctRef.key, correctRef.uuid);
            CreateTaskRefInDatabase(incorrectRef.key, incorrectRef.uuid);


            var systemWithWrongRef = await CreateSystemAsync(DefaultOrgUuid, AccessModifier.Public);
            var system1WithCorrectRef = await CreateSystemAsync(DefaultOrgUuid, AccessModifier.Public);
            var system2WithCorrectRef = await CreateSystemAsync(DefaultOrgUuid, AccessModifier.Public);

            using var addRefResponse1 = await ItSystemV2Helper.SendPatchSystemAsync(await GetGlobalToken(), system1WithCorrectRef, x => x.KLEUuids, correctRef.uuid.WrapAsEnumerable());
            using var addRefResponse2 = await ItSystemV2Helper.SendPatchSystemAsync(await GetGlobalToken(), system2WithCorrectRef, x => x.KLEUuids, correctRef.uuid.WrapAsEnumerable());
            using var addRefResponse3 = await ItSystemV2Helper.SendPatchSystemAsync(await GetGlobalToken(), systemWithWrongRef, x => x.KLEUuids, incorrectRef.uuid.WrapAsEnumerable());

            Assert.Equal(HttpStatusCode.OK, addRefResponse1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, addRefResponse2.StatusCode);
            Assert.Equal(HttpStatusCode.OK, addRefResponse3.StatusCode);

            //Act
            var kleKeyFilter = useKeyAsFilter ? correctRef.key : null;
            var kleUuidFilter = useKeyAsFilter ? (Guid?)null : correctRef.uuid;
            var systems = (await ItSystemV2Helper.GetManyInternalAsync(cookie, kleKey: kleKeyFilter, kleUuid: kleUuidFilter)).ToList();

            //Assert
            Assert.Equal(2, systems.Count);
            Assert.Contains(systems, x => x.Uuid == system1WithCorrectRef);
            Assert.Contains(systems, x => x.Uuid == system2WithCorrectRef);
        }

        [Fact]
        public async Task GET_Many_Internal_As_StakeHolder_With_NumberOfUsers_Filter()
        {
            //Arrange - Scope the test with additional rightsHolder filter so that we can control which response we get
            var (cookie, organization) = await CreateCookieStakeHolderUserInNewOrganizationAsync();
            var rightsHolder = await CreateOrganizationAsync();

            var excludedSinceTooFewUsages = await CreateSystemAsync(DefaultOrgUuid, AccessModifier.Public);
            var includedLowerBound = await CreateSystemAsync(DefaultOrgUuid, AccessModifier.Public);
            var includedAboveLowerBound = await CreateSystemAsync(DefaultOrgUuid, AccessModifier.Public);

            using var resp1 = await ItSystemV2Helper.PatchRightsHolderAsync(excludedSinceTooFewUsages, rightsHolder.Uuid);
            using var resp2 = await ItSystemV2Helper.PatchRightsHolderAsync(includedLowerBound, rightsHolder.Uuid);
            using var resp3 = await ItSystemV2Helper.PatchRightsHolderAsync(includedAboveLowerBound, rightsHolder.Uuid);

            await TakeSystemIntoUsageAsync(excludedSinceTooFewUsages, DefaultOrgUuid);
            await TakeMultipleSystemsIntoUsageAsync(includedLowerBound, DefaultOrgUuid, SecondOrgUuid);
            await TakeMultipleSystemsIntoUsageAsync(includedAboveLowerBound, DefaultOrgUuid, SecondOrgUuid, rightsHolder.Uuid);

            Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp3.StatusCode);

            //Act
            var systems = (await ItSystemV2Helper.GetManyInternalAsync(cookie, rightsHolderId: rightsHolder.Uuid, numberOfUsers: 2)).ToList();

            //Assert - only 2 are actually valid since the excluded one was hidden to the stakeholder
            Assert.Equal(2, systems.Count);
            Assert.Contains(systems, dto => dto.Uuid == includedLowerBound);
            Assert.Contains(systems, dto => dto.Uuid == includedAboveLowerBound);
        }

        [Fact]
        public async Task GET_Many_Internal_As_StakeHolder_With_ChangesSince_Filter()
        {
            //Arrange
            var (cookie, organization) = await CreateCookieStakeHolderUserInNewOrganizationAsync();
            var system1 = await CreateItSystemAsync(organization.Uuid);
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var system3 = await CreateItSystemAsync(organization.Uuid);

            await ItSystemV2Helper.SendPatchSystemNameAsync(await GetGlobalToken(), system2.Uuid, CreateName()).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await ItSystemV2Helper.SendPatchSystemNameAsync(await GetGlobalToken(), system3.Uuid, CreateName()).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            await ItSystemV2Helper.SendPatchSystemNameAsync(await GetGlobalToken(), system1.Uuid, CreateName()).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            var system3DTO = await ItSystemV2Helper.GetSingleAsync(await GetGlobalToken(), system3.Uuid); //system 3 was changed as the second one and system 1 the last


            //Act
            var dtos = (await ItSystemV2Helper.GetManyInternalAsync(cookie, changedSinceGtEq: system3DTO.LastModified, page: 0, pageSize: 30)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            Assert.Equal(new[] { system3.Uuid, system1.Uuid }, dtos.Select(x => x.Uuid).ToArray());
        }

        [Fact]
        public async Task GET_Many_Internal_As_StakeHolder_With_NameEqual_Filter()
        {
            //Arrange
            var (cookie, organization) = await CreateCookieStakeHolderUserInNewOrganizationAsync();
            var searchName = CreateName();
            var invalidSearchName = $"{searchName}1";
            await CreateItSystemAsync(organization.Uuid, invalidSearchName);
            var system2 = await CreateItSystemAsync(organization.Uuid, searchName);

            //Act
            var dtos = (await ItSystemV2Helper.GetManyInternalAsync(cookie, nameEquals: system2.Name, page: 0, pageSize: 10)).ToList();

            //Assert
            var dto = Assert.Single(dtos);
            Assert.Equal(system2.Name, dto.Name);
            Assert.Equal(system2.Uuid, dto.Uuid);
        }

        [Fact]
        public async Task GET_Many_Internal_As_StakeHolder_With_NameContains_Filter()
        {
            //Arrange
            var (cookie, organization) = await CreateCookieStakeHolderUserInNewOrganizationAsync();
            var baseName = CreateName();
            var searchName = $"{baseName}1";
            var validName2 = $"{searchName}2";

            await CreateItSystemAsync(organization.Uuid, baseName);
            var system2 = await CreateItSystemAsync(organization.Uuid, searchName);
            var system3 = await CreateItSystemAsync(organization.Uuid, validName2);

            //Act
            var dtos = (await ItSystemV2Helper.GetManyInternalAsync(cookie, nameContains: searchName, page: 0, pageSize: 10)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            Assert.Equal(new[] { system2.Uuid, system3.Uuid }, dtos.Select(x => x.Uuid).ToArray());
        }

        protected async Task<(Cookie cookie, ShallowOrganizationResponseDTO createdOrganization)> CreateCookieStakeHolderUserInNewOrganizationAsync()
        {
            var organization = await CreateOrganizationAsync();

            var (_, _, cookie) = await HttpApi.CreateUserAndLogin(CreateEmail(),
                OrganizationRole.User, organization.Uuid, false, true);
            return (cookie, organization);
        }
    }
}
