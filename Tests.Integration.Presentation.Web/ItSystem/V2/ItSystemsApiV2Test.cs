using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem.V2
{
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
            var businessType = await EntityOptionHelper.CreateBusinessTypeAsync(CreateName(), organizationId);
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

        [Fact]
        public async Task GET_Many_As_StakeHolder_With_BusinessTypeFilter()
        {
            //Arrange
            var (token, _) = await CreateStakeHolderUserInNewOrganizationAsync();
            var businessType1 = A<string>();
            var businessType2 = A<string>();
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var correctBusinessType = await EntityOptionHelper.CreateBusinessTypeAsync(businessType1, organizationId);
            var incorrectBusinessType = await EntityOptionHelper.CreateBusinessTypeAsync(businessType2, organizationId);
            var correctBusinessTypeId = TestEnvironment.GetEntityUuid<BusinessType>(correctBusinessType.Id);

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
            var businessType = await EntityOptionHelper.CreateBusinessTypeAsync(CreateName(), organizationId);
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
        [InlineData(true, true, true, true, true)]
        public async Task Can_POST_ItSystem_As_RightsHolder(bool withProvidedUuid, bool withBusinessType, bool withKleNumbers, bool withKleUuid, bool withParent)
        {
            //Arrange
            var (token, createdOrganization) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var parentCandidate = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), createdOrganization.Id, AccessModifier.Local);

            var businessType = DatabaseAccess.MapFromEntitySet<BusinessType, Guid>(repository => repository.AsQueryable().First(x => x.IsEnabled && x.IsObligatory).Uuid);
            var kle = DatabaseAccess.MapFromEntitySet<TaskRef, (string key, Guid uuid)>(x => x.AsQueryable().First().Transform(taskRef => (taskRef.TaskKey, taskRef.Uuid)));
            var dto = new RightsHolderCreateItSystemRequestDTO
            {
                RightsHolderUuid = createdOrganization.Uuid,
                Uuid = withProvidedUuid ? Guid.NewGuid() : null,
                Name = A<string>(),
                Description = A<string>(),
                FormerName = A<string>(),
                UrlReference = $"https://{A<int>()}.dk",
                BusinessTypeUuid = withBusinessType ? businessType : null,
                KLENumbers = withKleNumbers ? new[] { kle.key } : new string[0],
                KLEUuids = withKleUuid ? new[] { kle.uuid } : new Guid[0],
                ParentUuid = withParent ? parentCandidate.Uuid : null
            };

            //Act
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, dto);

            //Assert
            //TODO: Assert properties of response
        }
        //TODO: Error cases
        //TODO: Parent system cases
        //TODO: Minimum cases - maybe covered if we parameterize the test above

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
            Assert.Equal(dbSystem.ArchiveDuty?.ToString("G"), systemDTO.RecommendedArchiveDutyResponse.Id.ToString("G"));
            Assert.Equal(dbSystem.ArchiveDutyComment, systemDTO.RecommendedArchiveDutyResponse.Comment);
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
            var entityUuid = TestEnvironment.GetEntityUuid<Core.DomainModel.ItSystem.ItSystem>(createdSystem.Id);

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
    }
}
