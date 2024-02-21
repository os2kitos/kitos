using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using ExpectedObjects;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Request;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.System.RightsHolder;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem.V2
{
    [Collection(nameof(SequentialTestGroup))]
    public class ItSystemsApiV2RightsHoldersTest : BaseItSystemsApiV2Test
    {
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

        [Fact]
        public async Task GET_Many_As_RightsHolder_With_ChangesSince_Filter()
        {
            //Arrange
            var (token, organization) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var system3 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);

            await ItSystemHelper.SetNameAsync(system2.Id, CreateName(), organization.Id);
            await ItSystemHelper.SetNameAsync(system3.Id, CreateName(), organization.Id);
            await ItSystemHelper.SetNameAsync(system1.Id, CreateName(), organization.Id);
            var system3DTO = await ItSystemHelper.GetSystemAsync(system3.Id);

            //Act
            var dtos = (await ItSystemV2Helper.GetManyRightsHolderSystemsAsync(token, changedSinceGtEq: system3DTO.LastChanged, page: 0, pageSize: 10)).ToList();

            //Assert that the correct systems are returned in the correct order
            Assert.Equal(2, dtos.Count);
            Assert.Equal(new[] { system3.Uuid, system1.Uuid }, dtos.Select(x => x.Uuid).ToArray());
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
        [InlineData(true, true, true, true, true)]
        [InlineData(true, false, true, true, true)]
        [InlineData(false, false, true, true, true)]
        [InlineData(true, true, false, true, true)]
        [InlineData(true, false, true, false, true)]
        [InlineData(true, false, true, true, false)]
        [InlineData(false, false, false, false, false)]

        public async Task Can_POST_ItSystem_As_RightsHolder(bool withBusinessType, bool withKleUuid, bool withParent, bool withFormerName, bool withExternalUuid)
        {
            //Arrange
            var (userId, token, createdOrganization) = await CreateRightsHolderAccessUserInNewOrganizationAndGetFullUserAsync();
            var input = await PrepareCreateRightsHolderSystemRequestAsync(withBusinessType, withKleUuid, withParent, withFormerName, withExternalUuid, createdOrganization);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(r => r.AsQueryable().ById(userId));

            //Act - create it and GET it to verify that response DTO matches input requests AND that a consecutive GET returns the same data
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, input);
            var fetchedSystem = await ItSystemV2Helper.GetSingleRightsHolderSystemAsync(token, createdSystem.Uuid);

            //Assert
            Assert.Equal(input.Description, createdSystem.Description);
            Assert.Equal(input.Name, createdSystem.Name);
            Assert.Equal(input.PreviousName, createdSystem.FormerName);
            Assert.Equivalent(input.ExternalReferences, createdSystem.ExternalReferences);
            Assert.Equal(input.BusinessTypeUuid, createdSystem.BusinessType?.Uuid);

            if (withKleUuid)
                Assert.Equal(input.KLEUuids, createdSystem.KLE.Select(x => x.Uuid));
            if (!withKleUuid)
                Assert.Empty(createdSystem.KLE);

            Assert.Equal(input.ParentUuid, createdSystem.ParentSystem?.Uuid);
            Assert.Equal(DateTime.UtcNow.Date, createdSystem.Created.GetValueOrDefault().Date);
            Assert.Equal(createdOrganization.Uuid, createdSystem.RightsHolder.Uuid);
            Assert.Equal(createdOrganization.Name, createdSystem.RightsHolder.Name);
            Assert.Equal(createdOrganization.Cvr, createdSystem.RightsHolder.Cvr);
            Assert.Equal(user.Uuid, createdSystem.CreatedBy.Uuid);
            Assert.Equal(user.GetFullName(), createdSystem.CreatedBy.Name);

            //Check the fetched system
            createdSystem.ToExpectedObject().ShouldMatch(fetchedSystem);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        public async Task Cannot_POST_ItSystem_AsRightsHolder_WithoutAllRequiredFields(bool withoutRightsHolder, bool withoutName, bool withoutDescription)
        {
            //Arrange
            var (token, org) = await CreateRightsHolderAccessUserInNewOrganizationAsync();

            var input = new RightsHolderFullItSystemRequestDTO
            {
                RightsHolderUuid = withoutRightsHolder ? Guid.Empty : org.Uuid,
                Name = withoutName ? null : $"Name_{A<string>()}",
                Description = withoutDescription ? null : $"Description_{A<string>()}"
            };

            //Act
            using var createdSystem = await ItSystemV2Helper.SendCreateRightsHolderSystemAsync(token, input);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, createdSystem.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_ItSystem_As_RightsHolder_To_Organization_Where_User_Is_Not_RightsHolder()
        {
            //Arrange
            var (token, _) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var defaultOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            var input = new RightsHolderFullItSystemRequestDTO
            {
                RightsHolderUuid = defaultOrgUuid,
                Name = $"Name_{A<string>()}",
                Description = $"Description_{A<string>()}"
            };

            //Act 
            using var createdSystem = await ItSystemV2Helper.SendCreateRightsHolderSystemAsync(token, input);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, createdSystem.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_ItSystem_As_RightsHolder_With_Parent_System_Where_User_Is_Not_RightsHolder()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var (parentUuid, dbId) = await CreateSystemAsync(TestEnvironment.DefaultOrganizationId, AccessModifier.Local);

            var input = new RightsHolderFullItSystemRequestDTO
            {
                RightsHolderUuid = org.Uuid,
                Name = $"Name_{A<string>()}",
                Description = $"Description_{A<string>()}",
                ParentUuid = parentUuid
            };

            //Act 
            using var createdSystem = await ItSystemV2Helper.SendCreateRightsHolderSystemAsync(token, input);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, createdSystem.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_ItSystem_With_Duplicate_KLE()
        {
            //Arrange
            var (token, org) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var kle = DatabaseAccess.MapFromEntitySet<TaskRef, (string key, Guid uuid)>(x => x.AsQueryable().First().Transform(taskRef => (taskRef.TaskKey, taskRef.Uuid)));

            var kleUuids = Enumerable.Repeat(kle.uuid, 2);

            var input = new RightsHolderFullItSystemRequestDTO
            {
                RightsHolderUuid = org.Uuid,
                Name = $"Name_{A<string>()}",
                Description = $"Description_{A<string>()}",
                KLEUuids = kleUuids
            };

            //Act 
            using var createdSystem = await ItSystemV2Helper.SendCreateRightsHolderSystemAsync(token, input);

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
            using var result = await ItSystemV2Helper.SendUpdateRightsHolderSystemAsync(token, uuid, A<RightsHolderFullItSystemRequestDTO>());

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
            using var result = await ItSystemV2Helper.SendUpdateRightsHolderSystemAsync(token, uuid, A<RightsHolderFullItSystemRequestDTO>());

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
            using var result = await ItSystemV2Helper.SendUpdateRightsHolderSystemAsync(token, uuid, A<RightsHolderFullItSystemRequestDTO>());

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Theory]
        [InlineData(true, true, true, true, true, true, true, true)]
        [InlineData(true, true, true, true, true, true, false, false)]
        [InlineData(true, true, true, true, true, false, false, false)]
        [InlineData(true, true, true, true, false, false, false, false)]
        [InlineData(true, true, true, false, false, false, false, false)]
        [InlineData(true, true, false, false, false, false, false, false)]
        [InlineData(true, false, false, false, false, false, false, false)]
        [InlineData(false, false, false, false, false, false, false, true)]
        [InlineData(false, false, false, false, false, false, false, false)]
        public async Task Can_PUT_As_RightsHolder(bool updateName, bool updateFormerName, bool updateDescription, bool updateUrl, bool updateBusinessType, bool updateKle, bool updateParent, bool updateExternalUuid)
        {
            //Arrange
            var (token, rightsHolder) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var createSystemRequest = await PrepareCreateRightsHolderSystemRequestAsync(true, false, true, true, true, rightsHolder);
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest);
            var update = await A<RightsHolderFullItSystemRequestDTO>().Transform(async dto =>
            {
                dto.ExternalReferences.ToList().ForEach(x => x.MasterReference = false);
                dto.ExternalReferences.RandomItem().MasterReference = true;
                dto.Name = updateName ? dto.Name : createdSystem.Name;
                dto.PreviousName = updateFormerName ? dto.PreviousName : createdSystem.FormerName;
                dto.Description = updateDescription ? dto.Description : createdSystem.Description;
                dto.BusinessTypeUuid = updateBusinessType ? GetBusinessType(1) : createdSystem.BusinessType.Uuid;
                dto.ExternalReferences = updateUrl ? dto.ExternalReferences : createdSystem.ExternalReferences.Select(er => new ExternalReferenceDataWriteRequestDTO()
                {
                    DocumentId = er.DocumentId,
                    MasterReference = er.MasterReference,
                    Title = er.Title,
                    Url = er.Url
                });
                dto.KLEUuids = null;
                dto.ExternalUuid = updateExternalUuid ? dto.ExternalUuid : createdSystem.ExternalUuid;
                dto.ParentUuid = updateParent ? (await CreateSystemAsync(rightsHolder.Id, AccessModifier.Public)).uuid : createdSystem.ParentSystem.Uuid;
                return dto;
            });
            var expectedMasterReferenceAfter = update.ExternalReferences.Single(x => x.MasterReference);

            //Act
            var updatedSystem = await ItSystemV2Helper.UpdateRightsHolderSystemAsync(token, createdSystem.Uuid, update);

            //Assert
            Assert.Equal(createdSystem.Uuid, updatedSystem.Uuid); //No changes expected
            Assert.Equal(createdSystem.Created, updatedSystem.Created); //No changes expected
            createdSystem.RightsHolder.ToExpectedObject().ShouldMatch(updatedSystem.RightsHolder); //No changes expected
            Assert.Equal(update.Name, updatedSystem.Name);
            Assert.Equal(update.Description, updatedSystem.Description);
            Assert.Equal(update.PreviousName, updatedSystem.FormerName);
            Assert.Equal(update.ExternalUuid, updatedSystem.ExternalUuid);
            Assert.Equal(update.BusinessTypeUuid.GetValueOrDefault(), updatedSystem.BusinessType.Uuid);
            Assert.Equal(update.ParentUuid.GetValueOrDefault(), updatedSystem.ParentSystem.Uuid);

            var masterReferenceAfter = updatedSystem.ExternalReferences.Single(x => x.MasterReference);
            Assert.Equal(expectedMasterReferenceAfter.DocumentId, masterReferenceAfter.DocumentId);
            Assert.Equal(expectedMasterReferenceAfter.Title, masterReferenceAfter.Title);
            Assert.Equal(expectedMasterReferenceAfter.Url, masterReferenceAfter.Url);
        }

        [Theory]
        [InlineData(true, true, true, true, true, true, true, true)]
        [InlineData(true, true, true, true, true, true, false, false)]
        [InlineData(true, true, true, true, true, false, false, false)]
        [InlineData(true, true, true, true, false, false, false, false)]
        [InlineData(true, true, true, false, false, false, false, false)]
        [InlineData(true, true, false, false, false, false, false, false)]
        [InlineData(true, false, false, false, false, false, false, false)]
        [InlineData(false, false, false, false, false, false, false, true)]
        [InlineData(false, false, false, false, false, false, false, false)]
        public async Task Can_PATCH_As_RightsHolder(bool updateName, bool updateFormerName, bool updateDescription, bool updateUrl, bool updateBusinessType, bool updateKle, bool updateParent, bool updateExternalUuid)
        {
            //Arrange
            var (token, rightsHolder) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var createSystemRequest = await PrepareCreateRightsHolderSystemRequestAsync(true, false, true, true, true, rightsHolder);
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest);

            var changes = new Dictionary<string, object>();
            if (updateName) changes.Add(nameof(RightsHolderFullItSystemRequestDTO.Name), CreateName());
            if (updateFormerName) changes.Add(nameof(RightsHolderFullItSystemRequestDTO.PreviousName), A<string>());
            if (updateDescription) changes.Add(nameof(RightsHolderFullItSystemRequestDTO.Description), A<string>());
            if (updateBusinessType) changes.Add(nameof(RightsHolderFullItSystemRequestDTO.BusinessTypeUuid), GetBusinessType(1));
            if (updateParent) changes.Add(nameof(RightsHolderFullItSystemRequestDTO.ParentUuid), (await CreateSystemAsync(rightsHolder.Id, AccessModifier.Public)).uuid);
            if (updateUrl) changes.Add(nameof(RightsHolderFullItSystemRequestDTO.ExternalReferences), A<ExternalReferenceDataWriteRequestDTO>().Transform(x =>
            {
                x.MasterReference = true;
                return x;
            }).WrapAsEnumerable().ToList());
            if (updateExternalUuid) changes.Add(nameof(RightsHolderFullItSystemRequestDTO.ExternalUuid), A<Guid>());

            //Act
            var updatedSystem = await ItSystemV2Helper.PatchRightsHolderSystemAsync(token, createdSystem.Uuid, changes.ToArray());

            //Assert that only the patched properties have changed
            Assert.Equal(createdSystem.Uuid, updatedSystem.Uuid); //No changes expected
            Assert.Equal(createdSystem.Created, updatedSystem.Created); //No changes expected
            createdSystem.RightsHolder.ToExpectedObject().ShouldMatch(updatedSystem.RightsHolder); //No changes expected
            Assert.Equal(updateName ? changes[nameof(RightsHolderFullItSystemRequestDTO.Name)] : createdSystem.Name, updatedSystem.Name);
            Assert.Equal(updateFormerName ? changes[nameof(RightsHolderFullItSystemRequestDTO.PreviousName)] : createdSystem.FormerName, updatedSystem.FormerName);
            Assert.Equal(updateDescription ? changes[nameof(RightsHolderFullItSystemRequestDTO.Description)] : createdSystem.Description, updatedSystem.Description);
            Assert.Equal(updateBusinessType ? changes[nameof(RightsHolderFullItSystemRequestDTO.BusinessTypeUuid)] : createdSystem.BusinessType?.Uuid, updatedSystem.BusinessType?.Uuid);
            Assert.Equal(updateParent ? changes[nameof(RightsHolderFullItSystemRequestDTO.ParentUuid)] : createdSystem.ParentSystem?.Uuid, updatedSystem.ParentSystem?.Uuid);
            Assert.Equivalent(updateUrl ? changes[nameof(RightsHolderFullItSystemRequestDTO.ExternalReferences)] : createdSystem.ExternalReferences, updatedSystem.ExternalReferences);
            Assert.Equivalent(updateExternalUuid ? changes[nameof(RightsHolderFullItSystemRequestDTO.ExternalUuid)] : createdSystem.ExternalUuid, updatedSystem.ExternalUuid);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(true, false)]
        public async Task Cannot_PUT_As_RightsHolder_If_Required_Properties_Are_Missing(bool nullName, bool nullDescription)
        {
            //Arrange
            var (token, rightsHolder) = await CreateRightsHolderAccessUserInNewOrganizationAsync();
            var createSystemRequest = await PrepareCreateRightsHolderSystemRequestAsync(false, false, false, true, false, rightsHolder);
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest);
            var update = A<RightsHolderFullItSystemRequestDTO>().Transform(dto =>
            {
                dto.Name = nullName ? null : dto.Name;
                dto.Description = nullDescription ? null : dto.Description;
                dto.ParentUuid = null;
                dto.BusinessTypeUuid = null;
                dto.KLEUuids = null;
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

            var createSystemRequest = await PrepareCreateRightsHolderSystemRequestAsync(false, false, false, true, false, rightsHolder);
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest);
            var update = A<RightsHolderFullItSystemRequestDTO>().Transform(dto =>
            {
                dto.ParentUuid = parentUuid;
                dto.BusinessTypeUuid = null;
                dto.KLEUuids = null;
                dto.ExternalReferences = null;
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
            var createSystemRequest1 = await PrepareCreateRightsHolderSystemRequestAsync(false, false, false, true, false, rightsHolder);
            var createdSystem1 = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest1);
            var createSystemRequest2 = await PrepareCreateRightsHolderSystemRequestAsync(false, false, false, true, false, rightsHolder);
            var createdSystem2 = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest2);

            var update = A<RightsHolderFullItSystemRequestDTO>().Transform(dto =>
            {
                dto.Name = createdSystem2.Name; //Conflict with second system
                dto.ParentUuid = null;
                dto.BusinessTypeUuid = null;
                dto.KLEUuids = null;
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
            var createSystemRequest1 = await PrepareCreateRightsHolderSystemRequestAsync(false, false, false, true, false, rightsHolder);
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
            var createSystemRequest1 = await PrepareCreateRightsHolderSystemRequestAsync(false, false, false, true, false, rightsHolder);
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest1);

            var reason = new DeactivationReasonRequestDTO() { DeactivationReason = string.Empty };

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
            var createSystemRequest1 = await PrepareCreateRightsHolderSystemRequestAsync(false, false, false, true, false, rightsHolder);
            var createdSystem = await ItSystemV2Helper.CreateRightsHolderSystemAsync(token, createSystemRequest1);
            DatabaseAccess.MutateEntitySet<Core.DomainModel.ItSystem.ItSystem>(repository => repository.AsQueryable().ByUuid(createdSystem.Uuid).Deactivate());

            var reason = A<DeactivationReasonRequestDTO>();

            //Act
            using var result = await ItSystemV2Helper.SendDeleteRightsHolderSystemAsync(token, createdSystem.Uuid, reason);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Cannot_Invoke_Endpoint_Blocked_From_rightsHolders()
        {
            //Arrange
            var (token, _) = await CreateRightsHolderAccessUserInNewOrganizationAsync();

            //Act
            using var result = await ItSystemV2Helper.SendGetManyAsync(ItSystemV2Helper.BaseItSystemPath, token: token);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        private async Task<RightsHolderFullItSystemRequestDTO> PrepareCreateRightsHolderSystemRequestAsync(
            bool withBusinessType,
            bool withKleUuid,
            bool withParent,
            bool withFormerName,
            bool withExternalUuid,
            OrganizationDTO rightsHolderOrganization)
        {
            var parentCandidate = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), rightsHolderOrganization.Id, AccessModifier.Local);

            var businessType = GetBusinessType(0);
            var kle = DatabaseAccess.MapFromEntitySet<TaskRef, (string key, Guid uuid)>(x =>
                x.AsQueryable().First().Transform(taskRef => (taskRef.TaskKey, taskRef.Uuid)));
            return new RightsHolderFullItSystemRequestDTO
            {
                RightsHolderUuid = rightsHolderOrganization.Uuid,
                Name = $"Name_{A<string>()}",
                Description = $"Description_{A<string>()}",
                PreviousName = withFormerName ? $"FormerName_{A<string>()}" : null,
                BusinessTypeUuid = withBusinessType ? businessType : null,
                KLEUuids = withKleUuid ? new[] { kle.uuid } : Array.Empty<Guid>(),
                ParentUuid = withParent ? parentCandidate.Uuid : null,
                ExternalUuid = withExternalUuid ? A<Guid>() : null,
                ExternalReferences = A<ExternalReferenceDataWriteRequestDTO>().Transform(x =>
                {
                    x.MasterReference = true;
                    return x;
                }).WrapAsEnumerable()
            };
        }

        protected async Task<(string token, OrganizationDTO createdOrganization)> CreateRightsHolderAccessUserInNewOrganizationAsync()
        {
            var organization = await CreateOrganizationAsync();

            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(),
                OrganizationRole.RightsHolderAccess, organization.Id, true);
            return (token, organization);
        }

        protected async Task<(int userId, string token, OrganizationDTO createdOrganization)> CreateRightsHolderAccessUserInNewOrganizationAndGetFullUserAsync()
        {
            var organization = await CreateOrganizationAsync();

            var (id, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(),
                OrganizationRole.RightsHolderAccess, organization.Id, true);
            return (id, token, organization);
        }

        protected static Guid GetBusinessType(int skip)
        {
            return DatabaseAccess.MapFromEntitySet<BusinessType, Guid>(repository =>
                repository.AsQueryable().OrderBy(x => x.Id).Skip(skip).First(x => x.IsEnabled && x.IsObligatory).Uuid);
        }
    }
}
