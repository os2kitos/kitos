using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.Interface;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.Generic.Hierarchy;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.KLE;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Response.System;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.System;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.Internal.References;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem.V2
{
    [Collection(nameof(SequentialTestGroup))]
    public class ItSystemsApiV2Test : BaseItSystemsApiV2Test
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
            Assert.Equal(RegistrationScopeChoice.Global, system.Scope);
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
            Assert.Equal(RegistrationScopeChoice.Local, system.Scope);
        }

        [Fact]
        public async Task Can_GET_Local_ItSystem_As_Stakeholder()
        {
            //Arrange
            var (token, _) = await CreateStakeHolderUserInNewOrganizationAsync();
            var (entityUuid, _) = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);

            //Act
            using var systemResponse = await ItSystemV2Helper.SendGetSingleAsync(token, entityUuid);

            //Assert
            Assert.True(systemResponse.IsSuccessStatusCode);
            var system = await systemResponse.ReadResponseBodyAsAsync<ItSystemResponseDTO>();
            Assert.Equal(entityUuid, system.Uuid);
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
            var businessType = await EntityOptionHelper.CreateOptionTypeAsync(EntityOptionHelper.ResourceNames.BusinessType, CreateName(), organizationId);
            var exposedInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), organizationId, AccessModifier.Public));
            DatabaseAccess.MutateDatabase(db =>
            {
                var itSystem = db.ItSystems.AsQueryable().ByUuid(system.uuid);
                var interfaceToExpose = db.Set<ItInterface>().AsQueryable().ById(exposedInterface.Id);
                var taskRef = db.TaskRefs.AsQueryable().First();

                itSystem.PreviousName = A<string>();
                itSystem.Description = A<string>();
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
            DatabaseAccess.MutateEntitySet<Core.DomainModel.ItSystem.ItSystem>(systems =>
            {
                var itSystem = systems.AsQueryable().ByUuid(system.uuid);
                itSystem.Disabled = A<bool>(); //Cannot before setting into use because if it becomes false, the taking into use will fail
            });

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

            var expected1 = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);
            var expected2 = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var expected3 = await CreateSystemAsync(organization.Id, AccessModifier.Local);

            using var resp1 = await ItSystemHelper.SendSetBelongsToRequestAsync(expected1.dbId, rightsHolder.Id, TestEnvironment.DefaultOrganizationId);
            using var resp2 = await ItSystemHelper.SendSetBelongsToRequestAsync(expected2.dbId, rightsHolder.Id, TestEnvironment.DefaultOrganizationId);
            using var resp3 = await ItSystemHelper.SendSetBelongsToRequestAsync(expected3.dbId, rightsHolder.Id, organization.Id);

            Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
            Assert.Equal(HttpStatusCode.OK, resp3.StatusCode);

            //Act
            var systems = (await ItSystemV2Helper.GetManyAsync(token, rightsHolderId: rightsHolder.Uuid)).ToList();

            Assert.Equal(3, systems.Count);
            Assert.Contains(systems, dto => dto.Uuid == expected1.uuid);
            Assert.Contains(systems, dto => dto.Uuid == expected2.uuid);
            Assert.Contains(systems, dto => dto.Uuid == expected3.uuid);
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
        public async Task GET_Many_As_StakeHolder_With_UsedInOrganizationUuid_Filter()
        {
            //Assert
            var (token, org) = await CreateStakeHolderUserInNewOrganizationAsync();
            var org2 = await CreateOrganizationAsync();

            var system1 = await CreateSystemAsync(org.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(org.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(org.Id, AccessModifier.Public);

            await ItSystemHelper.TakeIntoUseAsync(system1.dbId, org.Id);
            await ItSystemHelper.TakeIntoUseAsync(system2.dbId, org.Id);
            await ItSystemHelper.TakeIntoUseAsync(system3.dbId, org2.Id);

            //Act
            var systems = (await ItSystemV2Helper.GetManyAsync(token, usedInOrganizationUuid: org.Uuid)).ToList();

            //Arrange
            Assert.Equal(2, systems.Count);
            Assert.Contains(systems, x => x.Uuid == system1.uuid);
            Assert.Contains(systems, x => x.Uuid == system2.uuid);
            Assert.DoesNotContain(systems, x => x.Uuid == system3.uuid);
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

        [Fact]
        public async Task GET_Many_As_StakeHolder_With_ChangesSince_Filter()
        {
            //Arrange
            var (token, organization) = await CreateStakeHolderUserInNewOrganizationAsync();
            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var system3 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);

            await ItSystemHelper.SetNameAsync(system2.Id, CreateName(), organization.Id);
            await ItSystemHelper.SetNameAsync(system3.Id, CreateName(), organization.Id);
            await ItSystemHelper.SetNameAsync(system1.Id, CreateName(), organization.Id);
            var system3DTO = await ItSystemV2Helper.GetSingleAsync(token, system3.Uuid); //system 3 was changed as the second one and system 1 the last


            //Act
            var dtos = (await ItSystemV2Helper.GetManyAsync(token, changedSinceGtEq: system3DTO.LastModified, page: 0, pageSize: 10)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            Assert.Equal(new[] { system3.Uuid, system1.Uuid }, dtos.Select(x => x.Uuid).ToArray());
        }

        [Fact]
        public async Task GET_Many_As_StakeHolder_With_NameContains_Filter()
        {
            //Arrange
            var (token, organization) = await CreateStakeHolderUserInNewOrganizationAsync();
            var baseName = CreateName();
            var searchName = $"{baseName}1";
            var validName2 = $"{searchName}2";

            await ItSystemHelper.CreateItSystemInOrganizationAsync(baseName, organization.Id, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(searchName, organization.Id, AccessModifier.Public);
            var system3 = await ItSystemHelper.CreateItSystemInOrganizationAsync(validName2, organization.Id, AccessModifier.Public);

            //Act
            var dtos = (await ItSystemV2Helper.GetManyAsync(token, nameContains: searchName, page: 0, pageSize: 10)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            Assert.Equal(new[] { system2.Uuid, system3.Uuid }, dtos.Select(x => x.Uuid).ToArray());
        }

        [Fact]
        public async Task Can_Get_Hierarchy()
        {
            //Arrange
            var (token, organization) = await CreateStakeHolderUserInNewOrganizationAsync();
            var (rootUuid, createdSystems) = CreateHierarchy(organization.Id);

            //Act
            var response = await ItSystemV2Helper.GetHierarchyAsync(token, rootUuid);

            //Assert
            AssertHierarchy(rootUuid, response, createdSystems);
        }

        [Fact]
        public async Task Can_Get_Internal_Hierarchy()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var (rootUuid, createdSystems) = CreateHierarchy(organization.Id);
            var firstSystem = createdSystems.First();
            await ItSystemHelper.TakeIntoUseAsync(firstSystem.Id, organization.Id);


            //Act
            var response = await ItSystemV2Helper.GetInternalHierarchyAsync(organization.Uuid, rootUuid);

            //Assert
            var hierarchy = response.ToList();
            AssertHierarchy(rootUuid, hierarchy, createdSystems);
            var usedSystem = Assert.Single(hierarchy.Where(x => x.IsInUse));
            Assert.Equal(firstSystem.Uuid, usedSystem.Node.Uuid);
        }

        private static void AssertHierarchy(Guid rootUuid, IEnumerable<RegistrationHierarchyNodeWithActivationStatusResponseDTO> response, IReadOnlyList<Core.DomainModel.ItSystem.ItSystem> createdSystems)
        {
            var hierarchy = response.ToList();
            Assert.Equal(createdSystems.Count, hierarchy.Count);

            foreach (var node in hierarchy)
            {
                var system = Assert.Single(createdSystems, x => x.Uuid == node.Node.Uuid);
                Assert.Equal(node.Deactivated, system.Disabled);
                if (system.Uuid == rootUuid)
                {
                    Assert.Null(node.Parent);
                }
                else
                {
                    Assert.NotNull(node.Parent);
                    Assert.Equal(node.Parent.Uuid, system.Parent.Uuid);
                }
            }
        }

        [Fact]
        public async Task Can_POST_And_DELETE_Minimum_ItSystem()
        {
            //Arrange
            var organizationDto = await CreateOrganizationAsync();
            var name = CreateName();
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var payload = new CreateItSystemRequestDTO
            {
                OrganizationUuid = organizationDto.Uuid,
                Name = name
            };

            //Act - POST, DELETE, GET
            var createdSystem = await ItSystemV2Helper.CreateSystemAsync(token.Token, payload);
            using var deleteResponse = await ItSystemV2Helper.SendDeleteSystemAsync(token.Token, createdSystem.Uuid);
            using var getAfterDeleteResponse = await ItSystemV2Helper.SendGetSingleAsync(token.Token, createdSystem.Uuid);

            //Assert creation, deletion and GET after deletion
            AssertOrganization(organizationDto, createdSystem.OrganizationContext);
            Assert.Equal(name, createdSystem.Name);
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
        }


        [Fact]
        public async Task Can_POST_AND_DELETE_Full_ItSystem()
        {
            //Arrange
            var context = await PrepareFullItSystem();
            var (fullRequest, kleChoices, businessType, parent, token, organizationDto, rightsHolder) = context;

            //Act - POST, DELETE, GET
            var createdSystem = await ItSystemV2Helper.CreateSystemAsync(token, fullRequest);
            using var deleteResponse = await ItSystemV2Helper.SendDeleteSystemAsync(token, createdSystem.Uuid);
            using var getAfterDeleteResponse = await ItSystemV2Helper.SendGetSingleAsync(token, createdSystem.Uuid);

            //Assert creation, deletion and GET after deletion
            AssertOrganization(organizationDto, createdSystem.OrganizationContext);
            Assert.Equal(fullRequest.Name, createdSystem.Name);
            AssertOrganization(rightsHolder, createdSystem.RightsHolder);
            Assert.Equal(fullRequest.PreviousName, createdSystem.FormerName);
            Assert.Equal(fullRequest.Description, createdSystem.Description);
            Assert.Equal(fullRequest.Deactivated, createdSystem.Deactivated);
            Assert.Equivalent(businessType, createdSystem.BusinessType);
            Assert.Equal(fullRequest.Scope, createdSystem.Scope);
            Assert.Equivalent(kleChoices.Select(x => new IdentityNamePairResponseDTO(x.Uuid, x.KleNumber)), createdSystem.KLE);
            Assert.Equivalent(parent.Transform(x => new IdentityNamePairResponseDTO(x.Uuid, x.Name)), createdSystem.ParentSystem);
            Assert.Equivalent(fullRequest.RecommendedArchiveDuty.Id, createdSystem.RecommendedArchiveDuty.Id);
            Assert.Equivalent(fullRequest.RecommendedArchiveDuty.Comment, createdSystem.RecommendedArchiveDuty.Comment);
            Assert.Equivalent(fullRequest.ExternalReferences, createdSystem.ExternalReferences);

            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_ItSystem_With_Overlapping_Name()
        {
            //Arrange
            var organizationDto = await CreateOrganizationAsync();
            var name = CreateName();
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var payload = new CreateItSystemRequestDTO
            {
                OrganizationUuid = organizationDto.Uuid,
                Name = name
            };

            //Act 
            using var createdSystemResponse = await ItSystemV2Helper.SendCreateSystemAsync(token.Token, payload);
            using var createDuplicateResponse = await ItSystemV2Helper.SendCreateSystemAsync(token.Token, payload);

            //Assert 
            Assert.Equal(HttpStatusCode.Created, createdSystemResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, createDuplicateResponse.StatusCode);
        }

        public static IEnumerable<object[]> GetUndefinedItSystemSectionsInput() => CreateGetUndefinedSectionsInput(11);

        [Theory]
        [MemberData(nameof(GetUndefinedItSystemSectionsInput))]
        public async Task Can_PATCH(
            bool updateName,
            bool updateFormerName,
            bool updateDescription,
            bool updateReferences,
            bool updateBusinessType,
            bool updateKle,
            bool updateParent,
            bool updateRightsHolder,
            bool updateDeactivated,
            bool updateScope,
            bool updateArchiveDutyRecommendation)
        {
            //Arrange
            var createSystemRequest = await PrepareFullItSystem();
            var (fullRequest, _, _, _, token, organizationDto, _) = createSystemRequest;
            var createdSystem = await ItSystemV2Helper.CreateSystemAsync(token, fullRequest);

            var changes = new Dictionary<string, object>();
            if (updateName) changes.Add(nameof(UpdateItSystemRequestDTO.Name), CreateName());
            if (updateFormerName) changes.Add(nameof(UpdateItSystemRequestDTO.PreviousName), A<string>());
            if (updateDescription) changes.Add(nameof(UpdateItSystemRequestDTO.Description), A<string>());
            if (updateBusinessType) changes.Add(nameof(UpdateItSystemRequestDTO.BusinessTypeUuid), (await GetRandomBusinessType(organizationDto)).Uuid);
            if (updateParent) changes.Add(nameof(UpdateItSystemRequestDTO.ParentUuid), (await CreateSystemAsync(organizationDto.Id, AccessModifier.Public)).uuid);
            if (updateReferences) changes.Add(nameof(UpdateItSystemRequestDTO.ExternalReferences), CreateExternalReferences());
            if (updateKle) changes.Add(nameof(UpdateItSystemRequestDTO.KLEUuids), (await GetRandomKleChoices(token)).Select(x => x.Uuid).ToList());
            if (updateRightsHolder) changes.Add(nameof(UpdateItSystemRequestDTO.RightsHolderUuid), (await CreateOrganizationAsync()).Uuid);
            if (updateDeactivated) changes.Add(nameof(UpdateItSystemRequestDTO.Deactivated), !createdSystem.Deactivated);
            if (updateScope) changes.Add(nameof(UpdateItSystemRequestDTO.Scope), EnumRange.AllExcept(createdSystem.Scope).First());
            if (updateArchiveDutyRecommendation) changes.Add(nameof(UpdateItSystemRequestDTO.RecommendedArchiveDuty), CreateNewArchiveDutyRecommendation());

            //Act
            var updatedSystem = await ItSystemV2Helper.PatchSystemAsync(token, createdSystem.Uuid, changes.ToArray());

            //Assert that only the patched properties have changed
            Assert.Equal(createdSystem.Uuid, updatedSystem.Uuid); //No changes expected
            Assert.Equal(createdSystem.Created, updatedSystem.Created); //No changes expected
            Assert.Equal(updateRightsHolder ? changes[nameof(UpdateItSystemRequestDTO.RightsHolderUuid)] : createdSystem.RightsHolder.Uuid, updatedSystem.RightsHolder.Uuid);
            Assert.Equal(updateName ? changes[nameof(UpdateItSystemRequestDTO.Name)] : createdSystem.Name, updatedSystem.Name);
            Assert.Equal(updateFormerName ? changes[nameof(UpdateItSystemRequestDTO.PreviousName)] : createdSystem.FormerName, updatedSystem.FormerName);
            Assert.Equal(updateDescription ? changes[nameof(UpdateItSystemRequestDTO.Description)] : createdSystem.Description, updatedSystem.Description);
            Assert.Equal(updateBusinessType ? changes[nameof(UpdateItSystemRequestDTO.BusinessTypeUuid)] : createdSystem.BusinessType?.Uuid, updatedSystem.BusinessType?.Uuid);
            Assert.Equal(updateParent ? changes[nameof(UpdateItSystemRequestDTO.ParentUuid)] : createdSystem.ParentSystem?.Uuid, updatedSystem.ParentSystem?.Uuid);
            Assert.Equal(updateKle ? changes[nameof(UpdateItSystemRequestDTO.KLEUuids)] : createdSystem.KLE.Select(x => x.Uuid), updatedSystem.KLE.Select(x => x.Uuid));
            Assert.Equivalent(updateReferences ? changes[nameof(UpdateItSystemRequestDTO.ExternalReferences)] : createdSystem.ExternalReferences, updatedSystem.ExternalReferences);
            Assert.Equal(updateDeactivated ? changes[nameof(UpdateItSystemRequestDTO.Deactivated)] : createdSystem.Deactivated, updatedSystem.Deactivated);
            Assert.Equal(updateScope ? changes[nameof(UpdateItSystemRequestDTO.Scope)] : createdSystem.Scope, updatedSystem.Scope);
            Assert.Equivalent(updateArchiveDutyRecommendation ? changes[nameof(UpdateItSystemRequestDTO.RecommendedArchiveDuty)] : createdSystem.RecommendedArchiveDuty, updatedSystem.RecommendedArchiveDuty);
        }

        [Fact]
        public async Task Can_Create_Update_And_Delete_ExternalReference()
        {
            //Arrange
            var organizationDto = await CreateOrganizationAsync();
            var name = CreateName();
            var token = (await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin)).Token;
            var payload = new CreateItSystemRequestDTO
            {
                OrganizationUuid = organizationDto.Uuid,
                Name = name
            };
            var request = new ExternalReferenceDataWriteRequestDTO
            {
                DocumentId = A<string>(),
                MasterReference = A<bool>(),
                Title = A<string>(),
                Url = A<string>()
            };

            var createdSystem = await ItSystemV2Helper.CreateSystemAsync(token, payload);

            //Act
            var createdReference = await ItSystemV2Helper.AddExternalReferenceAsync(token, createdSystem.Uuid, request);

            //Assert
            AssertExternalReference(request, createdReference);

            var afterCreate = await ItSystemV2Helper.GetSingleAsync(token, createdSystem.Uuid);

            var checkCreatedExternalReference = Assert.Single(afterCreate.ExternalReferences);
            AssertExternalReference(request, checkCreatedExternalReference);

            //Arrange - update
            var updateRequest = new ExternalReferenceDataWriteRequestDTO
            {
                DocumentId = A<string>(),
                MasterReference = request.MasterReference || A<bool>(),
                Title = A<string>(),
                Url = A<string>()
            };

            //Act - update
            var updatedReference = await ItSystemV2Helper.UpdateExternalReferenceAsync(token, createdSystem.Uuid, createdReference.Uuid, updateRequest);

            //Assert - update
            AssertExternalReference(updateRequest, updatedReference);

            var afterUpdate = await ItSystemV2Helper.GetSingleAsync(token, createdSystem.Uuid);

            var checkUpdatedExternalReference = Assert.Single(afterUpdate.ExternalReferences);
            AssertExternalReference(updateRequest, checkUpdatedExternalReference);

            //Act - delete
            await ItSystemV2Helper.DeleteExternalReferenceAsync(token, createdSystem.Uuid, createdReference.Uuid);

            //Assert - delete
            var afterDelete = await ItSystemV2Helper.GetSingleAsync(token, createdSystem.Uuid);
            Assert.Empty(afterDelete.ExternalReferences);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true, true, true, true)]
        [InlineData(OrganizationRole.LocalAdmin, true, true, true, false)]
        [InlineData(OrganizationRole.User, true, false, false, false)]
        public async Task Can_Get_ItSystem_Permissions(OrganizationRole role, bool read, bool modify, bool delete, bool modifyVisibility)
        {
            //Arrange
            var org = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUser(org.Id);

            await HttpApi.SendAssignRoleToUserAsync(user.Id, role, org.Id).DisposeAsync();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), org.Id, AccessModifier.Public);

            //Act
            var permissionsResponseDto = await ItSystemV2Helper.GetPermissionsAsync(token, system.Uuid);

            //Assert
            var expected = new ItSystemPermissionsResponseDTO
            {
                Read = read,
                Modify = modify,
                Delete = delete,
                DeletionConflicts = new List<SystemDeletionConflict>(),
                ModifyVisibility = modifyVisibility
            };
            Assert.Equivalent(expected, permissionsResponseDto);
        }

        [Fact]
        public async Task Can_Get_ItSystem_Modify_Visibility_Permission()
        {
            //Arrange
            var org = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUser(org.Id);

            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.GlobalAdmin, org.Id).DisposeAsync();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), org.Id, AccessModifier.Local);

            //Act
            var permissionsResponseDto = await ItSystemV2Helper.GetPermissionsAsync(token, system.Uuid);

            //Assert
            var expected = new ItSystemPermissionsResponseDTO
            {
                Read = true,
                Modify = true,
                Delete = true,
                DeletionConflicts = new List<SystemDeletionConflict>(),
                ModifyVisibility = true
            };
            Assert.Equivalent(expected, permissionsResponseDto);
        }

        [Fact]
        public async Task Can_Get_ItSystem_Permissions_With_InUse_Deletion_Conflicts()
        {
            await Test_Get_ItSystem_Permissions_With_Deletion_Conflicts((token, system) => ItSystemUsageV2Helper.PostAsync(token, new CreateItSystemUsageRequestDTO
            {
                OrganizationUuid = system.OrganizationContext.Uuid,
                SystemUuid = system.Uuid
            }), SystemDeletionConflict.HasItSystemUsages);
        }

        [Fact]
        public async Task Can_Get_ItSystem_Permissions_With_HasChildren_Deletion_Conflicts()
        {
            await Test_Get_ItSystem_Permissions_With_Deletion_Conflicts((token, system) => ItSystemV2Helper.CreateSystemAsync(token, new CreateItSystemRequestDTO
            {
                Name = A<string>(),
                OrganizationUuid = system.OrganizationContext.Uuid,
                Scope = RegistrationScopeChoice.Global,
                ParentUuid = system.Uuid
            }), SystemDeletionConflict.HasChildSystems);
        }

        [Fact]
        public async Task Can_Get_ItSystem_Permissions_With_InterfaceExposure_Deletion_Conflicts()
        {
            await Test_Get_ItSystem_Permissions_With_Deletion_Conflicts((token, system) => InterfaceV2Helper.CreateItInterfaceAsync(token, new CreateItInterfaceRequestDTO()
            {
                Name = A<string>(),
                OrganizationUuid = system.OrganizationContext.Uuid,
                ExposedBySystemUuid = system.Uuid
            }), SystemDeletionConflict.HasInterfaceExposures);
        }

        private async Task Test_Get_ItSystem_Permissions_With_Deletion_Conflicts(Func<string, ItSystemResponseDTO, Task> createConflict, SystemDeletionConflict expectedConflict)
        {
            //Arrange
            var org = await CreateOrganizationAsync();
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            var system = await ItSystemV2Helper.CreateSystemAsync(token.Token, new CreateItSystemRequestDTO
            {
                Name = A<string>(),
                OrganizationUuid = org.Uuid,
                Scope = RegistrationScopeChoice.Global
            });
            await createConflict(token.Token, system);

            //Act
            var permissionsResponseDto = await ItSystemV2Helper.GetPermissionsAsync(token.Token, system.Uuid);

            //Assert
            var expected = new ItSystemPermissionsResponseDTO
            {
                Read = true,
                Modify = true,
                Delete = true,
                DeletionConflicts = new List<SystemDeletionConflict>
                {
                    expectedConflict
                },
                ModifyVisibility = true
            };
            Assert.Equivalent(expected, permissionsResponseDto);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.LocalAdmin, false)]
        [InlineData(OrganizationRole.User, false)]
        public async Task Can_Get_ItSystem_CollectionPermissions(OrganizationRole role, bool create)
        {
            //Arrange
            var org = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUser(org.Id);

            await HttpApi.SendAssignRoleToUserAsync(user.Id, role, org.Id).DisposeAsync();

            //Act
            var permissionsResponseDto = await ItSystemV2Helper.GetCollectionPermissionsAsync(token, org.Uuid);

            //Assert
            var expected = new ResourceCollectionPermissionsResponseDTO
            {
                Create = create
            };
            Assert.Equivalent(expected, permissionsResponseDto);
        }

        private async Task<(User user, string token)> CreateApiUser(int organizationId)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organizationId, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
        }

        private static void AssertExternalReference(ExternalReferenceDataWriteRequestDTO expected, ExternalReferenceDataResponseDTO actual)
        {
            Assert.Equal(expected.DocumentId, actual.DocumentId);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.Url, actual.Url);
            Assert.Equal(expected.MasterReference, actual.MasterReference);
        }

        private (Guid rootUuid, IReadOnlyList<Core.DomainModel.ItSystem.ItSystem> createdItSystems) CreateHierarchy(int orgId)
        {
            var rootSystem = CreateNewItSystem(orgId);
            var childSystem = CreateNewItSystem(orgId);
            var grandchildSystem = CreateNewItSystem(orgId);

            var createdSystems = new List<Core.DomainModel.ItSystem.ItSystem> { rootSystem, childSystem, grandchildSystem };

            childSystem.Children = new List<Core.DomainModel.ItSystem.ItSystem>
            {
                grandchildSystem
            };
            rootSystem.Children = new List<Core.DomainModel.ItSystem.ItSystem>
            {
                childSystem
            };

            DatabaseAccess.MutateEntitySet<Core.DomainModel.ItSystem.ItSystem>(repository =>
            {
                repository.Insert(rootSystem);
            });

            return (rootSystem.Uuid, createdSystems);
        }

        private Core.DomainModel.ItSystem.ItSystem CreateNewItSystem(int orgId)
        {
            return new Core.DomainModel.ItSystem.ItSystem
            {
                Name = A<string>(),
                OrganizationId = orgId,
                ObjectOwnerId = TestEnvironment.DefaultUserId,
                LastChangedByUserId = TestEnvironment.DefaultUserId
            };
        }

        protected async Task<(string token, OrganizationDTO createdOrganization)> CreateStakeHolderUserInNewOrganizationAsync()
        {
            var organization = await CreateOrganizationAsync();

            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(),
                OrganizationRole.User, organization.Id, true, true);
            return (token, organization);
        }

        private static void AssertOrganization(OrganizationDTO expected, ShallowOrganizationResponseDTO actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Cvr, actual.Cvr);
            Assert.Equal(expected.Uuid, actual.Uuid);
        }

        private async Task<(
            CreateItSystemRequestDTO fullRequest,
            List<KLEDetailsDTO> kleChoices,
            IdentityNamePairResponseDTO businessType,
            ItSystemResponseDTO parent,
            string token,
            OrganizationDTO organizationDto,
            OrganizationDTO rightsHolder)> PrepareFullItSystem()
        {
            var organizationDto = await CreateOrganizationAsync();
            var rightsHolderOrgDto = await CreateOrganizationAsync();
            var name = CreateName();
            var token = (await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin)).Token;
            var parentSystemParams = new CreateItSystemRequestDTO
            {
                OrganizationUuid = organizationDto.Uuid,
                Name = $"parent_{name}"
            };
            var kleChoices = await GetRandomKleChoices(token);
            var businessType = await GetRandomBusinessType(organizationDto);
            var parent = await ItSystemV2Helper.CreateSystemAsync(token, parentSystemParams);
            var references = CreateExternalReferences();
            var fullSystemParams = new CreateItSystemRequestDTO
            {
                OrganizationUuid = organizationDto.Uuid,
                Name = name,
                ParentUuid = parent.Uuid,
                PreviousName = A<string>(),
                Description = A<string>(),
                ExternalReferences = references,
                BusinessTypeUuid = businessType.Uuid,
                Deactivated = false,
                RecommendedArchiveDuty = CreateNewArchiveDutyRecommendation(),
                RightsHolderUuid = rightsHolderOrgDto.Uuid,
                Scope = A<RegistrationScopeChoice>(),
                KLEUuids = kleChoices.Select(x => x.Uuid).ToList()
            };
            return (fullSystemParams, kleChoices, businessType, parent, token, organizationDto, rightsHolderOrgDto);
        }

        private RecommendedArchiveDutyRequestDTO CreateNewArchiveDutyRecommendation()
        {
            return new RecommendedArchiveDutyRequestDTO
            {
                Id = EnumRange.AllExcept(RecommendedArchiveDutyChoice.Undecided).RandomItem(),
                Comment = A<string>()
            };
        }

        private List<ExternalReferenceDataWriteRequestDTO> CreateExternalReferences()
        {
            return Many<ExternalReferenceDataWriteRequestDTO>().Transform(ExternalReferenceTestHelper.WithRandomMaster).ToList();
        }

        private static async Task<IdentityNamePairResponseDTO> GetRandomBusinessType(OrganizationDTO organizationDto)
        {
            return (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.BusinessType, organizationDto.Uuid, 10,
                    0))
                .RandomItem();
        }

        private static async Task<List<KLEDetailsDTO>> GetRandomKleChoices(string token)
        {
            return (await KleOptionV2Helper.GetKleNumbersAsync(token)).Payload.Take(2).ToList();
        }

        protected static IEnumerable<object[]> CreateGetUndefinedSectionsInput(int numberOfInputParameters)
        {
            var referenceValues = Enumerable.Repeat(false, numberOfInputParameters).ToList();
            yield return referenceValues.Cast<object>().ToArray();
            for (var i = 0; i < referenceValues.Count; i++)
            {
                var inputs = referenceValues.ToList();
                inputs[i] = true;
                yield return inputs.Cast<object>().ToArray();
            }

            yield return referenceValues.Select(_ => true).Cast<object>().ToArray();
        }
    }
}
