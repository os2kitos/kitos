using Core.DomainModel.ItSystem;
using Core.DomainServices.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Xunit;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Request.Interface;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Presentation.Web.Models.API.V2.Response.Interface;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.TestInputs;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Types.Interface;

namespace Tests.Integration.Presentation.Web.Interfaces.V2
{
    [Collection(nameof(SequentialTestGroup))]
    public class ItInterfaceApiV2Test : BaseItInterfaceApiV2Test
    {
        [Fact]
        public async Task Can_Get_Other_Organizations_Local_Interface_As_Stakeholder()
        {
            //Arrange
            var (token, _) = await CreateUserInNewOrg(true);

            var system = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface.Uuid, system.Uuid);
            //Act
            using var result = await InterfaceV2Helper.SendGetInterfaceAsync(token, itInterface.Uuid);

            //Assert
            Assert.True(result.IsSuccessStatusCode);
            var interfaceResponse = await result.ReadResponseBodyAsAsync<ItInterfaceResponseDTO>();
            Assert.Equal(itInterface.Uuid, interfaceResponse.Uuid);
        }

        [Fact]
        public async Task Can_Get_Interface_As_Stakeholder_With_Correct_Data()
        {
            //Arrange
            var (token, org) = await CreateUserInNewOrg(true);

            var system = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface = await CreateItInterfaceAsync(DefaultOrgUuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface.Uuid, system.Uuid);

            DatabaseAccess.MutateDatabase(db =>
            {
                var dbInterface = db.ItInterfaces.AsQueryable().ByUuid(itInterface.Uuid);

                dbInterface.Description = A<string>();
                dbInterface.ItInterfaceId = A<string>();
                dbInterface.Name = A<string>();
                dbInterface.Url = A<string>();
                dbInterface.Version = A<string>().Substring(0, ItInterface.MaxVersionLength); // Version has maxLength of 20
                dbInterface.Disabled = A<bool>();

                db.SaveChanges();
            });

            //Act
            var itInterfaceDTO = await InterfaceV2Helper.GetInterfaceAsync(token, itInterface.Uuid);

            //Assert
            Assert.NotNull(itInterfaceDTO);
            DatabaseAccess.MapFromEntitySet<ItInterface, bool>(x =>
            {
                var dbInterface = x.AsQueryable().ByUuid(itInterface.Uuid);
                BaseItInterfaceResponseDTODBCheck(dbInterface, itInterfaceDTO);

                Assert.Equal(dbInterface.LastChanged, itInterfaceDTO.LastModified);
                Assert.Equal(dbInterface.LastChanged, itInterfaceDTO.LastModified);

                Assert.Equal(dbInterface.LastChangedByUser.Uuid, itInterfaceDTO.LastModifiedBy.Uuid);
                Assert.Equal(dbInterface.LastChangedByUser.GetFullName(), itInterfaceDTO.LastModifiedBy.Name);

                return true;
            });
        }

        [Fact]
        public async Task Can_Get_Interfaces_As_Stakeholder()
        {
            //Arrange - Making sure there are at least 2 public interfaces
            var (token, org) = await CreateUserInNewOrg(true);

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface1 = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface2 = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Global);

            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface1.Uuid, system.Uuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface2.Uuid, system.Uuid);

            //Act
            var result = await InterfaceV2Helper.GetInterfacesAsync(token, pageSize, pageNumber);

            //Assert
            Assert.Equal(pageSize, result.Count());
        }

        [Fact]
        public async Task GET_Many_As_Stakeholder_With_ChangesSince_Filter()
        {
            //Arrange
            var (token, organization) = await CreateUserInNewOrg(true);
            var system = await CreateItSystemAsync(DefaultOrgUuid);
            var itInterface1 = await CreateItInterfaceAsync(DefaultOrgUuid);
            var itInterface2 = await CreateItInterfaceAsync(organization.Uuid);
            var itInterface3 = await CreateItInterfaceAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);

            await ItSystemV2Helper.PatchRightsHolderAsync(system.Uuid, DefaultOrgUuid).DisposeAsync();

            //Change all in a specific order
            foreach (var interfaceDto in new[] { itInterface2, itInterface3, itInterface1 })
            {
                await InterfaceV2Helper.PatchExposedBySystemAsync(interfaceDto.Uuid, system.Uuid);
            }

            var interface3LastModified = DatabaseAccess.MapFromEntitySet<ItInterface, DateTime>(x => x.AsQueryable().ByUuid(itInterface3.Uuid).LastChanged.Transform(dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc)));

            //Act
            var dtos = (await InterfaceV2Helper.GetInterfacesAsync(token, changedSinceGtEq: interface3LastModified, exposedBySystemUuid: system.Uuid, pageNumber: 0, pageSize: 10)).ToList();

            //Assert that the right interfaces are returned in the correct order
            Assert.Equal(2, dtos.Count);
            Assert.Equal(new[] { itInterface3.Uuid, itInterface1.Uuid }, dtos.Select(x => x.Uuid).ToArray());
        }

        [Fact]
        public async Task GET_Many_As_Stakeholder_With_Name_Filter()
        {
            //Arrange
            var (token, _) = await CreateStakeHolderUserInNewOrg();
            var searchName = A<string>();
            var invalidSearchName = $"{searchName}1";
            var itInterface1 = await CreateItInterfaceAsync(DefaultOrgUuid, searchName);
            await CreateItInterfaceAsync(DefaultOrgUuid, invalidSearchName);

            //Act
            var dtos = (await InterfaceV2Helper.GetInterfacesAsync(token, nameEquals: searchName, pageNumber: 0, pageSize: 10)).ToList();

            //Assert
            var dto = Assert.Single(dtos);
            Assert.Equal(dto.Name, itInterface1.Name);
            Assert.Equal(dto.Uuid, itInterface1.Uuid);
        }

        [Fact]
        public async Task GET_Many_As_Stakeholder_With_InterfaceId_Filter()
        {
            //Arrange
            var (token, _) = await CreateStakeHolderUserInNewOrg();
            var interfaceId = A<string>();
            var invalidInterfaceId = $"{interfaceId}1";
            var itInterface1 = await CreateItInterfaceAsync(DefaultOrgUuid, interfaceId: interfaceId);
            await CreateItInterfaceAsync(DefaultOrgUuid, interfaceId: invalidInterfaceId);

            //Act
            var dtos = (await InterfaceV2Helper.GetInterfacesAsync(token, interfaceId: interfaceId, pageNumber: 0, pageSize: 2)).ToList();

            //Assert
            var dto = Assert.Single(dtos);
            Assert.Equal(dto.InterfaceId, itInterface1.InterfaceId);
            Assert.Equal(dto.Uuid, itInterface1.Uuid);
        }

        [Fact]
        public async Task GET_Many_As_Stakeholder_With_OrganizationUuid_Filter()
        {
            //Arrange
            var (token, organization) = await CreateStakeHolderUserInNewOrg();
            var itInterface1 = await CreateItInterfaceAsync(organization.Uuid);
            await CreateItInterfaceAsync(DefaultOrgUuid);

            //Act
            var dtos = (await InterfaceV2Helper.GetInterfacesAsync(token, organizationUuid: organization.Uuid, pageNumber: 0, pageSize: 2)).ToList();

            //Assert
            var dto = Assert.Single(dtos);
            Assert.Equal(dto.Uuid, itInterface1.Uuid);
        }

        [Fact]
        public async Task GET_Many_As_Stakeholder_With_Part_Of_Name_Filter()
        {
            //Arrange
            var (token, org) = await CreateStakeHolderUserInNewOrg();
            var baseName = A<string>();
            var searchName = $"{baseName}1";
            var validName2 = searchName + A<string>();
            await CreateItInterfaceAsync(DefaultOrgUuid, baseName);
            var itInterface1 = await CreateItInterfaceAsync(org.Uuid, searchName);
            var itInterface2 = await CreateItInterfaceAsync(DefaultOrgUuid, validName2);

            //Act
            var dtos = (await InterfaceV2Helper.GetInterfacesAsync(token, nameContains: searchName, pageNumber: 0, pageSize: 10)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            Assert.Equal(new[] { itInterface1.Uuid, itInterface2.Uuid }, dtos.Select(x => x.Uuid).ToArray());
        }

        [Fact]
        public async Task GET_Many_As_Stakeholder_With_UsedInOrganizationUuid_Filter()
        {
            //Arrange
            var (token, organization) = await CreateStakeHolderUserInNewOrg();
            var system = await CreateItSystemAsync(organization.Uuid);
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var system3 = await CreateItSystemAsync(DefaultOrgUuid);

            var validInterface = await CreateItInterfaceAsync(organization.Uuid);
            await CreateItInterfaceAsync(organization.Uuid);
            var invalidNoUsageInterface = await CreateItInterfaceAsync(organization.Uuid);
            var invalidWrongOrganizationInterface = await CreateItInterfaceAsync(DefaultOrgUuid);

            await TakeSystemIntoUsageAsync(system.Uuid, organization.Uuid);
            await TakeSystemIntoUsageAsync(system3.Uuid, DefaultOrgUuid);

            await InterfaceV2Helper.PatchExposedBySystemAsync(validInterface.Uuid, system.Uuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(invalidNoUsageInterface.Uuid, system2.Uuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(invalidWrongOrganizationInterface.Uuid, system3.Uuid);

            //Act
            var dtos = (await InterfaceV2Helper.GetInterfacesAsync(token, usedInOrganizationUuid: organization.Uuid, pageNumber: 0, pageSize: 10)).ToList();

            //Assert
            var responseDto = Assert.Single(dtos);
            Assert.Equal(validInterface.Uuid, responseDto.Uuid);
        }

        [Fact]
        public async Task Can_Get_Interfaces_As_Local_Admin_With_PublicOrOrganizationUuid_Filter()
        {
            var (token, organization) = await CreateUserInNewOrg(false, OrganizationRole.LocalAdmin);
            var otherOrganization = await CreateOrganization("99887766");
            var systemInThisOrganization = await CreateItSystemAsync(organization.Uuid);
            var systemInOtherOrganization = await CreateItSystemAsync(otherOrganization.Uuid);

            var publicInterface = await CreateItInterfaceAsync(organization.Uuid);
            var localInterfaceInThisOrg = await CreateItInterfaceAsync(organization.Uuid, scope: RegistrationScopeChoice.Local);
            var localInterfaceInOtherOrg = await CreateItInterfaceAsync(otherOrganization.Uuid, scope: RegistrationScopeChoice.Local);

            await TakeSystemIntoUsageAsync(systemInThisOrganization.Uuid, organization.Uuid);
            await TakeSystemIntoUsageAsync(systemInOtherOrganization.Uuid, otherOrganization.Uuid);

            await InterfaceV2Helper.PatchExposedBySystemAsync(publicInterface.Uuid, systemInThisOrganization.Uuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(localInterfaceInThisOrg.Uuid, systemInThisOrganization.Uuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(localInterfaceInOtherOrg.Uuid, systemInThisOrganization.Uuid);

            var dtos = (await InterfaceV2Helper.GetInterfacesAsync(token,
                availableInOrganizationUuid: organization.Uuid, pageNumber: 0, pageSize: 200)).ToList();

            Assert.Contains(dtos, (dto) => dto.Uuid == publicInterface.Uuid);
            Assert.Contains(dtos, (dto) => dto.Uuid == localInterfaceInThisOrg.Uuid);
            Assert.DoesNotContain(dtos, (dto) => dto.Uuid == localInterfaceInOtherOrg.Uuid);
        }

        [Fact]
        public async Task Can_Get_Active_Interfaces_As_Stakeholder_By_Exposing_System()
        {
            var (token, org) = await CreateUserInNewOrg(true);

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system1 = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var system2 = await CreateItSystemAsync(DefaultOrgUuid, scope: RegistrationScopeChoice.Local);
            var itInterface1 = await CreateItInterfaceAsync(DefaultOrgUuid);
            var itInterface2 = await CreateItInterfaceAsync(DefaultOrgUuid);

            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface1.Uuid, system1.Uuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface2.Uuid, system2.Uuid);

            //Act
            var result = await InterfaceV2Helper.GetInterfacesAsync(token, pageSize, pageNumber, system1.Uuid);

            //Assert
            Assert.Single(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_Get_Interfaces_As_Stakeholder_Depends_On_IncludeDeactivated(bool shouldIncludeDeactivated)
        {
            var (token, org) = await CreateUserInNewOrg(true);

            var pageSize = 2;
            var pageNumber = 0; //Always takes the first page;

            var system = await CreateItSystemAsync(org.Uuid, scope: RegistrationScopeChoice.Local);
            var itInterface1 = await CreateItInterfaceAsync(DefaultOrgUuid);
            var itInterface2 = await CreateItInterfaceAsync(DefaultOrgUuid);

            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface1.Uuid, system.Uuid);
            await InterfaceV2Helper.PatchExposedBySystemAsync(itInterface2.Uuid, system.Uuid);

            // Disable second interface
            DatabaseAccess.MutateDatabase(db =>
            {
                var dbInterface = db.ItInterfaces.AsQueryable().ByUuid(itInterface2.Uuid);
                dbInterface.Disabled = true;
                db.SaveChanges();
            });

            //Act
            var result = await InterfaceV2Helper.GetInterfacesAsync(token, pageSize, pageNumber, system.Uuid, includeDeactivated: shouldIncludeDeactivated); // Limit by exposing system

            //Assert
            if (shouldIncludeDeactivated)
            {
                Assert.Equal(pageSize, result.Count());
                var interface1DTO = result.First(x => x.Name.Equals(itInterface1.Name));
                CheckBaseDTOValues(system, itInterface1, interface1DTO);
                Assert.False(interface1DTO.Deactivated);
                var interface2DTO = result.First(x => x.Name.Equals(itInterface2.Name));
                CheckBaseDTOValues(system, itInterface2, interface2DTO);
                Assert.True(interface2DTO.Deactivated);
            }
            else
            {
                var interfaceDTO = Assert.Single(result);
                Assert.False(interfaceDTO.Deactivated);
                CheckBaseDTOValues(system, itInterface1, interfaceDTO);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_Create_And_Delete_Minimum_ItInterface(bool withId)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var organization = await CreateOrganization();
            var input = new CreateItInterfaceRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                InterfaceId = withId ? CreateName() : null,
            };

            //Act
            var createdItInterface = await InterfaceV2Helper.CreateItInterfaceAsync(token.Token, input);
            using var deleteResponse = await InterfaceV2Helper.SendDeleteItInterfaceAsync(token.Token, createdItInterface.Uuid);
            using var getAfterDeleteRespose = await InterfaceV2Helper.SendGetInterfaceAsync(token.Token, createdItInterface.Uuid);

            //Assert
            CheckCreatedInterface(input, createdItInterface);
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteRespose.StatusCode);
        }

        [Fact]
        public async Task Can_Create_And_Delete_Minimum_Local_ItInterface_As_LocalAdmin()
        {
            //Arrange
            var (token, organization) = await CreateUserInNewOrg(role: OrganizationRole.LocalAdmin);
            var input = new CreateItInterfaceRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                Scope = RegistrationScopeChoice.Local
            };

            //Act
            var createdItInterface = await InterfaceV2Helper.CreateItInterfaceAsync(token, input);
            using var deleteResult = await InterfaceV2Helper.SendDeleteItInterfaceAsync(token, createdItInterface.Uuid);
            using var getAfterDeleteRespose = await InterfaceV2Helper.SendGetInterfaceAsync(token, createdItInterface.Uuid);

            //Assert
            CheckCreatedInterface(input, createdItInterface);
            Assert.Equal(HttpStatusCode.NoContent, deleteResult.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteRespose.StatusCode);
        }

        [Fact]
        public async Task Cannot_Create_Global_ItInterface_As_LocalAdmin()
        {
            //Arrange
            var (token, organization) = await CreateUserInNewOrg(role: OrganizationRole.LocalAdmin);
            var input = new CreateItInterfaceRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                Scope = RegistrationScopeChoice.Global
            };

            //Act
            using var createdItInterface = await InterfaceV2Helper.SendCreateItInterfaceAsync(token, input);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, createdItInterface.StatusCode);
        }

        [Fact]
        public async Task Can_Create_And_Delete_Full_ItInterface()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var organization = await CreateOrganization();
            var input = await CreateFullItInterfaceRequestAsync(token, organization);

            //Act
            var createdItInterface = await InterfaceV2Helper.CreateItInterfaceAsync(token.Token, input);
            using var deleteWithConflictResult = await InterfaceV2Helper.SendDeleteItInterfaceAsync(token.Token, createdItInterface.Uuid);
            using var removeExposingSystemResult = await InterfaceV2Helper.SendPatchInterfaceAsync(token.Token, createdItInterface.Uuid, new KeyValuePair<string, object>(nameof(UpdateItInterfaceRequestDTO.ExposedBySystemUuid), null));
            using var deleteAfterConflictResolutionResult = await InterfaceV2Helper.SendDeleteItInterfaceAsync(token.Token, createdItInterface.Uuid);
            using var getAfterDeleteRespose = await InterfaceV2Helper.SendGetInterfaceAsync(token.Token, createdItInterface.Uuid);

            //Assert
            CheckCreatedInterface(input, createdItInterface);
            Assert.Equal(HttpStatusCode.Conflict, deleteWithConflictResult.StatusCode);
            Assert.Equal(HttpStatusCode.OK, removeExposingSystemResult.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, deleteAfterConflictResolutionResult.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteRespose.StatusCode);
        }

        public static IEnumerable<object[]> Can_Patch_ItInterface_Inputs()
        {
            return BooleanInputMatrixFactory.Create(11);
        }

        [Theory, MemberData(nameof(Can_Patch_ItInterface_Inputs))]
        public async Task Can_Patch_ItInterface(
           bool withName,
           bool withInterfaceId,
           bool withExposedBySystem,
           bool withVersion,
           bool withDescription,
           bool withUrlReference,
           bool withDeactivated,
           bool withScope,
           bool withNote,
           bool withItInterfaceType,
           bool withData)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var organization = await CreateOrganization();
            var creationRequest = await CreateFullItInterfaceRequestAsync(token, organization);
            var createdInterface = await InterfaceV2Helper.CreateItInterfaceAsync(token.Token, creationRequest);
            var interfaceType = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItInterfaceTypes, organization.Uuid, 10, 0)).RandomItem();
            var interfaceDataType = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItInterfaceDataTypes, organization.Uuid, 10, 0)).RandomItem();

            var changes = new Dictionary<string, object>();
            if (withName) changes.Add(nameof(UpdateItInterfaceRequestDTO.Name), CreateName());
            if (withInterfaceId) changes.Add(nameof(UpdateItInterfaceRequestDTO.InterfaceId), A<string>());
            if (withExposedBySystem)
            {
                var newExposingSystem = await ItSystemV2Helper.CreateSystemAsync(token.Token, new CreateItSystemRequestDTO() { Name = CreateName(), OrganizationUuid = organization.Uuid });
                changes.Add(nameof(UpdateItInterfaceRequestDTO.ExposedBySystemUuid), newExposingSystem.Uuid);
            }
            if (withVersion) changes.Add(nameof(UpdateItInterfaceRequestDTO.Version), A<string>().Substring(0, 20));
            if (withDescription) changes.Add(nameof(UpdateItInterfaceRequestDTO.Description), A<string>());
            if (withUrlReference) changes.Add(nameof(UpdateItInterfaceRequestDTO.UrlReference), A<string>());
            if (withDeactivated) changes.Add(nameof(UpdateItInterfaceRequestDTO.Deactivated), !createdInterface.Deactivated);
            if (withScope) changes.Add(nameof(UpdateItInterfaceRequestDTO.Scope), A<RegistrationScopeChoice>());
            if (withNote) changes.Add(nameof(UpdateItInterfaceRequestDTO.Note), A<string>());
            if (withItInterfaceType) changes.Add(nameof(UpdateItInterfaceRequestDTO.ItInterfaceTypeUuid), interfaceType.Uuid);
            if (withData) changes.Add(nameof(UpdateItInterfaceRequestDTO.Data), new[] { new ItInterfaceDataRequestDTO() { DataTypeUuid = interfaceDataType.Uuid, Description = A<string>() } });

            //Act
            var updatedInterface = await InterfaceV2Helper.PatchInterfaceAsync(token.Token, createdInterface.Uuid, changes.ToArray());

            //Assert
            Assert.Equal(withName ? changes[nameof(UpdateItInterfaceRequestDTO.Name)] : createdInterface.Name, updatedInterface.Name);
            Assert.Equal(withInterfaceId ? changes[nameof(UpdateItInterfaceRequestDTO.InterfaceId)] : createdInterface.InterfaceId, updatedInterface.InterfaceId);
            Assert.Equal(withExposedBySystem ? changes[nameof(UpdateItInterfaceRequestDTO.ExposedBySystemUuid)] : createdInterface.ExposedBySystem?.Uuid, updatedInterface.ExposedBySystem?.Uuid);
            Assert.Equal(withVersion ? changes[nameof(UpdateItInterfaceRequestDTO.Version)] : createdInterface.Version, updatedInterface.Version);
            Assert.Equal(withDescription ? changes[nameof(UpdateItInterfaceRequestDTO.Description)] : createdInterface.Description, updatedInterface.Description);
            Assert.Equal(withUrlReference ? changes[nameof(UpdateItInterfaceRequestDTO.UrlReference)] : createdInterface.UrlReference, updatedInterface.UrlReference);
            Assert.Equal(withDeactivated ? changes[nameof(UpdateItInterfaceRequestDTO.Deactivated)] : createdInterface.Deactivated, updatedInterface.Deactivated);
            Assert.Equal(withScope ? changes[nameof(UpdateItInterfaceRequestDTO.Scope)] : createdInterface.Scope, updatedInterface.Scope);
            Assert.Equal(withNote ? changes[nameof(UpdateItInterfaceRequestDTO.Note)] : createdInterface.Notes, updatedInterface.Notes);
            Assert.Equal(withItInterfaceType ? changes[nameof(UpdateItInterfaceRequestDTO.ItInterfaceTypeUuid)] : createdInterface.ItInterfaceType?.Uuid, updatedInterface.ItInterfaceType?.Uuid);
            Assert.Equivalent(withData ? changes[nameof(UpdateItInterfaceRequestDTO.Data)] : createdInterface.Data?.Select(x => new ItInterfaceDataRequestDTO() { DataTypeUuid = x.DataType?.Uuid, Description = x.Description }), updatedInterface.Data?.Select(x => new ItInterfaceDataRequestDTO() { DataTypeUuid = x.DataType?.Uuid, Description = x.Description }));
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true, true, true)]
        [InlineData(OrganizationRole.LocalAdmin, true, true, true)]
        [InlineData(OrganizationRole.User, true, false, false)]
        public async Task Can_Get_ItInterface_Permissions(OrganizationRole role, bool read, bool modify, bool delete)
        {
            //Arrange
            var (token, org) = await CreateUserInNewOrg(false, role);
            var globalAdminToken = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var itInterface = await InterfaceV2Helper.CreateItInterfaceAsync(globalAdminToken.Token, new CreateItInterfaceRequestDTO
            {
                Name = A<string>(),
                OrganizationUuid = org.Uuid
            });

            //Act
            var permissionsResponseDto = await InterfaceV2Helper.GetPermissionsAsync(token, itInterface.Uuid);

            //Assert
            var expected = new ItInterfacePermissionsResponseDTO

            {
                Read = read,
                Modify = modify,
                Delete = delete,
                DeletionConflicts = new List<ItInterfaceDeletionConflict>()
            };
            Assert.Equivalent(expected, permissionsResponseDto);
        }

        [Fact]
        public async Task Can_Get_ItInterface_Permissions_With_ExposedBySystemConflict()
        {
            //Arrange
            var (token, org) = await CreateUserInNewOrg(false, OrganizationRole.GlobalAdmin);

            var system = await ItSystemV2Helper.CreateSystemAsync(token, new CreateItSystemRequestDTO
            {
                Name = A<string>(),
                OrganizationUuid = org.Uuid
            });
            var itInterface = await InterfaceV2Helper.CreateItInterfaceAsync(token, new CreateItInterfaceRequestDTO
            {
                Name = A<string>(),
                OrganizationUuid = org.Uuid,
                ExposedBySystemUuid = system.Uuid
            });

            //Act
            var permissionsResponseDto = await InterfaceV2Helper.GetPermissionsAsync(token, itInterface.Uuid);

            //Assert
            var expected = new ItInterfacePermissionsResponseDTO

            {
                Read = true,
                Modify = true,
                Delete = true,
                DeletionConflicts = new List<ItInterfaceDeletionConflict>()
                {
                    ItInterfaceDeletionConflict.ExposedByItSystem
                }
            };
            Assert.Equivalent(expected, permissionsResponseDto);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.User, false)]
        public async Task Can_Get_ItInterface_CollectionPermissions(OrganizationRole role, bool create)
        {
            //Arrange
            var (token, org) = await CreateUserInNewOrg(false, role);

            //Act
            var permissionsResponseDto = await InterfaceV2Helper.GetCollectionPermissionsAsync(token, org.Uuid);

            //Assert
            var expected = new ResourceCollectionPermissionsResponseDTO
            {
                Create = create
            };
            Assert.Equivalent(expected, permissionsResponseDto);
        }

        [Fact]
        public async Task Can_Create_Update_And_Delete_Data_ItInterface()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var organization = await CreateOrganization();
            var input = await CreateFullItInterfaceRequestAsync(token, organization);
            var createdItInterface = await InterfaceV2Helper.CreateItInterfaceAsync(token.Token, input);
            var interfaceDataTypes =
                (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItInterfaceDataTypes, organization.Uuid,
                    10, 0)).RandomItems(2).ToList();
            var initialType = interfaceDataTypes.First();
            var updatedType = interfaceDataTypes.Last();

            var creationRequest = new ItInterfaceDataRequestDTO
            {
                Description = A<string>(),
                DataTypeUuid = initialType.Uuid
            };
            var updateRequest = new ItInterfaceDataRequestDTO
            {
                Description = A<string>(),
                DataTypeUuid = initialType.Uuid
            };

            //Act
            var createdData = await InterfaceV2Helper.CreateItInterfaceDataDescriptionAsync(token.Token, createdItInterface.Uuid, creationRequest);
            var updatedData = await InterfaceV2Helper.UpdateItInterfaceDataDescriptionAsync(token.Token, createdItInterface.Uuid, createdData.Uuid, updateRequest);
            using var deletionResponse = await InterfaceV2Helper.SendDeleteItInterfaceDataDescriptionAsync(token.Token, createdItInterface.Uuid, createdData.Uuid);

            //Assert
            Assert.Equivalent(creationRequest, ToItInterfaceDataRequestDto(createdData));
            Assert.Equivalent(updateRequest, ToItInterfaceDataRequestDto(updatedData));
            Assert.Equal(HttpStatusCode.NoContent, deletionResponse.StatusCode);
            var afterDelete = await InterfaceV2Helper.GetInterfaceAsync(token.Token, createdItInterface.Uuid);
            Assert.DoesNotContain(afterDelete.Data, x => x.Uuid == createdData.Uuid);
        }

        private async Task<(string token, ShallowOrganizationResponseDTO createdOrganization)> CreateStakeHolderUserInNewOrg()
        {
            var org = await CreateOrganization();
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, org.Uuid, true, true);
            return (token, org);
        }

        protected async Task<(string token, ShallowOrganizationResponseDTO createdOrganization)> CreateUserInNewOrg(
            bool stakeHolderAccess = false,
            OrganizationRole role = OrganizationRole.User)
        {
            var org = await CreateOrganization();
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), role, org.Uuid, true, stakeHolderAccess);
            return (token, org);
        }

        private async Task<CreateItInterfaceRequestDTO> CreateFullItInterfaceRequestAsync(GetTokenResponseDTO token, ShallowOrganizationResponseDTO organization)
        {
            var exposingSystem = await ItSystemV2Helper.CreateSystemAsync(token.Token,
                new CreateItSystemRequestDTO() { OrganizationUuid = organization.Uuid, Name = CreateName() });
            var interfaceType =
                (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItInterfaceTypes, organization.Uuid, 10,
                    0)).RandomItem();
            var interfaceDataType =
                (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItInterfaceDataTypes, organization.Uuid,
                    10, 0)).RandomItem();
            var input = new CreateItInterfaceRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                InterfaceId = CreateName(),
                Deactivated = A<bool>(),
                Scope = A<RegistrationScopeChoice>(),
                Description = A<string>(),
                Note = A<string>(),
                Version = string.Join("", A<string>().RandomItems(20)),
                UrlReference = $"https://www.kitos-{A<Guid>():N}.dk",
                ExposedBySystemUuid = exposingSystem.Uuid,
                ItInterfaceTypeUuid = interfaceType.Uuid,
                Data = new[]
                {
                    new ItInterfaceDataRequestDTO
                    {
                        //with data type
                        DataTypeUuid = interfaceDataType.Uuid,
                        Description = A<string>()
                    },
                    new ItInterfaceDataRequestDTO
                    {
                        //without data type
                        Description = A<string>()
                    }
                }
            };
            return input;
        }
    }
}
