using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.SystemRelations;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage;
using Presentation.Web.Models.API.V2.Internal.Response.Roles;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.SystemUsage.V2
{
    public class ItSystemUsageApiV2Test : WithAutoFixture
    {
        [Fact]
        public async Task Can_Get_All_ItSystemUsages()
        {
            //Arrange
            var (token, user, organization1, system1) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.Id, organization1.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            var system2UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization2.Id);
            var system3UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization2.Id);

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

            var system2 = await CreateSystemAsync(organization.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization.Id, AccessModifier.Public);

            var system1Usage = await ItSystemHelper.TakeIntoUseAsync(system1.Id, organization.Id);
            var system2Usage = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization.Id);
            var system3Usage = await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization.Id);

            var toBeDisabled = new[] { system1Usage, system2Usage, system3Usage }.RandomItem();
            using var disabled = await ItSystemHelper.SendSetDisabledRequestAsync(toBeDisabled.ItSystem.Id, true);
            toBeDisabled.ItSystem.Disabled = true; //line up expected result

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyInternalAsync(organization.Uuid)).ToList();

            //Assert
            Assert.Equal(3, dtos.Count);
            AssertExpectedUsageShallow(system1Usage, dtos);
            AssertExpectedUsageShallow(system2Usage, dtos);
            AssertExpectedUsageShallow(system3Usage, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Internal_With_Paging()
        {
            //Arrange
            var (_, _, organization1, system1) = await CreatePrerequisitesAsync();

            var system2 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system4 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);

            var system1Usage = await ItSystemHelper.TakeIntoUseAsync(system1.Id, organization1.Id);
            var system2Usage = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            var system3Usage = await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization1.Id);
            var system4Usage = await ItSystemHelper.TakeIntoUseAsync(system4.dbId, organization1.Id);

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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAsync(organization.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization.Id, AccessModifier.Public);

            var system1Usage = await ItSystemHelper.TakeIntoUseAsync(system1.Id, organization.Id);
            var system2Usage = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization.Id);
            var system3Usage = await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization.Id);

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

            var system2 = await CreateSystemAsync(organization.Id, AccessModifier.Public);
            var relationTargetSystem = await CreateSystemAsync(organization.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.Id, organization.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization.Id);
            var system4UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(relationTargetSystem.dbId, organization.Id);

            await CreateRelationAsync(system1UsageOrg1, system4UsageOrg1);
            await CreateRelationAsync(system2UsageOrg1, system4UsageOrg1);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyInternalAsync(organization.Uuid, relationToSystemUuidFilter: relationTargetSystem.uuid)).ToList();

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

            var system1 = await CreateSystemAsync(organization.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(organization.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization.Id, AccessModifier.Public);
            var relationTargetSystem = await CreateSystemAsync(organization.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization.Id);
            await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization.Id);
            var relationTargetUsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(relationTargetSystem.dbId, organization.Id);

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

            var system1 = await CreateSystemAsync(organization.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(organization.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization.Id, AccessModifier.Public);
            var relationTargetSystem = await CreateSystemAsync(organization.Id, AccessModifier.Public);

            var systemUsage1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization.Id);
            var systemUsage2 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization.Id);
            var systemUsage3 = await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization.Id);
            var relationTargetUsage = await ItSystemHelper.TakeIntoUseAsync(relationTargetSystem.dbId, organization.Id);

            var contract = await ItContractHelper.CreateContract(CreateName(), organization.Id);

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

            var system1 = await CreateSystemAsync(organization.Id, AccessModifier.Public, $"{content}ONE");
            var system2 = await CreateSystemAsync(organization.Id, AccessModifier.Public, $"TWO{content}");
            var system3 = await CreateSystemAsync(organization.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization.Id);
            await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization.Id);

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
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.Id, organization1.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            var system2UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization2.Id);
            var system3UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization2.Id);

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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAsync(organization.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization.Id, AccessModifier.Public);

            var system1Usage = await ItSystemHelper.TakeIntoUseAsync(system1.Id, organization.Id);
            var system2Usage = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization.Id);
            var system3Usage = await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization.Id);

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
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.Id, organization1.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization2.Id);

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
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            await ItSystemHelper.TakeIntoUseAsync(system1.Id, organization1.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            var system2UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization2.Id);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, systemUuidFilter: system2.uuid)).ToList();

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
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);
            var relationTargetSystem = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.Id, organization1.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            var system2UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization2.Id);
            var system3UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization2.Id);
            var system4UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(relationTargetSystem.dbId, organization1.Id);
            var system4UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(relationTargetSystem.dbId, organization2.Id);

            await CreateRelationAsync(system1UsageOrg1, system4UsageOrg1);
            await CreateRelationAsync(system2UsageOrg1, system4UsageOrg1);
            await CreateRelationAsync(system2UsageOrg2, system4UsageOrg2);
            await CreateRelationAsync(system4UsageOrg2, system3UsageOrg2);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, relationToSystemUuidFilter: relationTargetSystem.uuid)).ToList();

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
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system1 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);
            var relationTargetSystem = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization1.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            var system2UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization2.Id);
            var relationTargetUsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(relationTargetSystem.dbId, organization1.Id);
            var relationTargetUsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(relationTargetSystem.dbId, organization2.Id);

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
            var (token, user, organization1, _) = await CreatePrerequisitesAsync();

            var system1 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var relationTargetSystem = await CreateSystemAsync(organization1.Id, AccessModifier.Public);

            var systemUsage1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization1.Id);
            var systemUsage2 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            var systemUsage3 = await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization1.Id);
            var relationTargetUsage = await ItSystemHelper.TakeIntoUseAsync(relationTargetSystem.dbId, organization1.Id);

            var contract = await ItContractHelper.CreateContract(CreateName(), organization1.Id);

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
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system1 = await CreateSystemAsync(organization1.Id, AccessModifier.Public, $"{content}ONE");
            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public, $"TWO{content}");
            var system3 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization1.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization2.Id);

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
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());

            await HttpApi.SendAssignRoleToUserAsync(user.Id, userRole, organization2.Id).DisposeAsync();

            await ItSystemHelper.TakeIntoUseAsync(system.Id, organization1.Id);
            var systemUsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system.Id, organization2.Id);

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
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());

            await HttpApi.SendAssignRoleToUserAsync(user.Id, userRole, organization2.Id).DisposeAsync();

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
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());

            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            await ItSystemHelper.TakeIntoUseAsync(system.Id, organization1.Id);
            var systemUsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system.Id, organization2.Id);

            //Act
            var dto = (await ItSystemUsageV2Helper.GetSingleAsync(token, systemUsageOrg2.Uuid));

            //Assert - exhaustive content assertions are done in the read-after-write assertion tests (POST/PUT)
            AssertExpectedUsageShallow(systemUsageOrg2, dto);
        }

        [Fact]
        public async Task Cannot_Get_Unknown_ItSystemUsage()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();

            //Act
            using var response = (await ItSystemUsageV2Helper.SendGetSingleAsync(token, A<Guid>()));

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Get_ItSystemUsage_If_NotAllowedTo()
        {
            //Arrange
            var (token, user, organization1, system) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            await ItSystemHelper.TakeIntoUseAsync(system.Id, organization1.Id);
            var systemUsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system.Id, organization2.Id);

            //Act
            using var response = (await ItSystemUsageV2Helper.SendGetSingleAsync(token, systemUsageOrg2.Uuid));

            //Assert - exhaustive content assertions are done in the read-after-write assertion tests (POST/PUT)
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Can_POST_With_No_Additional_Data()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);

            //Act
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Assert
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
            var (token, user, organization, system) = await CreatePrerequisitesAsync();

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
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            await ItSystemHelper.SendSetDisabledRequestAsync(system.Id, true).WithExpectedResponseCode(HttpStatusCode.NoContent).DisposeAsync();

            //Act
            using var response = await ItSystemUsageV2Helper.SendPostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_POST_With_Full_General_Data_Section()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            var dataClassification = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageDataClassification, organization.Uuid, 1, 0)).First();
            var request = CreatePostRequest(organization.Uuid, system.Uuid, new GeneralDataWriteRequestDTO
            {
                LocalCallName = A<string>(),
                LocalSystemId = A<string>(),
                SystemVersion = A<string>(),
                Notes = A<string>(),
                DataClassificationUuid = dataClassification.Uuid,
                NumberOfExpectedUsers = new ExpectedUsersIntervalDTO { LowerBound = 10, UpperBound = 50 },
                Validity = new ItSystemUsageValidityWriteRequestDTO
                {
                    LifeCycleStatus = A<LifeCycleStatusChoice?>(),
                    ValidFrom = DateTime.UtcNow.Date,
                    ValidTo = DateTime.UtcNow.Date.AddDays(Math.Abs(A<short>()))
                },
                WebAccessibilityCompliance = A<YesNoPartiallyChoice>(),
                LastWebAccessibilityCheck = A<DateTime>(),
                WebAccessibilityNotes = A<string>()
            });

            //Act
            var createdDTO = await ItSystemUsageV2Helper.PostAsync(token, request);

            //Assert a fresh GET DTO
            var freshReadDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDTO.Uuid);
            Assert.Equal(request.General.LocalCallName, freshReadDTO.General.LocalCallName);
            Assert.Equal(request.General.LocalSystemId, freshReadDTO.General.LocalSystemId);
            Assert.Equal(request.General.SystemVersion, freshReadDTO.General.SystemVersion);
            Assert.Equal(request.General.Notes, freshReadDTO.General.Notes);
            Assert.Equal(request.General.NumberOfExpectedUsers.LowerBound, freshReadDTO.General.NumberOfExpectedUsers.LowerBound);
            Assert.Equal(request.General.NumberOfExpectedUsers.UpperBound, freshReadDTO.General.NumberOfExpectedUsers.UpperBound);
            Assert.Equal(request.General.Validity.LifeCycleStatus, freshReadDTO.General.Validity.LifeCycleStatus);
            Assert.Equal(request.General.Validity.ValidFrom.GetValueOrDefault().Date, freshReadDTO.General.Validity.ValidFrom.GetValueOrDefault().Date);
            Assert.Equal(request.General.Validity.ValidTo.GetValueOrDefault().Date, freshReadDTO.General.Validity.ValidTo.GetValueOrDefault().Date);
            Assert.Equal(dataClassification.Uuid, freshReadDTO.General.DataClassification.Uuid);
            Assert.Equal(dataClassification.Name, freshReadDTO.General.DataClassification.Name);
            Assert.Equal(request.General.WebAccessibilityCompliance, freshReadDTO.General.WebAccessibilityCompliance);
            Assert.Equal(request.General.LastWebAccessibilityCheck, freshReadDTO.General.LastWebAccessibilityCheck);
            Assert.Equal(request.General.WebAccessibilityNotes, freshReadDTO.General.WebAccessibilityNotes);
        }

        [Fact]
        public async Task Can_PATCH_MainContract()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var contract1 = await ItContractHelper.CreateContract(CreateName(), organization.Id);
            var contract2 = await ItContractHelper.CreateContract(CreateName(), organization.Id);
            var usageId = GetUsageIdByUuid(newUsage.Uuid);
            await ItContractHelper.AddItSystemUsage(contract1.Id, usageId, organization.Id);
            await ItContractHelper.AddItSystemUsage(contract2.Id, usageId, organization.Id);

            //Act
            using var response1 = await ItSystemUsageV2Helper.SendPatchGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO { MainContractUuid = contract1.Uuid }).WithExpectedResponseCode(HttpStatusCode.OK);
            contract1.Active = true;
            await ItContractHelper.PatchContract(contract1.Id, organization.Id, contract1);

            //Assert
            var freshReadDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            Assert.Equal(contract1.Uuid, freshReadDTO.General.MainContract.Uuid);
            Assert.Equal(contract1.Name, freshReadDTO.General.MainContract.Name);
            Assert.True(freshReadDTO.General.Validity.ValidAccordingToMainContract);

            //Act - set to another contract
            using var response2 = await ItSystemUsageV2Helper.SendPatchGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO { MainContractUuid = contract2.Uuid }).WithExpectedResponseCode(HttpStatusCode.OK);
            contract2.Terminated = DateTime.UtcNow.AddDays(-1);
            await ItContractHelper.PatchContract(contract2.Id, organization.Id, contract2);

            //Assert
            freshReadDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            Assert.Equal(contract2.Uuid, freshReadDTO.General.MainContract.Uuid);
            Assert.Equal(contract2.Name, freshReadDTO.General.MainContract.Name);
            Assert.False(freshReadDTO.General.Validity.ValidAccordingToMainContract);

            //Act - set to contract to null
            using var response3 = await ItSystemUsageV2Helper.SendPatchGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO { MainContractUuid = null }).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            freshReadDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            Assert.True(freshReadDTO.General.Validity.ValidAccordingToMainContract);
        }

        [Fact]
        public async Task Can_PATCH_Reset_MainContract()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var contract = await ItContractHelper.CreateContract(CreateName(), organization.Id);
            var usageId = GetUsageIdByUuid(newUsage.Uuid);
            await ItContractHelper.AddItSystemUsage(contract.Id, usageId, organization.Id);

            //Act
            using var response1 = await ItSystemUsageV2Helper.SendPatchGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO { MainContractUuid = contract.Uuid }).WithExpectedResponseCode(HttpStatusCode.OK);
            using var resetResponse = await ItSystemUsageV2Helper.SendPatchGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO()).WithExpectedResponseCode(HttpStatusCode.OK); //Reset main contract

            //Assert
            var freshReadDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            Assert.Null(freshReadDTO.General.MainContract);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_POST_With_OrganizationalUsage(bool withResponsible)
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            var unit1 = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, CreateName());
            var unit2 = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, CreateName(), unit1.Id);
            var unit3 = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, CreateName(), unit1.Id);

            var units = new[] { unit1, unit2, unit3 }.OrderBy(x => A<int>()).Take(2).ToList();
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
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            var unit1 = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, CreateName());
            var unit2 = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, CreateName(), unit1.Id);
            var unit3 = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, CreateName(), unit1.Id);

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
            await AssertOrganizationalUsage(token, newUsage.Uuid, Enumerable.Empty<OrgUnitDTO>(), null);

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
            var (token, user, organization, system) = await CreatePrerequisitesAsync();

            var additionalTaskRefs = Many<Guid>(2).ToList();
            var taskRefsOnSystem = Many<Guid>(3).ToList();
            var potentialRemovals = taskRefsOnSystem.Take(2).ToList();

            AddTaskRefsInDatabase(additionalTaskRefs.Concat(taskRefsOnSystem));
            var dbIdsOfSystemTaskRefs = DatabaseAccess.MapFromEntitySet<TaskRef, int[]>(all => all.AsQueryable().Where(s => taskRefsOnSystem.Contains(s.Uuid)).Select(x => x.Id).ToArray());

            foreach (var taskRefId in dbIdsOfSystemTaskRefs)
            {
                using var addTaskRefResponse = await ItSystemHelper.SendAddTaskRefRequestAsync(system.Id, taskRefId, organization.Id);
                Assert.Equal(HttpStatusCode.OK, addTaskRefResponse.StatusCode);
            }

            var request = CreatePostRequest(organization.Uuid, system.Uuid, kleDeviationsRequest: new LocalKLEDeviationsRequestDTO
            {
                AddedKLEUuids = withAdditions ? additionalTaskRefs : null,
                RemovedKLEUuids = withRemovals ? potentialRemovals : null
            });

            //Act
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, request);

            //Assert
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertKLEDeviation(withAdditions, additionalTaskRefs, dto.LocalKLEDeviations.AddedKLE);
            AssertKLEDeviation(withRemovals, potentialRemovals, dto.LocalKLEDeviations.RemovedKLE);
        }

        [Fact]
        public async Task Can_PATCH_KLE()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();

            var additionalTaskRefs = Many<Guid>(2).ToList();
            var taskRefsOnSystem = Many<Guid>(3).ToList();
            var potentialRemovals = taskRefsOnSystem.Take(2).ToList();

            AddTaskRefsInDatabase(additionalTaskRefs.Concat(taskRefsOnSystem));
            var dbIdsOfSystemTaskRefs = DatabaseAccess.MapFromEntitySet<TaskRef, int[]>(all => all.AsQueryable().Where(s => taskRefsOnSystem.Contains(s.Uuid)).Select(x => x.Id).ToArray());

            foreach (var taskRefId in dbIdsOfSystemTaskRefs)
            {
                using var addTaskRefResponse = await ItSystemHelper.SendAddTaskRefRequestAsync(system.Id, taskRefId, organization.Id);
                Assert.Equal(HttpStatusCode.OK, addTaskRefResponse.StatusCode);
            }
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Act - add one addition
            using var put1 = await ItSystemUsageV2Helper.SendPatchKle(token, newUsage.Uuid, new LocalKLEDeviationsRequestDTO() { AddedKLEUuids = additionalTaskRefs.Take(1) }).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertKLEDeviation(true, additionalTaskRefs.Take(1), dto.LocalKLEDeviations.AddedKLE);
            AssertKLEDeviation(false, null, dto.LocalKLEDeviations.RemovedKLE);

            //Act - add another one
            using var put2 = await ItSystemUsageV2Helper.SendPatchKle(token, newUsage.Uuid, new LocalKLEDeviationsRequestDTO() { AddedKLEUuids = additionalTaskRefs }).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertKLEDeviation(true, additionalTaskRefs, dto.LocalKLEDeviations.AddedKLE);
            AssertKLEDeviation(false, null, dto.LocalKLEDeviations.RemovedKLE);

            //Act - remove some
            using var put3 = await ItSystemUsageV2Helper.SendPatchKle(token, newUsage.Uuid, new LocalKLEDeviationsRequestDTO() { AddedKLEUuids = additionalTaskRefs, RemovedKLEUuids = potentialRemovals }).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertKLEDeviation(true, additionalTaskRefs, dto.LocalKLEDeviations.AddedKLE);
            AssertKLEDeviation(true, potentialRemovals, dto.LocalKLEDeviations.RemovedKLE);

            //Act - reset
            using var put4 = await ItSystemUsageV2Helper.SendPatchKle(token, newUsage.Uuid, new LocalKLEDeviationsRequestDTO()).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertKLEDeviation(false, null, dto.LocalKLEDeviations.AddedKLE);
            AssertKLEDeviation(false, null, dto.LocalKLEDeviations.RemovedKLE);
        }

        [Fact]
        public async Task Can_POST_With_ExternalReferences()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
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
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            var request = CreatePostRequest(organization.Uuid, system.Uuid);
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, request);

            var inputs1 = CreateUpdateExternalReferenceDataWriteRequestDTOs().ToList();

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
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);

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
            var createdDTO = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid, roles: roles));

            //Assert
            var freshReadDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDTO.Uuid);
            Assert.Equal(2, freshReadDTO.Roles.Count());
            AssertSingleRight(role, user1, freshReadDTO.Roles.Where(x => x.User.Uuid == user1.Uuid));
            AssertSingleRight(role, user2, freshReadDTO.Roles.Where(x => x.User.Uuid == user2.Uuid));
        }

        [Fact]
        public async Task Can_PATCH_Modify_Roles()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);

            var user1 = await CreateUser(organization);
            var user2 = await CreateUser(organization);
            var role = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().First(r => r.IsObligatory && r.IsEnabled));

            var createdDTO = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            var initialRoles = new List<RoleAssignmentRequestDTO> { new() { RoleUuid = role.Uuid, UserUuid = user1.Uuid } };
            var modifyRoles = new List<RoleAssignmentRequestDTO> { new() { RoleUuid = role.Uuid, UserUuid = user2.Uuid } };

            //Act - Add role
            using var addInitialRolesRequest = await ItSystemUsageV2Helper.SendPatchRoles(token, createdDTO.Uuid, initialRoles).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            var initialRoleResponse = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDTO.Uuid);
            AssertSingleRight(role, user1, initialRoleResponse.Roles);

            //Act - Modify role
            using var modifiedRequest = await ItSystemUsageV2Helper.SendPatchRoles(token, createdDTO.Uuid, modifyRoles).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            var modifiedRoleResponse = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDTO.Uuid);
            AssertSingleRight(role, user2, modifiedRoleResponse.Roles);

            //Act - Remove role
            using var removedRequest = await ItSystemUsageV2Helper.SendPatchRoles(token, createdDTO.Uuid, new List<RoleAssignmentRequestDTO>()).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            var removedRoleResponse = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDTO.Uuid);
            Assert.Empty(removedRoleResponse.Roles);
        }

        [Fact]
        public async Task Can_PATCH_Add_RoleAssignment()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);

            var user1 = await CreateUser(organization);
            var user2 = await CreateUser(organization);
            var role1 = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().Where(r => r.IsObligatory && r.IsEnabled).RandomItem());
            var role2 = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().Where(r => r.IsObligatory && r.IsEnabled).RandomItem());

            var createdDTO = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            var assignment1 = new RoleAssignmentRequestDTO { RoleUuid = role1.Uuid, UserUuid = user1.Uuid };
            var assignment2 = new RoleAssignmentRequestDTO { RoleUuid = role2.Uuid, UserUuid = user2.Uuid };

            //Act
            using var assignmentResponse1 = await ItSystemUsageV2Helper.SendPatchAddRoleAssignment(token, createdDTO.Uuid, assignment1);
            using var duplicateAssignment1 = await ItSystemUsageV2Helper.SendPatchAddRoleAssignment(token, createdDTO.Uuid, assignment1);
            using var assignmentResponse2 = await ItSystemUsageV2Helper.SendPatchAddRoleAssignment(token, createdDTO.Uuid, assignment2);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, duplicateAssignment1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, assignmentResponse1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, assignmentResponse2.StatusCode);
            var updatedDTO = await assignmentResponse2.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();
            var roles = updatedDTO.Roles.ToList();
            Assert.Equal(2, roles.Count);
            Assert.Contains(roles, r => MatchExpectedAssignment(r, assignment1));
            Assert.Contains(roles, r => MatchExpectedAssignment(r, assignment2));
        }

        [Fact]
        public async Task Can_PATCH_Remove_RoleAssignment()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);

            var user1 = await CreateUser(organization);
            var user2 = await CreateUser(organization);
            var role1 = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().Where(r => r.IsObligatory && r.IsEnabled).RandomItem());
            var role2 = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().Where(r => r.IsObligatory && r.IsEnabled).RandomItem());

            var createdDTO = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            var assignment2 = new RoleAssignmentRequestDTO { RoleUuid = role2.Uuid, UserUuid = user2.Uuid };
            var assignment1 = new RoleAssignmentRequestDTO { RoleUuid = role1.Uuid, UserUuid = user1.Uuid };

            //Act
            using var assignment1Response = await ItSystemUsageV2Helper.SendPatchAddRoleAssignment(token, createdDTO.Uuid, assignment1);
            using var assignment2Response = await ItSystemUsageV2Helper.SendPatchAddRoleAssignment(token, createdDTO.Uuid, assignment2);
            using var removeAssignment = await ItSystemUsageV2Helper.SendPatchRemoveRoleAssignment(token, createdDTO.Uuid, assignment1);
            using var duplicateRemoveAssignment = await ItSystemUsageV2Helper.SendPatchRemoveRoleAssignment(token, createdDTO.Uuid, assignment1);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, duplicateRemoveAssignment.StatusCode);
            Assert.Equal(HttpStatusCode.OK, removeAssignment.StatusCode);
            var updatedDTO = await removeAssignment.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();
            var roleAssignment = Assert.Single(updatedDTO.Roles);
            MatchExpectedAssignment(roleAssignment, assignment2);
        }

        [Fact]
        public async Task Can_POST_With_GDPR()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();

            var gdprInput = A<GDPRWriteRequestDTO>(); //Start with random values and then correct the ones where values matter
            gdprInput.DataSensitivityLevels = Many<DataSensitivityLevelChoice>().Distinct().ToList(); //Must be unique
            var registerTypes = await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageRegisterTypes, organization.Uuid, 10, 0);
            var sensitiveTypes = await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemSensitivePersonalDataTypes, organization.Uuid, 10, 0);

            gdprInput.SensitivePersonDataUuids = sensitiveTypes.Take(2).Select(x => x.Uuid).ToList();
            gdprInput.RegisteredDataCategoryUuids = registerTypes.Take(2).Select(x => x.Uuid).ToList();
            gdprInput.TechnicalPrecautionsApplied = Many<TechnicalPrecautionChoice>().Distinct().ToList(); //must be unique

            //Act
            var createdDTO = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid, gdpr: gdprInput));

            //Assert
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDTO.Uuid);
            var gdprResponse = dto.GDPR;
            AssertGDPR(gdprInput, gdprResponse);
        }

        [Fact]
        public async Task Can_PATCH_GDPR()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();

            var gdprVersion1 = await CreateGDPRInputAsync(organization);
            var gdprVersion2 = await CreateGDPRInputAsync(organization);
            var gdprVersion3 = new GDPRWriteRequestDTO();

            gdprVersion2.SensitivePersonDataUuids = gdprVersion2.SensitivePersonDataUuids.Take(1).ToList();
            gdprVersion2.RegisteredDataCategoryUuids = gdprVersion2.RegisteredDataCategoryUuids.Take(1).ToList();

            var usageDTO = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Act
            await ItSystemUsageV2Helper.SendPatchGDPR(token, usageDTO.Uuid, gdprVersion1)
                .WithExpectedResponseCode(HttpStatusCode.OK)
                .DisposeAsync();

            //Assert version 1
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, usageDTO.Uuid);
            var gdprResponse = dto.GDPR;
            AssertGDPR(gdprVersion1, gdprResponse);

            //Act
            await ItSystemUsageV2Helper.SendPatchGDPR(token, usageDTO.Uuid, gdprVersion2)
                .WithExpectedResponseCode(HttpStatusCode.OK)
                .DisposeAsync();

            //Assert version 2
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, usageDTO.Uuid);
            gdprResponse = dto.GDPR;
            AssertGDPR(gdprVersion2, gdprResponse);

            //Act - reset
            await ItSystemUsageV2Helper.SendPatchGDPR(token, usageDTO.Uuid, gdprVersion3)
                .WithExpectedResponseCode(HttpStatusCode.OK)
                .DisposeAsync();

            //Assert version 3 - properties should have been reset
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, usageDTO.Uuid);
            gdprResponse = dto.GDPR;
            AssertGDPR(gdprVersion3, gdprResponse);
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
            var generalResponse = dto.General.Validity;
            AssertValidity(validityVersion1, generalResponse, expectedDateValidity: true, expectedLifeCycleValidity: true);

            //Act
            await ItSystemUsageV2Helper.SendPatchValidity(token, usageDto.Uuid, validityVersion2)
                .WithExpectedResponseCode(HttpStatusCode.OK)
                .DisposeAsync();

            //Assert version 2
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, usageDto.Uuid);
            generalResponse = dto.General.Validity;
            AssertValidity(validityVersion2, generalResponse, expectedDateValidity: false, expectedLifeCycleValidity: false);

            //Act - reset
            await ItSystemUsageV2Helper.SendPatchValidity(token, usageDto.Uuid, validityVersion3)
                .WithExpectedResponseCode(HttpStatusCode.OK)
                .DisposeAsync();

            //Assert version 3 - properties should have been reset
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, usageDto.Uuid);
            generalResponse = dto.General.Validity;
            AssertValidity(validityVersion3, generalResponse, expectedDateValidity: true, expectedLifeCycleValidity: true);
        }

        [Fact]
        public async Task Can_POST_With_Archiving()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var archiveType = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTypes, organization.Uuid, 1, 0)).First();
            var archiveLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveLocations, organization.Uuid, 1, 0)).First();
            var archiveTestLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTestLocations, organization.Uuid, 1, 0)).First();

            var inputs = await CreateArchivingCreationRequestDTO(archiveType.Uuid, archiveLocation.Uuid, archiveTestLocation.Uuid, organization.Uuid);

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
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);

            var createRequest = CreatePostRequest(organization.Uuid, system.Uuid);
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, createRequest);

            var archiveType = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTypes, organization.Uuid, 1, 0)).First();
            var archiveLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveLocations, organization.Uuid, 1, 0)).First();
            var archiveTestLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTestLocations, organization.Uuid, 1, 0)).First();

            var inputs = await CreateArchivingUpdateRequestDTO(archiveType.Uuid, archiveLocation.Uuid, archiveTestLocation.Uuid, organization.Uuid);

            //Act - Add archiving data
            using var addedArchivingDataUsage = await ItSystemUsageV2Helper.SendPatchArchiving(token, newUsage.Uuid, inputs);

            //Assert 
            Assert.Equal(HttpStatusCode.OK, addedArchivingDataUsage.StatusCode);
            var addedDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertArchivingParametersSet<ArchivingUpdateRequestDTO, JournalPeriodUpdateRequestDTO>(inputs, addedDTO.Archiving);

            //Act - Update archiving data
            var updatedArchiveType = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTypes, organization.Uuid, 1, 1)).First();
            var updatedArchiveLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveLocations, organization.Uuid, 1, 1)).First();
            var updatedArchiveTestLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTestLocations, organization.Uuid, 1, 1)).First();
            var updatedInputs = await CreateArchivingUpdateRequestDTO(updatedArchiveType.Uuid, updatedArchiveLocation.Uuid, updatedArchiveTestLocation.Uuid, organization2.Uuid);

            using var updatedArchivingDataUsage = await ItSystemUsageV2Helper.SendPatchArchiving(token, newUsage.Uuid, updatedInputs);

            //Assert
            Assert.Equal(HttpStatusCode.OK, updatedArchivingDataUsage.StatusCode);
            var updatedDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertArchivingParametersSet<ArchivingUpdateRequestDTO, JournalPeriodUpdateRequestDTO>(updatedInputs, updatedDTO.Archiving);

            //Act - Remove archiving data
            using var removedArchivingDataUsage = await ItSystemUsageV2Helper.SendPatchArchiving(token, newUsage.Uuid, new ArchivingUpdateRequestDTO() { JournalPeriods = new List<JournalPeriodUpdateRequestDTO>() });

            //Assert 
            Assert.Equal(HttpStatusCode.OK, removedArchivingDataUsage.StatusCode);
            var removedDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertArchivingParametersNotSet(removedDTO.Archiving);
        }

        [Fact]
        public async Task Can_PATCH_With_Archiving_With_Specific_Journal_Periods()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            var usageDTO = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var initialJournalPeriodInputs = await CreateArchivingUpdateRequestDTO(organization.Uuid);
            using var firstArchiveResponse = await ItSystemUsageV2Helper.SendPatchArchiving(token, usageDTO.Uuid, initialJournalPeriodInputs);
            Assert.Equal(HttpStatusCode.OK, firstArchiveResponse.StatusCode);
            usageDTO = await firstArchiveResponse.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();
            var changedInputs = await CreateArchivingUpdateRequestDTO(organization.Uuid);
            //Set one of the journal periods to be an update of an existing
            var periodToChange = usageDTO.Archiving.JournalPeriods.RandomItem();
            var inputPeriodToChange = changedInputs.JournalPeriods.RandomItem();
            inputPeriodToChange.Uuid = periodToChange.Uuid;

            var journalPeriodUuidsBefore = usageDTO.Archiving.JournalPeriods.Select(x => x.Uuid).ToList();

            //Act
            using var secondArchivingResponse = await ItSystemUsageV2Helper.SendPatchArchiving(token, usageDTO.Uuid, changedInputs);

            //Assert
            Assert.Equal(HttpStatusCode.OK, secondArchivingResponse.StatusCode);
            usageDTO = await secondArchivingResponse.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();
            Assert.Contains(usageDTO.Archiving.JournalPeriods, x =>
                x.Uuid == inputPeriodToChange.Uuid.GetValueOrDefault() &&
                x.Approved == inputPeriodToChange.Approved &&
                x.StartDate == inputPeriodToChange.StartDate &&
                x.EndDate == inputPeriodToChange.EndDate &&
                x.ArchiveId == inputPeriodToChange.ArchiveId);
            Assert.Single(usageDTO.Archiving.JournalPeriods.Select(x => x.Uuid).Intersect(journalPeriodUuidsBefore));
        }

        [Fact]
        public async Task Can_Delete_ItSystemUsage()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            var usageDTO = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            using var getResult = await ItSystemUsageV2Helper.SendGetSingleAsync(token, usageDTO.Uuid);
            Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);

            //Act
            using var deleteResult = await ItSystemUsageV2Helper.SendDeleteAsync(token, usageDTO.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResult.StatusCode);
            using var notGetResult = await ItSystemUsageV2Helper.SendGetSingleAsync(token, usageDTO.Uuid);
            Assert.Equal(HttpStatusCode.NotFound, notGetResult.StatusCode);
        }

        [Fact]
        public async Task Can_Delete_ItSystemUsage_Fails_If_System_Not_Exists()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();

            //Act
            using var deleteResult = await ItSystemUsageV2Helper.SendDeleteAsync(token, A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, deleteResult.StatusCode);
        }

        [Fact]
        public async Task Can_Delete_ItSystemUsage_Fails_If_Not_Allowed_To_Delete()
        {
            //Arrange
            var (token1, user1, organization1, system1) = await CreatePrerequisitesAsync();
            var (token2, user2, organization2, system2) = await CreatePrerequisitesAsync();

            var usageDTO = await ItSystemUsageV2Helper.PostAsync(token1, CreatePostRequest(organization1.Uuid, system1.Uuid));
            using var getResult = await ItSystemUsageV2Helper.SendGetSingleAsync(token1, usageDTO.Uuid);
            Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);

            //Act
            using var deleteResult = await ItSystemUsageV2Helper.SendDeleteAsync(token2, usageDTO.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, deleteResult.StatusCode);
            using var getStillExistsResult = await ItSystemUsageV2Helper.SendGetSingleAsync(token1, usageDTO.Uuid);
            Assert.Equal(HttpStatusCode.OK, getStillExistsResult.StatusCode);
        }

        [Fact]
        public async Task Can_POST_With_All_Data()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            var (generalData, orgUnit, organizationUsageData, addedTaskRefs, removedTaskRefs, kleDeviations, externalReferences, roles, gdpr, archiving) = await CreateFullDataRequestDTO(organization, system);

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

            await AssertOrganizationalUsage(token, createdUsage.Uuid, new OrgUnitDTO[] { orgUnit }, orgUnit);

            AssertKLEDeviation(true, addedTaskRefs, createdUsage.LocalKLEDeviations.AddedKLE);
            AssertKLEDeviation(true, removedTaskRefs, createdUsage.LocalKLEDeviations.RemovedKLE);

            AssertExternalReferenceResults(request.ExternalReferences.ToList(), createdUsage);

            AssertRoles(request.Roles, createdUsage.Roles);

            AssertGDPR(request.GDPR, createdUsage.GDPR);

            AssertArchivingParametersSet<ArchivingCreationRequestDTO, JournalPeriodDTO>(request.Archiving, createdUsage.Archiving);
        }

        [Fact]
        public async Task Can_PUT_With_All_Data()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            var (generalData1, orgUnit1, organizationUsageData1, addedTaskRefs1, removedTaskRefs1, kleDeviations1, externalReferences1, roles1, gdpr1, archiving1) = await CreateUpdateFullDataRequestDTO(organization, system);
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

            AssertKLEDeviation(true, addedTaskRefs1, updatedUsage1.LocalKLEDeviations.AddedKLE);
            AssertKLEDeviation(true, removedTaskRefs1, updatedUsage1.LocalKLEDeviations.RemovedKLE);

            AssertExternalReferenceResults(updateRequest1.ExternalReferences.ToList(), updatedUsage1, true);

            AssertRoles(updateRequest1.Roles, updatedUsage1.Roles);

            AssertGDPR(updateRequest1.GDPR, updatedUsage1.GDPR);

            AssertArchivingParametersSet<ArchivingUpdateRequestDTO, JournalPeriodUpdateRequestDTO>(updateRequest1.Archiving, updatedUsage1.Archiving);

            //Act - PUT on filled system usage
            var (generalData2, orgUnit2, organizationUsageData2, addedTaskRefs2, removedTaskRefs2, kleDeviations2,
                externalReferences2, roles2, gdpr2, archiving2) = await CreateUpdateFullDataRequestDTO(organization, system, updatedUsage1.ExternalReferences);
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

            await AssertOrganizationalUsage(token, updatedUsage2.Uuid, new OrgUnitDTO[] { orgUnit2 }, orgUnit2);

            AssertKLEDeviation(true, addedTaskRefs2, updatedUsage2.LocalKLEDeviations.AddedKLE);
            AssertKLEDeviation(true, removedTaskRefs2, updatedUsage2.LocalKLEDeviations.RemovedKLE);

            AssertExternalReferenceResults(updateRequest2.ExternalReferences.ToList(), updatedUsage2);

            AssertRoles(updateRequest2.Roles, updatedUsage2.Roles);

            AssertGDPR(updateRequest2.GDPR, updatedUsage2.GDPR);

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

            await AssertOrganizationalUsage(token, updatedUsage3.Uuid, new OrgUnitDTO[] { }, null);

            Assert.Empty(updatedUsage3.LocalKLEDeviations.AddedKLE);
            Assert.Empty(updatedUsage3.LocalKLEDeviations.RemovedKLE);

            Assert.Empty(updatedUsage3.ExternalReferences);

            Assert.Empty(updatedUsage3.Roles);

            AssertGDPR(updateRequest3.GDPR, updatedUsage3.GDPR);

            AssertArchivingParametersNotSet(updatedUsage3.Archiving);
        }

        [Fact]
        public async Task Can_GET_SystemUsageRelation()
        {
            //Arrange
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var (interfaceUuid, interfaceName) = await CreateExhibitingInterface(organization.Id, system2.Id);
            var contract = await ItContractHelper.CreateContract(CreateName(), organization.Id);
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
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();

            //Act
            using var getResult = await ItSystemUsageV2Helper.SendGetRelationAsync(token, A<Guid>(), A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, getResult.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_SystemUsageRelation_If_Not_Allowed()
        {
            //Arrange
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var (interfaceUuid, interfaceName) = await CreateExhibitingInterface(organization.Id, system2.Id);
            var contract = await ItContractHelper.CreateContract(CreateName(), organization.Id);
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var (interfaceUuid, interfaceName) = await CreateExhibitingInterface(organization.Id, system2.Id);
            var contract = await ItContractHelper.CreateContract(CreateName(), organization.Id);
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var input = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage2.Uuid
            };

            //Act
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, input);

            //Assert
            Assert.Equal(usage2.Uuid, createdRelation.ToSystemUsage.Uuid);
        }

        [Fact]
        public async Task Can_POST_SystemUsageRelation_With_All_Data_Set()
        {
            //Arrange
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var (interfaceUuid, interfaceName) = await CreateExhibitingInterface(organization.Id, system2.Id);
            var contract = await ItContractHelper.CreateContract(CreateName(), organization.Id);
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var (interfaceUuid, interfaceName) = await CreateExhibitingInterface(organization.Id, system2.Id);
            var contract = await ItContractHelper.CreateContract(CreateName(), organization.Id);
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            // Create relation
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, new SystemRelationWriteRequestDTO { ToSystemUsageUuid = usage2.Uuid });
            Assert.Equal(usage2.Uuid, createdRelation.ToSystemUsage.Uuid);

            // Starting from 3 as we have 3 systems
            var system3 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage3 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system3.Uuid));

            var (interfaceUuid3, interfaceName3) = await CreateExhibitingInterface(organization.Id, system3.Id);
            var contract3 = await ItContractHelper.CreateContract(CreateName(), organization.Id);
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
            var system4 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage4 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system4.Uuid));

            var (interfaceUuid4, interfaceName4) = await CreateExhibitingInterface(organization.Id, system4.Id);
            var contract4 = await ItContractHelper.CreateContract(CreateName(), organization.Id);
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
            var system5 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage5 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system5.Uuid));

            var updateInput5 = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage5.Uuid
            };

            var updatedRelation5 = await ItSystemUsageV2Helper.PutRelationAsync(token, usage1.Uuid, createdRelation.Uuid, updateInput5);

            //Assert - Update to empty
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            // Create relation
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, new SystemRelationWriteRequestDTO { ToSystemUsageUuid = usage2.Uuid });
            Assert.Equal(usage2.Uuid, createdRelation.ToSystemUsage.Uuid);


            var system3 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));

            // Starting from 3 as we have 3 systems
            var system3 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var createInput = new SystemRelationWriteRequestDTO
            {
                ToSystemUsageUuid = usage2.Uuid
            };

            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, createInput);

            var system3 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            // Create relation
            var createdRelation = await ItSystemUsageV2Helper.PostRelationAsync(token, usage1.Uuid, new SystemRelationWriteRequestDTO { ToSystemUsageUuid = usage2.Uuid });
            Assert.Equal(usage2.Uuid, createdRelation.ToSystemUsage.Uuid);

            // Starting from 3 as we have 3 systems
            var system3 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage3 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system3.Uuid));

            var (interfaceUuid3, interfaceName3) = await CreateExhibitingInterface(organization.Id, system3.Id);
            var contract3 = await ItContractHelper.CreateContract(CreateName(), organization.Id);
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var usage1 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system1.Uuid));
            var usage2 = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system2.Uuid));

            var (interfaceUuid, interfaceName) = await CreateExhibitingInterface(organization.Id, system2.Id);
            var contract = await ItContractHelper.CreateContract(CreateName(), organization.Id);
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
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
            var (token, user, organization, system1) = await CreatePrerequisitesAsync();
            var system2 = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
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
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            var usageDTO = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var journalPeriods = CreateNewJournalPeriods(3);
            var newJournalPeriodInput = journalPeriods.RandomItem();

            //Act
            var postResult = await ItSystemUsageV2Helper.CreateJournalPeriodAsync(token, usageDTO.Uuid, newJournalPeriodInput);
            var getResult = await ItSystemUsageV2Helper.GetJournalPeriodAsync(token, usageDTO.Uuid, postResult.Uuid);

            //Assert
            AssertJournalPeriod(newJournalPeriodInput, postResult);
            Assert.Equivalent(postResult, getResult);

        }

        [Fact]
        public async Task Can_PUT_Journal_Period()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            var usageDTO = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var journalPeriods = CreateNewJournalPeriods(3);
            var newJournalPeriodInputs = journalPeriods.RandomItems(2);
            var initial = newJournalPeriodInputs.First();
            var updated = newJournalPeriodInputs.Last();
            var postResult = await ItSystemUsageV2Helper.CreateJournalPeriodAsync(token, usageDTO.Uuid, initial);

            //Act
            var putResult = await ItSystemUsageV2Helper.UpdateJournalPeriodAsync(token, usageDTO.Uuid, postResult.Uuid, updated);
            var getResult = await ItSystemUsageV2Helper.GetJournalPeriodAsync(token, usageDTO.Uuid, postResult.Uuid);

            //Assert
            AssertJournalPeriod(updated, putResult);
            Assert.Equivalent(putResult, getResult);
        }

        [Fact]
        public async Task Can_DELETE_Journal_Period()
        {
            //Arrange
            var (token, user, organization, system) = await CreatePrerequisitesAsync();
            var usageDTO = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var journalPeriods = CreateNewJournalPeriods(3);
            var newJournalPeriodInputs = journalPeriods.RandomItems(2);
            var initial = newJournalPeriodInputs.First();
            var postResult = await ItSystemUsageV2Helper.CreateJournalPeriodAsync(token, usageDTO.Uuid, initial);

            //Act
            using var deleteResult = await ItSystemUsageV2Helper.SendDeleteJournalPeriodAsync(token, usageDTO.Uuid, postResult.Uuid);
            using var getAfterDeleteResult = await ItSystemUsageV2Helper.SendGetJournalPeriodAsync(token, usageDTO.Uuid, postResult.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResult.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResult.StatusCode);
        }

        [Fact]
        public async Task Can_GET_Roles_From_Internal_Endpoint()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);

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
            var createdDTO = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid, roles: roles));

            //Act
            var assignedRoles = (await ItSystemUsageV2Helper.GetRoleAssignmentsInternalAsync(createdDTO.Uuid)).ToList();

            //Assert
            Assert.Equal(2, assignedRoles.Count);
            Assert.Contains(assignedRoles, assignment => MatchExpectedAssignment(assignment, role1, user1));
            Assert.Contains(assignedRoles, assignment => MatchExpectedAssignment(assignment, role2, user2));
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
            Assert.Equal(newJournalPeriodInput.StartDate, result.StartDate);
            Assert.Equal(newJournalPeriodInput.EndDate, result.EndDate);
            Assert.Equal(newJournalPeriodInput.ArchiveId, result.ArchiveId);
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
            var otherOrganization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (_, token) = await CreateApiUser(otherOrganization);
            return token;
        }

        private static void AssertRelation(SystemRelationWriteRequestDTO expected, string expectedInterfaceName, string expectedContractName, string expectedFrequencyName, OutgoingSystemRelationResponseDTO actual)
        {
            Assert.Equal(expected.ToSystemUsageUuid, actual.ToSystemUsage.Uuid);
            AssertBaseRelation(expected, expectedInterfaceName, expectedContractName, expectedFrequencyName, actual);
        }

        private static void AssertIncomingRelation(SystemRelationWriteRequestDTO expected, Guid expectedFromSystemUsageUuid, string expectedInterfaceName, string expectedContractName, string expectedFrequencyName, IncomingSystemRelationResponseDTO actual)
        {
            Assert.Equal(expectedFromSystemUsageUuid, actual.FromSystemUsage.Uuid);
            AssertBaseRelation(expected, expectedInterfaceName, expectedContractName, expectedFrequencyName, actual);
        }

        private static void AssertBaseRelation(SystemRelationWriteRequestDTO expected, string expectedInterfaceName,
            string expectedContractName, string expectedFrequencyName, BaseSystemRelationResponseDTO actual)
        {
            Assert.Equal(expected.RelationInterfaceUuid, actual.RelationInterface.Uuid);
            Assert.Equal(expectedInterfaceName, actual.RelationInterface.Name);
            Assert.Equal(expected.AssociatedContractUuid, actual.AssociatedContract.Uuid);
            Assert.Equal(expectedContractName, actual.AssociatedContract.Name);
            Assert.Equal(expected.RelationFrequencyUuid, actual.RelationFrequency.Uuid);
            Assert.Equal(expectedFrequencyName, actual.RelationFrequency.Name);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.UrlReference, actual.UrlReference);
        }

        private async Task<(Guid, string)> CreateExhibitingInterface(int orgId, int systemId)
        {
            var targetInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(CreateName(), CreateName(), orgId, AccessModifier.Public));
            await InterfaceExhibitHelper.CreateExhibit(systemId, targetInterface.Id);
            return (targetInterface.Uuid, targetInterface.Name);
        }

        private async Task<(GeneralDataWriteRequestDTO,
            OrgUnitDTO,
            OrganizationUsageWriteRequestDTO,
            Guid[],
            Guid[],
            LocalKLEDeviationsRequestDTO,
            IEnumerable<UpdateExternalReferenceDataWriteRequestDTO>,
            IEnumerable<RoleAssignmentRequestDTO>,
            GDPRWriteRequestDTO,
            ArchivingUpdateRequestDTO)> CreateUpdateFullDataRequestDTO(OrganizationDTO organization, ItSystemDTO system, IEnumerable<ExternalReferenceDataResponseDTO> existingExternalReferences = null)
        {
            var fullData = await CreateFullDataRequestDTO(organization, system);
            var archiving = await CreateArchivingUpdateRequestDTO(organization.Uuid);
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
            OrgUnitDTO unit1,
            OrganizationUsageWriteRequestDTO organizationUsageData,
            Guid[] addedTaskRefs,
            Guid[] removedTaskRefs,
            LocalKLEDeviationsRequestDTO kleDeviations,
            IEnumerable<ExternalReferenceDataWriteRequestDTO> externalReferences,
            IEnumerable<RoleAssignmentRequestDTO> roles,
            GDPRWriteRequestDTO gdpr,
            ArchivingCreationRequestDTO archiving)> CreateFullDataRequestDTO(OrganizationDTO organization, ItSystemDTO system)
        {
            var dataClassification = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageDataClassification, organization.Uuid, 1, 0)).First();
            var generalData = CreateGeneralDataWriteRequestDTO(dataClassification.Uuid);

            var unit1 = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, CreateName());
            var organizationUsageData = CreateOrganizationUsageWriteRequestDTO(new Guid[] { unit1.Uuid }, unit1.Uuid);

            var addedTaskRefs = Many<Guid>(2).ToArray();
            var taskRefsOnSystem = Many<Guid>(3).ToArray();
            var removedTaskRefs = taskRefsOnSystem.Take(2).ToArray();
            AddTaskRefsInDatabase(addedTaskRefs.Concat(taskRefsOnSystem));
            var dbIdsOfSystemTaskRefs = DatabaseAccess.MapFromEntitySet<TaskRef, int[]>(all => all.AsQueryable().Where(s => taskRefsOnSystem.Contains(s.Uuid)).Select(x => x.Id).ToArray());
            foreach (var taskRefId in dbIdsOfSystemTaskRefs)
            {
                using var addTaskRefResponse = await ItSystemHelper.SendAddTaskRefRequestAsync(system.Id, taskRefId, organization.Id);
                Assert.Equal(HttpStatusCode.OK, addTaskRefResponse.StatusCode);
            }
            var kleDeviations = CreateLocalKLEDeviationsRequestDTO(addedTaskRefs, removedTaskRefs);

            var externalReferences = CreateExternalReferenceDataDTOs<ExternalReferenceDataWriteRequestDTO>();

            var userToGainRole = await CreateUser(organization);
            var role = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().First(r => r.IsObligatory && r.IsEnabled));
            var roles = CreateRoleAssignmentRequestDTOs(role.Uuid, userToGainRole.Uuid);

            var gdpr = await CreateGDPRInputAsync(organization);

            var archiving = await CreateArchivingCreationRequestDTO(organization.Uuid);

            return (generalData, unit1, organizationUsageData, addedTaskRefs, removedTaskRefs, kleDeviations, externalReferences, roles, gdpr, archiving);
        }


        private static void AssertGeneralData(GeneralDataWriteRequestDTO expected, GeneralDataResponseDTO actual, bool hasData = true)
        {
            Assert.Equal(expected.LocalCallName, actual.LocalCallName);
            Assert.Equal(expected.LocalSystemId, actual.LocalSystemId);
            Assert.Equal(expected.SystemVersion, actual.SystemVersion);
            Assert.Equal(expected.Notes, actual.Notes);
            if (hasData)
            {
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

                Assert.Null(actual.Validity.LifeCycleStatus);
                Assert.Null(actual.Validity.ValidFrom);
                Assert.Null(actual.Validity.ValidTo);

                Assert.Null(actual.DataClassification);
            }
        }

        private void AssertRoles(IEnumerable<RoleAssignmentRequestDTO> expected, IEnumerable<RoleAssignmentResponseDTO> actual)
        {
            Assert.Equal(expected.Count(), actual.Count());
            foreach (var expectedRight in expected)
            {
                Assert.Single(actual.Where(x => x.User.Uuid == expectedRight.UserUuid && x.Role.Uuid == expectedRight.RoleUuid));
            }
        }

        private IEnumerable<RoleAssignmentRequestDTO> CreateRoleAssignmentRequestDTOs(Guid roleUuid, Guid userUuid)
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

        private IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> CreateUpdateExternalReferenceDataWriteRequestDTOs()
        {
            return CreateExternalReferenceDataDTOs<UpdateExternalReferenceDataWriteRequestDTO>()
                .Select(
                    x =>
                    {
                        x.Uuid = null;
                        return x;
                    });
        }

        private IEnumerable<T> CreateExternalReferenceDataDTOs<T>() where T : ExternalReferenceDataWriteRequestDTO
        {
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            return Many<T>().Transform(WithRandomMaster).ToList();
        }

        private LocalKLEDeviationsRequestDTO CreateLocalKLEDeviationsRequestDTO(Guid[] addedUuids, Guid[] removedUuids)
        {
            return new LocalKLEDeviationsRequestDTO
            {
                AddedKLEUuids = addedUuids,
                RemovedKLEUuids = removedUuids
            };
        }

        private OrganizationUsageWriteRequestDTO CreateOrganizationUsageWriteRequestDTO(Guid[] orgUnitsUuids, Guid ResponsibleUuid)
        {
            return new OrganizationUsageWriteRequestDTO()
            {
                UsingOrganizationUnitUuids = orgUnitsUuids,
                ResponsibleOrganizationUnitUuid = ResponsibleUuid
            };
        }

        private static IEnumerable<Maybe<(int lower, int? upper)>> GetValidIntervals()
        {
            yield return (0, 9);
            yield return (10, 50);
            yield return (50, 100);
            yield return (100, null);
        }

        private GeneralDataWriteRequestDTO CreateGeneralDataWriteRequestDTO(Guid dataClassificationUuid)
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
            };
        }

        private async Task<GDPRWriteRequestDTO> CreateGDPRInputAsync(OrganizationDTO organization)
        {
            var registerTypes =
                await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageRegisterTypes,
                    organization.Uuid, 10, 0);
            var sensitiveTypes =
                await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemSensitivePersonalDataTypes,
                    organization.Uuid, 10, 0);

            var gdprInput = A<GDPRWriteRequestDTO>(); //Start with random values and then correct the ones where values matter
            var sensitivityLevels = Many<DataSensitivityLevelChoice>().Distinct().ToList();
            if (sensitivityLevels.Contains(DataSensitivityLevelChoice.PersonData) == false)
                sensitivityLevels.Add(DataSensitivityLevelChoice.PersonData);

            gdprInput.DataSensitivityLevels = sensitivityLevels; //Must be unique
            gdprInput.SpecificPersonalData = Many<GDPRPersonalDataChoice>().Distinct().ToList();
            gdprInput.SensitivePersonDataUuids = sensitiveTypes.Take(2).Select(x => x.Uuid).ToList();
            gdprInput.RegisteredDataCategoryUuids = registerTypes.Take(2).Select(x => x.Uuid).ToList();
            gdprInput.TechnicalPrecautionsApplied = Many<TechnicalPrecautionChoice>().Distinct().ToList(); //must be unique
            gdprInput.PlannedRiskAssessmentDate = A<DateTime>();
            return gdprInput;
        }

        private static void AssertGDPR(GDPRWriteRequestDTO gdprInput, GDPRRegistrationsResponseDTO gdprResponse)
        {
            Assert.Equal(gdprInput.Purpose, gdprResponse.Purpose);
            Assert.Equal(gdprInput.BusinessCritical, gdprResponse.BusinessCritical);
            Assert.Equal(gdprInput.HostedAt, gdprResponse.HostedAt);
            (gdprInput.DirectoryDocumentation ?? new SimpleLinkDTO()).ToExpectedObject().ShouldMatch(gdprResponse.DirectoryDocumentation);
            Assert.Equal((gdprInput.DataSensitivityLevels ?? new List<DataSensitivityLevelChoice>()).OrderBy(x => x), gdprResponse.DataSensitivityLevels.OrderBy(x => x));
            Assert.Equal((gdprInput.SensitivePersonDataUuids ?? new List<Guid>()).OrderBy(x => x), gdprResponse.SensitivePersonData.Select(x => x.Uuid).OrderBy(x => x));
            Assert.Equal((gdprInput.RegisteredDataCategoryUuids ?? new List<Guid>()).OrderBy(x => x), gdprResponse.RegisteredDataCategories.Select(x => x.Uuid).OrderBy(x => x));
            Assert.Equal(gdprInput.TechnicalPrecautionsInPlace, gdprResponse.TechnicalPrecautionsInPlace);
            Assert.Equal((gdprInput.TechnicalPrecautionsApplied ?? new List<TechnicalPrecautionChoice>()).OrderBy(x => x), gdprResponse.TechnicalPrecautionsApplied.OrderBy(x => x));
            (gdprInput.TechnicalPrecautionsDocumentation ?? new SimpleLinkDTO()).ToExpectedObject().ShouldMatch(gdprResponse.TechnicalPrecautionsDocumentation);
            Assert.Equal(gdprInput.UserSupervision, gdprResponse.UserSupervision);
            Assert.Equal(gdprInput.UserSupervisionDate, gdprResponse.UserSupervisionDate);
            (gdprInput.UserSupervisionDocumentation ?? new SimpleLinkDTO()).ToExpectedObject().ShouldMatch(gdprResponse.UserSupervisionDocumentation);
            Assert.Equal(gdprInput.RiskAssessmentConducted, gdprResponse.RiskAssessmentConducted);
            Assert.Equal(gdprInput.RiskAssessmentConductedDate, gdprResponse.RiskAssessmentConductedDate);
            Assert.Equal(gdprInput.RiskAssessmentResult, gdprResponse.RiskAssessmentResult);
            (gdprInput.RiskAssessmentDocumentation ?? new SimpleLinkDTO()).ToExpectedObject().ShouldMatch(gdprResponse.RiskAssessmentDocumentation);
            Assert.Equal(gdprInput.RiskAssessmentNotes, gdprResponse.RiskAssessmentNotes);
            Assert.Equal(gdprInput.PlannedRiskAssessmentDate, gdprResponse.PlannedRiskAssessmentDate);
            Assert.Equal(gdprInput.DPIAConducted, gdprResponse.DPIAConducted);
            Assert.Equal(gdprInput.DPIADate, gdprResponse.DPIADate);
            (gdprInput.DPIADocumentation ?? new SimpleLinkDTO()).ToExpectedObject().ShouldMatch(gdprResponse.DPIADocumentation);
            Assert.Equal(gdprInput.RetentionPeriodDefined, gdprResponse.RetentionPeriodDefined);
            Assert.Equal(gdprInput.NextDataRetentionEvaluationDate, gdprResponse.NextDataRetentionEvaluationDate);
            Assert.Equal(gdprInput.DataRetentionEvaluationFrequencyInMonths ?? 0, gdprResponse.DataRetentionEvaluationFrequencyInMonths);
            Assert.Equal(gdprInput.SpecificPersonalData ?? new List<GDPRPersonalDataChoice>().OrderBy(x => x), gdprResponse.SpecificPersonalData.OrderBy(x => x));
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

        private static void AssertValidity(ItSystemUsageValidityWriteRequestDTO validityInput, ItSystemUsageValidityResponseDTO validityResponse, bool expectedDateValidity, bool expectedLifeCycleValidity)
        {
            Assert.Equal(validityResponse?.ValidFrom, validityInput?.ValidFrom);
            Assert.Equal(validityResponse?.ValidTo, validityInput?.ValidTo);
            Assert.Equal(validityResponse?.LifeCycleStatus, validityInput?.LifeCycleStatus);
            Assert.Equal(expectedDateValidity, validityResponse?.ValidAccordingToValidityPeriod);
            Assert.Equal(expectedLifeCycleValidity, validityResponse?.ValidAccordingToLifeCycle);
        }

        private async Task<(string token, User user, OrganizationDTO organization, ItSystemDTO system)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
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
            Assert.Equal(expected.ArchiveDuty, actual.ArchiveDuty);
            Assert.Equal(expected.TypeUuid, actual.Type.Uuid);
            Assert.Equal(expected.LocationUuid, actual.Location.Uuid);
            Assert.Equal(expected.TestLocationUuid, actual.TestLocation.Uuid);
            Assert.Equal(expected.SupplierOrganizationUuid, actual.Supplier.Uuid);
            Assert.Equal(expected.Active, actual.Active);
            Assert.Equal(expected.FrequencyInMonths, actual.FrequencyInMonths);
            Assert.Equal(expected.DocumentBearing, actual.DocumentBearing);
            Assert.Equal(expected.Notes, actual.Notes);

            Assert.Equal(expected.JournalPeriods.Count(), actual.JournalPeriods.Count());
            var firstJournalPeriod = expected.JournalPeriods.First();
            var journalPeriodFromServer = Assert.Single(actual.JournalPeriods.Where(x => x.ArchiveId == firstJournalPeriod.ArchiveId));
            Assert.Equal(firstJournalPeriod.Approved, journalPeriodFromServer.Approved);
            Assert.Equal(firstJournalPeriod.ArchiveId, journalPeriodFromServer.ArchiveId);
            Assert.Equal(firstJournalPeriod.StartDate, journalPeriodFromServer.StartDate);
            Assert.Equal(firstJournalPeriod.EndDate, journalPeriodFromServer.EndDate);
        }

        private async Task<ArchivingCreationRequestDTO> CreateArchivingCreationRequestDTO(Guid organizationUuid)
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

        private async Task<ArchivingCreationRequestDTO> CreateArchivingCreationRequestDTO(Guid archiveTypeUuid, Guid archiveLocationUuid, Guid archiveTestLocationUuid, Guid organizationUuid)
        {
            var requestDto = await CreateArchivingCreationRequestDTO(organizationUuid);
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

        private async Task<ArchivingUpdateRequestDTO> CreateArchivingUpdateRequestDTO(Guid organizationUuid)
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

        private async Task<ArchivingUpdateRequestDTO> CreateArchivingUpdateRequestDTO(Guid archiveTypeUuid, Guid archiveLocationUuid, Guid archiveTestLocationUuid, Guid organizationUuid)
        {
            var dto = await CreateArchivingUpdateRequestDTO(organizationUuid);
            UpdateArchivingChoices(archiveTypeUuid, archiveLocationUuid, archiveTestLocationUuid, dto);
            return dto;
        }

        private IEnumerable<T> WithRandomMaster<T>(IEnumerable<T> references) where T : ExternalReferenceDataWriteRequestDTO
        {
            var orderedRandomly = references.OrderBy(x => A<int>()).ToList();
            orderedRandomly.First().MasterReference = true;
            foreach (var externalReferenceDataDto in orderedRandomly.Skip(1))
                externalReferenceDataDto.MasterReference = false;

            return orderedRandomly;
        }

        private IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> WithRandomMaster(IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> references)
        {
            var orderedRandomly = references.OrderBy(x => A<int>()).ToList();
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

        private static void AssertKLEDeviation(bool withDeviation, IEnumerable<Guid> expectedDeviation, IEnumerable<IdentityNamePairResponseDTO> actualDeviation)
        {
            if (withDeviation)
            {
                Assert.Equal(expectedDeviation.OrderBy(uuid => uuid),
                    actualDeviation.Select(x => x.Uuid).OrderBy(uuid => uuid));
            }
            else
                Assert.Empty(actualDeviation);
        }

        private int TaskKeyIndex = 0;
        private readonly object TaskKeyLock = new object();

        private string ReserveKey()
        {
            const string prefix = "V2:";
            lock (TaskKeyLock)
            {
                bool exists;
                string currentKey;
                do
                {
                    currentKey = $"{prefix}{TaskKeyIndex++}";
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

        private static async Task AssertOrganizationalUsage(string token, Guid systemUsageUuid, IEnumerable<OrgUnitDTO> expectedUnits, OrgUnitDTO expectedResponsible)
        {
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, systemUsageUuid);
            var expectedOrgUnits = expectedUnits
                .Select(unitDto => new { Uuid = unitDto.Uuid, Name = unitDto.Name })
                .OrderBy(x => x.Uuid)
                .ToList();

            var actualOrgUnits = dto
                .OrganizationUsage
                .UsingOrganizationUnits
                .Select(x => new { Uuid = x.Uuid, Name = x.Name })
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
            GeneralDataWriteRequestDTO generalSection = null,
            OrganizationUsageWriteRequestDTO organizationalUsageSection = null,
            LocalKLEDeviationsRequestDTO kleDeviationsRequest = null,
            IEnumerable<ExternalReferenceDataWriteRequestDTO> referenceDataDtos = null,
            IEnumerable<RoleAssignmentRequestDTO> roles = null,
            GDPRWriteRequestDTO gdpr = null,
            ArchivingCreationRequestDTO archiving = null)
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
            GeneralDataWriteRequestDTO generalSection = null,
            OrganizationUsageWriteRequestDTO organizationalUsageSection = null,
            LocalKLEDeviationsRequestDTO kleDeviationsRequest = null,
            IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> referenceDataDtos = null,
            IEnumerable<RoleAssignmentRequestDTO> roles = null,
            GDPRWriteRequestDTO gdpr = null,
            ArchivingUpdateRequestDTO baseArchiving = null)
        {
            var generalUpdateRequest = new GeneralDataUpdateRequestDTO()
            {
                DataClassificationUuid = generalSection.DataClassificationUuid,
                LocalCallName = generalSection.LocalCallName,
                LocalSystemId = generalSection.LocalSystemId,
                Notes = generalSection.Notes,
                NumberOfExpectedUsers = generalSection.NumberOfExpectedUsers,
                SystemVersion = generalSection.SystemVersion,
                Validity = generalSection.Validity
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

        private string CreateEmail()
        {
            return $"{CreateName()}{DateTime.Now.Ticks}@kitos.dk";
        }

        private static void AssertExpectedUsageShallow(ItSystemUsageDTO expectedContent, IEnumerable<ItSystemUsageResponseDTO> dtos)
        {
            var dto = Assert.Single(dtos, usage => usage.Uuid == expectedContent.Uuid);
            AssertExpectedUsageShallow(expectedContent, dto);
        }

        private static void AssertExpectedUsageShallow(ItSystemUsageDTO expectedContent, IEnumerable<ItSystemUsageSearchResultResponseDTO> dtos)
        {
            var dto = Assert.Single(dtos, usage => usage.Uuid == expectedContent.Uuid);
            AssertExpectedUsageShallow(expectedContent, dto);
        }
        private static void AssertExpectedUsageShallow(ItSystemUsageDTO expectedContent, ItSystemUsageSearchResultResponseDTO dto)
        {
            Assert.Equal(expectedContent.ItSystem.Uuid, dto.SystemContext.Uuid);
            Assert.Equal(expectedContent.ItSystem.Name, dto.SystemContext.Name);
            Assert.Equal(expectedContent.ItSystem.Disabled, dto.SystemContext.Deactivated);
        }

        private static void AssertExpectedUsageShallow(ItSystemUsageDTO expectedContent, ItSystemUsageResponseDTO dto)
        {
            Assert.Equal(expectedContent.Organization.Uuid, dto.OrganizationContext.Uuid);
            Assert.Equal(expectedContent.Organization.Name, dto.OrganizationContext.Name);
            Assert.Equal(expectedContent.Organization.Cvr, dto.OrganizationContext.Cvr);
            Assert.Equal(expectedContent.ItSystem.Uuid, dto.SystemContext.Uuid);
            Assert.Equal(expectedContent.ItSystem.Name, dto.SystemContext.Name);
        }

        private async Task<(Guid uuid, int dbId)> CreateSystemAsync(int organizationId, AccessModifier accessModifier, string name = null)
        {
            var createdSystem = await CreateSystemAndGetAsync(organizationId, accessModifier, name);

            return (createdSystem.Uuid, createdSystem.Id);
        }

        private async Task<ItSystemDTO> CreateSystemAndGetAsync(int organizationId, AccessModifier accessModifier, string name = null)
        {
            var systemName = name ?? CreateName();
            var createdSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, accessModifier);

            return createdSystem;
        }

        private async Task<(User user, string token)> CreateApiUser(OrganizationDTO organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Id, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
        }

        private async Task<User> CreateUser(OrganizationDTO organization)
        {
            var userId = await HttpApi.CreateOdataUserAsync(ObjectCreateHelper.MakeSimpleApiUserDto(CreateEmail(), false), OrganizationRole.User, organization.Id);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userId));
            return user;
        }

        private async Task<OrganizationDTO> CreateOrganizationAsync(OrganizationTypeKeys orgType)
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, String.Empty, orgType, AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return $"{nameof(ItSystemUsageApiV2Test)}æøå{A<string>()}";
        }

        private static async Task<SystemRelationDTO> CreateRelationAsync(ItSystemUsageDTO fromUsage, ItSystemUsageDTO toUsage, ItContractDTO contract = null)
        {
            return await SystemRelationHelper.PostRelationAsync(new CreateSystemRelationDTO { FromUsageId = fromUsage.Id, ToUsageId = toUsage.Id, ContractId = contract?.Id });
        }

        private static int GetUsageIdByUuid(Guid uuid)
        {
            return DatabaseAccess.MapFromEntitySet<ItSystemUsage, int>(all => all.AsQueryable().ByUuid(uuid).Id);
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
