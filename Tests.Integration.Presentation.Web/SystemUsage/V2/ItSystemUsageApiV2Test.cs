using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using ExpectedObjects;
using Infrastructure.Services.Types;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.SystemRelations;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Tests.Integration.Presentation.Web.Tools.Model;
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
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system1 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization1.Id);
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
        public async Task Can_Get_All_ItSystemUsages_With_Paging()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system1 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization1.Id);
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
        public async Task Can_Get_All_ItSystemUsages_Filtered_By_Organization()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system1 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization1.Id);
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
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system1 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization1.Id);
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
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system1 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);
            var relationTargetSystem = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization1.Id);
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
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
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
        public async Task Can_Get_All_ItSystemUsages_Filtered_By_SystemNameContent()
        {
            //Arrange
            var content = $"CONTENT_{A<Guid>()}";
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
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

        [Fact]
        public async Task Can_Get_Specific_ItSystemUsage()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            await ItSystemHelper.TakeIntoUseAsync(system.dbId, organization1.Id);
            var systemUsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system.dbId, organization2.Id);

            //Act
            var dto = (await ItSystemUsageV2Helper.GetSingleAsync(token, systemUsageOrg2.Uuid));

            //Assert - exhaustive content assertions are done in the read-after-write assertion tests (POST/PUT)
            AssertExpectedUsageShallow(systemUsageOrg2, dto);
        }

        [Fact]
        public async Task Cannot_Get_Unknown_ItSystemUsage()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            //Act
            using var response = (await ItSystemUsageV2Helper.SendGetSingleAsync(token, A<Guid>()));

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Get_ItSystemUsage_If_NotAllowedTo()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();

            var system = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            await ItSystemHelper.TakeIntoUseAsync(system.dbId, organization1.Id);
            var systemUsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system.dbId, organization2.Id);

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
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);

            //Act
            await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var newUsage = await ItSystemUsageV2Helper.SendPostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, newUsage.StatusCode);
        }

        [Fact]
        public async Task Can_POST_With_Full_General_Data_Section()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var project1 = await ItProjectHelper.CreateProject(CreateName(), organization.Id);
            var project2 = await ItProjectHelper.CreateProject(CreateName(), organization.Id);
            var dataClassification = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageDataClassification, organization.Uuid, 1, 0)).First();
            var request = CreatePostRequest(organization.Uuid, system.Uuid, new GeneralDataWriteRequestDTO
            {
                LocalCallName = A<string>(),
                LocalSystemId = A<string>(),
                SystemVersion = A<string>(),
                Notes = A<string>(),
                AssociatedProjectUuids = new[] { project1.Uuid, project2.Uuid },
                DataClassificationUuid = dataClassification.Uuid,
                NumberOfExpectedUsers = new ExpectedUsersIntervalDTO { LowerBound = 10, UpperBound = 50 },
                Validity = new ValidityWriteRequestDTO
                {
                    EnforcedValid = A<bool>(),
                    ValidFrom = DateTime.UtcNow.Date,
                    ValidTo = DateTime.UtcNow.Date.AddDays(Math.Abs(A<short>()))
                },
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
            Assert.Equal(request.General.Validity.EnforcedValid, freshReadDTO.General.Validity.EnforcedValid);
            Assert.Equal(request.General.Validity.ValidFrom.GetValueOrDefault().Date, freshReadDTO.General.Validity.ValidFrom.GetValueOrDefault().Date);
            Assert.Equal(request.General.Validity.ValidTo.GetValueOrDefault().Date, freshReadDTO.General.Validity.ValidTo.GetValueOrDefault().Date);
            Assert.Equal(dataClassification.Uuid, freshReadDTO.General.DataClassification.Uuid);
            Assert.Equal(dataClassification.Name, freshReadDTO.General.DataClassification.Name);
            var expectedProjects = new[] { new { Uuid = project1.Uuid, Name = project1.Name }, new { Uuid = project2.Uuid, Name = project2.Name } }.OrderBy(x => x.Uuid);
            var actualProjects = freshReadDTO.General.AssociatedProjects.Select(x => new { Uuid = x.Uuid, Name = x.Name }).OrderBy(x => x.Uuid);
            Assert.Equal(expectedProjects, actualProjects);
        }

        [Fact]
        public async Task Can_PUT_MainContract()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var contract1 = await ItContractHelper.CreateContract(CreateName(), organization.Id);
            var contract2 = await ItContractHelper.CreateContract(CreateName(), organization.Id);
            var usageId = DatabaseAccess.MapFromEntitySet<ItSystemUsage, int>(all => all.AsQueryable().ByUuid(newUsage.Uuid).Id);
            await ItContractHelper.AddItSystemUsage(contract1.Id, usageId, organization.Id);
            await ItContractHelper.AddItSystemUsage(contract2.Id, usageId, organization.Id);

            //Act
            using var response1 = await ItSystemUsageV2Helper.SendPutGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO { MainContractUuid = contract1.Uuid });
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            //Assert
            var freshReadDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            Assert.Equal(contract1.Uuid, freshReadDTO.General.MainContract.Uuid);
            Assert.Equal(contract1.Name, freshReadDTO.General.MainContract.Name);

            //Act - set to another contract
            using var response2 = await ItSystemUsageV2Helper.SendPutGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO { MainContractUuid = contract2.Uuid });
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

            //Assert
            freshReadDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            Assert.Equal(contract2.Uuid, freshReadDTO.General.MainContract.Uuid);
            Assert.Equal(contract2.Name, freshReadDTO.General.MainContract.Name);
        }

        [Fact]
        public async Task Can_PUT_Reset_MainContract()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var contract = await ItContractHelper.CreateContract(CreateName(), organization.Id);
            var usageId = DatabaseAccess.MapFromEntitySet<ItSystemUsage, int>(all => all.AsQueryable().ByUuid(newUsage.Uuid).Id);
            await ItContractHelper.AddItSystemUsage(contract.Id, usageId, organization.Id);

            //Act
            using var response1 = await ItSystemUsageV2Helper.SendPutGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO { MainContractUuid = contract.Uuid });
            using var resetResponse = await ItSystemUsageV2Helper.SendPutGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO()); //Reset main contract
            Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);

            //Assert
            var freshReadDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            Assert.Null(freshReadDTO.General.MainContract);
        }

        [Fact]
        public async Task Can_PUT_Modify_Projects()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));
            var project1 = await ItProjectHelper.CreateProject(CreateName(), organization.Id);
            var project2 = await ItProjectHelper.CreateProject(CreateName(), organization.Id);

            //Act
            using var response1 = await ItSystemUsageV2Helper.SendPutGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO { AssociatedProjectUuids = new[] { project1.Uuid, project2.Uuid } });
            using var modifyResponse = await ItSystemUsageV2Helper.SendPutGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO { AssociatedProjectUuids = new[] { project1.Uuid } }); //Remove one project
            Assert.Equal(HttpStatusCode.OK, modifyResponse.StatusCode);

            //Assert
            var freshReadDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            var project = Assert.Single(freshReadDTO.General.AssociatedProjects);
            Assert.Equal(project1.Uuid, project.Uuid);

            //Act - reset
            using var resetResponse = await ItSystemUsageV2Helper.SendPutGeneral(token, newUsage.Uuid, new GeneralDataUpdateRequestDTO());
            Assert.Equal(HttpStatusCode.OK, modifyResponse.StatusCode);

            //Assert
            freshReadDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            Assert.Empty(freshReadDTO.General.AssociatedProjects);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_POST_With_OrganizationalUsage(bool withResponsible)
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var unit1 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, CreateName());
            var unit2 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, CreateName(), unit1.Id);
            var unit3 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, CreateName(), unit1.Id);

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
        public async Task Can_PUT_Modify_OrganizationalUsage()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            var unit1 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, CreateName());
            var unit2 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, CreateName(), unit1.Id);
            var unit3 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, CreateName(), unit1.Id);

            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Act
            using var modificationResponse1 = await ItSystemUsageV2Helper.SendPutOrganizationalUsage(token,
                newUsage.Uuid, new OrganizationUsageWriteRequestDTO()
                {
                    UsingOrganizationUnitUuids = new[] { unit1.Uuid, unit2.Uuid },
                    ResponsibleOrganizationUnitUuid = unit2.Uuid
                });
            Assert.Equal(HttpStatusCode.OK, modificationResponse1.StatusCode);

            //Assert
            await AssertOrganizationalUsage(token, newUsage.Uuid, new[] { unit1, unit2 }, unit2);

            //Act - swap one unit as well as responsible
            using var modificationResponse2 = await ItSystemUsageV2Helper.SendPutOrganizationalUsage(token,
                newUsage.Uuid, new OrganizationUsageWriteRequestDTO
                {
                    UsingOrganizationUnitUuids = new[] { unit1.Uuid, unit3.Uuid },
                    ResponsibleOrganizationUnitUuid = unit3.Uuid
                });
            Assert.Equal(HttpStatusCode.OK, modificationResponse2.StatusCode);

            //Assert
            await AssertOrganizationalUsage(token, newUsage.Uuid, new[] { unit1, unit3 }, unit3);

            //Act - reset all
            using var modificationResponse3 = await ItSystemUsageV2Helper.SendPutOrganizationalUsage(token,
                newUsage.Uuid, new OrganizationUsageWriteRequestDTO());
            Assert.Equal(HttpStatusCode.OK, modificationResponse3.StatusCode);

            //Assert
            await AssertOrganizationalUsage(token, newUsage.Uuid, Enumerable.Empty<OrgUnitDTO>(), null);

            //Act - set using orgs but no responsible
            using var modificationResponse4 = await ItSystemUsageV2Helper.SendPutOrganizationalUsage(token,
                newUsage.Uuid, new OrganizationUsageWriteRequestDTO
                {
                    UsingOrganizationUnitUuids = new[] { unit1.Uuid, unit2.Uuid }
                });
            Assert.Equal(HttpStatusCode.OK, modificationResponse4.StatusCode);

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
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();

            var additionalTaskRefs = Many<Guid>(2).ToList();
            var taskRefsOnSystem = Many<Guid>(3).ToList();
            var potentialRemovals = taskRefsOnSystem.Take(2).ToList();

            AddTaskRefsInDatabase(additionalTaskRefs.Concat(taskRefsOnSystem));
            var dbIdsOfSystemTaskRefs = DatabaseAccess.MapFromEntitySet<TaskRef, int[]>(all => all.AsQueryable().Where(s => taskRefsOnSystem.Contains(s.Uuid)).Select(x => x.Id).ToArray());

            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
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
        public async Task Can_PUT_KLE()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();

            var additionalTaskRefs = Many<Guid>(2).ToList();
            var taskRefsOnSystem = Many<Guid>(3).ToList();
            var potentialRemovals = taskRefsOnSystem.Take(2).ToList();

            AddTaskRefsInDatabase(additionalTaskRefs.Concat(taskRefsOnSystem));
            var dbIdsOfSystemTaskRefs = DatabaseAccess.MapFromEntitySet<TaskRef, int[]>(all => all.AsQueryable().Where(s => taskRefsOnSystem.Contains(s.Uuid)).Select(x => x.Id).ToArray());

            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            foreach (var taskRefId in dbIdsOfSystemTaskRefs)
            {
                using var addTaskRefResponse = await ItSystemHelper.SendAddTaskRefRequestAsync(system.Id, taskRefId, organization.Id);
                Assert.Equal(HttpStatusCode.OK, addTaskRefResponse.StatusCode);
            }
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            //Act - add one addition
            using var put1 = await ItSystemUsageV2Helper.SendPutKle(token, newUsage.Uuid, new LocalKLEDeviationsRequestDTO() { AddedKLEUuids = additionalTaskRefs.Take(1) });
            Assert.Equal(HttpStatusCode.OK, put1.StatusCode);

            //Assert
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertKLEDeviation(true, additionalTaskRefs.Take(1), dto.LocalKLEDeviations.AddedKLE);
            AssertKLEDeviation(false, null, dto.LocalKLEDeviations.RemovedKLE);

            //Act - add another one
            using var put2 = await ItSystemUsageV2Helper.SendPutKle(token, newUsage.Uuid, new LocalKLEDeviationsRequestDTO() { AddedKLEUuids = additionalTaskRefs });
            Assert.Equal(HttpStatusCode.OK, put2.StatusCode);

            //Assert
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertKLEDeviation(true, additionalTaskRefs, dto.LocalKLEDeviations.AddedKLE);
            AssertKLEDeviation(false, null, dto.LocalKLEDeviations.RemovedKLE);

            //Act - remove some
            using var put3 = await ItSystemUsageV2Helper.SendPutKle(token, newUsage.Uuid, new LocalKLEDeviationsRequestDTO() { AddedKLEUuids = additionalTaskRefs, RemovedKLEUuids = potentialRemovals });
            Assert.Equal(HttpStatusCode.OK, put3.StatusCode);

            //Assert
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertKLEDeviation(true, additionalTaskRefs, dto.LocalKLEDeviations.AddedKLE);
            AssertKLEDeviation(true, potentialRemovals, dto.LocalKLEDeviations.RemovedKLE);

            //Act - reset
            using var put4 = await ItSystemUsageV2Helper.SendPutKle(token, newUsage.Uuid, new LocalKLEDeviationsRequestDTO());
            Assert.Equal(HttpStatusCode.OK, put4.StatusCode);

            //Assert
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertKLEDeviation(false, null, dto.LocalKLEDeviations.AddedKLE);
            AssertKLEDeviation(false, null, dto.LocalKLEDeviations.RemovedKLE);
        }

        [Fact]
        public async Task Can_POST_With_ExternalReferences()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            var inputs = Many<ExternalReferenceDataDTO>().Transform(WithRandomMaster).ToList();

            var request = CreatePostRequest(organization.Uuid, system.Uuid, referenceDataDtos: inputs);

            //Act
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, request);

            //Assert
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            Assert.Equal(inputs.Count, dto.ExternalReferences.Count());
            AssertExternalReferenceResults(inputs, dto);
        }

        [Fact]
        public async Task Can_PUT_ExternalReferences()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            var request = CreatePostRequest(organization.Uuid, system.Uuid);
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, request);

            var inputs1 = Many<ExternalReferenceDataDTO>().Transform(WithRandomMaster).ToList();

            //Act
            using var response1 = await ItSystemUsageV2Helper.SendPutExternalReferences(token, newUsage.Uuid, inputs1);
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            //Assert
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertExternalReferenceResults(inputs1, dto);

            //Act - reset
            var inputs2 = Enumerable.Empty<ExternalReferenceDataDTO>().ToList();
            using var response2 = await ItSystemUsageV2Helper.SendPutExternalReferences(token, newUsage.Uuid, inputs2);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

            //Assert
            dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertExternalReferenceResults(inputs2, dto);
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

            var inputs = CreateArchivingWriteRequestDTO(archiveType.Uuid, archiveLocation.Uuid, archiveTestLocation.Uuid, organization.Uuid);

            var request = CreatePostRequest(organization.Uuid, system.Uuid, archiving: inputs);

            //Act
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, request);

            //Assert
            var dto = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertArchivingParametersSet(inputs, dto.Archiving);
        }

        [Fact]
        public async Task Can_PUT_With_Archiving()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);

            var createRequest = CreatePostRequest(organization.Uuid, system.Uuid);
            var newUsage = await ItSystemUsageV2Helper.PostAsync(token, createRequest);

            var archiveType = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTypes, organization.Uuid, 1, 0)).First();
            var archiveLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveLocations, organization.Uuid, 1, 0)).First();
            var archiveTestLocation = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTestLocations, organization.Uuid, 1, 0)).First();

            var inputs = CreateArchivingWriteRequestDTO(archiveType.Uuid, archiveLocation.Uuid, archiveTestLocation.Uuid, organization.Uuid);

            //Act - Add archiving data
            var addedArchivingDataUsage = await ItSystemUsageV2Helper.SendPutArchiving(token, newUsage.Uuid, inputs);

            //Assert 
            Assert.Equal(HttpStatusCode.OK, addedArchivingDataUsage.StatusCode);
            var addedDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertArchivingParametersSet(inputs, addedDTO.Archiving);

            //Act - Remove archiving data
            var removedArchivingDataUsage = await ItSystemUsageV2Helper.SendPutArchiving(token, newUsage.Uuid, new ArchivingWriteRequestDTO(){ ArchiveJournalPeriods = new List<JournalPeriodDTO>()});

            //Assert 
            Assert.Equal(HttpStatusCode.OK, removedArchivingDataUsage.StatusCode);
            var removedDTO = await ItSystemUsageV2Helper.GetSingleAsync(token, newUsage.Uuid);
            AssertArchivingParametersNotSet(removedDTO.Archiving);
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

        private static void AssertArchivingParametersSet(ArchivingWriteRequestDTO expected, ArchivingRegistrationsResponseDTO actual)
        {
            Assert.Equal(expected.ArchiveDuty, actual.ArchiveDuty);
            Assert.Equal(expected.ArchiveTypeUuid, actual.Type.Uuid);
            Assert.Equal(expected.ArchiveLocationUuid, actual.Location.Uuid);
            Assert.Equal(expected.ArchiveTestLocationUuid, actual.TestLocation.Uuid);
            Assert.Equal(expected.ArchiveSupplierOrganizationUuid, actual.Supplier.Uuid);
            Assert.Equal(expected.ArchiveActive, actual.Active);
            Assert.Equal(expected.ArchiveFrequencyInMonths, actual.FrequencyInMonths);
            Assert.Equal(expected.ArchiveDocumentBearing, actual.DocumentBearing);
            Assert.Equal(expected.ArchiveNotes, actual.Notes);

            Assert.Equal(expected.ArchiveJournalPeriods.Count(), actual.JournalPeriods.Count());
            var firstJournalPeriod = expected.ArchiveJournalPeriods.First();
            var journalPeriodFromServer = Assert.Single(actual.JournalPeriods.Where(x => x.ArchiveId == firstJournalPeriod.ArchiveId));
            Assert.Equal(firstJournalPeriod.Approved, journalPeriodFromServer.Approved);
            Assert.Equal(firstJournalPeriod.ArchiveId, journalPeriodFromServer.ArchiveId);
            Assert.Equal(firstJournalPeriod.StartDate, journalPeriodFromServer.StartDate);
            Assert.Equal(firstJournalPeriod.EndDate, journalPeriodFromServer.EndDate);
        }

        private ArchivingWriteRequestDTO CreateArchivingWriteRequestDTO(Guid archiveTypeUuid, Guid archiveLocationUuid, Guid archiveTestLocationUuid, Guid organizationUuid)
        {
            return new ArchivingWriteRequestDTO()
            {
                ArchiveDuty = A<ArchiveDutyChoice>(),
                ArchiveTypeUuid = archiveTypeUuid,
                ArchiveLocationUuid = archiveLocationUuid,
                ArchiveTestLocationUuid = archiveTestLocationUuid,
                ArchiveSupplierOrganizationUuid = organizationUuid,
                ArchiveActive = A<bool>(),
                ArchiveFrequencyInMonths = A<int>(),
                ArchiveDocumentBearing = A<bool>(),
                ArchiveNotes = A<string>(),
                ArchiveJournalPeriods = new Fixture()
                    .Build<JournalPeriodDTO>()
                    .Without(x => x.EndDate)
                    .Without(x => x.StartDate)
                    .Do(x =>
                    {
                        var startDate = A<DateTime>();
                        x.StartDate = startDate;
                        x.EndDate = startDate.AddDays(1);
                    })
                    .CreateMany()
            };
        }

        private IEnumerable<ExternalReferenceDataDTO> WithRandomMaster(IEnumerable<ExternalReferenceDataDTO> references)
        {
            var orderedRandomly = references.OrderBy(x => A<int>()).ToList();
            orderedRandomly.First().MasterReference = true;
            foreach (var externalReferenceDataDto in orderedRandomly.Skip(1))
                externalReferenceDataDto.MasterReference = false;

            return orderedRandomly;
        }

        private static void AssertExternalReferenceResults(List<ExternalReferenceDataDTO> expected, ItSystemUsageResponseDTO actual)
        {
            expected.OrderBy(x => x.DocumentId).ToList().ToExpectedObject()
                .ShouldMatch(actual.ExternalReferences.OrderBy(x => x.DocumentId).ToList());
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
            var role = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().First());
            var roles = new List<RoleAssignmentRequestDTO>
            {
                new RoleAssignmentRequestDTO()
                {
                    RoleUuid = role.Uuid,
                    UserUuid = user1.Uuid
                },
                new RoleAssignmentRequestDTO()
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
        public async Task Can_PUT_Modify_Roles()
        {
            //Arrange
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            var system = await CreateSystemAndGetAsync(organization.Id, AccessModifier.Public);

            var user1 = await CreateUser(organization);
            var user2 = await CreateUser(organization);
            var role = DatabaseAccess.MapFromEntitySet<ItSystemRole, ItSystemRole>(x => x.AsQueryable().First());

            var createdDTO = await ItSystemUsageV2Helper.PostAsync(token, CreatePostRequest(organization.Uuid, system.Uuid));

            var initialRoles = new List<RoleAssignmentRequestDTO>
            {
                new RoleAssignmentRequestDTO()
                {
                    RoleUuid = role.Uuid,
                    UserUuid = user1.Uuid
                }
            };
            var modifyRoles = new List<RoleAssignmentRequestDTO>
            {
                new RoleAssignmentRequestDTO()
                {
                    RoleUuid = role.Uuid,
                    UserUuid = user2.Uuid
                }
            };

            //Act - Add role
            var addInitialRolesRequest = await ItSystemUsageV2Helper.SendPutRoles(token, createdDTO.Uuid, initialRoles);

            //Assert
            Assert.Equal(HttpStatusCode.OK, addInitialRolesRequest.StatusCode);
            var initialRoleResponse = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDTO.Uuid);
            AssertSingleRight(role, user1, initialRoleResponse.Roles);

            //Act - Modify role
            var modifiedRequest = await ItSystemUsageV2Helper.SendPutRoles(token, createdDTO.Uuid, modifyRoles);

            //Assert
            Assert.Equal(HttpStatusCode.OK, modifiedRequest.StatusCode);
            var modifiedRoleResponse = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDTO.Uuid);
            AssertSingleRight(role, user2, modifiedRoleResponse.Roles);
            
            //Act - Remove role
            var removedRequest = await ItSystemUsageV2Helper.SendPutRoles(token, createdDTO.Uuid, new List<RoleAssignmentRequestDTO>());

            //Assert
            Assert.Equal(HttpStatusCode.OK, removedRequest.StatusCode);
            var removedRoleResponse = await ItSystemUsageV2Helper.GetSingleAsync(token, createdDTO.Uuid);
            Assert.Empty(removedRoleResponse.Roles);
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
            IEnumerable<ExternalReferenceDataDTO> referenceDataDtos = null,
            IEnumerable<RoleAssignmentRequestDTO> roles = null,
            ArchivingWriteRequestDTO archiving = null)
        {
            return new CreateItSystemUsageRequestDTO
            {
                OrganizationUuid = organizationId,
                SystemUuid = systemId,
                General = generalSection,
                OrganizationUsage = organizationalUsageSection,
                LocalKleDeviations = kleDeviationsRequest,
                ExternalReferences = referenceDataDtos?.ToList(),
                Roles = roles,
                Archiving = archiving
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

        private async Task<(User user, string token)> CreateApiUser(Organization organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Id, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
        }

        private async Task<User> CreateUser(Organization organization)
        {
            var userId = await HttpApi.CreateOdataUserAsync(ObjectCreateHelper.MakeSimpleApiUserDto(CreateEmail(), false), OrganizationRole.User, organization.Id);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userId));
            return user;
        }

        private async Task<Organization> CreateOrganizationAsync(OrganizationTypeKeys orgType)
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, string.Join("", Many<int>(8).Select(x => Math.Abs(x) % 9)), orgType, AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return $"{nameof(ItSystemUsageApiV2Test)}{A<string>()}";
        }

        private static async Task<SystemRelationDTO> CreateRelationAsync(ItSystemUsageDTO fromUsage, ItSystemUsageDTO toUsage)
        {
            return await SystemRelationHelper.PostRelationAsync(new CreateSystemRelationDTO { FromUsageId = fromUsage.Id, ToUsageId = toUsage.Id });
        }
    }
}
