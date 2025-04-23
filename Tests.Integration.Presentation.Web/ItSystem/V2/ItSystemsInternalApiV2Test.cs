using System.Linq;
using Core.DomainModel;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Xunit;
using System.Net;
using Core.DomainServices.Extensions;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using System;
using Presentation.Web.Models.API.V1;

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
            await CreateSystemAsync(organization.Id, AccessModifier.Local);
            await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);

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

            var expected1 = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var expected2 = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var expected3 = await CreateSystemAsync(otherOrganization.Id, AccessModifier.Local);

            using var resp1 = await ItSystemHelper.SendSetBelongsToRequestAsync(expected1.dbId, otherOrganization.Id, TestEnvironment.DefaultOrganizationId);
            using var resp2 = await ItSystemHelper.SendSetBelongsToRequestAsync(expected2.dbId, otherOrganization.Id, TestEnvironment.DefaultOrganizationId);
            using var resp3 = await ItSystemHelper.SendSetBelongsToRequestAsync(expected3.dbId, otherOrganization.Id, otherOrganization.Id);

            Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp3.StatusCode);

            //Act
            var systems = (await ItSystemV2Helper.GetManyInternalAsync(cookie, rightsHolderId: otherOrganization.Uuid)).ToList();

            Assert.Equal(3, systems.Count);
            Assert.Contains(systems, dto => dto.Uuid == expected1.uuid);
            Assert.Contains(systems, dto => dto.Uuid == expected2.uuid);
            Assert.Contains(systems, dto => dto.Uuid == expected3.uuid);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GET_Many_Internal_As_StakeHolder_Depends_On_IncludeDeactivated(bool shouldIncludeDeactivated)
        {
            //Arrange
            var (cookie, _) = await CreateCookieStakeHolderUserInNewOrganizationAsync();
            var rightsHolder = await CreateOrganizationAsync();

            var inactive = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var active = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);

            using var resp1 = await ItSystemHelper.SendSetBelongsToRequestAsync(inactive.dbId, rightsHolder.Id, TestEnvironment.DefaultOrganizationId);
            using var resp2 = await ItSystemHelper.SendSetBelongsToRequestAsync(active.dbId, rightsHolder.Id, TestEnvironment.DefaultOrganizationId);

            Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);

            DatabaseAccess.MutateDatabase(db =>
            {
                var dbSystem = db.ItSystems.AsQueryable().ById(inactive.dbId);
                dbSystem.Disabled = true;
                db.SaveChanges();
            });

            //Act
            var systems = (await ItSystemV2Helper.GetManyInternalAsync(cookie, rightsHolderId: rightsHolder.Uuid, includeDeactivated: shouldIncludeDeactivated)).ToList(); // Limit to only take systems in rightsholder org

            //Assert
            if (shouldIncludeDeactivated)
            {
                Assert.Equal(2, systems.Count);
                var activeSystemDTO = systems.First(x => x.Uuid.Equals(active.uuid));
                Assert.False(activeSystemDTO.Deactivated);
                var inactiveSystemDTO = systems.First(x => x.Uuid.Equals(inactive.uuid));
                Assert.True(inactiveSystemDTO.Deactivated);
            }
            else
            {
                var systemResult = Assert.Single(systems);
                Assert.Equal(systemResult.Uuid, active.uuid);
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
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var correctBusinessType = await EntityOptionHelper.CreateOptionTypeAsync(EntityOptionHelper.ResourceNames.BusinessType, businessType1, organizationId);
            var incorrectBusinessType = await EntityOptionHelper.CreateOptionTypeAsync(EntityOptionHelper.ResourceNames.BusinessType, businessType2, organizationId);
            var correctBusinessTypeId = DatabaseAccess.GetEntityUuid<BusinessType>(correctBusinessType.Id);

            var unexpectedWrongBusinessType = await CreateSystemAsync(organizationId, AccessModifier.Public);
            var expected = await CreateSystemAsync(organizationId, AccessModifier.Public);

            using var setBt1 = await ItSystemHelper.SendSetBusinessTypeRequestAsync(expected.dbId, correctBusinessType.Id, organizationId);
            using var setBt2 = await ItSystemHelper.SendSetBusinessTypeRequestAsync(unexpectedWrongBusinessType.dbId, incorrectBusinessType.Id, organizationId);
            Assert.Equal(HttpStatusCode.OK, setBt1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, setBt2.StatusCode);

            //Act
            var systems = (await ItSystemV2Helper.GetManyInternalAsync(cookie, businessTypeId: correctBusinessTypeId)).ToList();

            //Assert
            var dto = Assert.Single(systems);
            Assert.Equal(dto.Uuid, expected.uuid);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GET_Many_Internal_As_StakeHolder_With_KLE_Filter(bool useKeyAsFilter)
        {
            //Arrange
            var (cookie, _) = await CreateCookieStakeHolderUserInNewOrganizationAsync();
            const int organizationId = TestEnvironment.DefaultOrganizationId;
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

            var correctRefDbId = DatabaseAccess.MapFromEntitySet<TaskRef, int>(rep => rep.AsQueryable().ByUuid(correctRef.uuid).Id);
            var incorrectRefDbId = DatabaseAccess.MapFromEntitySet<TaskRef, int>(rep => rep.AsQueryable().ByUuid(incorrectRef.uuid).Id);

            var systemWithWrongRef = await CreateSystemAsync(organizationId, AccessModifier.Public);
            var system1WithCorrectRef = await CreateSystemAsync(organizationId, AccessModifier.Public);
            var system2WithCorrectRef = await CreateSystemAsync(organizationId, AccessModifier.Public);

            using var addRefResponse1 = await ItSystemHelper.SendAddTaskRefRequestAsync(system1WithCorrectRef.dbId, correctRefDbId, organizationId);
            using var addRefResponse2 = await ItSystemHelper.SendAddTaskRefRequestAsync(system2WithCorrectRef.dbId, correctRefDbId, organizationId);
            using var addRefResponse3 = await ItSystemHelper.SendAddTaskRefRequestAsync(systemWithWrongRef.dbId, incorrectRefDbId, organizationId);
            Assert.Equal(HttpStatusCode.OK, addRefResponse1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, addRefResponse2.StatusCode);
            Assert.Equal(HttpStatusCode.OK, addRefResponse3.StatusCode);

            //Act
            var kleKeyFilter = useKeyAsFilter ? correctRef.key : null;
            var kleUuidFilter = useKeyAsFilter ? (Guid?)null : correctRef.uuid;
            var systems = (await ItSystemV2Helper.GetManyInternalAsync(cookie, kleKey: kleKeyFilter, kleUuid: kleUuidFilter)).ToList();

            //Assert
            Assert.Equal(2, systems.Count);
            Assert.Contains(systems, x => x.Uuid == system1WithCorrectRef.uuid);
            Assert.Contains(systems, x => x.Uuid == system2WithCorrectRef.uuid);
        }

        [Fact]
        public async Task GET_Many_Internal_As_StakeHolder_With_NumberOfUsers_Filter()
        {
            //Arrange - Scope the test with additional rightsHolder filter so that we can control which response we get
            var (cookie, organization) = await CreateCookieStakeHolderUserInNewOrganizationAsync();
            var rightsHolder = await CreateOrganizationAsync();

            var excludedSinceTooFewUsages = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var includedLowerBound = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var includedAboveLowerBound = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);

            using var resp1 = await ItSystemHelper.SendSetBelongsToRequestAsync(excludedSinceTooFewUsages.dbId, rightsHolder.Id, TestEnvironment.DefaultOrganizationId);
            using var resp2 = await ItSystemHelper.SendSetBelongsToRequestAsync(includedLowerBound.dbId, rightsHolder.Id, TestEnvironment.DefaultOrganizationId);
            using var resp3 = await ItSystemHelper.SendSetBelongsToRequestAsync(includedAboveLowerBound.dbId, rightsHolder.Id, organization.Id);

            await TakeSystemIntoUseIn(excludedSinceTooFewUsages.dbId, TestEnvironment.DefaultOrganizationId);
            await TakeSystemIntoUseIn(includedLowerBound.dbId, TestEnvironment.DefaultOrganizationId, TestEnvironment.SecondOrganizationId);
            await TakeSystemIntoUseIn(includedAboveLowerBound.dbId, TestEnvironment.DefaultOrganizationId, TestEnvironment.SecondOrganizationId, rightsHolder.Id);

            Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp3.StatusCode);

            //Act
            var systems = (await ItSystemV2Helper.GetManyInternalAsync(cookie, rightsHolderId: rightsHolder.Uuid, numberOfUsers: 2)).ToList();

            //Assert - only 2 are actually valid since the excluded one was hidden to the stakeholder
            Assert.Equal(2, systems.Count);
            Assert.Contains(systems, dto => dto.Uuid == includedLowerBound.uuid);
            Assert.Contains(systems, dto => dto.Uuid == includedAboveLowerBound.uuid);
        }

        [Fact]
        public async Task GET_Many_Internal_As_StakeHolder_With_ChangesSince_Filter()
        {
            //Arrange
            var (cookie, organization) = await CreateCookieStakeHolderUserInNewOrganizationAsync();
            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var system3 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);

            await ItSystemHelper.SetNameAsync(system2.Id, CreateName(), organization.Id);
            await ItSystemHelper.SetNameAsync(system3.Id, CreateName(), organization.Id);
            await ItSystemHelper.SetNameAsync(system1.Id, CreateName(), organization.Id);
            var system3DTO = await ItSystemHelper.GetSystemAsync(system3.Id); //system 3 was changed as the second one and system 1 the last


            //Act
            var dtos = (await ItSystemV2Helper.GetManyInternalAsync(cookie, changedSinceGtEq: system3DTO.LastChanged, page: 0, pageSize: 10)).ToList();

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
            await ItSystemHelper.CreateItSystemInOrganizationAsync(invalidSearchName, organization.Id, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(searchName, organization.Id, AccessModifier.Public);

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

            await ItSystemHelper.CreateItSystemInOrganizationAsync(baseName, organization.Id, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(searchName, organization.Id, AccessModifier.Public);
            var system3 = await ItSystemHelper.CreateItSystemInOrganizationAsync(validName2, organization.Id, AccessModifier.Public);

            //Act
            var dtos = (await ItSystemV2Helper.GetManyInternalAsync(cookie, nameContains: searchName, page: 0, pageSize: 10)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            Assert.Equal(new[] { system2.Uuid, system3.Uuid }, dtos.Select(x => x.Uuid).ToArray());
        }

        protected async Task<(Cookie cookie, OrganizationDTO createdOrganization)> CreateCookieStakeHolderUserInNewOrganizationAsync()
        {
            var organization = await CreateOrganizationAsync();

            var (_, _, cookie) = await HttpApi.CreateUserAndLogin(CreateEmail(),
                OrganizationRole.User, organization.Id, false, true);
            return (cookie, organization);
        }
    }
}
