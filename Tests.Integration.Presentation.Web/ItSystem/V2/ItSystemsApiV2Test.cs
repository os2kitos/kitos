using System;
using System.ComponentModel;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using ExpectedObjects;
using Infrastructure.Services.Types;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Presentation.Web.Models.External.V2.Response.System;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem.V2
{
    [Collection(nameof(SequentialTestGroup))]
    public class ItSystemsApiV2Test : WithAutoFixture
    {
        [Fact]
        public async Task Can_GET_Public_ItSystem_As_Stakeholder_If_Placed_In_Other_Organization()
        {
            //Arrange - create user in new org and mark as stakeholder - then ensure that public data can be read from another org
            var (token, _) = await CreateStakeHolderUserInNewOrganizationAsync();
            var (entityUuid, _) = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);

            //Act
            var system = await ItSystemV2Helper.GetSingleAsync(token, entityUuid);

            //Assert
            Assert.NotNull(system);
            Assert.Equal(entityUuid, system.Uuid);
        }

        [Fact]
        public async Task Can_GET_Local_ItSystem_As_Stakeholder_If_Placed_In_Organization_Which_User_Is_Member_Of()
        {
            //Arrange - create user in new org and mark as stakeholder - then ensure that public data can be read from another org
            var (token, organization) = await CreateStakeHolderUserInNewOrganizationAsync();
            var (entityUuid, _) = await CreateSystemAsync(organization.Id, AccessModifier.Local);

            //Act
            var system = await ItSystemV2Helper.GetSingleAsync(token, entityUuid);

            //Assert
            Assert.NotNull(system);
            Assert.Equal(entityUuid, system.Uuid);
        }

        [Fact]
        public async Task Cannot_GET_ItSystem_As_Stakeholder_If_Local_System_Placed_In_Organization_Which_User_Is_Not_Member_Of()
        {
            //Arrange
            var (token, _) = await CreateStakeHolderUserInNewOrganizationAsync();
            var (entityUuid, _) = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);

            //Act
            using var systemResponse = await ItSystemV2Helper.SendGetSingleAsync(token, entityUuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, systemResponse.StatusCode);
        }

        [Fact]
        public async Task GET_ItSystem_As_StakeHolder_Returns_Expected_Data()
        {
            //Arrange
            var (token, _) = await CreateStakeHolderUserInNewOrganizationAsync();
            var rightsHolderOrganization = await CreateOrganizationAsync();
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var system = await CreateSystemAsync(organizationId, AccessModifier.Public);
            var parentSystem = await CreateSystemAsync(organizationId, AccessModifier.Public);
            var businessType = await EntityOptionHelper.CreateOptionTypeAsync(EntityOptionHelper.ResourceNames.BusinessType,CreateName(), organizationId);
            var exposedInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), organizationId, AccessModifier.Public));
            DatabaseAccess.MutateDatabase(db =>
            {
                var itSystem = db.ItSystems.AsQueryable().ByUuid(system.uuid);
                var interfaceToExpose = db.Set<ItInterface>().AsQueryable().ById(exposedInterface.Id);
                var taskRef = db.TaskRefs.AsQueryable().First();

                itSystem.PreviousName = A<string>();
                itSystem.Description = A<string>();
                itSystem.Disabled = A<bool>();
                itSystem.ArchiveDuty = A<ArchiveDutyRecommendationTypes>();
                itSystem.ArchiveDutyComment = A<string>();
                itSystem.ParentId = parentSystem.dbId;
                itSystem.BelongsToId = rightsHolderOrganization.Id;
                itSystem.BusinessTypeId = businessType.Id;

                itSystem.TaskRefs.Add(taskRef);
                db.ItInterfaceExhibits.Add(new ItInterfaceExhibit { ItInterface = interfaceToExpose, ItSystem = itSystem, ObjectOwnerId = 1, LastChangedByUserId = 1 });

                var externalReference = new ExternalReference
                {
                    ObjectOwnerId = 1,
                    LastChangedByUserId = 1,
                    ItSystem = itSystem,
                    Title = A<string>(),
                    URL = A<string>()
                };
                db.ExternalReferences.Add(externalReference);
                itSystem.SetMasterReference(externalReference);

                db.SaveChanges();
            });
            await ItSystemHelper.TakeIntoUseAsync(system.dbId, organizationId);
            await ItSystemHelper.TakeIntoUseAsync(system.dbId, rightsHolderOrganization.Id);

            //Act
            var systemDTO = await ItSystemV2Helper.GetSingleAsync(token, system.uuid);

            //Assert - compare db entity with the response DTO
            Assert.NotNull(systemDTO);
            DatabaseAccess.MapFromEntitySet<Core.DomainModel.ItSystem.ItSystem, bool>(repository =>
            {
                var dbSystem = repository.AsQueryable().ByUuid(system.uuid);

                AssertBaseSystemDTO(dbSystem, systemDTO);

                Assert.Equal(dbSystem.LastChanged, systemDTO.LastModified);
                Assert.Equal(dbSystem.LastChangedByUser.Uuid, systemDTO.LastModifiedBy.Uuid);
                Assert.Equal(dbSystem.LastChangedByUser.GetFullName(), systemDTO.LastModifiedBy.Name);

                var dtoOrgs = systemDTO.UsingOrganizations.ToDictionary(dto => dto.Uuid, dto => (dto.Name, dto.Cvr));
                var dbOrgs = dbSystem.Usages.Select(itSystemUsage => itSystemUsage.Organization).ToDictionary(organization => organization.Uuid, organization => (organization.Name, organization.GetActiveCvr()));
                Assert.Equal(dbOrgs, dtoOrgs);

                return true;
            });
        }

        [Fact]
        public async Task GET_Many_As_StakeHolder_Without_Filters()
        {
            //Arrange - make sure there are always systems to satisfy the test regardless of order
            var (token, organization) = await CreateStakeHolderUserInNewOrganizationAsync();
            await CreateSystemAsync(organization.Id, AccessModifier.Local);
            await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);

            //Act
            var systems = await ItSystemV2Helper.GetManyAsync(token, pageSize: 2);

            //Assert
            Assert.Equal(2, systems.Count());
        }

        [Fact]
        public async Task GET_Many_As_StakeHolder_With_RightsHolderFilter()
        {
            //Arrange - make sure there are always systems to satisfy the test regardless of order
            var (token, organization) = await CreateStakeHolderUserInNewOrganizationAsync();
            var rightsHolder = await CreateOrganizationAsync();

            var unExpectedAsItIsLocalInNonMemberOrg = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var expected1 = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var expected2 = await CreateSystemAsync(organization.Id, AccessModifier.Local);

            using var resp1 = await ItSystemHelper.SendSetBelongsToRequestAsync(unExpectedAsItIsLocalInNonMemberOrg.dbId, rightsHolder.Id, TestEnvironment.DefaultOrganizationId);
            using var resp2 = await ItSystemHelper.SendSetBelongsToRequestAsync(expected1.dbId, rightsHolder.Id, TestEnvironment.DefaultOrganizationId);
            using var resp3 = await ItSystemHelper.SendSetBelongsToRequestAsync(expected2.dbId, rightsHolder.Id, organization.Id);

            Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp3.StatusCode);

            //Act
            var systems = (await ItSystemV2Helper.GetManyAsync(token, rightsHolderId: rightsHolder.Uuid)).ToList();

            //Assert - only 2 are actually valid since the excluded one was hidden to the stakeholder
            Assert.Equal(2, systems.Count);
            Assert.Contains(systems, dto => dto.Uuid == expected1.uuid);
            Assert.Contains(systems, dto => dto.Uuid == expected2.uuid);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GET_Many_As_StakeHolder_Depends_On_IncludeDeactivated(bool shouldIncludeDeactivated)
        {
            //Arrange
            var (token, organization) = await CreateStakeHolderUserInNewOrganizationAsync();
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
            var systems = (await ItSystemV2Helper.GetManyAsync(token, rightsHolderId: rightsHolder.Uuid, includeDeactivated: shouldIncludeDeactivated)).ToList(); // Limit to only take systems in rightsholder org

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
        public async Task GET_Many_As_StakeHolder_With_BusinessTypeFilter()
        {
            //Arrange
            var (token, _) = await CreateStakeHolderUserInNewOrganizationAsync();
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
            var systems = (await ItSystemV2Helper.GetManyAsync(token, businessTypeId: correctBusinessTypeId)).ToList();

            //Assert
            var dto = Assert.Single(systems);
            Assert.Equal(dto.Uuid, expected.uuid);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GET_Many_As_StakeHolder_With_KLE_Filter(bool useKeyAsFilter)
        {
            //Arrange
            var (token, _) = await CreateStakeHolderUserInNewOrganizationAsync();
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
            var systems = (await ItSystemV2Helper.GetManyAsync(token, kleKey: kleKeyFilter, kleUuid: kleUuidFilter)).ToList();

            //Assert
            Assert.Equal(2, systems.Count);
            Assert.Contains(systems, x => x.Uuid == system1WithCorrectRef.uuid);
            Assert.Contains(systems, x => x.Uuid == system2WithCorrectRef.uuid);
        }

        [Fact]
        public async Task GET_Many_As_StakeHolder_With_NumberOfUsers_Filter()
        {
            //Arrange - Scope the test with additional rightsHolder filter so that we can control which response we get
            var (token, organization) = await CreateStakeHolderUserInNewOrganizationAsync();
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
            var systems = (await ItSystemV2Helper.GetManyAsync(token, rightsHolderId: rightsHolder.Uuid, numberOfUsers: 2)).ToList();

            //Assert - only 2 are actually valid since the excluded one was hidden to the stakeholder
            Assert.Equal(2, systems.Count);
            Assert.Contains(systems, dto => dto.Uuid == includedLowerBound.uuid);
            Assert.Contains(systems, dto => dto.Uuid == includedAboveLowerBound.uuid);
        }

        [Theory]
        [InlineData(AccessModifier.Local)]
        [InlineData(AccessModifier.Public)]
        public async Task Can_GET_RightsHolderSystem_In_Other_Organization_If_SystemRightsHolder_Matches_A_RightsHolder_Access_For_User(AccessModifier accessModifier)
        {
            //Arrange
            var (token, createdOrganization) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var system = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, accessModifier);
            using var sendSetBelongsToResponse = await ItSystemHelper.SendSetBelongsToRequestAsync(system.dbId, createdOrganization.Id, TestEnvironment.DefaultOrganizationId);
            Assert.Equal(HttpStatusCode.OK, sendSetBelongsToResponse.StatusCode);

            //Act
            var systemDTO = await ItSystemV2Helper.GetSingleRightsHolderSystemAsync(token, system.uuid);

            //Assert
            Assert.Equal(system.uuid, systemDTO.Uuid);
        }

        [Theory]
        [InlineData(AccessModifier.Local)]
        [InlineData(AccessModifier.Public)]
        public async Task Cannot_GET_RightsHolderSystem_System_In_Other_Organization_If_No_RightsHolderAccess_To_That_Organization(AccessModifier accessModifier)
        {
            //Arrange - system has different rightsholder
            var (token, _) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var system = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, accessModifier);
            using var sendSetBelongsToResponse = await ItSystemHelper.SendSetBelongsToRequestAsync(system.dbId, TestEnvironment.SecondOrganizationId, TestEnvironment.DefaultOrganizationId);
            Assert.Equal(HttpStatusCode.OK, sendSetBelongsToResponse.StatusCode);

            //Act
            using var response = await ItSystemV2Helper.SendGetSingleRightsHolderSystemAsync(token, system.uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory, Description("Ensures that when requesting systems with rightsholder access, only those are returned. No default rules overrule this")]
        [InlineData(AccessModifier.Local)]
        [InlineData(AccessModifier.Public)]
        public async Task Cannot_GET_RightsHolderSystem_System_In_OWN_Organization_If_No_RightsHolderAccess(AccessModifier accessModifier)
        {
            //Arrange - system has different rightsholder
            var (token, ownOrganization) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var system = await CreateSystemAsync(ownOrganization.Id, accessModifier);

            //Set rightsholder to a different organization
            using var sendSetBelongsToResponse = await ItSystemHelper.SendSetBelongsToRequestAsync(system.dbId, TestEnvironment.SecondOrganizationId, ownOrganization.Id);

            //Act
            using var response = await ItSystemV2Helper.SendGetSingleRightsHolderSystemAsync(token, system.uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GET_RightsHolderSystem_Returns_ExpectedData()
        {
            //Arrange
            var (token, rightsHolderOrganization) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var system = await CreateSystemAsync(organizationId, AccessModifier.Public);
            var parentSystem = await CreateSystemAsync(organizationId, AccessModifier.Public);
            var businessType = await EntityOptionHelper.CreateOptionTypeAsync(EntityOptionHelper.ResourceNames.BusinessType, CreateName(), organizationId);
            var exposedInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), organizationId, AccessModifier.Public));
            DatabaseAccess.MutateDatabase(db =>
            {
                var itSystem = db.ItSystems.AsQueryable().ByUuid(system.uuid);
                var interfaceToExpose = db.Set<ItInterface>().AsQueryable().ById(exposedInterface.Id);
                var taskRef = db.TaskRefs.AsQueryable().First();

                itSystem.PreviousName = A<string>();
                itSystem.Description = A<string>();
                itSystem.Disabled = A<bool>();
                itSystem.ArchiveDuty = A<ArchiveDutyRecommendationTypes>();
                itSystem.ArchiveDutyComment = A<string>();
                itSystem.ParentId = parentSystem.dbId;
                itSystem.BelongsToId = rightsHolderOrganization.Id;
                itSystem.BusinessTypeId = businessType.Id;

                itSystem.TaskRefs.Add(taskRef);
                db.ItInterfaceExhibits.Add(new ItInterfaceExhibit { ItInterface = interfaceToExpose, ItSystem = itSystem, ObjectOwnerId = 1, LastChangedByUserId = 1 });

                var externalReference = new ExternalReference
                {
                    ObjectOwnerId = 1,
                    LastChangedByUserId = 1,
                    ItSystem = itSystem,
                    Title = A<string>(),
                    URL = A<string>()
                };
                db.ExternalReferences.Add(externalReference);
                itSystem.SetMasterReference(externalReference);

                db.SaveChanges();
            });

            //Act
            var systemDTO = await ItSystemV2Helper.GetSingleRightsHolderSystemAsync(token, system.uuid);

            //Assert - compare db entity with the response DTO
            Assert.NotNull(systemDTO);
            DatabaseAccess.MapFromEntitySet<Core.DomainModel.ItSystem.ItSystem, bool>(repository =>
            {
                var dbSystem = repository.AsQueryable().ByUuid(system.uuid);
                AssertBaseSystemDTO(dbSystem, systemDTO);
                return true;
            });
        }

        [Fact]
        public async Task Can_GET_Many_RightsHolderSystems()
        {
            //Arrange - create three systems in different organizations but with the right rightsholder
            var (token, rightsHolderOrganization) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var system1 = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var system2 = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var system3 = await CreateSystemAsync(TestEnvironment.SecondOrganizationId, AccessModifier.Local);
            const int pageSize = 2;

            var systems = new[] { system1, system2, system3 };
            foreach (var system in systems)
            {
                using var setBelongsToResponse = await ItSystemHelper.SendSetBelongsToRequestAsync(system.dbId, rightsHolderOrganization.Id, TestEnvironment.DefaultOrganizationId);
                Assert.Equal(HttpStatusCode.OK, setBelongsToResponse.StatusCode);
            }

            //Act - page 1 + page 2
            var page1 = await ItSystemV2Helper.GetManyRightsHolderSystemsAsync(token, page: 0, pageSize: pageSize);
            var page2 = await ItSystemV2Helper.GetManyRightsHolderSystemsAsync(token, page: 1, pageSize: pageSize);

            //Assert
            Assert.Equal(new[] { system1.uuid, system2.uuid }, page1.Select(x => x.Uuid));
            Assert.Equal(new[] { system3.uuid }, page2.Select(x => x.Uuid));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_GET_Many_RightsHolderSystems_Depends_On_IncludeDeactivated(bool shouldIncludeDeactivated)
        {
            //Arrange
            var (token, rightsHolderOrganization) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var inactive = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var active = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            const int pageSize = 2;

            var systems = new[] { inactive, active };
            foreach (var system in systems)
            {
                using var setBelongsToResponse = await ItSystemHelper.SendSetBelongsToRequestAsync(system.dbId, rightsHolderOrganization.Id, TestEnvironment.DefaultOrganizationId);
                Assert.Equal(HttpStatusCode.OK, setBelongsToResponse.StatusCode);
            }

            DatabaseAccess.MutateDatabase(db =>
            {
                var dbSystem = db.ItSystems.AsQueryable().ById(inactive.dbId);
                dbSystem.Disabled = true;
                db.SaveChanges();
            });

            //Act
            var result = await ItSystemV2Helper.GetManyRightsHolderSystemsAsync(token, page: 0, pageSize: pageSize, includeDeactivated: shouldIncludeDeactivated);

            //Assert
            if (shouldIncludeDeactivated)
            {
                Assert.Equal(2, result.Count());
                var activeSystemDTO = result.First(x => x.Uuid.Equals(active.uuid));
                Assert.False(activeSystemDTO.Deactivated);
                var inactiveSystemDTO = result.First(x => x.Uuid.Equals(inactive.uuid));
                Assert.True(inactiveSystemDTO.Deactivated);
            }
            else
            {
                var systemResult = Assert.Single(result);
                Assert.Equal(systemResult.Uuid, active.uuid);
                Assert.False(systemResult.Deactivated);
            }
        }

        [Fact]
        public async Task Can_GET_Many_RightsHolderSystems_And_Filter_By_Specific_RightsHolder()
        {
            //Arrange - create three systems in different organizations but with the right rightsholder
            var (userId, token, rightsHolderOrganization) = await CreateRightsHolderAccessUserInNewOrganizationAndGetFullUserAsync();
            var rightsHolder2Org = await CreateOrganizationAsync();

            const int mainOrganizationId = TestEnvironment.DefaultOrganizationId;
            var systemRightsHolder1 = await CreateSystemAsync(mainOrganizationId, AccessModifier.Local);
            var systemRightsHolder2 = await CreateSystemAsync(mainOrganizationId, AccessModifier.Local);

            using var belongsToResponse1 = await ItSystemHelper.SendSetBelongsToRequestAsync(systemRightsHolder1.dbId, rightsHolderOrganization.Id, mainOrganizationId);
            using var belongsToResponse2 = await ItSystemHelper.SendSetBelongsToRequestAsync(systemRightsHolder2.dbId, rightsHolder2Org.Id, mainOrganizationId);
            Assert.Equal(HttpStatusCode.OK, belongsToResponse1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, belongsToResponse2.StatusCode);
            using var assignRightsHolderInOrg2Response = await HttpApi.SendAssignRoleToUserAsync(userId, OrganizationRole.RightsHolderAccess, rightsHolder2Org.Id);
            Assert.Equal(HttpStatusCode.Created, assignRightsHolderInOrg2Response.StatusCode);

            //Act
            var response = await ItSystemV2Helper.GetManyRightsHolderSystemsAsync(token, rightsHolderUuid: rightsHolder2Org.Uuid);

            //Assert
            var systemResponseDto = Assert.Single(response);
            Assert.Equal(systemRightsHolder2.uuid, systemResponseDto.Uuid);
        }

        [Theory]
        [InlineData(true, true, true, false, true, true)]
        [InlineData(false, true, false, true, true, true)]
        [InlineData(true, false, true, false, true, true)]
        [InlineData(true, true, false, true, true, true)]
        [InlineData(true, true, false, true, false, true)]
        [InlineData(true, true, true, false, true, false)]
        [InlineData(false, false, false, false, false, false)]

        public async Task Can_POST_ItSystem_As_RightsHolder(bool withProvidedUuid, bool withBusinessType, bool withKleNumbers, bool withKleUuid, bool withParent, bool withFormerName)
        {
            //Arrange
            var (userId, token, createdOrganization) = await CreateRightsHolderAccessUserInNewOrganizationAndGetFullUserAsync();
            var input = await PrepareCreateRightsHolderSystemRequestAsync(withProvidedUuid, withBusinessType, withKleNumbers, withKleUuid, withParent, withFormerName, createdOrganization);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(r => r.AsQueryable().ById(userId));

            //Act - create it and GET it to verify that response DTO matches input requests AND that a consecutive GET returns the same data
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, input);
            var fetchedSystem = await ItSystemV2Helper.GetSingleRightsHolderSystemAsync(token, createdSystem.Uuid);

            //Assert
            if (withProvidedUuid)
                Assert.Equal(input.Uuid, createdSystem.Uuid);
            else
                Assert.NotEqual(Guid.Empty, createdSystem.Uuid);
            Assert.Equal(input.Description, createdSystem.Description);
            Assert.Equal(input.Name, createdSystem.Name);
            Assert.Equal(input.FormerName, createdSystem.FormerName);
            Assert.Equal(input.UrlReference, createdSystem.UrlReference);
            Assert.Equal(input.BusinessTypeUuid, createdSystem.BusinessType?.Uuid);

            if (withKleNumbers)
                Assert.Equal(input.KLENumbers, createdSystem.KLE.Select(x => x.Name));
            if (withKleUuid)
                Assert.Equal(input.KLEUuids, createdSystem.KLE.Select(x => x.Uuid));
            if (!withKleUuid && !withKleNumbers)
                Assert.Empty(createdSystem.KLE);

            Assert.Equal(input.ParentUuid, createdSystem.ParentSystem?.Uuid);
            Assert.Equal(DateTime.UtcNow.Date, createdSystem.Created.GetValueOrDefault().Date);
            Assert.Empty(createdSystem.ExposedInterfaces);
            Assert.Equal(createdOrganization.Uuid, createdSystem.RightsHolder.Uuid);
            Assert.Equal(createdOrganization.Name, createdSystem.RightsHolder.Name);
            Assert.Equal(createdOrganization.Cvr, createdSystem.RightsHolder.Cvr);
            Assert.Equal(user.Uuid, createdSystem.CreatedBy.Uuid);
            Assert.Equal(user.GetFullName(), createdSystem.CreatedBy.Name);

            //Check the fetched system
            createdSystem.ToExpectedObject().ShouldMatch(fetchedSystem);
        }

        [Theory]
        [InlineData(true, true, true, true)]
        [InlineData(true, true, true, false)]
        [InlineData(true, true, false, true)]
        [InlineData(true, false, true, true)]
        [InlineData(false, true, true, true)]
        public async Task Cannot_POST_ItSystem_AsRightsHolder_WithoutAllRequiredFields(bool withoutRightsHolder, bool withoutName, bool withoutDescription, bool withoutReference)
        {
            //Arrange
            var (token, org) = await CreateRightsHolderAccessUserInNewOrganizationAsync();

            var input = new RightsHolderCreateItSystemRequestDTO
            {
                RightsHolderUuid = withoutRightsHolder ? Guid.Empty : org.Uuid,
                Name = withoutName ? null : $"Name_{A<string>()}",
                Description = withoutDescription ? null : $"Description_{A<string>()}",
                UrlReference = withoutReference ? null : $"https://{A<int>()}.dk",
            };

            //Act
            var createdSystem = await ItSystemV2Helper.SendCreateRightsHolderSystemAsync(token, input);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, createdSystem.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_ItSystem_As_RightsHolder_To_Organization_Where_User_Is_Not_RightsHolder()
        {
            //Arrange
            var (token, _) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var defaultOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            var input = new RightsHolderCreateItSystemRequestDTO
            {
                RightsHolderUuid = defaultOrgUuid,
                Name = $"Name_{A<string>()}",
                Description = $"Description_{A<string>()}",
                UrlReference = $"https://{A<int>()}.dk"
            };

            //Act 
            var createdSystem = await ItSystemV2Helper.SendCreateRightsHolderSystemAsync(token, input);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, createdSystem.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_ItSystem_As_RightsHolder_With_Parent_System_Where_User_Is_Not_RightsHolder()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var (parentUuid, dbId) = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);

            var input = new RightsHolderCreateItSystemRequestDTO
            {
                RightsHolderUuid = org.Uuid,
                Name = $"Name_{A<string>()}",
                Description = $"Description_{A<string>()}",
                UrlReference = $"https://{A<int>()}.dk",
                ParentUuid = parentUuid
            };

            //Act 
            var createdSystem = await ItSystemV2Helper.SendCreateRightsHolderSystemAsync(token, input);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, createdSystem.StatusCode);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(0, 2)]
        [InlineData(2, 0)]
        public async Task Cannot_POST_ItSystem_With_Duplicate_KLE(int instancesInKleNumbers, int instancesInUuids)
        {
            //Arrange
            var (token, org) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var kle = DatabaseAccess.MapFromEntitySet<TaskRef, (string key, Guid uuid)>(x => x.AsQueryable().First().Transform(taskRef => (taskRef.TaskKey, taskRef.Uuid)));

            var kleNumbers = Enumerable.Repeat(kle.key, instancesInKleNumbers);
            var kleUuids = Enumerable.Repeat(kle.uuid, instancesInUuids);

            var input = new RightsHolderCreateItSystemRequestDTO
            {
                RightsHolderUuid = org.Uuid,
                Name = $"Name_{A<string>()}",
                Description = $"Description_{A<string>()}",
                UrlReference = $"https://{A<int>()}.dk",
                KLENumbers = kleNumbers, //Same kle by both key and uuid
                KLEUuids = kleUuids
            };

            //Act 
            var createdSystem = await ItSystemV2Helper.SendCreateRightsHolderSystemAsync(token, input);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, createdSystem.StatusCode);
        }

        [Fact]
        public async Task Cannot_PUT_As_RightsHolder_To_Unknown_System()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var uuid = A<Guid>();

            //Act
            using var result = await ItSystemV2Helper.SendUpdateRightsHolderSystemAsync(token, uuid, A<RightsHolderWritableITSystemPropertiesDTO>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_PUT_As_RightsHolder_To_System_With_Wrong_RightsHolder()
        {
            //Arrange
            var (token, _) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var (uuid, _) = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);

            //Act
            using var result = await ItSystemV2Helper.SendUpdateRightsHolderSystemAsync(token, uuid, A<RightsHolderWritableITSystemPropertiesDTO>());

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_PUT_As_RightsHolder_To_System_Which_Has_Been_Deactivated()
        {
            //Arrange
            var (token, rightsHolder) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var (uuid, _) = await CreateSystemAsync(rightsHolder.Id, AccessModifier.Local);
            DatabaseAccess.MutateEntitySet<Core.DomainModel.ItSystem.ItSystem>(systems => systems.AsQueryable().ByUuid(uuid).Disabled = true);

            //Act
            using var result = await ItSystemV2Helper.SendUpdateRightsHolderSystemAsync(token, uuid, A<RightsHolderWritableITSystemPropertiesDTO>());

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Theory]
        [InlineData(true, true, true, true, true, true, true)]
        [InlineData(true, true, true, true, true, true, false)]
        [InlineData(true, true, true, true, true, false, false)]
        [InlineData(true, true, true, true, false, false, false)]
        [InlineData(true, true, true, false, false, false, false)]
        [InlineData(true, true, false, false, false, false, false)]
        [InlineData(true, false, false, false, false, false, false)]
        [InlineData(false, false, false, false, false, false, false)]
        public async Task Can_PUT_As_RightsHolder(bool updateName, bool updateFormerName, bool updateDescription, bool updateUrl, bool updateBusinessType, bool updateKle, bool updateParent)
        {
            //Arrange
            var (token, rightsHolder) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var createSystemRequest = await PrepareCreateRightsHolderSystemRequestAsync(false, true, true, false, true, true, rightsHolder);
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest);
            var update = await A<RightsHolderWritableITSystemPropertiesDTO>().Transform(async dto =>
            {
                dto.Name = updateName ? dto.Name : createdSystem.Name;
                dto.FormerName = updateFormerName ? dto.FormerName : createdSystem.FormerName;
                dto.Description = updateDescription ? dto.Description : createdSystem.Description;
                dto.BusinessTypeUuid = updateBusinessType ? GetBusinessType(1) : createdSystem.BusinessType.Uuid;
                dto.UrlReference = updateUrl ? dto.UrlReference : createdSystem.UrlReference;
                dto.KLEUuids = null;
                dto.KLENumbers = updateKle ? new[] { CreateNewTaskRefAndGetKey() } : createdSystem.KLE.Select(x => x.Name).ToList();
                dto.ParentUuid = updateParent ? (await CreateSystemAsync(rightsHolder.Id, AccessModifier.Public)).uuid : createdSystem.ParentSystem.Uuid;
                return dto;
            });


            //Act
            var updatedSystem = await ItSystemV2Helper.UpdateRightsHolderSystemAsync(token, createdSystem.Uuid, update);

            //Assert
            Assert.Equal(createdSystem.Uuid, updatedSystem.Uuid); //No changes expected
            Assert.Equal(createdSystem.Created, updatedSystem.Created); //No changes expected
            createdSystem.RightsHolder.ToExpectedObject().ShouldMatch(updatedSystem.RightsHolder); //No changes expected
            Assert.Equal(update.Name, updatedSystem.Name);
            Assert.Equal(update.Description, updatedSystem.Description);
            Assert.Equal(update.FormerName, updatedSystem.FormerName);
            Assert.Equal(update.BusinessTypeUuid.GetValueOrDefault(), updatedSystem.BusinessType.Uuid);
            Assert.Equal(update.ParentUuid.GetValueOrDefault(), updatedSystem.ParentSystem.Uuid);
            Assert.Equal(update.UrlReference, updatedSystem.UrlReference);
            Assert.Equal(update.KLENumbers, updatedSystem.KLE.Select(x => x.Name));
        }

        [Theory]
        [InlineData(false, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        public async Task Cannot_PUT_As_RightsHolder_If_Required_Properties_Are_Missing(bool nullName, bool nullUrl, bool nullDescription)
        {
            //Arrange
            var (token, rightsHolder) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var createSystemRequest = await PrepareCreateRightsHolderSystemRequestAsync(false, false, false, false, false, true, rightsHolder);
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest);
            var update = A<RightsHolderWritableITSystemPropertiesDTO>().Transform(dto =>
            {
                dto.Name = nullName ? null : dto.Name;
                dto.UrlReference = nullUrl ? null : dto.UrlReference;
                dto.Description = nullDescription ? null : dto.Description;
                dto.ParentUuid = null;
                dto.BusinessTypeUuid = null;
                dto.KLEUuids = null;
                dto.KLENumbers = null;
                return dto;
            });

            //Act
            using var result = await ItSystemV2Helper.SendUpdateRightsHolderSystemAsync(token, createdSystem.Uuid, update);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_PUT_As_RightsHolder_If_No_RightsHolderAccess_To_ParentSystem()
        {
            //Arrange
            var (token, rightsHolder) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var (parentUuid, _) = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);

            var createSystemRequest = await PrepareCreateRightsHolderSystemRequestAsync(false, false, false, false, false, true, rightsHolder);
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest);
            var update = A<RightsHolderWritableITSystemPropertiesDTO>().Transform(dto =>
            {
                dto.ParentUuid = parentUuid;
                dto.BusinessTypeUuid = null;
                dto.KLEUuids = null;
                dto.KLENumbers = null;
                return dto;
            });

            //Act
            using var result = await ItSystemV2Helper.SendUpdateRightsHolderSystemAsync(token, createdSystem.Uuid, update);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_PUT_As_RightsHolder_If_Name_Is_Overlapping()
        {
            //Arrange
            var (token, rightsHolder) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var createSystemRequest1 = await PrepareCreateRightsHolderSystemRequestAsync(false, false, false, false, false, true, rightsHolder);
            var createdSystem1 = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest1);
            var createSystemRequest2 = await PrepareCreateRightsHolderSystemRequestAsync(false, false, false, false, false, true, rightsHolder);
            var createdSystem2 = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest2);

            var update = A<RightsHolderWritableITSystemPropertiesDTO>().Transform(dto =>
            {
                dto.Name = createdSystem2.Name; //Conflict with second system
                dto.ParentUuid = null;
                dto.BusinessTypeUuid = null;
                dto.KLEUuids = null;
                dto.KLENumbers = null;
                return dto;
            });

            //Act
            using var result = await ItSystemV2Helper.SendUpdateRightsHolderSystemAsync(token, createdSystem1.Uuid, update);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        }

        [Fact]
        public async Task DELETE_As_RightsHolder_Deactivates_System()
        {
            //Arrange
            var (token, rightsHolder) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var createSystemRequest1 = await PrepareCreateRightsHolderSystemRequestAsync(false, false, false, false, false, true, rightsHolder);
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest1);

            var reason = A<DeactivationReasonRequestDTO>();

            //Act
            using var result = await ItSystemV2Helper.SendDeleteRightsHolderSystemAsync(token, createdSystem.Uuid, reason);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            var dto = await ItSystemV2Helper.GetSingleRightsHolderSystemAsync(token, createdSystem.Uuid);
            Assert.True(dto.Deactivated);
        }

        [Fact]
        public async Task Cannot_DELETE_As_RightsHolder_Without_Reason()
        {
            //Arrange
            var (token, rightsHolder) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var createSystemRequest1 = await PrepareCreateRightsHolderSystemRequestAsync(false, false, false, false, false, true, rightsHolder);
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest1);

            var reason = new DeactivationReasonRequestDTO() {DeactivationReason = string.Empty};

            //Act
            using var result = await ItSystemV2Helper.SendDeleteRightsHolderSystemAsync(token, createdSystem.Uuid, reason);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var dto = await ItSystemV2Helper.GetSingleRightsHolderSystemAsync(token, createdSystem.Uuid);
            Assert.False(dto.Deactivated); //Deactivation should not have happened
        }

        [Fact]
        public async Task Cannot_DELETE_As_RightsHolder_Without_Access()
        {
            //Arrange
            var (token, _) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var system = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);

            var reason = A<DeactivationReasonRequestDTO>();

            //Act
            using var result = await ItSystemV2Helper.SendDeleteRightsHolderSystemAsync(token, system.uuid, reason);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_DELETE_Unknown_System()
        {
            //Arrange
            var (token, _) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var uuid = A<Guid>();

            var reason = A<DeactivationReasonRequestDTO>();

            //Act
            using var result = await ItSystemV2Helper.SendDeleteRightsHolderSystemAsync(token, uuid, reason);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_DELETE_As_RightsHolder_If_Already_Deactivated()
        {
            //Arrange
            var (token, rightsHolder) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var createSystemRequest1 = await PrepareCreateRightsHolderSystemRequestAsync(false, false, false, false, false, true, rightsHolder);
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest1);
            DatabaseAccess.MutateEntitySet<Core.DomainModel.ItSystem.ItSystem>(repository=>repository.AsQueryable().ByUuid(createdSystem.Uuid).Deactivate());

            var reason = A<DeactivationReasonRequestDTO>();

            //Act
            using var result = await ItSystemV2Helper.SendDeleteRightsHolderSystemAsync(token, createdSystem.Uuid, reason);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        private static void AssertBaseSystemDTO(Core.DomainModel.ItSystem.ItSystem dbSystem, BaseItSystemResponseDTO systemDTO)
        {
            var dbTaskKeys = dbSystem.TaskRefs.ToDictionary(x => x.Uuid, x => x.TaskKey);
            var dtoTaskKeys = systemDTO.KLE.ToDictionary(x => x.Uuid, x => x.Name);

            var dbInterfaces = dbSystem.ItInterfaceExhibits.Select(x => x.ItInterface).ToDictionary(x => x.Uuid, x => x.Name);
            var dtoInterfaces = systemDTO.ExposedInterfaces.ToDictionary(x => x.Uuid, x => x.Name);

            Assert.Equal(dbSystem.Uuid, systemDTO.Uuid);
            Assert.Equal(dbSystem.Name, systemDTO.Name);
            Assert.Equal(dbSystem.Description, systemDTO.Description);
            Assert.Equal(dbSystem.PreviousName, systemDTO.FormerName);
            Assert.Equal(dbSystem.Disabled, systemDTO.Deactivated);
            Assert.Equal(dbSystem.Created, systemDTO.Created);
            Assert.Equal(dbSystem.ObjectOwner.Uuid, systemDTO.CreatedBy.Uuid);
            Assert.Equal(dbSystem.ObjectOwner.GetFullName(), systemDTO.CreatedBy.Name);
            Assert.Equal(dbSystem.ArchiveDuty?.ToString("G"), systemDTO.RecommendedArchiveDuty.Id.ToString("G"));
            Assert.Equal(dbSystem.ArchiveDutyComment, systemDTO.RecommendedArchiveDuty.Comment);
            Assert.Equal(dbSystem.Parent.Uuid, systemDTO.ParentSystem.Uuid);
            Assert.Equal(dbSystem.Parent.Name, systemDTO.ParentSystem.Name);
            Assert.Equal(dbSystem.BelongsTo.Uuid, systemDTO.RightsHolder.Uuid);
            Assert.Equal(dbSystem.BelongsTo.Name, systemDTO.RightsHolder.Name);
            Assert.Equal(dbSystem.BelongsTo.GetActiveCvr(), systemDTO.RightsHolder.Cvr);
            Assert.Equal(dbSystem.BusinessType.Uuid, systemDTO.BusinessType.Uuid);
            Assert.Equal(dbSystem.BusinessType.Name, systemDTO.BusinessType.Name);
            Assert.Equal(dbTaskKeys, dtoTaskKeys);
            Assert.Equal(dbInterfaces, dtoInterfaces);
            Assert.Equal(dbSystem.Reference.URL, systemDTO.UrlReference);
        }

        private static async Task TakeSystemIntoUseIn(int systemDbId, params int[] organizationIds)
        {
            foreach (var organizationId in organizationIds)
            {
                await ItSystemHelper.TakeIntoUseAsync(systemDbId, organizationId);
            }
        }

        private async Task<(Guid uuid, int dbId)> CreateSystemAsync(int organizationId, AccessModifier accessModifier)
        {
            var systemName = CreateName();
            var createdSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, accessModifier);
            var entityUuid = DatabaseAccess.GetEntityUuid<Core.DomainModel.ItSystem.ItSystem>(createdSystem.Id);

            return (entityUuid, createdSystem.Id);
        }

        private async Task<(string token, Organization createdOrganization)> CreateStakeHolderUserInNewOrganizationAsync()
        {
            var organization = await CreateOrganizationAsync();

            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(),
                OrganizationRole.User, organization.Id, true, true);
            return (token, organization);
        }

        private async Task<(string token, Organization createdOrganization)> CreateRightsHolderAccessUserInNewOrganizationAsync()
        {
            var organization = await CreateOrganizationAsync();

            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(),
                OrganizationRole.RightsHolderAccess, organization.Id, true);
            return (token, organization);
        }

        private async Task<(int userId, string token, Organization createdOrganization)> CreateRightsHolderAccessUserInNewOrganizationAndGetFullUserAsync()
        {
            var organization = await CreateOrganizationAsync();

            var (id, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(),
                OrganizationRole.RightsHolderAccess, organization.Id, true);
            return (id, token, organization);
        }

        private async Task<Organization> CreateOrganizationAsync()
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, "11224455", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return $"{nameof(ItSystemsApiV2Test)}{A<string>()}";
        }

        private string CreateEmail()
        {
            return $"{CreateName()}@kitos.dk";
        }

        private static void CreateTaskRefInDatabase(string key, Guid uuid)
        {
            DatabaseAccess.MutateEntitySet<TaskRef>(repo => repo.Insert(new TaskRef
            {
                Uuid = uuid,
                TaskKey = key,
                ObjectOwnerId = 1,
                LastChangedByUserId = 1,
                OwnedByOrganizationUnitId = 1
            }));
        }

        private async Task<RightsHolderCreateItSystemRequestDTO> PrepareCreateRightsHolderSystemRequestAsync(
            bool withProvidedUuid,
            bool withBusinessType,
            bool withKleNumbers,
            bool withKleUuid,
            bool withParent,
            bool withFormerName,
            Organization rightsHolderOrganization)
        {
            var parentCandidate = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), rightsHolderOrganization.Id, AccessModifier.Local);

            var businessType = GetBusinessType(0);
            var kle = DatabaseAccess.MapFromEntitySet<TaskRef, (string key, Guid uuid)>(x =>
                x.AsQueryable().First().Transform(taskRef => (taskRef.TaskKey, taskRef.Uuid)));
            return new RightsHolderCreateItSystemRequestDTO
            {
                RightsHolderUuid = rightsHolderOrganization.Uuid,
                Uuid = withProvidedUuid ? Guid.NewGuid() : null,
                Name = $"Name_{A<string>()}",
                Description = $"Description_{A<string>()}",
                FormerName = withFormerName ? $"FormerName_{A<string>()}" : null,
                UrlReference = $"https://{A<int>()}.dk",
                BusinessTypeUuid = withBusinessType ? businessType : null,
                KLENumbers = withKleNumbers ? new[] { kle.key } : new string[0],
                KLEUuids = withKleUuid ? new[] { kle.uuid } : new Guid[0],
                ParentUuid = withParent ? parentCandidate.Uuid : null
            };
        }

        private static Guid GetBusinessType(int skip)
        {
            return DatabaseAccess.MapFromEntitySet<BusinessType, Guid>(repository =>
                repository.AsQueryable().OrderBy(x => x.Id).Skip(skip).First(x => x.IsEnabled && x.IsObligatory).Uuid);
        }

        private string CreateNewTaskRefAndGetKey()
        {
            var availableKey = DatabaseAccess.MapFromEntitySet<TaskRef, string>(refs =>
            {
                var i = 0;
                var success = false;
                string key = default;
                while (!success)
                {
                    key = i.ToString("X");
                    i++;
                    var match = key;
                    success = refs.AsQueryable().FirstOrDefault(x => x.TaskKey == match) == null;
                }

                return key;
            });
            DatabaseAccess.MutateEntitySet<TaskRef>(refs => refs.Insert(new TaskRef
            {
                Uuid = A<Guid>(),
                TaskKey = availableKey,
                ObjectOwnerId = 1,
                LastChangedByUserId = 1,
                OwnedByOrganizationUnitId = 1
            }));
            return availableKey;
        }
    }
}
