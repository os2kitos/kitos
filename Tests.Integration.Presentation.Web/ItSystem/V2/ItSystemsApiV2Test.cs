using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.V2;
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
        public async Task GET_ItSystem_Returns_Expected_Data()
        {
            //Arrange
            var (token, _) = await CreateStakeHolderUserInNewOrganizationAsync();
            var rightsHolderOrganization = await CreateOrganizationAsync();
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var system = await CreateSystemAsync(organizationId, AccessModifier.Public);
            var parentSystem = await CreateSystemAsync(organizationId, AccessModifier.Public);
            var businessType = await EntityOptionHelper.CreateBusinessTypeAsync(CreateName(), organizationId);
            var businessTypeUuid = TestEnvironment.GetEntityUuid<BusinessType>(businessType.Id);
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

                var externalReference = new ExternalReference()
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
            var systemDTO = await ItSystemV2Helper.GetSingleAsync(token, system.uuid);

            //Assert - compare db entity with the response DTO
            Assert.NotNull(systemDTO);
            DatabaseAccess.MapFromEntitySet<Core.DomainModel.ItSystem.ItSystem, bool>(x =>
             {
                 var dbSystem = x.AsQueryable().ByUuid(system.uuid);
                 var parentDbSystem = x.AsQueryable().ByUuid(parentSystem.uuid);
                 var taskRef = dbSystem.TaskRefs.Single();
                 var dbExposedInterface = dbSystem.ItInterfaceExhibits.Single().ItInterface;

                 Assert.Equal(dbSystem.Uuid, systemDTO.Uuid);
                 Assert.Equal(dbSystem.Name, systemDTO.Name);
                 Assert.Equal(dbSystem.Description, systemDTO.Description);
                 Assert.Equal(dbSystem.PreviousName, systemDTO.FormerName);
                 Assert.Equal(dbSystem.Disabled, systemDTO.Deactivated);
                 Assert.Equal(dbSystem.Created, systemDTO.Created);
                 Assert.Equal(dbSystem.ObjectOwner.Uuid, systemDTO.CreatedBy.Uuid);
                 Assert.Equal(dbSystem.ObjectOwner.GetFullName(), systemDTO.CreatedBy.Name);
                 Assert.Equal(dbSystem.LastChanged, systemDTO.LastModified);
                 Assert.Equal(dbSystem.LastChangedByUser.Uuid, systemDTO.LastModifiedBy.Uuid);
                 Assert.Equal(dbSystem.LastChangedByUser.GetFullName(), systemDTO.LastModifiedBy.Name);
                 Assert.Equal(dbSystem.ArchiveDuty?.ToString("G"), systemDTO.RecommendedArchiveDutyResponse.Id.ToString("G"));
                 Assert.Equal(dbSystem.ArchiveDutyComment, systemDTO.RecommendedArchiveDutyResponse.Comment);
                 Assert.Equal(parentDbSystem.Uuid, systemDTO.ParentSystem.Uuid);
                 Assert.Equal(parentDbSystem.Name, systemDTO.ParentSystem.Name);
                 Assert.Equal(rightsHolderOrganization.Uuid, systemDTO.RightsHolder.Uuid);
                 Assert.Equal(rightsHolderOrganization.Name, systemDTO.RightsHolder.Name);
                 Assert.Equal(rightsHolderOrganization.GetActiveCvr(), systemDTO.RightsHolder.Cvr);
                 Assert.Equal(businessTypeUuid, systemDTO.BusinessType.Uuid);
                 Assert.Equal(businessType.Name, systemDTO.BusinessType.Name);
                 Assert.Equal(taskRef.Uuid, systemDTO.KLE.Single().Uuid);
                 Assert.Equal(taskRef.TaskKey, systemDTO.KLE.Single().Name);
                 Assert.Equal(dbExposedInterface.Uuid, systemDTO.ExposedInterfaces.Single().Uuid);
                 Assert.Equal(dbExposedInterface.Name, systemDTO.ExposedInterfaces.Single().Name);
                 Assert.Equal(dbSystem.Reference.URL, systemDTO.UrlReference);

                 return true;
             });
        }

        [Fact]
        public async Task GET_Many_Without_Filters()
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
        public async Task GET_Many_With_RightsHolderFilter()
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
        public async Task GET_Many_With_BusinessTypeFilter()
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
        public async Task GET_Many_With_KLE_Filter(bool useKeyAsFilter)
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
        public async Task GET_Many_With_NumberOfUsers_Filter()
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
