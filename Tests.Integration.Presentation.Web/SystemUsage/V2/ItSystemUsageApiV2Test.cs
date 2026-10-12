using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using ExpectedObjects;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage;
using Presentation.Web.Models.API.V2.Internal.Response.Roles;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.Interface;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.Contract;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Response.System;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Extensions;
using Xunit;
using OrganizationType = Presentation.Web.Models.API.V2.Types.Organization.OrganizationType;

namespace Tests.Integration.Presentation.Web.SystemUsage.V2
{
    public class ItSystemUsageApiV2Test : BaseItSystemUsageApiV2Test
    {
        [Fact]
        public async Task Can_Get_All_ItSystemUsages()
        {
            //Arrange
            var (token, user, organization1, system1) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync(type: A<OrganizationType>());
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization2.Uuid).DisposeAsync();

            var system2 = await CreateItSystemAsync(organization2.Uuid);
            var system3 = await CreateItSystemAsync(organization2.Uuid);

            var system1UsageOrg1 = await TakeSystemIntoUsageAsync(system1.Uuid, organization1.Uuid);
            var system2UsageOrg1 = await TakeSystemIntoUsageAsync(system2.Uuid, organization1.Uuid);
            var system2UsageOrg2 = await TakeSystemIntoUsageAsync(system2.Uuid, organization2.Uuid);
            var system3UsageOrg2 = await TakeSystemIntoUsageAsync(system3.Uuid, organization2.Uuid);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token)).ToList();

            //Assert
            Assert.Equal(4, dtos.Count);
            AssertExpectedUsageShallow(system1UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg2, dtos);
            AssertExpectedUsageShallow(system3UsageOrg2, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Internal()
        {
            //Arrange
            var (_, _, organization, system1) = await CreatePrerequisitesAsync();

            var system2 = await CreateItSystemAsync(organization.Uuid);
            var system3 = await CreateItSystemAsync(organization.Uuid);

            var system1Usage = await TakeSystemIntoUsageAsync(system1.Uuid, organization.Uuid);
            var system2Usage = await TakeSystemIntoUsageAsync(system2.Uuid, organization.Uuid);
            var system3Usage = await TakeSystemIntoUsageAsync(system3.Uuid, organization.Uuid);

            var toBeDisabled = new[] { system1Usage, system2Usage, system3Usage }.RandomItem();
            using var disabled = await ItSystemV2Helper.SendPatchSystemAsync(await GetGlobalToken(),
                toBeDisabled.SystemContext.Uuid, x => x.Deactivated, true);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyInternalAsync(organization.Uuid)).ToList();

            //Assert
            Assert.Equal(3, dtos.Count);
            AssertExpectedUsageShallow(system1Usage, dtos);
            AssertExpectedUsageShallow(system2Usage, dtos);
            AssertExpectedUsageShallow(system3Usage, dtos);
        }

        [Fact]
        public async Task Can_Create_Multiple_System_Relations_Internal()
        {
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var systemUsage1 = await TakeSystemIntoUsageAsync(system.Uuid, organization.Uuid);
            var interface1 = await InterfaceV2Helper.CreateItInterfaceAsync(token, new CreateItInterfaceRequestDTO()
            {
                ExposedBySystemUuid = system.Uuid,
                OrganizationUuid = organization.Uuid,
                Name = A<string>(),
                Description = A<string>(),
                UrlReference = A<string>()
            });
            var interface2 = await InterfaceV2Helper.CreateItInterfaceAsync(token, new CreateItInterfaceRequestDTO()
            {
                ExposedBySystemUuid = system.Uuid,
                OrganizationUuid = organization.Uuid,
                Name = A<string>(),
                Description = A<string>(),
                UrlReference = A<string>()
            });
            var systemUsage2 = await CreateSystemAndTakeItIntoUsage(organization.Uuid);
            var dtos = new List<SystemRelationWriteRequestDTO>()
            {
                new ()
                {
                    ToSystemUsageUuid = systemUsage1.Uuid,
                    RelationInterfaceUuid = interface1.Uuid,
                },
                new ()
                {
                    ToSystemUsageUuid = systemUsage1.Uuid,
                    RelationInterfaceUuid = interface2.Uuid,
                }
            };

            var response = await ItSystemUsageV2Helper.PostManyRelationsAsync(systemUsage2.Uuid, dtos);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = (await response.ReadResponseBodyAsAsync<IEnumerable<OutgoingSystemRelationResponseDTO>>()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.RelationInterface?.Uuid == interface1.Uuid);
            Assert.Contains(result, x => x.RelationInterface?.Uuid == interface2.Uuid);
        }

        [Fact]
        public async Task Cannot_Create_Multiple_System_Relations_If_Any_Creation_Fails_Internal()
        {
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var systemUsage1 = await TakeSystemIntoUsageAsync(system.Uuid, organization.Uuid);
            var interface1 = await InterfaceV2Helper.CreateItInterfaceAsync(token, new CreateItInterfaceRequestDTO()
            {
                ExposedBySystemUuid = system.Uuid,
                OrganizationUuid = organization.Uuid,
                Name = A<string>(),
                Description = A<string>(),
                UrlReference = A<string>()
            });
            var interface2 = await InterfaceV2Helper.CreateItInterfaceAsync(token, new CreateItInterfaceRequestDTO()
            {
                ExposedBySystemUuid = system.Uuid,
                OrganizationUuid = organization.Uuid,
                Name = A<string>(),
                Description = A<string>(),
                UrlReference = A<string>()
            });
            var systemUsage2 = await CreateSystemAndTakeItIntoUsage(organization.Uuid);
            var dtos = new List<SystemRelationWriteRequestDTO>()
            {
                new ()
                {
                    ToSystemUsageUuid = systemUsage1.Uuid,
                    RelationInterfaceUuid = interface1.Uuid,
                },
                new ()
                {
                    ToSystemUsageUuid = Guid.Empty,
                    RelationInterfaceUuid = interface2.Uuid,
                }
            };

            var response = await ItSystemUsageV2Helper.PostManyRelationsAsync(systemUsage2.Uuid, dtos);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Internal_With_Paging()
        {
            //Arrange
            var (_, _, organization1, system1) = await CreatePrerequisitesAsync();

            var system1Usage = await TakeSystemIntoUsageAsync(system1.Uuid, organization1.Uuid);
            var system2Usage = await CreateSystemAndTakeItIntoUsage(organization1.Uuid);
            var system3Usage = await CreateSystemAndTakeItIntoUsage(organization1.Uuid);
            var system4Usage = await CreateSystemAndTakeItIntoUsage(organization1.Uuid);

            //Act
            var page1Dtos = (await ItSystemUsageV2Helper.GetManyInternalAsync(organization1.Uuid, page: 0, pageSize: 2)).ToList();
            var page2Dtos = (await ItSystemUsageV2Helper.GetManyInternalAsync(organization1.Uuid, page: 1, pageSize: 2)).ToList();

            //Assert
            Assert.Equal(2, page1Dtos.Count);
            Assert.Equal(2, page2Dtos.Count);
            AssertExpectedUsageShallow(system1Usage, page1Dtos);
            AssertExpectedUsageShallow(system2Usage, page1Dtos);
            AssertExpectedUsageShallow(system3Usage, page2Dtos);
            AssertExpectedUsageShallow(system4Usage, page2Dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Internal_Filtered_By_LastModified()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var system3 = await CreateItSystemAsync(organization.Uuid);

            var system1Usage = await TakeSystemIntoUsageAsync(system1.Uuid, organization.Uuid);
            var system2Usage = await TakeSystemIntoUsageAsync(system2.Uuid, organization.Uuid);
            var system3Usage = await TakeSystemIntoUsageAsync(system3.Uuid, organization.Uuid);

            foreach (var systemUsageDto in new[] { system2Usage, system3Usage, system1Usage })
            {
                using var patchResponse = await ItSystemUsageV2Helper.SendPatchGeneral(token, systemUsageDto.Uuid, new GeneralDataUpdateRequestDTO() { Notes = A<string>() });
                Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
            }

            var referenceChange = await ItSystemUsageV2Helper.GetSingleAsync(token, system3Usage.Uuid);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyInternalAsync(organization.Uuid, changedSinceGtEq: referenceChange.LastModified, page: 0, pageSize: 10)).ToList();

            //Assert that the correct dtos are provided in the right order
            Assert.Equal(new[] { system3Usage.Uuid, system1Usage.Uuid }, dtos.Select(x => x.Uuid));
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Internal_Filtered_By_RelationToSystemUuId()
        {
            //Arrange - setup multiple relations across orgs
            var (_, _, organization, system1) = await CreatePrerequisitesAsync();

            var system2 = await CreateItSystemAsync(organization.Uuid);
            var relationTargetSystem = await CreateItSystemAsync(organization.Uuid);

            var system1UsageOrg1 = await TakeSystemIntoUsageAsync(system1.Uuid, organization.Uuid);
            var system2UsageOrg1 = await TakeSystemIntoUsageAsync(system2.Uuid, organization.Uuid);
            var system4UsageOrg1 = await TakeSystemIntoUsageAsync(relationTargetSystem.Uuid, organization.Uuid);

            await CreateRelationAsync(system1UsageOrg1, system4UsageOrg1);
            await CreateRelationAsync(system2UsageOrg1, system4UsageOrg1);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyInternalAsync(organization.Uuid, relationToSystemUuidFilter: relationTargetSystem.Uuid)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            AssertExpectedUsageShallow(system1UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Internal_Filtered_By_RelationToSystemUsageUuId()
        {
            //Arrange - setup multiple relations across orgs
            var (_, _, organization, _) = await CreatePrerequisitesAsync();

            var relationTargetSystem = await CreateItSystemAsync(organization.Uuid);

            var system1UsageOrg1 = await CreateSystemAndTakeItIntoUsage(organization.Uuid);
            var system2UsageOrg1 = await CreateSystemAndTakeItIntoUsage(organization.Uuid);
            await CreateSystemAndTakeItIntoUsage(organization.Uuid);
            var relationTargetUsageOrg1 = await TakeSystemIntoUsageAsync(relationTargetSystem.Uuid, organization.Uuid);

            await CreateRelationAsync(system1UsageOrg1, relationTargetUsageOrg1);
            await CreateRelationAsync(system2UsageOrg1, relationTargetUsageOrg1);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyInternalAsync(organization.Uuid, relationToSystemUsageUuidFilter: relationTargetUsageOrg1.Uuid)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            AssertExpectedUsageShallow(system1UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Internal_Filtered_By_RelationContractUuId()
        {
            //Arrange - setup multiple relations across orgs
            var (_, _, organization, _) = await CreatePrerequisitesAsync();

            var relationTargetSystem = await CreateItSystemAsync(organization.Uuid);

            var systemUsage1 = await CreateSystemAndTakeItIntoUsage(organization.Uuid);
            var systemUsage2 = await CreateSystemAndTakeItIntoUsage(organization.Uuid);
            var systemUsage3 = await CreateSystemAndTakeItIntoUsage(organization.Uuid);
            var relationTargetUsage = await TakeSystemIntoUsageAsync(relationTargetSystem.Uuid, organization.Uuid);

            var contract = await CreateItContractAsync(organization.Uuid);

            await CreateRelationAsync(systemUsage1, relationTargetUsage, contract);
            await CreateRelationAsync(relationTargetUsage, systemUsage2, contract);
            await CreateRelationAsync(systemUsage3, relationTargetUsage);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyInternalAsync(organization.Uuid, relationToContractUuidFilter: contract.Uuid)).ToList();

            //Assert
            // Get by contract returns only "From" system usages
            Assert.Equal(2, dtos.Count);
            AssertExpectedUsageShallow(systemUsage1, dtos);
            AssertExpectedUsageShallow(relationTargetUsage, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Internal_Filtered_By_SystemNameContent()
        {
            //Arrange
            var content = $"CONTENT_{A<Guid>()}";
            var (_, _, organization, _) = await CreatePrerequisitesAsync();

            var system1 = await CreateItSystemAsync(organization.Uuid, $"{content}ONE");
            var system2 = await CreateItSystemAsync(organization.Uuid, $"TWO{content}");
            var system3 = await CreateItSystemAsync(organization.Uuid);

            var system1UsageOrg1 = await TakeSystemIntoUsageAsync(system1.Uuid, organization.Uuid);
            var system2UsageOrg1 = await TakeSystemIntoUsageAsync(system2.Uuid, organization.Uuid);
            await TakeSystemIntoUsageAsync(system3.Uuid, organization.Uuid);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyInternalAsync(organization.Uuid, systemNameContentFilter: content)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            AssertExpectedUsageShallow(system1UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_With_Paging()
        {
            //Arrange
            var (token, user, organization1, system1) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization2.Uuid).DisposeAsync();

            var system2 = await CreateItSystemAsync(organization2.Uuid);
            var system3 = await CreateItSystemAsync(organization2.Uuid);

            var system1UsageOrg1 = await TakeSystemIntoUsageAsync(system1.Uuid, organization1.Uuid);
            var system2UsageOrg1 = await TakeSystemIntoUsageAsync(system2.Uuid, organization1.Uuid);
            var system2UsageOrg2 = await TakeSystemIntoUsageAsync(system2.Uuid, organization2.Uuid);
            var system3UsageOrg2 = await TakeSystemIntoUsageAsync(system3.Uuid, organization2.Uuid);

            //Act
            var page1Dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, page: 0, pageSize: 2)).ToList();
            var page2Dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, page: 1, pageSize: 2)).ToList();

            //Assert
            Assert.Equal(2, page1Dtos.Count);
            Assert.Equal(2, page2Dtos.Count);
            AssertExpectedUsageShallow(system1UsageOrg1, page1Dtos);
            AssertExpectedUsageShallow(system2UsageOrg1, page1Dtos);
            AssertExpectedUsageShallow(system2UsageOrg2, page2Dtos);
            AssertExpectedUsageShallow(system3UsageOrg2, page2Dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Filtered_By_LastModified()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var system3 = await CreateItSystemAsync(organization.Uuid);

            var system1Usage = await TakeSystemIntoUsageAsync(system1.Uuid, organization.Uuid);
            var system2Usage = await TakeSystemIntoUsageAsync(system2.Uuid, organization.Uuid);
            var system3Usage = await TakeSystemIntoUsageAsync(system3.Uuid, organization.Uuid);

            foreach (var systemUsageDto in new[] { system2Usage, system3Usage, system1Usage })
            {
                using var patchResponse = await ItSystemUsageV2Helper.SendPatchGeneral(token, systemUsageDto.Uuid, new GeneralDataUpdateRequestDTO() { Notes = A<string>() });
                Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
            }

            var referenceChange = await ItSystemUsageV2Helper.GetSingleAsync(token, system3Usage.Uuid);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, organization.Uuid, changedSinceGtEq: referenceChange.LastModified, page: 0, pageSize: 10)).ToList();

            //Assert that the correct dtos are provided in the right order
            Assert.Equal(new[] { system3Usage.Uuid, system1Usage.Uuid }, dtos.Select(x => x.Uuid));

        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Filtered_By_Organization()
        {
            //Arrange
            var (token, user, organization1, system1) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization2.Uuid).DisposeAsync();

            var system2 = await CreateItSystemAsync(organization2.Uuid);

            var system1UsageOrg1 = await TakeSystemIntoUsageAsync(system1.Uuid, organization1.Uuid);
            var system2UsageOrg1 = await TakeSystemIntoUsageAsync(system2.Uuid, organization1.Uuid);
            await TakeSystemIntoUsageAsync(system2.Uuid, organization2.Uuid);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, organization1.Uuid)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            AssertExpectedUsageShallow(system1UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Filtered_By_SystemUuId()
        {
            //Arrange
            var (token, user, organization1, system1) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization2.Uuid).DisposeAsync();

            var system2 = await CreateItSystemAsync(organization2.Uuid);

            await TakeSystemIntoUsageAsync(system1.Uuid, organization1.Uuid);
            var system2UsageOrg1 = await TakeSystemIntoUsageAsync(system2.Uuid, organization1.Uuid);
            var system2UsageOrg2 = await TakeSystemIntoUsageAsync(system2.Uuid, organization2.Uuid);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, systemUuidFilter: system2.Uuid)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg2, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Filtered_By_RelationToSystemUuId()
        {
            //Arrange - setup multiple relations across orgs
            var (token, user, organization1, system1) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync(type: A<OrganizationType>());
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization2.Uuid).DisposeAsync();

            var system2 = await CreateItSystemAsync(organization2.Uuid);
            var system3 = await CreateItSystemAsync(organization2.Uuid);
            var relationTargetSystem = await CreateItSystemAsync(organization2.Uuid);

            var system1UsageOrg1 = await TakeSystemIntoUsageAsync(system1.Uuid, organization1.Uuid);
            var system2UsageOrg1 = await TakeSystemIntoUsageAsync(system2.Uuid, organization1.Uuid);
            var system2UsageOrg2 = await TakeSystemIntoUsageAsync(system2.Uuid, organization2.Uuid);
            var system3UsageOrg2 = await TakeSystemIntoUsageAsync(system3.Uuid, organization2.Uuid);
            var system4UsageOrg1 = await TakeSystemIntoUsageAsync(relationTargetSystem.Uuid, organization1.Uuid);
            var system4UsageOrg2 = await TakeSystemIntoUsageAsync(relationTargetSystem.Uuid, organization2.Uuid);

            await CreateRelationAsync(system1UsageOrg1, system4UsageOrg1);
            await CreateRelationAsync(system2UsageOrg1, system4UsageOrg1);
            await CreateRelationAsync(system2UsageOrg2, system4UsageOrg2);
            await CreateRelationAsync(system4UsageOrg2, system3UsageOrg2);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, relationToSystemUuidFilter: relationTargetSystem.Uuid)).ToList();

            //Assert
            Assert.Equal(3, dtos.Count);
            AssertExpectedUsageShallow(system1UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg2, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Filtered_By_RelationToSystemUsageUuId()
        {
            //Arrange - setup multiple relations across orgs
            var (token, user, organization1, _) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync(type: A<OrganizationType>());
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization2.Uuid).DisposeAsync();

            var system1 = await CreateItSystemAsync(organization1.Uuid);
            var system2 = await CreateItSystemAsync(organization2.Uuid);
            var relationTargetSystem = await CreateItSystemAsync(organization2.Uuid);

            var system1UsageOrg1 = await TakeSystemIntoUsageAsync(system1.Uuid, organization1.Uuid);
            var system2UsageOrg1 = await TakeSystemIntoUsageAsync(system2.Uuid, organization1.Uuid);
            var system2UsageOrg2 = await TakeSystemIntoUsageAsync(system2.Uuid, organization2.Uuid);
            var relationTargetUsageOrg1 = await TakeSystemIntoUsageAsync(relationTargetSystem.Uuid, organization1.Uuid);
            var relationTargetUsageOrg2 = await TakeSystemIntoUsageAsync(relationTargetSystem.Uuid, organization2.Uuid);

            await CreateRelationAsync(system1UsageOrg1, relationTargetUsageOrg1);
            await CreateRelationAsync(system2UsageOrg1, relationTargetUsageOrg1);
            await CreateRelationAsync(system2UsageOrg2, relationTargetUsageOrg2);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, relationToSystemUsageUuidFilter: relationTargetUsageOrg1.Uuid)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            AssertExpectedUsageShallow(system1UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Filtered_By_RelationContractUuId()
        {
            //Arrange - setup multiple relations across orgs
            var (token, _, organization1, _) = await CreatePrerequisitesAsync();

            var relationTargetSystem = await CreateItSystemAsync(organization1.Uuid);

            var systemUsage1 = await CreateSystemAndTakeItIntoUsage(organization1.Uuid);
            var systemUsage2 = await CreateSystemAndTakeItIntoUsage(organization1.Uuid);
            var systemUsage3 = await CreateSystemAndTakeItIntoUsage(organization1.Uuid);
            var relationTargetUsage = await TakeSystemIntoUsageAsync(relationTargetSystem.Uuid, organization1.Uuid);

            var contract = await CreateItContractAsync(organization1.Uuid);

            await CreateRelationAsync(systemUsage1, relationTargetUsage, contract);
            await CreateRelationAsync(relationTargetUsage, systemUsage2, contract);
            await CreateRelationAsync(systemUsage3, relationTargetUsage);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, relationToContractUuidFilter: contract.Uuid)).ToList();

            //Assert
            // Get by contract returns only "From" system usages
            Assert.Equal(2, dtos.Count);
            AssertExpectedUsageShallow(systemUsage1, dtos);
            AssertExpectedUsageShallow(relationTargetUsage, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Filtered_By_SystemNameContent()
        {
            //Arrange
            var content = $"CONTENT_{A<Guid>()}";
            var (token, user, organization1, _) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync(type: A<OrganizationType>());
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization2.Uuid).DisposeAsync();

            var system1 = await CreateItSystemAsync(organization1.Uuid, $"{content}ONE");
            var system2 = await CreateItSystemAsync(organization2.Uuid, $"TWO{content}");
            var system3 = await CreateItSystemAsync(organization2.Uuid);

            var system1UsageOrg1 = await TakeSystemIntoUsageAsync(system1.Uuid, organization1.Uuid);
            var system2UsageOrg1 = await TakeSystemIntoUsageAsync(system2.Uuid, organization1.Uuid);
            await TakeSystemIntoUsageAsync(system3.Uuid, organization2.Uuid);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, systemNameContentFilter: content)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            AssertExpectedUsageShallow(system1UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
        }

        [Theory]
        [InlineData(OrganizationRole.LocalAdmin, true, true, true)]
        [InlineData(OrganizationRole.User, true, false, false)]
        public async Task Can_Get_Specific_ItSystemUsage_Permissions(OrganizationRole userRole, bool expectRead, bool expectModify, bool expectDelete)
        {
            //Arrange
            var (token, user, organization1, system) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync(type: A<OrganizationType>());

            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, userRole, organization2.Uuid).DisposeAsync();

            await TakeSystemIntoUsageAsync(system.Uuid, organization1.Uuid);
            var systemUsageOrg2 = await TakeSystemIntoUsageAsync(system.Uuid, organization2.Uuid);

            //Act
            var permissionsResponseDto = await ItSystemUsageV2Helper.GetPermissionsAsync(token, systemUsageOrg2.Uuid);

            //Assert - exhaustive content assertions are done in the read-after-write assertion tests (POST/PUT)
            var expected = new ResourcePermissionsResponseDTO()
            {
                Read = expectRead,
                Modify = expectModify,
                Delete = expectDelete
            };
            Assert.Equivalent(expected, permissionsResponseDto);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.User, false)]
        public async Task Can_Get_ItSystemUsage_CollectionPermissions(OrganizationRole userRole, bool expectCreate)
        {
            //Arrange
            var (token, user, _, _) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync(type: A<OrganizationType>());

            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, userRole, organization2.Uuid).DisposeAsync();

            //Act
            var permissionsResponseDto = await ItSystemUsageV2Helper.GetCollectionPermissionsAsync(token, organization2.Uuid);

            //Assert
            var expected = new ResourceCollectionPermissionsResponseDTO()
            {
                Create = expectCreate
            };
            Assert.Equivalent(expected, permissionsResponseDto);
        }

        [Fact]
        public async Task Can_Get_Specific_ItSystemUsage()
        {
            //Arrange
            var (token, user, organization1, system) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync();

            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization2.Uuid).DisposeAsync();

            await TakeSystemIntoUsageAsync(system.Uuid, organization1.Uuid);
            var systemUsageOrg2 = await TakeSystemIntoUsageAsync(system.Uuid, organization2.Uuid);

            //Act
            var dto = (await ItSystemUsageV2Helper.GetSingleAsync(token, systemUsageOrg2.Uuid));

            //Assert - exhaustive content assertions are done in the read-after-write assertion tests (POST/PUT)
            AssertExpectedUsageShallow(systemUsageOrg2, dto);
        }

        [Fact]
        public async Task Cannot_Get_Unknown_ItSystemUsage()
        {
            //Arrange
            var (token, _, __, ___) = await CreatePrerequisitesAsync();

            //Act
            using var response = (await ItSystemUsageV2Helper.SendGetSingleAsync(token, A<Guid>()));

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Get_ItSystemUsage_If_NotAllowedTo()
        {
            //Arrange
            var (token, _, organization1, system) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync();
            await TakeSystemIntoUsageAsync(system.Uuid, organization1.Uuid);
            var systemUsageOrg2 = await TakeSystemIntoUsageAsync(system.Uuid, organization2.Uuid);

            //Act
            using var response = (await ItSystemUsageV2Helper.SendGetSingleAsync(token, systemUsageOrg2.Uuid));

            //Assert - exhaustive content assertions are done in the read-after-write assertion tests (POST/PUT)
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Can_POST_With_No_Additional_Data()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(type: A<OrganizationType>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Uuid, AccessModifier.Public);

            //Act
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Assert
            Assert.NotNull(newUsage.OrganizationContext);
            Assert.Equal(organization.Uuid, newUsage.OrganizationContext.Uuid);
            Assert.Equal(organization.Name, newUsage.OrganizationContext.Name);
            Assert.Equal(organization.Cvr, newUsage.OrganizationContext.Cvr);
            Assert.Equal(system.Uuid, newUsage.SystemContext.Uuid);
            Assert.Equal(system.Name, newUsage.SystemContext.Name);
        }

        [Fact]
        public async Task Cannot_POST_Duplicate_With_No_Additional_Data()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();

            //Act
            await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            using var newUsage = await ItSystemUsageV2Helper.SendPostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, newUsage.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_If_ItSystem_Is_Disabled()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();

            await ItSystemV2Helper.SendPatchSystemAsync(await GetGlobalToken(), system.Uuid, x => x.Deactivated, true).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Act
            using var response = await ItSystemUsageV2Helper.SendPostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_POST_With_Full_General_Data_Section()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var dataClassification = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageDataClassification, organization.Uuid, 1, 0)).First();
            var request = CreatePostRequest(organization.Uuid, system.Uuid, new GeneralDataWriteRequestDTO
            {
                LocalCallName = A<string>(),
                LocalSystemId = A<string>(),
                SystemVersion = A<string>(),
                Notes = A<string>(),
                DataClassificationUuid = dataClassification.Uuid,
                NumberOfExpectedUsers = new ExpectedUsersIntervalDTO { LowerBound = 10, UpperBound = 49 },
                Validity = new ItSystemUsageValidityWriteRequestDTO
                {
                    LifeCycleStatus = A<LifeCycleStatusChoice?>(),
                    ValidFrom = DateTime.UtcNow.Date,
                    ValidTo = DateTime.UtcNow.Date.AddDays(Math.Abs(A<short>()))
                },
                WebAccessibilityCompliance = A<YesNoPartiallyChoice>(),
                LastWebAccessibilityCheck = A<DateTime>(),
                WebAccessibilityNotes = A<string>(),
                IsSociallyCritical = A<YesNoDontKnowChoice>(),
            });

            //Act
            var createdDto = await ItSystemUsageV2Helper.PostAsync(token, request);

            //Assert a fresh GET DTO
            var freshReadDto = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDto.Uuid);
            Assert.NotNull(request.General);
            Assert.NotNull(request.General.NumberOfExpectedUsers);
            Assert.NotNull(request.General.Validity);
            Assert.NotNull(freshReadDto.General.NumberOfExpectedUsers);
            Assert.NotNull(freshReadDto.General.Validity);
            Assert.NotNull(freshReadDto.General.DataClassification);
            Assert.Equal(request.General.LocalCallName, freshReadDto.General.LocalCallName);
            Assert.Equal(request.General.LocalSystemId, freshReadDto.General.LocalSystemId);
            Assert.Equal(request.General.SystemVersion, freshReadDto.General.SystemVersion);
            Assert.Equal(request.General.Notes, freshReadDto.General.Notes);
            Assert.Equal(request.General.NumberOfExpectedUsers.LowerBound, freshReadDto.General.NumberOfExpectedUsers.LowerBound);
            Assert.Equal(request.General.NumberOfExpectedUsers.UpperBound, freshReadDto.General.NumberOfExpectedUsers.UpperBound);
            Assert.Equal(request.General.Validity.LifeCycleStatus, freshReadDto.General.Validity.LifeCycleStatus);
            Assert.Equal(request.General.Validity.ValidFrom.GetValueOrDefault().Date, freshReadDto.General.Validity.ValidFrom.GetValueOrDefault().Date);
            Assert.Equal(request.General.Validity.ValidTo.GetValueOrDefault().Date, freshReadDto.General.Validity.ValidTo.GetValueOrDefault().Date);
            Assert.Equal(dataClassification.Uuid, freshReadDto.General.DataClassification.Uuid);
            Assert.Equal(dataClassification.Name, freshReadDto.General.DataClassification.Name);
            Assert.Equal(request.General.WebAccessibilityCompliance, freshReadDto.General.WebAccessibilityCompliance);
            AssertTimestampEqual(request.General.LastWebAccessibilityCheck, freshReadDto.General.LastWebAccessibilityCheck);
            Assert.Equal(request.General.WebAccessibilityNotes, freshReadDto.General.WebAccessibilityNotes);
            Assert.Equal(request.General.IsSociallyCritical, freshReadDto.General.IsSociallyCritical);
        }

        [Fact]
        public async Task Can_PATCH_MainContract()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var contract1 = await CreateItContractAsync(organization.Uuid);
            var contract2 = await CreateItContractAsync(organization.Uuid);

            await ItContractV2Helper.SendPatchSystemUsagesAsync(await GetGlobalToken(), contract1.Uuid,
                newUsage.Uuid.WrapAsEnumerable());
            await ItContractV2Helper.SendPatchSystemUsagesAsync(await GetGlobalToken(), contract2.Uuid,
                newUsage.Uuid.WrapAsEnumerable());

            //Act
            using var response1 = await ItSystemUsageV2Helper.SendPatchGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO { MainContractUuid = contract1.Uuid }).WithExpectedResponseCode(HttpStatusCode.OK);

            await ItContractV2Helper.SendPatchContractGeneralDataAsync(await GetGlobalToken(), contract1.Uuid,
                new ContractGeneralDataWriteRequestDTO
                {
                    Validity = new ContractValidityWriteRequestDTO
                    {
                        EnforcedValid = true
                    }
                });

            //Assert
            var freshReadDto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            Assert.NotNull(freshReadDto.General.MainContract);
            Assert.NotNull(freshReadDto.General.Validity);
            Assert.Equal(contract1.Uuid, freshReadDto.General.MainContract.Uuid);
            Assert.Equal(contract1.Name, freshReadDto.General.MainContract.Name);
            Assert.True(freshReadDto.General.Validity.ValidAccordingToMainContract);
            Assert.Equal(MainContractStateChoice.Active, freshReadDto.General.Validity.MainContractState);

            //Act - set to another contract
            using var response2 = await ItSystemUsageV2Helper.SendPatchGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO { MainContractUuid = contract2.Uuid }).WithExpectedResponseCode(HttpStatusCode.OK);

            await ItContractV2Helper.SendPatchTerminationAsync(await GetGlobalToken(), contract2.Uuid,
                new ContractTerminationDataWriteRequestDTO
                {
                    TerminatedAt = DateTime.UtcNow.AddDays(-1)
                });

            //Assert
            freshReadDto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            Assert.NotNull(freshReadDto.General.MainContract);
            Assert.NotNull(freshReadDto.General.Validity);
            Assert.Equal(contract2.Uuid, freshReadDto.General.MainContract.Uuid);
            Assert.Equal(contract2.Name, freshReadDto.General.MainContract.Name);
            Assert.False(freshReadDto.General.Validity.ValidAccordingToMainContract);
            Assert.Equal(MainContractStateChoice.Inactive, freshReadDto.General.Validity.MainContractState);

            //Act - set to contract to null
            using var response3 = await ItSystemUsageV2Helper.SendPatchGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO { MainContractUuid = null }).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            freshReadDto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            Assert.NotNull(freshReadDto.General.Validity);
            Assert.True(freshReadDto.General.Validity.ValidAccordingToMainContract);
            Assert.Equal(MainContractStateChoice.NoContract, freshReadDto.General.Validity.MainContractState);
        }

        [Fact]
        public async Task Can_PATCH_Reset_MainContract()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var contract = await CreateItContractAsync(organization.Uuid);
            await ItContractV2Helper.SendPatchSystemUsagesAsync(await GetGlobalToken(), contract.Uuid, newUsage.Uuid.WrapAsEnumerable());

            //Act
            using var response1 = await ItSystemUsageV2Helper.SendPatchGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO { MainContractUuid = contract.Uuid }).WithExpectedResponseCode(HttpStatusCode.OK);
            using var resetResponse = await ItSystemUsageV2Helper.SendPatchGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO()).WithExpectedResponseCode(HttpStatusCode.OK); //Reset main contract

            //Assert
            var freshReadDto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            Assert.Null(freshReadDto.General.MainContract);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_POST_With_OrganizationalUsage(bool withResponsible)
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var unit1 = await CreateOrganizationUnitAsync(organization.Uuid);
            var unit2 = await CreateOrganizationUnitAsync(organization.Uuid, parentUnitUuid: unit1.Uuid);
            var unit3 = await CreateOrganizationUnitAsync(organization.Uuid, parentUnitUuid: unit1.Uuid);

            var units = new[] { unit1, unit2, unit3 }.OrderBy(_ => A<int>()).Take(2).ToList();
            var responsible = units.First();

            //Act
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid, organizationalUsageSection: new OrganizationUsageWriteRequestDTO()
            {
                UsingOrganizationUnitUuids = units.Select(x => x.Uuid).ToList(),
                ResponsibleOrganizationUnitUuid = withResponsible ? responsible.Uuid : null
            }));

            //Assert
            await AssertOrganizationalUsage(token, newUsage.Uuid, units, withResponsible ? responsible : null);
        }

        [Fact]
        public async Task Can_PATCH_Modify_OrganizationalUsage()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var unit1 = await CreateOrganizationUnitAsync(organization.Uuid);
            var unit2 = await CreateOrganizationUnitAsync(organization.Uuid, parentUnitUuid: unit1.Uuid);
            var unit3 = await CreateOrganizationUnitAsync(organization.Uuid, parentUnitUuid: unit1.Uuid);

            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Act
            using var modificationResponse1 = await ItSystemUsageV2Helper.SendPatchOrganizationalUsage(token,
                newUsage.Uuid, new OrganizationUsageWriteRequestDTO()
                {
                    UsingOrganizationUnitUuids = new[] { unit1.Uuid, unit2.Uuid },
                    ResponsibleOrganizationUnitUuid = unit2.Uuid
                }).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            await AssertOrganizationalUsage(token, newUsage.Uuid, new[] { unit1, unit2 }, unit2);

            //Act - swap one unit as well as responsible
            using var modificationResponse2 = await ItSystemUsageV2Helper.SendPatchOrganizationalUsage(token,
                newUsage.Uuid, new OrganizationUsageWriteRequestDTO
                {
                    UsingOrganizationUnitUuids = new[] { unit1.Uuid, unit3.Uuid },
                    ResponsibleOrganizationUnitUuid = unit3.Uuid
                }).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            await AssertOrganizationalUsage(token, newUsage.Uuid, new[] { unit1, unit3 }, unit3);

            //Act - reset all
            using var modificationResponse3 = await ItSystemUsageV2Helper.SendPatchOrganizationalUsage(token,
                newUsage.Uuid, new OrganizationUsageWriteRequestDTO()).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            await AssertOrganizationalUsage(token, newUsage.Uuid, Enumerable.Empty<OrganizationUnitResponseDTO>(), null);

            //Act - set using orgs but no responsible
            using var modificationResponse4 = await ItSystemUsageV2Helper.SendPatchOrganizationalUsage(token,
                newUsage.Uuid, new OrganizationUsageWriteRequestDTO
                {
                    UsingOrganizationUnitUuids = new[] { unit1.Uuid, unit2.Uuid }
                }).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            await AssertOrganizationalUsage(token, newUsage.Uuid, new[] { unit1, unit2 }, null);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        public async Task Can_POST_With_KLE_Deviations(bool withAdditions, bool withRemovals)
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();

            var additionalTaskRefs = Many<Guid>(2).ToList();
            var taskRefsOnSystem = Many<Guid>(3).ToList();
            var potentialRemovals = taskRefsOnSystem.Take(2).ToList();

            AddTaskRefsInDatabase(additionalTaskRefs.Concat(taskRefsOnSystem));


            using var addTaskRefResponse = await ItSystemV2Helper.SendPatchSystemAsync(await GetGlobalToken(),
                    system.Uuid, x => x.KLEUuids, taskRefsOnSystem);

            var request = CreatePostRequest(organization.Uuid, system.Uuid, kleDeviationsRequest: new LocalKLEDeviationsRequestDTO
            {
                AddedKLEUuids = withAdditions ? additionalTaskRefs : null,
                RemovedKLEUuids = withRemovals ? potentialRemovals : null
            });

            //Act
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, request);

            //Assert
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertKleDeviation(withAdditions, additionalTaskRefs, dto.LocalKLEDeviations.AddedKLE);
            AssertKleDeviation(withRemovals, potentialRemovals, dto.LocalKLEDeviations.RemovedKLE);
        }

        [Fact]
        public async Task Can_PATCH_KLE()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();

            var additionalTaskRefs = Many<Guid>(2).ToList();
            var taskRefsOnSystem = Many<Guid>(3).ToList();
            var potentialRemovals = taskRefsOnSystem.Take(2).ToList();

            AddTaskRefsInDatabase(additionalTaskRefs.Concat(taskRefsOnSystem));

            await ItSystemV2Helper.SendPatchSystemAsync(await GetGlobalToken(), system.Uuid, x => x.KLEUuids,
                taskRefsOnSystem).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Act - add one addition
            using var put1 = await ItSystemUsageV2Helper.SendPatchKle(token, newUsage.Uuid, new LocalKLEDeviationsRequestDTO() { AddedKLEUuids = additionalTaskRefs.Take(1) }).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertKleDeviation(true, additionalTaskRefs.Take(1), dto.LocalKLEDeviations.AddedKLE);
            AssertKleDeviation(false, null, dto.LocalKLEDeviations.RemovedKLE);

            //Act - add another one
            using var put2 = await ItSystemUsageV2Helper.SendPatchKle(token, newUsage.Uuid, new LocalKLEDeviationsRequestDTO() { AddedKLEUuids = additionalTaskRefs }).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertKleDeviation(true, additionalTaskRefs, dto.LocalKLEDeviations.AddedKLE);
            AssertKleDeviation(false, null, dto.LocalKLEDeviations.RemovedKLE);

            //Act - remove some
            using var put3 = await ItSystemUsageV2Helper.SendPatchKle(token, newUsage.Uuid, new LocalKLEDeviationsRequestDTO() { AddedKLEUuids = additionalTaskRefs, RemovedKLEUuids = potentialRemovals }).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertKleDeviation(true, additionalTaskRefs, dto.LocalKLEDeviations.AddedKLE);
            AssertKleDeviation(true, potentialRemovals, dto.LocalKLEDeviations.RemovedKLE);

            //Act - reset
            using var put4 = await ItSystemUsageV2Helper.SendPatchKle(token, newUsage.Uuid, new LocalKLEDeviationsRequestDTO()).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertKleDeviation(false, null, dto.LocalKLEDeviations.AddedKLE);
            AssertKleDeviation(false, null, dto.LocalKLEDeviations.RemovedKLE);
        }

        [Fact]
        public async Task Can_POST_With_ExternalReferences()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            var inputs = Many<ExternalReferenceDataWriteRequestDTO>().Transform(WithRandomMaster).ToList();

            var request = CreatePostRequest(organization.Uuid, system.Uuid, referenceDataDtos: inputs);

            //Act
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, request);

            //Assert
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            Assert.Equal(inputs.Count, dto.ExternalReferences.Count());
            AssertExternalReferenceResults(inputs, dto);
        }

        [Fact]
        public async Task Can_PATCH_ExternalReferences()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            var request = CreatePostRequest(organization.Uuid, system.Uuid);
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, request);

            var inputs1 = CreateUpdateExternalReferenceDataWriteRequestDtOs().ToList();

            //Act
            using var response1 = await ItSystemUsageV2Helper.SendPatchExternalReferences(token, newUsage.Uuid, inputs1).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertExternalReferenceResults(inputs1, dto, true);

            //Act - reset
            var inputs2 = Enumerable.Empty<UpdateExternalReferenceDataWriteRequestDTO>().ToList();
            using var response2 = await ItSystemUsageV2Helper.SendPatchExternalReferences(token, newUsage.Uuid, inputs2).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertExternalReferenceResults(inputs2, dto);
        }

        [Fact]
        public async Task Can_POST_With_Roles()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(type: A<OrganizationType>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Uuid, AccessModifier.Public);

            var user1 = await CreateUser(organization);
            var user2 = await CreateUser(organization);
            var role = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().First(r => r.IsObligatory && r.IsEnabled));
            var roles = new List<RoleAssignmentRequestDTO>
            {
                new()
                {
                    RoleUuid = role.Uuid,
                    UserUuid = user1.Uuid
                },
                new()
                {
                    RoleUuid = role.Uuid,
                    UserUuid = user2.Uuid
                }
            };

            //Act
            var createdDto = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid, roles: roles));

            //Assert
            var freshReadDto = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDto.Uuid);
            Assert.Equal(2, freshReadDto.Roles.Count());
            AssertSingleRight(role, user1, freshReadDto.Roles.Where(x => x.User.Uuid == user1.Uuid));
            AssertSingleRight(role, user2, freshReadDto.Roles.Where(x => x.User.Uuid == user2.Uuid));
        }

        [Fact]
        public async Task Can_PATCH_Modify_Roles()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(type: A<OrganizationType>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Uuid, AccessModifier.Public);

            var user1 = await CreateUser(organization);
            var user2 = await CreateUser(organization);
            var role = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().First(r => r.IsObligatory && r.IsEnabled));

            var createdDto = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            var initialRoles = new List<RoleAssignmentRequestDTO> { new() { RoleUuid = role.Uuid, UserUuid = user1.Uuid } };
            var modifyRoles = new List<RoleAssignmentRequestDTO> { new() { RoleUuid = role.Uuid, UserUuid = user2.Uuid } };

            //Act - Add role
            using var addInitialRolesRequest = await ItSystemUsageV2Helper.SendPatchRoles(token, createdDto.Uuid, initialRoles).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            var initialRoleResponse = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDto.Uuid);
            AssertSingleRight(role, user1, initialRoleResponse.Roles);

            //Act - Modify role
            using var modifiedRequest = await ItSystemUsageV2Helper.SendPatchRoles(token, createdDto.Uuid, modifyRoles).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            var modifiedRoleResponse = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDto.Uuid);
            AssertSingleRight(role, user2, modifiedRoleResponse.Roles);

            //Act - Remove role
            using var removedRequest = await ItSystemUsageV2Helper.SendPatchRoles(token, createdDto.Uuid, new List<RoleAssignmentRequestDTO>()).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            var removedRoleResponse = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDto.Uuid);
            Assert.Empty(removedRoleResponse.Roles);
        }

        [Fact]
        public async Task Can_PATCH_Add_RoleAssignment()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(type: A<OrganizationType>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Uuid, AccessModifier.Public);

            var user1 = await CreateUser(organization);
            var user2 = await CreateUser(organization);
            var role1 = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().Where(r => r.IsObligatory && r.IsEnabled).RandomItem());
            var role2 = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().Where(r => r.IsObligatory && r.IsEnabled).RandomItem());

            var createdDto = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            var assignment1 = new RoleAssignmentRequestDTO { RoleUuid = role1.Uuid, UserUuid = user1.Uuid };
            var assignment2 = new RoleAssignmentRequestDTO { RoleUuid = role2.Uuid, UserUuid = user2.Uuid };

            //Act
            using var assignmentResponse1 = await ItSystemUsageV2Helper.SendPatchAddRoleAssignment(token, createdDto.Uuid, assignment1);
            using var duplicateAssignment1 = await ItSystemUsageV2Helper.SendPatchAddRoleAssignment(token, createdDto.Uuid, assignment1);
            using var assignmentResponse2 = await ItSystemUsageV2Helper.SendPatchAddRoleAssignment(token, createdDto.Uuid, assignment2);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, duplicateAssignment1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, assignmentResponse1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, assignmentResponse2.StatusCode);
            var updatedDto = await assignmentResponse2.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();
            var roles = updatedDto.Roles.ToList();
            Assert.Equal(2, roles.Count);
            Assert.Contains(roles, r => MatchExpectedAssignment(r, assignment1));
            Assert.Contains(roles, r => MatchExpectedAssignment(r, assignment2));
        }

        [Fact]
        public async Task Can_PATCH_Remove_RoleAssignment()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(type: A<OrganizationType>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Uuid, AccessModifier.Public);

            var user1 = await CreateUser(organization);
            var user2 = await CreateUser(organization);
            var role1 = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().Where(r => r.IsObligatory && r.IsEnabled).RandomItem());
            var role2 = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().Where(r => r.IsObligatory && r.IsEnabled).RandomItem());

            var createdDto = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            var assignment2 = new RoleAssignmentRequestDTO { RoleUuid = role2.Uuid, UserUuid = user2.Uuid };
            var assignment1 = new RoleAssignmentRequestDTO { RoleUuid = role1.Uuid, UserUuid = user1.Uuid };

            //Act
            using var assignment1Response = await ItSystemUsageV2Helper.SendPatchAddRoleAssignment(token, createdDto.Uuid, assignment1);
            using var assignment2Response = await ItSystemUsageV2Helper.SendPatchAddRoleAssignment(token, createdDto.Uuid, assignment2);
            using var removeAssignment = await ItSystemUsageV2Helper.SendPatchRemoveRoleAssignment(token, createdDto.Uuid, assignment1);
            using var duplicateRemoveAssignment = await ItSystemUsageV2Helper.SendPatchRemoveRoleAssignment(token, createdDto.Uuid, assignment1);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, duplicateRemoveAssignment.StatusCode);
            Assert.Equal(HttpStatusCode.OK, removeAssignment.StatusCode);
            var updatedDto = await removeAssignment.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();
            var roleAssignment = Assert.Single(updatedDto.Roles);
            MatchExpectedAssignment(roleAssignment, assignment2);
        }

        [Fact]
        public async Task Can_POST_With_GDPR()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var gdprInput = await CreateGDPRInputAsync(organization);

            //Act
            var createdDto = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid, gdpr: gdprInput));

            //Assert
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDto.Uuid);
            var gdprResponse = dto.GDPR;
            AssertGdpr(gdprInput, gdprResponse);
        }

        [Fact]
        public async Task Can_PATCH_GDPR()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();

            var gdprVersion1 = await CreateGDPRInputAsync(organization);
            var gdprVersion2 = await CreateGDPRInputAsync(organization);
            var gdprVersion3 = new GDPRWriteRequestDTO();

            Assert.NotNull(gdprVersion2.SensitivePersonDataUuids);
            Assert.NotNull(gdprVersion2.RegisteredDataCategoryUuids);
            gdprVersion2.SensitivePersonDataUuids = gdprVersion2.SensitivePersonDataUuids.Take(1).ToList();
            gdprVersion2.RegisteredDataCategoryUuids = gdprVersion2.RegisteredDataCategoryUuids.Take(1).ToList();

            var usageDto = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Act
            await ItSystemUsageV2Helper.SendPatchGDPR(token, usageDto.Uuid, gdprVersion1)
                .WithExpectedResponseCode(HttpStatusCode.OK)
                .DisposeAsync();

            //Assert version 1
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, usageDto.Uuid);
            var gdprResponse = dto.GDPR;
            AssertGdpr(gdprVersion1, gdprResponse);

            //Act
            await ItSystemUsageV2Helper.SendPatchGDPR(token, usageDto.Uuid, gdprVersion2)
                .WithExpectedResponseCode(HttpStatusCode.OK)
                .DisposeAsync();

            //Assert version 2
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, usageDto.Uuid);
            gdprResponse = dto.GDPR;
            AssertGdpr(gdprVersion2, gdprResponse);

            //Act - reset
            await ItSystemUsageV2Helper.SendPatchGDPR(token, usageDto.Uuid, gdprVersion3)
                .WithExpectedResponseCode(HttpStatusCode.OK)
                .DisposeAsync();

            //Assert version 3 - properties should have been reset
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, usageDto.Uuid);
            gdprResponse = dto.GDPR;
            AssertGdpr(gdprVersion3, gdprResponse);
        }

        [Fact]
        public async Task Can_PATCH_Validity()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();

            var date1 = DateTime.UtcNow;
            var lifeCycle1 = LifeCycleStatusChoice.Operational;
            var date2 = DateTime.UtcNow.AddDays(-1 - A<int>());
            var lifeCycle2 = LifeCycleStatusChoice.NotInUse;

            var validityVersion1 = CreateValidityInput(date1, lifeCycle1);
            var validityVersion2 = CreateValidityInput(date2, lifeCycle2);
            var validityVersion3 = new ItSystemUsageValidityWriteRequestDTO();

            var usageDto = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Act
            await ItSystemUsageV2Helper.SendPatchValidity(token, usageDto.Uuid, validityVersion1)
                .WithExpectedResponseCode(HttpStatusCode.OK)
                .DisposeAsync();

            //Assert version 1
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, usageDto.Uuid);
            var validityResponse = dto.General.Validity;
            AssertValidity(validityVersion1, validityResponse, expectedDateValidity: true, expectedLifeCycleValidity: true);

            //Act
            await ItSystemUsageV2Helper.SendPatchValidity(token, usageDto.Uuid, validityVersion2)
                .WithExpectedResponseCode(HttpStatusCode.OK)
                .DisposeAsync();

            //Assert version 2
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, usageDto.Uuid);
            validityResponse = dto.General.Validity;
            AssertValidity(validityVersion2, validityResponse, expectedDateValidity: false, expectedLifeCycleValidity: false);

            //Act - reset
            await ItSystemUsageV2Helper.SendPatchValidity(token, usageDto.Uuid, validityVersion3)
                .WithExpectedResponseCode(HttpStatusCode.OK)
                .DisposeAsync();

            //Assert version 3 - properties should have been reset
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, usageDto.Uuid);
            validityResponse = dto.General.Validity;
            AssertValidity(validityVersion3, validityResponse, expectedDateValidity: true, expectedLifeCycleValidity: true);
        }

        [Fact]
        public async Task Can_POST_With_Archiving()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(type: A<OrganizationType>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Uuid, AccessModifier.Public);
            var archiveType = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTypes, organization.Uuid, 1, 0)).First();
            var archiveLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveLocations, organization.Uuid, 1, 0)).First();
            var archiveTestLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTestLocations, organization.Uuid, 1, 0)).First();

            var inputs = await CreateArchivingCreationRequestDto(archiveType.Uuid, archiveLocation.Uuid, archiveTestLocation.Uuid, organization.Uuid);

            var request = CreatePostRequest(organization.Uuid, system.Uuid, archiving: inputs);

            //Act
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, request);

            //Assert
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertArchivingParametersSet<ArchivingCreationRequestDTO, JournalPeriodDTO>(inputs, dto.Archiving);
        }

        [Fact]
        public async Task Can_PATCH_With_Archiving()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(type: A<OrganizationType>());
            var organization2 = await CreateOrganizationAsync(type: A<OrganizationType>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Uuid, AccessModifier.Public);

            var createRequest = CreatePostRequest(organization.Uuid, system.Uuid);
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, createRequest);

            var archiveType = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTypes, organization.Uuid, 1, 0)).First();
            var archiveLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveLocations, organization.Uuid, 1, 0)).First();
            var archiveTestLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTestLocations, organization.Uuid, 1, 0)).First();

            var inputs = await CreateArchivingUpdateRequestDto(archiveType.Uuid, archiveLocation.Uuid, archiveTestLocation.Uuid, organization.Uuid);

            //Act - Add archiving data
            using var addedArchivingDataUsage = await ItSystemUsageV2Helper.SendPatchArchiving(token, newUsage.Uuid, inputs);

            //Assert 
            Assert.Equal(HttpStatusCode.OK, addedArchivingDataUsage.StatusCode);
            var addedDto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertArchivingParametersSet<ArchivingUpdateRequestDTO, JournalPeriodUpdateRequestDTO>(inputs, addedDto.Archiving);

            //Act - Update archiving data
            var updatedArchiveType = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTypes, organization.Uuid, 1, 1)).First();
            var updatedArchiveLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveLocations, organization.Uuid, 1, 1)).First();
            var updatedArchiveTestLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTestLocations, organization.Uuid, 1, 1)).First();
            var updatedInputs = await CreateArchivingUpdateRequestDto(updatedArchiveType.Uuid, updatedArchiveLocation.Uuid, updatedArchiveTestLocation.Uuid, organization2.Uuid);

            using var updatedArchivingDataUsage = await ItSystemUsageV2Helper.SendPatchArchiving(token, newUsage.Uuid, updatedInputs);

            //Assert
            Assert.Equal(HttpStatusCode.OK, updatedArchivingDataUsage.StatusCode);
            var updatedDto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertArchivingParametersSet<ArchivingUpdateRequestDTO, JournalPeriodUpdateRequestDTO>(updatedInputs, updatedDto.Archiving);

            //Act - Remove archiving data
            using var removedArchivingDataUsage = await ItSystemUsageV2Helper.SendPatchArchiving(token, newUsage.Uuid, new ArchivingUpdateRequestDTO() { JournalPeriods = new List<JournalPeriodUpdateRequestDTO>() });

            //Assert 
            Assert.Equal(HttpStatusCode.OK, removedArchivingDataUsage.StatusCode);
            var removedDto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertArchivingParametersNotSet(removedDto.Archiving);
        }

        [Fact]
        public async Task Can_PATCH_With_Archiving_With_Specific_Journal_Periods()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var usageDto = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var initialJournalPeriodInputs = await CreateArchivingUpdateRequestDto(organization.Uuid);
            using var firstArchiveResponse = await ItSystemUsageV2Helper.SendPatchArchiving(token, usageDto.Uuid, initialJournalPeriodInputs);
            Assert.Equal(HttpStatusCode.OK, firstArchiveResponse.StatusCode);
            usageDto = await firstArchiveResponse.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();
            var changedInputs = await CreateArchivingUpdateRequestDto(organization.Uuid);
            //Set one of the journal periods to be an update of an existing
            var periodToChange = usageDto.Archiving.JournalPeriods.RandomItem();
            Assert.NotNull(changedInputs.JournalPeriods);
            var inputPeriodToChange = changedInputs.JournalPeriods.RandomItem();
            inputPeriodToChange.Uuid = periodToChange.Uuid;

            var journalPeriodUuidsBefore = usageDto.Archiving.JournalPeriods.Select(x => x.Uuid).ToList();

            //Act
            using var secondArchivingResponse = await ItSystemUsageV2Helper.SendPatchArchiving(token, usageDto.Uuid, changedInputs);

            //Assert
            Assert.Equal(HttpStatusCode.OK, secondArchivingResponse.StatusCode);
            usageDto = await secondArchivingResponse.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();
            Assert.Contains(usageDto.Archiving.JournalPeriods, x =>
                x.Uuid == inputPeriodToChange.Uuid.GetValueOrDefault() &&
                x.Approved == inputPeriodToChange.Approved &&
                x.StartDate == inputPeriodToChange.StartDate &&
                x.EndDate == inputPeriodToChange.EndDate &&
                x.ArchiveId == inputPeriodToChange.ArchiveId);
            Assert.Single(usageDto.Archiving.JournalPeriods.Select(x => x.Uuid).Intersect(journalPeriodUuidsBefore));
        }

        [Fact]
        public async Task Can_Delete_ItSystemUsage()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var usageDto = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            using var getResult = await ItSystemUsageV2Helper.SendGetSingleAsync(token, usageDto.Uuid);
            Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);

            //Act
            using var deleteResult = await ItSystemUsageV2Helper.SendDeleteAsync(token, usageDto.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResult.StatusCode);
            using var notGetResult = await ItSystemUsageV2Helper.SendGetSingleAsync(token, usageDto.Uuid);
            Assert.Equal(HttpStatusCode.NotFound, notGetResult.StatusCode);
        }

        [Fact]
        public async Task Can_Delete_ItSystemUsage_Fails_If_System_Not_Exists()
        {
            //Arrange
            var (token, _, __, ___) = await CreatePrerequisitesAsync();

            //Act
            using var deleteResult = await ItSystemUsageV2Helper.SendDeleteAsync(token, A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Can_Delete_ItSystemUsage_Fails_If_Not_Allowed_To_Delete()
        {
            //Arrange
            var (token1, _, organization1, system1) = await CreatePrerequisitesAsync();
            var (token2, _, __, ___) = await CreatePrerequisitesAsync();

            var usageDto = await ItSystemUsageV2Helper.PostAsync(token1, CreatePostRequest(organization1.Uuid, system1.Uuid));
            using var getResult = await ItSystemUsageV2Helper.SendGetSingleAsync(token1, usageDto.Uuid);
            Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);

            //Act
            using var deleteResult = await ItSystemUsageV2Helper.SendDeleteAsync(token2, usageDto.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, deleteResult.StatusCode);
            using var getStillExistsResult = await ItSystemUsageV2Helper.SendGetSingleAsync(token1, usageDto.Uuid);
            Assert.Equal(HttpStatusCode.OK, getStillExistsResult.StatusCode);
        }

        [Fact]
        public async Task Can_POST_With_All_Data()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var (generalData, orgUnit, organizationUsageData, addedTaskRefs, removedTaskRefs, kleDeviations, externalReferences, roles, gdpr, archiving) = await CreateFullDataRequestDto(organization, system);

            var request = CreatePostRequest(organization.Uuid, system.Uuid,
                    generalSection: generalData,
                    organizationalUsageSection: organizationUsageData,
                    kleDeviationsRequest: kleDeviations,
                    referenceDataDtos: externalReferences,
                    roles: roles,
                    gdpr: gdpr,
                    archiving: archiving);

            //Act
            var createdUsage = await ItSystemUsageV2Helper.PostAsync(token, request);

            //Assert
            AssertGeneralData(request.General, createdUsage.General);

            await AssertOrganizationalUsage(token, createdUsage.Uuid, new[] { orgUnit }, orgUnit);

            AssertKleDeviation(true, addedTaskRefs, createdUsage.LocalKLEDeviations.AddedKLE);
            AssertKleDeviation(true, removedTaskRefs, createdUsage.LocalKLEDeviations.RemovedKLE);

            Assert.NotNull(request.ExternalReferences);
            AssertExternalReferenceResults(request.ExternalReferences.ToList(), createdUsage);

            Assert.NotNull(request.Roles);
            AssertRoles(request.Roles, createdUsage.Roles);

            Assert.NotNull(request.GDPR);
            AssertGdpr(request.GDPR, createdUsage.GDPR);

            Assert.NotNull(request.Archiving);
            AssertArchivingParametersSet<ArchivingCreationRequestDTO, JournalPeriodDTO>(request.Archiving, createdUsage.Archiving);
        }

        [Fact]
        public async Task Can_PUT_With_All_Data()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            var (generalData1, orgUnit1, organizationUsageData1, addedTaskRefs1, removedTaskRefs1, kleDeviations1, externalReferences1, roles1, gdpr1, archiving1) = await CreateUpdateFullDataRequestDto(organization, system);
            var updateRequest1 = CreatePutRequest(
                    generalSection: generalData1,
                    organizationalUsageSection: organizationUsageData1,
                    kleDeviationsRequest: kleDeviations1,
                    referenceDataDtos: externalReferences1,
                    roles: roles1,
                    gdpr: gdpr1,
                    baseArchiving: archiving1);

            //Act - PUT on empty system usage
            var updatedUsage1 = await ItSystemUsageV2Helper.PutAsync(token, newUsage.Uuid, updateRequest1);

            //Assert - PUT on empty system usage
            AssertGeneralData(updateRequest1.General, updatedUsage1.General);

            await AssertOrganizationalUsage(token, updatedUsage1.Uuid, new[] { orgUnit1 }, orgUnit1);

            AssertKleDeviation(true, addedTaskRefs1, updatedUsage1.LocalKLEDeviations.AddedKLE);
            AssertKleDeviation(true, removedTaskRefs1, updatedUsage1.LocalKLEDeviations.RemovedKLE);

            Assert.NotNull(updateRequest1.ExternalReferences);
            AssertExternalReferenceResults(updateRequest1.ExternalReferences.ToList(), updatedUsage1, true);

            Assert.NotNull(updateRequest1.Roles);
            AssertRoles(updateRequest1.Roles, updatedUsage1.Roles);

            Assert.NotNull(updateRequest1.GDPR);
            AssertGdpr(updateRequest1.GDPR, updatedUsage1.GDPR);

            Assert.NotNull(updateRequest1.Archiving);
            AssertArchivingParametersSet<ArchivingUpdateRequestDTO, JournalPeriodUpdateRequestDTO>(updateRequest1.Archiving, updatedUsage1.Archiving);

            //Act - PUT on filled system usage
            var (generalData2, orgUnit2, organizationUsageData2, addedTaskRefs2, removedTaskRefs2, kleDeviations2,
                externalReferences2, roles2, gdpr2, archiving2) = await CreateUpdateFullDataRequestDto(organization, system, updatedUsage1.ExternalReferences);
            var updateRequest2 = CreatePutRequest(
                    generalSection: generalData2,
                    organizationalUsageSection: organizationUsageData2,
                    kleDeviationsRequest: kleDeviations2,
                    referenceDataDtos: externalReferences2,
                    roles: roles2,
                    gdpr: gdpr2,
                    baseArchiving: archiving2);

            var updatedUsage2 = await ItSystemUsageV2Helper.PutAsync(token, newUsage.Uuid, updateRequest2);

            //Assert - PUT on filled system usage
            AssertGeneralData(updateRequest2.General, updatedUsage2.General);

            await AssertOrganizationalUsage(token, updatedUsage2.Uuid, new[] { orgUnit2 }, orgUnit2);

            AssertKleDeviation(true, addedTaskRefs2, updatedUsage2.LocalKLEDeviations.AddedKLE);
            AssertKleDeviation(true, removedTaskRefs2, updatedUsage2.LocalKLEDeviations.RemovedKLE);

            Assert.NotNull(updateRequest2.ExternalReferences);
            AssertExternalReferenceResults(updateRequest2.ExternalReferences.ToList(), updatedUsage2);

            Assert.NotNull(updateRequest2.Roles);
            AssertRoles(updateRequest2.Roles, updatedUsage2.Roles);

            Assert.NotNull(updateRequest2.GDPR);
            AssertGdpr(updateRequest2.GDPR, updatedUsage2.GDPR);

            Assert.NotNull(updateRequest2.Archiving);
            AssertArchivingParametersSet<ArchivingUpdateRequestDTO, JournalPeriodUpdateRequestDTO>(updateRequest2.Archiving, updatedUsage2.Archiving);

            //Act - PUT empty on filled system usage
            var updateRequest3 = CreatePutRequest(
                    generalSection: new GeneralDataWriteRequestDTO(),
                    organizationalUsageSection: new OrganizationUsageWriteRequestDTO(),
                    kleDeviationsRequest: new LocalKLEDeviationsRequestDTO(),
                    referenceDataDtos: new List<UpdateExternalReferenceDataWriteRequestDTO>(),
                    roles: new List<RoleAssignmentRequestDTO>(),
                    gdpr: new GDPRWriteRequestDTO(),
                    baseArchiving: new ArchivingUpdateRequestDTO());

            var updatedUsage3 = await ItSystemUsageV2Helper.PutAsync(token, newUsage.Uuid, updateRequest3);

            //Assert - PUT empty on filled system usage
            AssertGeneralData(updateRequest3.General, updatedUsage3.General, false);

            await AssertOrganizationalUsage(token, updatedUsage3.Uuid, new OrganizationUnitResponseDTO[] { }, null);

            Assert.Empty(updatedUsage3.LocalKLEDeviations.AddedKLE);
            Assert.Empty(updatedUsage3.LocalKLEDeviations.RemovedKLE);

            Assert.Empty(updatedUsage3.ExternalReferences);

            Assert.Empty(updatedUsage3.Roles);

            Assert.NotNull(updateRequest3.GDPR);
            AssertGdpr(updateRequest3.GDPR, updatedUsage3.GDPR);

            AssertArchivingParametersNotSet(updatedUsage3.Archiving);
        }

        [Fact]
        public async Task Can_GET_SystemUsageRelation()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var (interfaceUuid, interfaceName) = await CreateExhibitingInterface(organization.Uuid, system2.Uuid);
            var contract = await CreateItContractAsync(organization.Uuid);
            var relationFrequency = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageRelationFrequencies, organization.Uuid, 1, 0)).First();

            var input = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage2.Uuid,
                RelationInterfaceUuid = interfaceUuid,
                AssociatedContractUuid = contract.Uuid,
                RelationFrequencyUuid = relationFrequency.Uuid,
                Description = A<string>(),
                UrlReference = A<string>()
            };
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, input);

            //Act
            var retrievedRelation = await ItSystemUsageV2Helper.GetRelationAsync(token, usage1.Uuid, createdRelation.Uuid);

            //Assert
            AssertRelation(input, interfaceName, contract.Name, relationFrequency.Name, retrievedRelation);
        }

        [Fact]
        public async Task Cannot_GET_SystemUsageRelation_If_Relation_Not_Exists()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var usage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Act
            using var getResult = await ItSystemUsageV2Helper.SendGetRelationAsync(token, usage.Uuid, A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, getResult.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_SystemUsageRelation_If_SystemUsage_Not_Exists()
        {
            //Arrange
            var (token, _, __, ___) = await CreatePrerequisitesAsync();

            //Act
            using var getResult = await ItSystemUsageV2Helper.SendGetRelationAsync(token, A<Guid>(), A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, getResult.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_SystemUsageRelation_If_Not_Allowed()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var (interfaceUuid, _) = await CreateExhibitingInterface(organization.Uuid, system2.Uuid);
            var contract = await CreateItContractAsync(organization.Uuid);
            var relationFrequency = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageRelationFrequencies, organization.Uuid, 1, 0)).First();

            var input = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage2.Uuid,
                RelationInterfaceUuid = interfaceUuid,
                AssociatedContractUuid = contract.Uuid,
                RelationFrequencyUuid = relationFrequency.Uuid,
                Description = A<string>(),
                UrlReference = A<string>()
            };
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, input);

            var tokenForOtherUser = await CreateUserInNewOrgAndGetToken();

            //Act
            using var getResult = await ItSystemUsageV2Helper.SendGetRelationAsync(tokenForOtherUser, usage1.Uuid, createdRelation.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, getResult.StatusCode);
        }

        [Fact]
        public async Task Can_GET_Incoming_SystemUsageRelation()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var (interfaceUuid, interfaceName) = await CreateExhibitingInterface(organization.Uuid, system2.Uuid);
            var contract = await CreateItContractAsync(organization.Uuid);
            var relationFrequency = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageRelationFrequencies, organization.Uuid, 1, 0)).First();

            var input = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage2.Uuid,
                RelationInterfaceUuid = interfaceUuid,
                AssociatedContractUuid = contract.Uuid,
                RelationFrequencyUuid = relationFrequency.Uuid,
                Description = A<string>(),
                UrlReference = A<string>()
            };
            await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, input);

            //Act
            var retrievedRelation = await ItSystemUsageV2Helper.GetIncomingRelationsAsync(token, usage2.Uuid);

            //Assert
            var relation = Assert.Single(retrievedRelation);
            AssertIncomingRelation(input, usage1.Uuid, interfaceName, contract.Name, relationFrequency.Name, relation);
        }

        [Fact]
        public async Task Can_POST_SystemUsageRelation_With_Just_SystemUsages()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var input = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage2.Uuid
            };

            //Act
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, input);

            //Assert
            Assert.NotNull(createdRelation.ToSystemUsage);
            Assert.Equal(usage2.Uuid, createdRelation.ToSystemUsage.Uuid);
        }

        [Fact]
        public async Task Can_POST_SystemUsageRelation_With_All_Data_Set()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var (interfaceUuid, interfaceName) = await CreateExhibitingInterface(organization.Uuid, system2.Uuid);
            var contract = await CreateItContractAsync(organization.Uuid);
            var relationFrequency = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageRelationFrequencies, organization.Uuid, 1, 0)).First();

            var input = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage2.Uuid,
                RelationInterfaceUuid = interfaceUuid,
                AssociatedContractUuid = contract.Uuid,
                RelationFrequencyUuid = relationFrequency.Uuid,
                Description = A<string>(),
                UrlReference = A<string>()
            };

            //Act
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, input);

            //Assert
            AssertRelation(input, interfaceName, contract.Name, relationFrequency.Name, createdRelation);
        }

        [Fact]
        public async Task Cannot_POST_SystemUsageRelation_If_From_Usage_Not_Exists()
        {
            //Arrange
            var (token, _, organization, _) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var input = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage2.Uuid
            };

            //Act
            using var createdResponse = await ItSystemUsageV2Helper.SendPostRelationAsync(token, A<Guid>(), input);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, createdResponse.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_SystemUsageRelation_If_Not_Allowed()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var input = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage2.Uuid
            };

            var tokenForOtherUser = await CreateUserInNewOrgAndGetToken();

            //Act
            using var createdResponse = await ItSystemUsageV2Helper.SendPostRelationAsync(tokenForOtherUser, usage1.Uuid, input);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, createdResponse.StatusCode);
        }

        [Theory]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, false, true)]
        public async Task Cannot_POST_SystemUsageRelation_If_DTO_Contains_Bad_Input(bool badToUsage, bool badInterface, bool badContract, bool badFrequency)
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var (interfaceUuid, _) = await CreateExhibitingInterface(organization.Uuid, system2.Uuid);
            var contract = await CreateItContractAsync(organization.Uuid);
            var relationFrequency = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageRelationFrequencies, organization.Uuid, 1, 0)).First();


            var input = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = badToUsage ? A<Guid>() : usage2.Uuid,
                RelationInterfaceUuid = badInterface ? A<Guid>() : interfaceUuid,
                AssociatedContractUuid = badContract ? A<Guid>() : contract.Uuid,
                RelationFrequencyUuid = badFrequency ? A<Guid>() : relationFrequency.Uuid,
            };

            //Act
            using var createdResponse = await ItSystemUsageV2Helper.SendPostRelationAsync(token, usage1.Uuid, input);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, createdResponse.StatusCode);
        }

        [Fact]
        public async Task Can_PUT_SystemUsageRelation()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            // Create relation
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, new SystemRelationWriteRequestDTO { ToSystemUsageUuid = usage2.Uuid });
            Assert.NotNull(createdRelation.ToSystemUsage);
            Assert.Equal(usage2.Uuid, createdRelation.ToSystemUsage.Uuid);

            // Starting from 3 as we have 3 systems
            var system3 = await CreateItSystemAsync(organization.Uuid);
            var usage3 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system3.Uuid));

            var (interfaceUuid3, interfaceName3) = await CreateExhibitingInterface(organization.Uuid, system3.Uuid);
            var contract3 = await CreateItContractAsync(organization.Uuid);
            var relationFrequency3 = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageRelationFrequencies, organization.Uuid, 1, 0)).First();

            var updateInput3 = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage3.Uuid,
                RelationInterfaceUuid = interfaceUuid3,
                AssociatedContractUuid = contract3.Uuid,
                RelationFrequencyUuid = relationFrequency3.Uuid,
                Description = A<string>(),
                UrlReference = A<string>()
            };

            //Act - Update from empty
            var updatedRelation3 = await ItSystemUsageV2Helper.PutRelationAsync(token, usage1.Uuid, createdRelation.Uuid, updateInput3);

            //Assert - Update from empty
            AssertRelation(updateInput3, interfaceName3, contract3.Name, relationFrequency3.Name, updatedRelation3);

            //Act - Update from filled
            var system4 = await CreateItSystemAsync(organization.Uuid);
            var usage4 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system4.Uuid));

            var (interfaceUuid4, interfaceName4) = await CreateExhibitingInterface(organization.Uuid, system4.Uuid);
            var contract4 = await CreateItContractAsync(organization.Uuid);
            var relationFrequency4 = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageRelationFrequencies, organization.Uuid, 1, 1)).First();

            var updateInput4 = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage4.Uuid,
                RelationInterfaceUuid = interfaceUuid4,
                AssociatedContractUuid = contract4.Uuid,
                RelationFrequencyUuid = relationFrequency4.Uuid,
                Description = A<string>(),
                UrlReference = A<string>()
            };

            var updatedRelation4 = await ItSystemUsageV2Helper.PutRelationAsync(token, usage1.Uuid, createdRelation.Uuid, updateInput4);

            //Assert - Update from filled
            AssertRelation(updateInput4, interfaceName4, contract4.Name, relationFrequency4.Name, updatedRelation4);

            //Act - Update to empty
            var system5 = await CreateItSystemAsync(organization.Uuid);
            var usage5 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system5.Uuid));

            var updateInput5 = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage5.Uuid
            };

            var updatedRelation5 = await ItSystemUsageV2Helper.PutRelationAsync(token, usage1.Uuid, createdRelation.Uuid, updateInput5);

            //Assert - Update to empty
            Assert.NotNull(updatedRelation5.ToSystemUsage);
            Assert.Equal(usage5.Uuid, updatedRelation5.ToSystemUsage.Uuid);
            Assert.Null(updatedRelation5.RelationInterface);
            Assert.Null(updatedRelation5.AssociatedContract);
            Assert.Null(updatedRelation5.RelationFrequency);
            Assert.Null(updatedRelation5.Description);
            Assert.Null(updatedRelation5.UrlReference);
        }

        [Fact]
        public async Task Cannot_PUT_SystemUsageRelation_If_From_Usage_Not_Exists()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            // Create relation
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, new SystemRelationWriteRequestDTO { ToSystemUsageUuid = usage2.Uuid });
            Assert.NotNull(createdRelation.ToSystemUsage);
            Assert.Equal(usage2.Uuid, createdRelation.ToSystemUsage.Uuid);


            var system3 = await CreateItSystemAsync(organization.Uuid);
            var usage3 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system3.Uuid));
            var updateInput = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage3.Uuid
            };

            //Act
            using var updatedResponse = await ItSystemUsageV2Helper.SendPutRelationAsync(token, A<Guid>(), createdRelation.Uuid, updateInput);

            //Assert 
            Assert.Equal(HttpStatusCode.NotFound, updatedResponse.StatusCode);
        }

        [Fact]
        public async Task Cannot_PUT_SystemUsageRelation_If_Relation_Not_Exists()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));

            // Starting from 3 as we have 3 systems
            var system3 = await CreateItSystemAsync(organization.Uuid);
            var usage3 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system3.Uuid));

            var updateInput = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage3.Uuid
            };

            //Act
            using var updatedResponse = await ItSystemUsageV2Helper.SendPutRelationAsync(token, usage1.Uuid, A<Guid>(), updateInput);

            //Assert 
            Assert.Equal(HttpStatusCode.BadRequest, updatedResponse.StatusCode);
        }

        [Fact]
        public async Task Cannot_PUT_SystemUsageRelation_If_Not_Allowed()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var createInput = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage2.Uuid
            };

            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, createInput);

            var system3 = await CreateItSystemAsync(organization.Uuid);
            var usage3 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system3.Uuid));

            var updateInput = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage3.Uuid
            };

            var tokenForOtherUser = await CreateUserInNewOrgAndGetToken();

            //Act
            using var updatedResponse = await ItSystemUsageV2Helper.SendPutRelationAsync(tokenForOtherUser, usage1.Uuid, createdRelation.Uuid, updateInput);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, updatedResponse.StatusCode);
        }

        [Theory]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, false, true)]
        public async Task Cannot_PUT_SystemUsageRelation_If_DTO_Contains_Bad_Input(bool badToUsage, bool badInterface, bool badContract, bool badFrequency)
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            // Create relation
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, new SystemRelationWriteRequestDTO { ToSystemUsageUuid = usage2.Uuid });
            Assert.NotNull(createdRelation.ToSystemUsage);
            Assert.Equal(usage2.Uuid, createdRelation.ToSystemUsage.Uuid);

            // Starting from 3 as we have 3 systems
            var system3 = await CreateItSystemAsync(organization.Uuid);
            var usage3 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system3.Uuid));

            var (interfaceUuid3, _) = await CreateExhibitingInterface(organization.Uuid, system3.Uuid);
            var contract3 = await CreateItContractAsync(organization.Uuid);
            var relationFrequency3 = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageRelationFrequencies, organization.Uuid, 1, 0)).First();

            var updateInput = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = badToUsage ? A<Guid>() : usage3.Uuid,
                RelationInterfaceUuid = badInterface ? A<Guid>() : interfaceUuid3,
                AssociatedContractUuid = badContract ? A<Guid>() : contract3.Uuid,
                RelationFrequencyUuid = badFrequency ? A<Guid>() : relationFrequency3.Uuid,
                Description = A<string>(),
                UrlReference = A<string>()
            };

            //Act
            using var updatedResponse = await ItSystemUsageV2Helper.SendPutRelationAsync(token, usage1.Uuid, createdRelation.Uuid, updateInput);

            //Assert 
            Assert.Equal(HttpStatusCode.BadRequest, updatedResponse.StatusCode);
        }

        [Fact]
        public async Task Can_DELETE_SystemUsageRelation_With_Just_SystemUsages()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var input = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage2.Uuid
            };
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, input);

            //Act
            using var deleteResult = await ItSystemUsageV2Helper.SendDeleteRelationAsync(token, usage1.Uuid, createdRelation.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Can_DELETE_SystemUsageRelation_With_All_Data_Set()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var (interfaceUuid, _) = await CreateExhibitingInterface(organization.Uuid, system2.Uuid);
            var contract = await CreateItContractAsync(organization.Uuid);
            var relationFrequency = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageRelationFrequencies, organization.Uuid, 1, 0)).First();

            var input = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage2.Uuid,
                RelationInterfaceUuid = interfaceUuid,
                AssociatedContractUuid = contract.Uuid,
                RelationFrequencyUuid = relationFrequency.Uuid,
                Description = A<string>(),
                UrlReference = A<string>()
            };
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, input);

            //Act
            using var deleteResult = await ItSystemUsageV2Helper.SendDeleteRelationAsync(token, usage1.Uuid, createdRelation.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Cannot_DELETE_SystemUsageRelation_If_From_Usage_Not_Exists()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var input = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage2.Uuid
            };
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, input);

            //Act
            using var deleteResult = await ItSystemUsageV2Helper.SendDeleteRelationAsync(token, A<Guid>(), createdRelation.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Cannot_DELETE_SystemUsageRelation_If_Relation_Not_Exists()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));

            //Act
            using var deleteResult = await ItSystemUsageV2Helper.SendDeleteRelationAsync(token, usage1.Uuid, A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Cannot_DELETE_SystemUsageRelation_If_Not_Allowed()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var input = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage2.Uuid
            };
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, input);

            var tokenForOtherUser = await CreateUserInNewOrgAndGetToken();

            //Act
            using var deleteResult = await ItSystemUsageV2Helper.SendDeleteRelationAsync(tokenForOtherUser, usage1.Uuid, createdRelation.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Can_Create_Update_And_Delete_ExternalReference()
        {
            //Arrange
            var (token, _, organization, system1) = await CreatePrerequisitesAsync();
            var usage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));

            var request = new ExternalReferenceDataWriteRequestDTO
            {
                DocumentId = A<string>(),
                MasterReference = A<bool>(),
                Title = A<string>(),
                Url = A<string>()
            };

            //Act
            var createdReference = await ItSystemUsageV2Helper.AddExternalReferenceAsync(token, usage.Uuid, request);

            //Assert
            AssertExternalReference(request, createdReference);

            var usageAfterCreate = await ItSystemUsageV2Helper.GetSingleAsync(token, usage.Uuid);

            var checkCreatedExternalReference = Assert.Single(usageAfterCreate.ExternalReferences);
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
            var updatedReference = await ItSystemUsageV2Helper.UpdateExternalReferenceAsync(token, usage.Uuid, createdReference.Uuid, updateRequest);

            //Assert - update
            AssertExternalReference(updateRequest, updatedReference);

            var usageAfterUpdate = await ItSystemUsageV2Helper.GetSingleAsync(token, usage.Uuid);

            var checkUpdatedExternalReference = Assert.Single(usageAfterUpdate.ExternalReferences);
            AssertExternalReference(updateRequest, checkUpdatedExternalReference);

            //Act - delete
            await ItSystemUsageV2Helper.DeleteExternalReferenceAsync(token, usage.Uuid, createdReference.Uuid);

            //Assert - delete
            var usageAfterDelete = await ItSystemUsageV2Helper.GetSingleAsync(token, usage.Uuid);
            Assert.Empty(usageAfterDelete.ExternalReferences);
        }

        [Fact]
        public async Task Can_POST_And_GET_Journal_Period()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var usageDto = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var journalPeriods = CreateNewJournalPeriods(3);
            var newJournalPeriodInput = journalPeriods.RandomItem();

            //Act
            var postResult = await ItSystemUsageV2Helper.CreateJournalPeriodAsync(token, usageDto.Uuid, newJournalPeriodInput);
            var getResult = await ItSystemUsageV2Helper.GetJournalPeriodAsync(token, usageDto.Uuid, postResult.Uuid);

            //Assert
            AssertJournalPeriod(newJournalPeriodInput, postResult);
            AssertJournalPeriod(postResult, getResult);

        }

        [Fact]
        public async Task Can_PUT_Journal_Period()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var usageDto = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var journalPeriods = CreateNewJournalPeriods(3);
            var newJournalPeriodInputs = journalPeriods.RandomItems(2).ToList();
            var initial = newJournalPeriodInputs.First();
            var updated = newJournalPeriodInputs.Last();
            var postResult = await ItSystemUsageV2Helper.CreateJournalPeriodAsync(token, usageDto.Uuid, initial);

            //Act
            var putResult = await ItSystemUsageV2Helper.UpdateJournalPeriodAsync(token, usageDto.Uuid, postResult.Uuid, updated);
            var getResult = await ItSystemUsageV2Helper.GetJournalPeriodAsync(token, usageDto.Uuid, postResult.Uuid);

            //Assert
            AssertJournalPeriod(updated, putResult);
            AssertJournalPeriod(putResult, getResult);
        }

        [Fact]
        public async Task Can_DELETE_Journal_Period()
        {
            //Arrange
            var (token, _, organization, system) = await CreatePrerequisitesAsync();
            var usageDto = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var journalPeriods = CreateNewJournalPeriods(3);
            var newJournalPeriodInputs = journalPeriods.RandomItems(2);
            var initial = newJournalPeriodInputs.First();
            var postResult = await ItSystemUsageV2Helper.CreateJournalPeriodAsync(token, usageDto.Uuid, initial);

            //Act
            using var deleteResult = await ItSystemUsageV2Helper.SendDeleteJournalPeriodAsync(token, usageDto.Uuid, postResult.Uuid);
            using var getAfterDeleteResult = await ItSystemUsageV2Helper.SendGetJournalPeriodAsync(token, usageDto.Uuid, postResult.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResult.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResult.StatusCode);
        }

        [Fact]
        public async Task Can_GET_Roles_From_Internal_Endpoint()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(type: A<OrganizationType>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Uuid, AccessModifier.Public);

            var user1 = await CreateUser(organization);
            var user2 = await CreateUser(organization);
            var rawRoles = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole[]>(x => x.AsQueryable().AsEnumerable().Where(r => r.IsObligatory && r.IsEnabled).RandomItems(2).ToArray());
            var role1 = rawRoles.First();
            var role2 = rawRoles.Last();
            var roles = new List<RoleAssignmentRequestDTO>
            {
                new()
                {
                    RoleUuid = role1.Uuid,
                    UserUuid = user1.Uuid
                },
                new()
                {
                    RoleUuid = role2.Uuid,
                    UserUuid = user2.Uuid
                }
            };
            var createdDto = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid, roles: roles));

            //Act
            var assignedRoles = (await ItSystemUsageV2Helper.GetRoleAssignmentsInternalAsync(createdDto.Uuid)).ToList();

            //Assert
            Assert.Equal(2, assignedRoles.Count);
            Assert.Contains(assignedRoles, assignment => MatchExpectedAssignment(assignment, role1, user1));
            Assert.Contains(assignedRoles, assignment => MatchExpectedAssignment(assignment, role2, user2));
        }

        public static IEnumerable<object[]> InvalidGdprRequests()
        {
            var fixture = new Fixture();

            var nonYesValues = new List<YesNoDontKnowChoice?>
                { YesNoDontKnowChoice.No, YesNoDontKnowChoice.Undecided, YesNoDontKnowChoice.DontKnow, null };

            var nonYesValuesWithIrrelevant = new List<YesNoDontKnowIrrelevantChoice?>
                { YesNoDontKnowIrrelevantChoice.No, YesNoDontKnowIrrelevantChoice.Undecided, YesNoDontKnowIrrelevantChoice.DontKnow, YesNoDontKnowIrrelevantChoice.Irrelevant , null };


            return new List<object[]>
            {
                new object[]
                {
                    new GDPRWriteRequestDTO
                    {
                        RiskAssessmentConducted = nonYesValuesWithIrrelevant.RandomItem(),
                        RiskAssessmentNotes     = fixture.Create<string>()
                    }
                },
                new object[]
                {
                    new GDPRWriteRequestDTO
                    {
                        TechnicalPrecautionsInPlace       = nonYesValues.RandomItem(),
                        TechnicalPrecautionsDocumentation = fixture.Create<SimpleLinkDTO>()
                    }
                },
                new object[]
                {
                    new GDPRWriteRequestDTO
                    {
                        DPIAConducted = nonYesValues.RandomItem(),
                        DPIADate = fixture.Create<DateTime>()
                    }
                },
                new object[]
                {
                    new GDPRWriteRequestDTO
                    {
                        UserSupervision = nonYesValues.RandomItem(),
                        UserSupervisionDocumentation = fixture.Create<SimpleLinkDTO>()
                    }
                },
                new object[]
                {
                    new GDPRWriteRequestDTO
                    {
                        RetentionPeriodDefined = nonYesValues.RandomItem(),
                        NextDataRetentionEvaluationDate = fixture.Create<DateTime>()
                    }
                }
            };
        }

        [Theory]
        [MemberData(nameof(InvalidGdprRequests))]
        public async Task Patch_Gdpr_Returns_Bad_Request_For_Invalid_Requests(GDPRWriteRequestDTO invalidRequest)
        {
            var organization = await CreateOrganizationAsync();
            var usage = await CreateSystemAndTakeItIntoUsage(organization.Uuid);
            var token = await GetGlobalToken();

            var response = await ItSystemUsageV2Helper.SendPatchGDPR(token, usage.Uuid, invalidRequest);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Patching_Risk_Assessment_To_Non_Yes_Value_Resets_Other_Fields()
        {
            var organization = await CreateOrganizationAsync();
            var usage = await CreateSystemAndTakeItIntoUsage(organization.Uuid);
            var token = await GetGlobalToken();
            var patchBefore = new GDPRWriteRequestDTO
            {
                RiskAssessmentConducted = YesNoDontKnowIrrelevantChoice.Yes,
                RiskAssessmentConductedDate = A<DateTime>(),
                RiskAssessmentNotes = A<string>(),
                RiskAssessmentDocumentation = A<SimpleLinkDTO>(),
                RiskAssessmentResult = A<RiskLevelChoice>()
            };
            await ItSystemUsageV2Helper.SendPatchGDPR(token, usage.Uuid, patchBefore).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();
            var resetRequest = new GDPRWriteRequestDTO { RiskAssessmentConducted = YesNoDontKnowIrrelevantChoice.No };

            var response = await ItSystemUsageV2Helper.SendPatchGDPR(token, usage.Uuid, resetRequest);

            var system = await response.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();
            var gdpr = system.GDPR;
            Assert.Null(gdpr.RiskAssessmentConductedDate);
            Assert.Null(gdpr.RiskAssessmentNotes);
            Assert.Null(gdpr.RiskAssessmentDocumentation?.Name);
            Assert.Null(gdpr.RiskAssessmentDocumentation?.Url);
            Assert.Null(gdpr.RiskAssessmentResult);
        }

        [Fact]
        public async Task Can_Archive_SystemUsage()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            var localCallName = A<string>();
            var localSystemId = A<string>();
            var systemUsage = await ItSystemUsageV2Helper.PostAsync(token, new CreateItSystemUsageRequestDTO
            {
                SystemUuid = system.Uuid,
                OrganizationUuid = organization.Uuid,
                General = new GeneralDataWriteRequestDTO
                {
                    LocalCallName = localCallName,
                    LocalSystemId = localSystemId
                }
            });
            var archivingDate = A<DateTime>();
            var takenIntoUsageDate = A<DateTime>();
            var referenceName = A<string>();
            var note = A<string>();
            var existingReference = new SimpleLinkDTO
            {
                Name = A<string>(),
                Url = $"https://{A<string>()}.example.com/archive-1"
            };

            var request = new CreateItSystemUsageArchiveRequestDTO
            {
                ArchivingDate = archivingDate,
                TakenIntoUsageDate = takenIntoUsageDate,
                ReferenceName = referenceName,
                Note = note,
                ArchiveReferences = [existingReference]
            };

            //Act
            var archive = await ItSystemUsageV2Helper.ArchiveAsync(token, systemUsage.Uuid, request);

            //Assert
            Assert.NotNull(archive);
            Assert.Equal(archivingDate, archive.ArchivingDate);
            Assert.Equal(referenceName, archive.ReferenceName);
            Assert.Equal(note, archive.Note);
            Assert.Equal(system.Uuid, archive.ItSystemUuid);
            Assert.Equal(system.Name, archive.LegacyName);
            Assert.Equal(localCallName, archive.LocalName);
            Assert.Equal(localSystemId, archive.LocalId);
            Assert.Equal(organization.Uuid, archive.Organization?.Uuid);
            Assert.Equal(user.GetFullName(), archive.ArchivedByFullName);
            var reference = Assert.Single(archive.ArchiveReferences);

            Assert.Equal(existingReference.Name, reference.Name);
            Assert.Equal(existingReference.Url, reference.Url);
        }

        private static bool MatchExpectedAssignment(ExtendedRoleAssignmentResponseDTO assignment, ItSystemRole expectedRole, User expectedUser)
        {
            return assignment.Role.Name == expectedRole.Name &&
                   assignment.Role.Uuid == expectedRole.Uuid &&
                   assignment.User.Email == expectedUser.Email &&
                   assignment.User.Name == expectedUser.GetFullName() &&
                   assignment.User.Uuid == expectedUser.Uuid;
        }

        private static void AssertJournalPeriod(JournalPeriodDTO newJournalPeriodInput, JournalPeriodResponseDTO result)
        {
            Assert.Equal(newJournalPeriodInput.Approved, result.Approved);
            AssertTimestampEqual(newJournalPeriodInput.StartDate, result.StartDate);
            AssertTimestampEqual(newJournalPeriodInput.EndDate, result.EndDate);
            Assert.Equal(newJournalPeriodInput.ArchiveId, result.ArchiveId);
        }

        private static void AssertJournalPeriod(JournalPeriodResponseDTO expected, JournalPeriodResponseDTO actual)
        {
            Assert.Equal(expected.Uuid, actual.Uuid);
            Assert.Equal(expected.Approved, actual.Approved);
            AssertTimestampEqual(expected.StartDate, actual.StartDate);
            AssertTimestampEqual(expected.EndDate, actual.EndDate);
            Assert.Equal(expected.ArchiveId, actual.ArchiveId);
        }

        private static void AssertTimestampEqual(DateTime? expected, DateTime? actual)
        {
            Assert.Equal(NormalizeTimestamp(expected), NormalizeTimestamp(actual));
        }

        private static DateTime? NormalizeTimestamp(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            var utcValue = value.Value.Kind switch
            {
                DateTimeKind.Utc => value.Value,
                DateTimeKind.Local => value.Value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            };

            // PostgreSQL timestamps are stored with microsecond precision.
            var truncatedTicks = utcValue.Ticks - (utcValue.Ticks % 10);
            return new DateTime(truncatedTicks, DateTimeKind.Utc);
        }

        private static void AssertExternalReference(ExternalReferenceDataWriteRequestDTO expected, ExternalReferenceDataResponseDTO actual)
        {
            Assert.Equal(expected.DocumentId, actual.DocumentId);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.Url, actual.Url);
            Assert.Equal(expected.MasterReference, actual.MasterReference);
        }

        private async Task<string> CreateUserInNewOrgAndGetToken()
        {
            var otherOrganization = await CreateOrganizationAsync(type: A<OrganizationType>());
            var (_, token) = await CreateApiUser(otherOrganization);
            return token;
        }

        private static void AssertRelation(SystemRelationWriteRequestDTO expected, string expectedInterfaceName, string expectedContractName, string expectedFrequencyName, OutgoingSystemRelationResponseDTO actual)
        {
            Assert.NotNull(actual.ToSystemUsage);
            Assert.Equal(expected.ToSystemUsageUuid, actual.ToSystemUsage.Uuid);
            AssertBaseRelation(expected, expectedInterfaceName, expectedContractName, expectedFrequencyName, actual);
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void AssertIncomingRelation(SystemRelationWriteRequestDTO expected, Guid expectedFromSystemUsageUuid, string expectedInterfaceName, string expectedContractName, string expectedFrequencyName, IncomingSystemRelationResponseDTO actual)
        {
            Assert.NotNull(actual.FromSystemUsage);
            Assert.Equal(expectedFromSystemUsageUuid, actual.FromSystemUsage.Uuid);
            AssertBaseRelation(expected, expectedInterfaceName, expectedContractName, expectedFrequencyName, actual);
        }

        private static void AssertBaseRelation(SystemRelationWriteRequestDTO expected, string expectedInterfaceName,
            string expectedContractName, string expectedFrequencyName, BaseSystemRelationResponseDTO actual)
        {
            Assert.NotNull(actual.RelationInterface);
            Assert.NotNull(actual.AssociatedContract);
            Assert.NotNull(actual.RelationFrequency);
            Assert.Equal(expected.RelationInterfaceUuid, actual.RelationInterface.Uuid);
            Assert.Equal(expectedInterfaceName, actual.RelationInterface.Name);
            Assert.Equal(expected.AssociatedContractUuid, actual.AssociatedContract.Uuid);
            Assert.Equal(expectedContractName, actual.AssociatedContract.Name);
            Assert.Equal(expected.RelationFrequencyUuid, actual.RelationFrequency.Uuid);
            Assert.Equal(expectedFrequencyName, actual.RelationFrequency.Name);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.UrlReference, actual.UrlReference);
        }

        private async Task<(Guid, string)> CreateExhibitingInterface(Guid organizationUuid, Guid systemUuid)
        {
            var targetInterface = await CreateItInterfaceAsync(organizationUuid);
            await InterfaceV2Helper.SendPatchExposedBySystemAsync(await GetGlobalToken(), targetInterface.Uuid, systemUuid);
            return (targetInterface.Uuid, targetInterface.Name!);
        }

        private async Task<(GeneralDataWriteRequestDTO,
            OrganizationUnitResponseDTO,
            OrganizationUsageWriteRequestDTO,
            Guid[],
            Guid[],
            LocalKLEDeviationsRequestDTO,
            IEnumerable<UpdateExternalReferenceDataWriteRequestDTO>,
            IEnumerable<RoleAssignmentRequestDTO>,
            GDPRWriteRequestDTO,
            ArchivingUpdateRequestDTO)> CreateUpdateFullDataRequestDto(ShallowOrganizationResponseDTO organization, ItSystemResponseDTO system, IEnumerable<ExternalReferenceDataResponseDTO>? existingExternalReferences = null)
        {
            var fullData = await CreateFullDataRequestDto(organization, system);
            var archiving = await CreateArchivingUpdateRequestDto(organization.Uuid);
            var mappedFullData = (
                fullData.generalData,
                fullData.unit1,
                fullData.organizationUsageData,
                fullData.addedTaskRefs,
                fullData.removedTaskRefs,
                fullData.kleDeviations,
                existingExternalReferences != null ? CreateNewExternalReferenceDataWithOldUuid(existingExternalReferences) : MapExternalReferenceDtosToUpdateDtos(fullData.externalReferences),
                fullData.roles,
                fullData.gdpr,
                archiving);
            return mappedFullData;
        }

        private async Task<(GeneralDataWriteRequestDTO generalData,
            OrganizationUnitResponseDTO unit1,
            OrganizationUsageWriteRequestDTO organizationUsageData,
            Guid[] addedTaskRefs,
            Guid[] removedTaskRefs,
            LocalKLEDeviationsRequestDTO kleDeviations,
            IEnumerable<ExternalReferenceDataWriteRequestDTO> externalReferences,
            IEnumerable<RoleAssignmentRequestDTO> roles,
            GDPRWriteRequestDTO gdpr,
            ArchivingCreationRequestDTO archiving)> CreateFullDataRequestDto(ShallowOrganizationResponseDTO organization, ItSystemResponseDTO system)
        {
            var dataClassification = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageDataClassification, organization.Uuid, 1, 0)).First();
            var generalData = CreateGeneralDataWriteRequestDto(dataClassification.Uuid);

            var unit1 = await CreateOrganizationUnitAsync(organization.Uuid);
            var organizationUsageData = CreateOrganizationUsageWriteRequestDto(new[] { unit1.Uuid }, unit1.Uuid);

            var addedTaskRefs = Many<Guid>(2).ToArray();
            var taskRefsOnSystem = Many<Guid>(3).ToArray();
            var removedTaskRefs = taskRefsOnSystem.Take(2).ToArray();
            AddTaskRefsInDatabase(addedTaskRefs.Concat(taskRefsOnSystem));

            await ItSystemV2Helper.SendPatchSystemAsync(await GetGlobalToken(), system.Uuid, x => x.KLEUuids,
                taskRefsOnSystem).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            var kleDeviations = CreateLocalKleDeviationsRequestDto(addedTaskRefs, removedTaskRefs);

            var externalReferences = CreateExternalReferenceDataDtOs<ExternalReferenceDataWriteRequestDTO>();

            var userToGainRole = await CreateUserAsync(organization.Uuid);
            var role = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().First(r => r.IsObligatory && r.IsEnabled));
            var roles = CreateRoleAssignmentRequestDtOs(role.Uuid, userToGainRole.Uuid);

            var gdpr = await CreateGDPRInputAsync(organization);

            var archiving = await CreateArchivingCreationRequestDto(organization.Uuid);

            return (generalData, unit1, organizationUsageData, addedTaskRefs, removedTaskRefs, kleDeviations, externalReferences, roles, gdpr, archiving);
        }


        private static void AssertGeneralData(GeneralDataWriteRequestDTO? expected, GeneralDataResponseDTO actual, bool hasData = true)
        {
            Assert.NotNull(expected);
            Assert.Equal(expected.LocalCallName, actual.LocalCallName);
            Assert.Equal(expected.LocalSystemId, actual.LocalSystemId);
            Assert.Equal(expected.SystemVersion, actual.SystemVersion);
            Assert.Equal(expected.Notes, actual.Notes);
            Assert.Equal(expected.HostedAt, actual.HostedAt);
            Assert.Equal(expected.IsBusinessCritical, actual.IsBusinessCritical);
            if (hasData)
            {
                Assert.NotNull(expected.NumberOfExpectedUsers);
                Assert.NotNull(actual.NumberOfExpectedUsers);
                Assert.NotNull(expected.Validity);
                Assert.NotNull(actual.Validity);
                Assert.NotNull(actual.DataClassification);
                Assert.Equal(expected.NumberOfExpectedUsers.LowerBound, actual.NumberOfExpectedUsers.LowerBound);
                Assert.Equal(expected.NumberOfExpectedUsers.UpperBound, actual.NumberOfExpectedUsers.UpperBound);
                Assert.Equal(expected.Validity.LifeCycleStatus.GetValueOrDefault(), actual.Validity.LifeCycleStatus.GetValueOrDefault());
                Assert.Equal(expected.Validity.ValidFrom.GetValueOrDefault().Date, actual.Validity.ValidFrom.GetValueOrDefault().Date);
                Assert.Equal(expected.Validity.ValidTo.GetValueOrDefault().Date, actual.Validity.ValidTo.GetValueOrDefault().Date);
                Assert.Equal(expected.DataClassificationUuid, actual.DataClassification.Uuid);
            }
            else
            {
                Assert.Null(actual.NumberOfExpectedUsers);

                Assert.NotNull(actual.Validity);
                Assert.Null(actual.Validity.LifeCycleStatus);
                Assert.Null(actual.Validity.ValidFrom);
                Assert.Null(actual.Validity.ValidTo);

                Assert.Null(actual.DataClassification);
            }
        }

        private static void AssertRoles(IEnumerable<RoleAssignmentRequestDTO> expected, IEnumerable<RoleAssignmentResponseDTO> actual)
        {
            var expectedList = expected.ToList();
            var actualList = actual.ToList();
            
            Assert.Equal(expectedList.Count, actualList.Count);
            foreach (var expectedRight in expectedList)
            {
                Assert.Single(actualList, x => x.User.Uuid == expectedRight.UserUuid && x.Role.Uuid == expectedRight.RoleUuid);
            }
        }

        private IEnumerable<RoleAssignmentRequestDTO> CreateRoleAssignmentRequestDtOs(Guid roleUuid, Guid userUuid)
        {
            return new List<RoleAssignmentRequestDTO>
            {
                new()
                {
                    RoleUuid = roleUuid,
                    UserUuid = userUuid
                }
            };
        }

        private IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> CreateNewExternalReferenceDataWithOldUuid(IEnumerable<ExternalReferenceDataResponseDTO> createExternalReferences)
        {
            return createExternalReferences.Select(externalReference => new UpdateExternalReferenceDataWriteRequestDTO
            {
                Uuid = externalReference.Uuid,
                Title = A<string>(),
                DocumentId = A<string>(),
                Url = A<string>(),
                MasterReference = externalReference.MasterReference
            })
                .ToList();
        }

        private IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> CreateUpdateExternalReferenceDataWriteRequestDtOs()
        {
            return CreateExternalReferenceDataDtOs<UpdateExternalReferenceDataWriteRequestDTO>()
                .Select(
                    x =>
                    {
                        x.Uuid = null;
                        return x;
                    });
        }

        private IEnumerable<T> CreateExternalReferenceDataDtOs<T>() where T : ExternalReferenceDataWriteRequestDTO
        {
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            return Many<T>().Transform(WithRandomMaster).ToList();
        }

        private LocalKLEDeviationsRequestDTO CreateLocalKleDeviationsRequestDto(Guid[] addedUuids, Guid[] removedUuids)
        {
            return new LocalKLEDeviationsRequestDTO
            {
                AddedKLEUuids = addedUuids,
                RemovedKLEUuids = removedUuids
            };
        }

        private OrganizationUsageWriteRequestDTO CreateOrganizationUsageWriteRequestDto(Guid[] orgUnitsUuids, Guid responsibleUuid)
        {
            return new OrganizationUsageWriteRequestDTO()
            {
                UsingOrganizationUnitUuids = orgUnitsUuids,
                ResponsibleOrganizationUnitUuid = responsibleUuid
            };
        }

        private static IEnumerable<Maybe<(int lower, int? upper)>> GetValidIntervals()
        {
            yield return (0, 9);
            yield return (10, 49);
            yield return (50, 99);
            yield return (100, 499);
            yield return (500, null);
        }

        private GeneralDataWriteRequestDTO CreateGeneralDataWriteRequestDto(Guid dataClassificationUuid)
        {
            return new GeneralDataWriteRequestDTO
            {
                LocalCallName = A<string>(),
                LocalSystemId = A<string>(),
                SystemVersion = A<string>(),
                Notes = A<string>(),
                DataClassificationUuid = dataClassificationUuid,
                NumberOfExpectedUsers = GetValidIntervals()
                    .RandomItem()
                    .Select(interval => new ExpectedUsersIntervalDTO
                    {
                        LowerBound = interval.lower,
                        UpperBound = interval.upper
                    })
                    .GetValueOrDefault(),
                Validity = new ItSystemUsageValidityWriteRequestDTO
                {
                    LifeCycleStatus = A<LifeCycleStatusChoice?>(),
                    ValidFrom = DateTime.UtcNow.Date,
                    ValidTo = DateTime.UtcNow.Date.AddDays(Math.Abs(A<short>()))
                },
                IsBusinessCritical = A<YesNoDontKnowChoice?>(),
            };
        }

        private static void AssertGdpr(GDPRWriteRequestDTO gdprInput, GDPRRegistrationsResponseDTO gdprResponse)
        {
            Assert.NotNull(gdprResponse.DataSensitivityLevels);
            Assert.NotNull(gdprResponse.SensitivePersonData);
            Assert.NotNull(gdprResponse.RegisteredDataCategories);
            Assert.NotNull(gdprResponse.TechnicalPrecautionsApplied);
            Assert.NotNull(gdprResponse.SpecificPersonalData);
            Assert.Equal(gdprInput.ProcessingPurpose, gdprResponse.ProcessingPurpose);
            (gdprInput.DirectoryDocumentation ?? new SimpleLinkDTO()).ToExpectedObject().ShouldMatch(gdprResponse.DirectoryDocumentation);
            Assert.Equal((gdprInput.DataSensitivityLevels ?? new List<DataSensitivityLevelChoice>()).OrderBy(x => x), gdprResponse.DataSensitivityLevels.OrderBy(x => x));
            Assert.Equal((gdprInput.SensitivePersonDataUuids ?? new List<Guid>()).OrderBy(x => x), gdprResponse.SensitivePersonData.Select(x => x.Uuid).OrderBy(x => x));
            Assert.Equal((gdprInput.RegisteredDataCategoryUuids ?? new List<Guid>()).OrderBy(x => x), gdprResponse.RegisteredDataCategories.Select(x => x.Uuid).OrderBy(x => x));
            Assert.Equal(gdprInput.TechnicalPrecautionsInPlace, gdprResponse.TechnicalPrecautionsInPlace);
            Assert.Equal((gdprInput.TechnicalPrecautionsApplied ?? new List<TechnicalPrecautionChoice>()).OrderBy(x => x), gdprResponse.TechnicalPrecautionsApplied.OrderBy(x => x));
            (gdprInput.TechnicalPrecautionsDocumentation ?? new SimpleLinkDTO()).ToExpectedObject().ShouldMatch(gdprResponse.TechnicalPrecautionsDocumentation);
            Assert.Equal(gdprInput.UserSupervision, gdprResponse.UserSupervision);
            AssertTimestampEqual(gdprInput.UserSupervisionDate, gdprResponse.UserSupervisionDate);
            (gdprInput.UserSupervisionDocumentation ?? new SimpleLinkDTO()).ToExpectedObject().ShouldMatch(gdprResponse.UserSupervisionDocumentation);
            Assert.Equal(gdprInput.RiskAssessmentConducted, gdprResponse.RiskAssessmentConducted);
            AssertTimestampEqual(gdprInput.RiskAssessmentConductedDate, gdprResponse.RiskAssessmentConductedDate);
            Assert.Equal(gdprInput.RiskAssessmentResult, gdprResponse.RiskAssessmentResult);
            (gdprInput.RiskAssessmentDocumentation ?? new SimpleLinkDTO()).ToExpectedObject().ShouldMatch(gdprResponse.RiskAssessmentDocumentation);
            Assert.Equal(gdprInput.RiskAssessmentNotes, gdprResponse.RiskAssessmentNotes);
            AssertTimestampEqual(gdprInput.PlannedRiskAssessmentDate, gdprResponse.PlannedRiskAssessmentDate);
            Assert.Equal(gdprInput.DPIAConducted, gdprResponse.DPIAConducted);
            AssertTimestampEqual(gdprInput.DPIADate, gdprResponse.DPIADate);
            (gdprInput.DPIADocumentation ?? new SimpleLinkDTO()).ToExpectedObject().ShouldMatch(gdprResponse.DPIADocumentation);
            Assert.Equal(gdprInput.RetentionPeriodDefined, gdprResponse.RetentionPeriodDefined);
            AssertTimestampEqual(gdprInput.NextDataRetentionEvaluationDate, gdprResponse.NextDataRetentionEvaluationDate);
            Assert.Equal(gdprInput.DataRetentionEvaluationFrequencyInMonths ?? 0, gdprResponse.DataRetentionEvaluationFrequencyInMonths);
            Assert.Equal(gdprInput.SpecificPersonalData ?? new List<GDPRPersonalDataChoice>().OrderBy(x => x), gdprResponse.SpecificPersonalData.OrderBy(x => x));
            Assert.Equal(gdprInput.IsDataProcessingAgreementRequired, gdprResponse.IsDataProcessingAgreementRequired);
        }

        private static ItSystemUsageValidityWriteRequestDTO CreateValidityInput(DateTime baseDate, LifeCycleStatusChoice lifeCycleStatus)
        {
            return new ItSystemUsageValidityWriteRequestDTO
            {
                ValidFrom = baseDate.AddDays(-1).Date,
                ValidTo = baseDate.AddDays(1).Date,
                LifeCycleStatus = lifeCycleStatus
            };
        }

        private static void AssertValidity(ItSystemUsageValidityWriteRequestDTO validityInput, ItSystemUsageValidityResponseDTO? validityResponse, bool expectedDateValidity, bool expectedLifeCycleValidity)
        {
            Assert.Equal(validityResponse?.ValidFrom, validityInput.ValidFrom);
            Assert.Equal(validityResponse?.ValidTo, validityInput.ValidTo);
            Assert.Equal(validityResponse?.LifeCycleStatus, validityInput.LifeCycleStatus);
            Assert.Equal(expectedDateValidity, validityResponse?.ValidAccordingToValidityPeriod);
            Assert.Equal(expectedLifeCycleValidity, validityResponse?.ValidAccordingToLifeCycle);
        }

        private async Task<(string token, User user, ShallowOrganizationResponseDTO organization, ItSystemResponseDTO system)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUser(organization.Uuid);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();
            var system = await CreateItSystemAsync(organization.Uuid);
            return (token, user, organization, system);
        }

        private static void AssertArchivingParametersNotSet(ArchivingRegistrationsResponseDTO actual)
        {
            Assert.Null(actual.ArchiveDuty);
            Assert.Null(actual.Type);
            Assert.Null(actual.Location);
            Assert.Null(actual.TestLocation);
            Assert.Null(actual.Supplier);
            Assert.Null(actual.Active);
            Assert.Null(actual.FrequencyInMonths);
            Assert.Null(actual.DocumentBearing);
            Assert.Null(actual.Notes);

            Assert.Empty(actual.JournalPeriods);
        }

        private static void AssertArchivingParametersSet<T, TJournalPeriod>(T expected, ArchivingRegistrationsResponseDTO actual)
            where T : BaseArchivingWriteRequestDTO, IHasJournalPeriods<TJournalPeriod>
            where TJournalPeriod : JournalPeriodDTO
        {
            Assert.NotNull(actual.Type);
            Assert.NotNull(actual.Location);
            Assert.NotNull(actual.TestLocation);
            Assert.NotNull(actual.Supplier);
            Assert.Equal(expected.ArchiveDuty, actual.ArchiveDuty);
            Assert.Equal(expected.TypeUuid, actual.Type.Uuid);
            Assert.Equal(expected.LocationUuid, actual.Location.Uuid);
            Assert.Equal(expected.TestLocationUuid, actual.TestLocation.Uuid);
            Assert.Equal(expected.SupplierOrganizationUuid, actual.Supplier.Uuid);
            Assert.Equal(expected.Active, actual.Active);
            Assert.Equal(expected.FrequencyInMonths, actual.FrequencyInMonths);
            Assert.Equal(expected.DocumentBearing, actual.DocumentBearing);
            Assert.Equal(expected.Notes, actual.Notes);

            Assert.NotNull(expected.JournalPeriods);
            Assert.Equal(expected.JournalPeriods.Count(), actual.JournalPeriods.Count());
            var firstJournalPeriod = expected.JournalPeriods.First();
            var journalPeriodFromServer = Assert.Single(actual.JournalPeriods, x => x.ArchiveId == firstJournalPeriod.ArchiveId);
            Assert.Equal(firstJournalPeriod.Approved, journalPeriodFromServer.Approved);
            Assert.Equal(firstJournalPeriod.ArchiveId, journalPeriodFromServer.ArchiveId);
            AssertTimestampEqual(firstJournalPeriod.StartDate, journalPeriodFromServer.StartDate);
            AssertTimestampEqual(firstJournalPeriod.EndDate, journalPeriodFromServer.EndDate);
        }

        private async Task<ArchivingCreationRequestDTO> CreateArchivingCreationRequestDto(Guid organizationUuid)
        {

            var dto = new ArchivingCreationRequestDTO();
            await AssignArchivingProperties(organizationUuid, dto);
            dto.JournalPeriods = CreateNewJournalPeriods(3);
            return dto;
        }

        private IEnumerable<JournalPeriodDTO> CreateNewJournalPeriods(int count)
        {
            return new Fixture()
                .Build<JournalPeriodDTO>()
                .Without(x => x.EndDate)
                .Without(x => x.StartDate)
                .Do(x =>
                {
                    var startDate = A<DateTime>();
                    x.StartDate = startDate;
                    x.EndDate = startDate.AddDays(1);
                })
                .CreateMany(count);
        }

        private async Task<ArchivingCreationRequestDTO> CreateArchivingCreationRequestDto(Guid archiveTypeUuid, Guid archiveLocationUuid, Guid archiveTestLocationUuid, Guid organizationUuid)
        {
            var requestDto = await CreateArchivingCreationRequestDto(organizationUuid);
            UpdateArchivingChoices(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, requestDto);
            return requestDto;
        }

        private static void UpdateArchivingChoices(Guid archiveTypeUuid, Guid archiveLocationUuid, Guid archiveTestLocationUuid, BaseArchivingWriteRequestDTO requestDto)
        {
            requestDto.TypeUuid = archiveTypeUuid;
            requestDto.LocationUuid = archiveLocationUuid;
            requestDto.TestLocationUuid = archiveTestLocationUuid;
        }

        private async Task AssignArchivingProperties(Guid organizationUuid, BaseArchivingWriteRequestDTO dto)
        {
            var archiveType = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTypes, organizationUuid, 1, 0)).First();
            var archiveLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveLocations, organizationUuid, 1, 0)).First();
            var archiveTestLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTestLocations, organizationUuid, 1, 0)).First();
            dto.ArchiveDuty = A<ArchiveDutyChoice>();
            dto.TypeUuid = archiveType.Uuid;
            dto.LocationUuid = archiveLocation.Uuid;
            dto.TestLocationUuid = archiveTestLocation.Uuid;
            dto.SupplierOrganizationUuid = organizationUuid;
            dto.Active = A<bool>();
            dto.FrequencyInMonths = A<int>();
            dto.DocumentBearing = A<bool>();
            dto.Notes = A<string>();
        }

        private async Task<ArchivingUpdateRequestDTO> CreateArchivingUpdateRequestDto(Guid organizationUuid)
        {
            var dto = new ArchivingUpdateRequestDTO();
            await AssignArchivingProperties(organizationUuid, dto);
            dto.JournalPeriods = CreateJournalPeriodUpdates(3);
            return dto;
        }

        private IEnumerable<JournalPeriodUpdateRequestDTO> CreateJournalPeriodUpdates(int count)
        {
            return new Fixture()
                .Build<JournalPeriodUpdateRequestDTO>()
                .Without(x => x.Uuid)
                .Without(x => x.EndDate)
                .Without(x => x.StartDate)
                .Do(x =>
                {
                    var startDate = A<DateTime>();
                    x.StartDate = startDate;
                    x.EndDate = startDate.AddDays(1);
                })
                .CreateMany(count).ToList();
        }

        private async Task<ArchivingUpdateRequestDTO> CreateArchivingUpdateRequestDto(Guid archiveTypeUuid, Guid archiveLocationUuid, Guid archiveTestLocationUuid, Guid organizationUuid)
        {
            var dto = await CreateArchivingUpdateRequestDto(organizationUuid);
            UpdateArchivingChoices(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, dto);
            return dto;
        }

        private IEnumerable<T> WithRandomMaster<T>(IEnumerable<T> references) where T : ExternalReferenceDataWriteRequestDTO
        {
            var orderedRandomly = references.OrderBy(_ => A<int>()).ToList();
            orderedRandomly.First().MasterReference = true;
            foreach (var externalReferenceDataDto in orderedRandomly.Skip(1))
                externalReferenceDataDto.MasterReference = false;

            return orderedRandomly;
        }

        private static void AssertExternalReferenceResults(IReadOnlyCollection<UpdateExternalReferenceDataWriteRequestDTO> expected, ItSystemUsageResponseDTO actual, bool ignoreUuid = false)
        {
            Assert.Equal(expected.Count, actual.ExternalReferences.Count());

            expected.OrderBy(x => x.DocumentId).ToList().ToExpectedObject()
                .ShouldMatch(actual.ExternalReferences.OrderBy(x => x.DocumentId).Select(x =>
                    new UpdateExternalReferenceDataWriteRequestDTO
                    {
                        Uuid = ignoreUuid ? null : x.Uuid,
                        DocumentId = x.DocumentId,
                        MasterReference = x.MasterReference,
                        Title = x.Title,
                        Url = x.Url
                    }).ToList());
        }

        private static void AssertExternalReferenceResults(IReadOnlyCollection<ExternalReferenceDataWriteRequestDTO> expected, ItSystemUsageResponseDTO actual)
        {
            Assert.Equal(expected.Count, actual.ExternalReferences.Count());

            expected.OrderBy(x => x.DocumentId).ToList().ToExpectedObject()
                .ShouldMatch(actual.ExternalReferences.OrderBy(x => x.DocumentId).Select(x =>
                    new ExternalReferenceDataWriteRequestDTO
                    {
                        DocumentId = x.DocumentId,
                        MasterReference = x.MasterReference,
                        Title = x.Title,
                        Url = x.Url
                    }).ToList());
        }

        private static void AssertKleDeviation(bool withDeviation, IEnumerable<Guid>? expectedDeviation, IEnumerable<IdentityNamePairResponseDTO> actualDeviation)
        {
            if (withDeviation)
            {
                Assert.NotNull(expectedDeviation);
                Assert.Equal(expectedDeviation.OrderBy(uuid => uuid),
                    actualDeviation.Select(x => x.Uuid).OrderBy(uuid => uuid));
            }
            else
                Assert.Empty(actualDeviation);
        }

        private int _taskKeyIndex;
        private readonly Lock _taskKeyLock = new Lock();

        private string ReserveKey()
        {
            const string prefix = "V2:";
            lock (_taskKeyLock)
            {
                bool exists;
                string currentKey;
                do
                {
                    currentKey = $"{prefix}{_taskKeyIndex++}";
                    var matchKey = currentKey;
                    exists = DatabaseAccess.MapFromEntitySet<TaskRef, bool>(all => all.AsQueryable().Any(taskRef => taskRef.TaskKey == matchKey));
                } while (exists);

                return currentKey;
            }
        }

        private void AddTaskRefsInDatabase(IEnumerable<Guid> idsOfTaskRefsToAdd)
        {
            var uuidToKeys = idsOfTaskRefsToAdd.ToDictionary(id => id, _ => ReserveKey());

            var orgUnitId = DatabaseAccess.MapFromEntitySet<OrganizationUnit, int>(all => all.AsQueryable().First().Id);
            DatabaseAccess.MutateEntitySet<TaskRef>(all =>
            {
                var taskRefs = uuidToKeys
                    .Keys
                    .Select(uuid => new TaskRef
                    {
                        Uuid = uuid,
                        Description = A<string>(),
                        TaskKey = uuidToKeys[uuid],
                        OwnedByOrganizationUnitId = orgUnitId,
                        ObjectOwnerId = TestEnvironment.DefaultUserId,
                        LastChangedByUserId = TestEnvironment.DefaultUserId
                    })
                    .ToList();
                all.AddRange(taskRefs);
            });
        }

        private static async Task AssertOrganizationalUsage(string token, Guid systemUsageUuid, IEnumerable<OrganizationUnitResponseDTO> expectedUnits, OrganizationUnitResponseDTO? expectedResponsible)
        {
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, systemUsageUuid);
            var expectedOrgUnits = expectedUnits
                .Select(unitDto => new { unitDto.Uuid, unitDto.Name })
                .OrderBy(x => x.Uuid)
                .ToList();

            var actualOrgUnits = dto
                .OrganizationUsage
                .UsingOrganizationUnits
                .Select(x => new { x.Uuid, x.Name })
                .OrderBy(x => x.Uuid)
                .ToList();

            Assert.Equal(expectedOrgUnits, actualOrgUnits);
            if (expectedResponsible != null)
            {
                Assert.NotNull(dto.OrganizationUsage.ResponsibleOrganizationUnit);
                Assert.Equal(expectedResponsible.Uuid, dto.OrganizationUsage.ResponsibleOrganizationUnit.Uuid);
                Assert.Equal(expectedResponsible.Name, dto.OrganizationUsage.ResponsibleOrganizationUnit.Name);
            }
            else
            {
                Assert.Null(dto.OrganizationUsage.ResponsibleOrganizationUnit);
            }
        }

        private static void AssertSingleRight(ItSystemRole expectedRole, User expectedUser, IEnumerable<RoleAssignmentResponseDTO> rightList)
        {
            var actualRight = Assert.Single(rightList);
            Assert.Equal(expectedRole.Name, actualRight.Role.Name);
            Assert.Equal(expectedRole.Uuid, actualRight.Role.Uuid);
            Assert.Equal(expectedUser.Uuid, actualRight.User.Uuid);
            Assert.Equal(expectedUser.GetFullName(), actualRight.User.Name);
        }

        private static CreateItSystemUsageRequestDTO CreatePostRequest(
            Guid organizationId,
            Guid systemId,
            GeneralDataWriteRequestDTO? generalSection = null,
            OrganizationUsageWriteRequestDTO? organizationalUsageSection = null,
            LocalKLEDeviationsRequestDTO? kleDeviationsRequest = null,
            IEnumerable<ExternalReferenceDataWriteRequestDTO>? referenceDataDtos = null,
            IEnumerable<RoleAssignmentRequestDTO>? roles = null,
            GDPRWriteRequestDTO? gdpr = null,
            ArchivingCreationRequestDTO? archiving = null)
        {
            return new CreateItSystemUsageRequestDTO
            {
                OrganizationUuid = organizationId,
                SystemUuid = systemId,
                General = generalSection,
                OrganizationUsage = organizationalUsageSection,
                LocalKleDeviations = kleDeviationsRequest,
                ExternalReferences = referenceDataDtos?.ToList(),
                Roles = roles?.ToList(),
                GDPR = gdpr,
                Archiving = archiving
            };
        }

        private static UpdateItSystemUsageRequestDTO CreatePutRequest(
            GeneralDataWriteRequestDTO? generalSection = null,
            OrganizationUsageWriteRequestDTO? organizationalUsageSection = null,
            LocalKLEDeviationsRequestDTO? kleDeviationsRequest = null,
            IEnumerable<UpdateExternalReferenceDataWriteRequestDTO>? referenceDataDtos = null,
            IEnumerable<RoleAssignmentRequestDTO>? roles = null,
            GDPRWriteRequestDTO? gdpr = null,
            ArchivingUpdateRequestDTO? baseArchiving = null)
        {
            var generalUpdateRequest = new GeneralDataUpdateRequestDTO()
            {
                DataClassificationUuid = generalSection?.DataClassificationUuid,
                LocalCallName = generalSection?.LocalCallName,
                LocalSystemId = generalSection?.LocalSystemId,
                Notes = generalSection?.Notes,
                NumberOfExpectedUsers = generalSection?.NumberOfExpectedUsers,
                SystemVersion = generalSection?.SystemVersion,
                Validity = generalSection?.Validity
            };
            return new UpdateItSystemUsageRequestDTO
            {
                General = generalUpdateRequest,
                OrganizationUsage = organizationalUsageSection,
                LocalKleDeviations = kleDeviationsRequest,
                ExternalReferences = referenceDataDtos?.ToList(),
                Roles = roles?.ToList(),
                GDPR = gdpr,
                Archiving = baseArchiving
            };
        }

        private static void AssertExpectedUsageShallow(ItSystemUsageResponseDTO expectedContent, IEnumerable<ItSystemUsageResponseDTO> dtos)
        {
            var dto = Assert.Single(dtos, usage => usage.Uuid == expectedContent.Uuid);
            AssertExpectedUsageShallow(expectedContent, dto);
        }

        private static void AssertExpectedUsageShallow(ItSystemUsageResponseDTO expectedContent, IEnumerable<ItSystemUsageSearchResultResponseDTO> dtos)
        {
            var dto = Assert.Single(dtos, usage => usage.Uuid == expectedContent.Uuid);
            AssertExpectedUsageShallow(expectedContent, dto);
        }
        private static void AssertExpectedUsageShallow(ItSystemUsageResponseDTO expectedContent, ItSystemUsageSearchResultResponseDTO dto)
        {
            Assert.Equal(expectedContent.SystemContext.Uuid, dto.SystemContext.Uuid);
            Assert.Equal(expectedContent.SystemContext.Name, dto.SystemContext.Name);

            var system = DatabaseAccess.MapFromEntitySet<ItSystemUsage, Core.DomainModel.ItSystem.ItSystem>(repo =>
                repo.AsQueryable().First(x => x.Uuid == expectedContent.Uuid).ItSystem);
            Assert.Equal(system.Disabled, dto.SystemContext.Deactivated);
        }

        private static void AssertExpectedUsageShallow(ItSystemUsageResponseDTO expectedContent, ItSystemUsageResponseDTO dto)
        {
            Assert.NotNull(dto.OrganizationContext);
            Assert.NotNull(expectedContent.OrganizationContext);
            Assert.Equal(expectedContent.OrganizationContext.Uuid, dto.OrganizationContext.Uuid);
            Assert.Equal(expectedContent.OrganizationContext.Name, dto.OrganizationContext.Name);
            Assert.Equal(expectedContent.OrganizationContext.Cvr, dto.OrganizationContext.Cvr);
            Assert.Equal(expectedContent.SystemContext.Uuid, dto.SystemContext.Uuid);
            Assert.Equal(expectedContent.SystemContext.Name, dto.SystemContext.Name);
        }

        private async Task<ItSystemResponseDTO> CreateSystemAndGetAsync(Guid organizationUuid, AccessModifier accessModifier, string? name = null)
        {
            var systemName = name ?? CreateName();
            var createdSystem = await CreateItSystemAsync(organizationUuid, systemName, accessModifier.ToChoice());

            return createdSystem;
        }

        private async Task<(User user, string token)> CreateApiUser(Guid organizationUuid)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organizationUuid, true);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ByUuid(userAndGetToken.userUuid) ?? throw new InvalidOperationException("CreateApiUser - organizationUuid version - userAndGetToken cannot be empty"));
            return (user, userAndGetToken.token);
        }

        private async Task<(User user, string token)> CreateApiUser(ShallowOrganizationResponseDTO organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Uuid, true);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ByUuid(userAndGetToken.userUuid) ?? throw new InvalidOperationException("CreateApiUser - ShallowOrganizationResponseDTO version - userAndGetToken cannot be empty"));
            return (user, userAndGetToken.token);
        }

        private async Task<User> CreateUser(ShallowOrganizationResponseDTO organization)
        {
            var userResponse = await CreateUserAsync(organization.Uuid);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ByUuid(userResponse.Uuid) ?? throw new InvalidOperationException("CreateUser - userAndGetToken cannot be empty"));
            return user;
        }

        private string CreateName()
        {
            return $"{nameof(ItSystemUsageApiV2Test)}æøå{A<string>()}";
        }

        private static async Task CreateRelationAsync(ItSystemUsageResponseDTO fromUsage,
            ItSystemUsageResponseDTO toUsage, ItContractResponseDTO? contract = null)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            await ItSystemUsageV2Helper.PostRelationAsync(token.Token, fromUsage.Uuid, new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = toUsage.Uuid,
                AssociatedContractUuid = contract?.Uuid
            });
        }

        private IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> MapExternalReferenceDtosToUpdateDtos(
            IEnumerable<ExternalReferenceDataWriteRequestDTO> references)
        {
            return references.Select(MapExternalReferenceDtoToUpdateDto).ToList();
        }

        private UpdateExternalReferenceDataWriteRequestDTO MapExternalReferenceDtoToUpdateDto(
            ExternalReferenceDataWriteRequestDTO reference)
        {
            return new UpdateExternalReferenceDataWriteRequestDTO
            {
                DocumentId = reference.DocumentId,
                Title = reference.Title,
                MasterReference = reference.MasterReference,
                Url = reference.Url,
                Uuid = null
            };
        }

        private static bool MatchExpectedAssignment(RoleAssignmentResponseDTO actual, RoleAssignmentRequestDTO expected)
        {
            return actual.Role.Uuid == expected.RoleUuid && actual.User.Uuid == expected.UserUuid;
        }
    }
}
