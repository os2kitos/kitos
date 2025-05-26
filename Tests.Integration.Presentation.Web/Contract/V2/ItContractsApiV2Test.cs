using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using ExpectedObjects;
using Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Contract;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.SharedProperties;
using Presentation.Web.Models.API.V2.Types.Contract;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Integration.Presentation.Web.Contract.V2
{
    public class ItContractsApiV2Test : BaseTest
    {
        [Fact]
        public async Task Can_GET_Specific_Contract()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var newContract = await CreateItContractAsync(organization.Uuid);

            //Act
            var dto = await ItContractV2Helper.GetItContractAsync(token, newContract.Uuid);

            //Assert
            AssertExpectedShallowContract(newContract, organization, dto);
        }

        [Fact]
        public async Task Cannot_Get_Contract_If_Unknown()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            //Act
            using var response = await ItContractV2Helper.SendGetItContractAsync(token, A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Get_Contract_If_NotAllowedTo()
        {
            //Arrange
            var (token, user, organization1) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync();
            var newContract = await CreateItContractAsync(organization2.Uuid);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractAsync(token, newContract.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Get_Contract_If_Empty_Uuid_In_Request()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            //Act
            using var response = await ItContractV2Helper.SendGetItContractAsync(token, Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_GET_All_Contracts()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var contract1 = await CreateItContractAsync(organization.Uuid);
            var contract2 = await CreateItContractAsync(organization.Uuid);

            //Act
            var contracts = (await ItContractV2Helper.GetItContractsAsync(token)).ToList();

            //Assert
            Assert.Equal(2, contracts.Count());
            AssertExpectedShallowContracts(contract1, organization, contracts);
            AssertExpectedShallowContracts(contract2, organization, contracts);
        }

        [Fact]
        public async Task Can_GET_All_Contracts_With_Paging()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var contract1 = await CreateItContractAsync(organization.Uuid);
            var contract2 = await CreateItContractAsync(organization.Uuid);
            var contract3 = await CreateItContractAsync(organization.Uuid);

            //Act
            var page1Contracts = (await ItContractV2Helper.GetItContractsAsync(token, page: 0, pageSize: 2)).ToList();
            var page2Contracts = (await ItContractV2Helper.GetItContractsAsync(token, page: 1, pageSize: 2)).ToList();

            //Assert
            Assert.Equal(2, page1Contracts.Count());
            AssertExpectedShallowContracts(contract1, organization, page1Contracts);
            AssertExpectedShallowContracts(contract2, organization, page1Contracts);

            var page2Contract = Assert.Single(page2Contracts);
            AssertExpectedShallowContract(contract3, organization, page2Contract);
        }

        [Fact]
        public async Task Can_GET_All_Contracts_With_LastModifiedFiltering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var contract1 = await CreateItContractAsync(organization.Uuid);
            var contract2 = await CreateItContractAsync(organization.Uuid);
            var contract3 = await CreateItContractAsync(organization.Uuid);

            foreach (var contract in new[] { contract2, contract3, contract1 })
            {
                using var patchResponse = await ItContractV2Helper.SendPatchContractGeneralDataAsync(token, contract.Uuid, new ContractGeneralDataWriteRequestDTO() { Notes = A<string>() });
                Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
            }

            var referenceChange = await ItContractV2Helper.GetItContractAsync(token, contract3.Uuid);

            //Act
            var dtos = await ItContractV2Helper.GetItContractsAsync(token, changedSinceGtEq: referenceChange.LastModified);

            //Assert that the right contracts are returned in the correct order
            Assert.Equal(new[] { contract3.Uuid, contract1.Uuid }, dtos.Select(x => x.Uuid));

        }

        [Fact]
        public async Task Can_GET_All_Contracts_With_User_OrganizationFiltering_Implicit()
        {
            //Arrange
            var (token, user, organization1) = await CreatePrerequisitesAsync();
            var contract1 = await CreateItContractAsync(organization1.Uuid);
            var organization2 = await CreateOrganizationAsync();
            var contract2 = await CreateItContractAsync(organization2.Uuid);

            //Act
            var contracts = (await ItContractV2Helper.GetItContractsAsync(token)).ToList();

            //Assert
            var retrievedContract = Assert.Single(contracts);
            AssertExpectedShallowContract(contract1, organization1, retrievedContract);
        }

        [Fact]
        public async Task Can_GET_All_Contracts_With_OrganizationFiltering_Explicit()
        {
            //Arrange
            var (token, user, organization1) = await CreatePrerequisitesAsync();
            var contract1 = await CreateItContractAsync(organization1.Uuid);
            var organization2 = await CreateOrganizationAsync();
            var contract2 = await CreateItContractAsync(organization2.Uuid);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization2.Uuid).DisposeAsync();

            //Act
            var contracts = (await ItContractV2Helper.GetItContractsAsync(token, organizationUuid: organization1.Uuid)).ToList();

            //Assert
            var retrievedContract = Assert.Single(contracts);
            AssertExpectedShallowContract(contract1, organization1, retrievedContract);
        }

        [Fact]
        public async Task Can_GET_All_Contracts_With_SystemFiltering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var newSystem = await CreateItSystemAsync(organization.Uuid);
            var newSystemUsage = await TakeSystemIntoUsageAsync(newSystem.Uuid, organization.Uuid);
            var contract1 = await CreateItContractAsync(organization.Uuid);
            var contract2 = await CreateItContractAsync(organization.Uuid);
            await ItContractV2Helper.SendPatchSystemUsagesAsync(await GetGlobalToken(), contract1.Uuid,
                newSystemUsage.Uuid.WrapAsEnumerable());

            //Act
            var contracts = (await ItContractV2Helper.GetItContractsAsync(token, systemUuid: newSystem.Uuid)).ToList();

            //Assert
            var retrievedContract = Assert.Single(contracts);
            AssertExpectedShallowContract(contract1, organization, retrievedContract);
        }

        [Fact]
        public async Task Can_GET_All_Contracts_With_SystemUsageFiltering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var newSystem = await CreateItSystemAsync(organization.Uuid);
            var newSystemUsage = await TakeSystemIntoUsageAsync(newSystem.Uuid, organization.Uuid);
            var contract1 = await CreateItContractAsync(organization.Uuid);
            var contract2 = await CreateItContractAsync(organization.Uuid);
            await ItContractV2Helper.SendPatchSystemUsagesAsync(await GetGlobalToken(), contract1.Uuid,
                newSystemUsage.Uuid.WrapAsEnumerable());

            //Act
            var contracts = (await ItContractV2Helper.GetItContractsAsync(token, systemUsageUuid: newSystemUsage.Uuid)).ToList();

            //Assert
            var retrievedContract = Assert.Single(contracts);
            AssertExpectedShallowContract(contract1, organization, retrievedContract);
        }

        [Fact]
        public async Task Can_GET_All_Contracts_With_DPRFiltering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dpr = await CreateDPRAsync(organization.Uuid);
            var contract1 = await CreateItContractAsync(organization.Uuid);
            var contract2 = await CreateItContractAsync(organization.Uuid);
            using var dprAssignmentResponse =
                await ItContractV2Helper.SendPatchDataProcessingRegistrationsAsync(await GetGlobalToken(),
                    contract1.Uuid, dpr.Uuid.WrapAsEnumerable());
            Assert.Equal(HttpStatusCode.OK, dprAssignmentResponse.StatusCode);

            //Act
            var contracts = (await ItContractV2Helper.GetItContractsAsync(token, dataProcessingRegistrationUuid: dpr.Uuid)).ToList();

            //Assert
            var retrievedContract = Assert.Single(contracts);
            AssertExpectedShallowContract(contract1, organization, retrievedContract);
        }

        [Fact]
        public async Task Can_GET_All_Contracts_With_ResponsibleOrgUnitFiltering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var orgUnit = await CreateOrganizationUnitAsync(organization.Uuid);
            var contract1 = await CreateItContractAsync(organization.Uuid);
            var contract2 = await CreateItContractAsync(organization.Uuid);
            using var responsibleOrgUnitAssignmentResponse = await ItContractV2Helper.SendPatchContractResponsibleAsync(
                await GetGlobalToken(), contract1.Uuid, new ContractResponsibleDataWriteRequestDTO
                {
                    OrganizationUnitUuid = orgUnit.Uuid
                });
            Assert.Equal(HttpStatusCode.OK, responsibleOrgUnitAssignmentResponse.StatusCode);

            //Act
            var contracts = (await ItContractV2Helper.GetItContractsAsync(token, responsibleOrgUnitUuid: orgUnit.Uuid)).ToList();

            //Assert
            var retrievedContract = Assert.Single(contracts);
            AssertExpectedShallowContract(contract1, organization, retrievedContract);
        }

        [Fact]
        public async Task Can_GET_All_Contracts_With_SupplierFiltering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var supplier = await CreateOrganizationAsync();
            var contract1 = await CreateItContractAsync(organization.Uuid);
            var contract2 = await CreateItContractAsync(organization.Uuid);
            using var supplierAssignmentResponse = await ItContractV2Helper.SendPatchContractSupplierAsync(
                await GetGlobalToken(), contract1.Uuid,
                new ContractSupplierDataWriteRequestDTO { OrganizationUuid = supplier.Uuid });
            Assert.Equal(HttpStatusCode.OK, supplierAssignmentResponse.StatusCode);

            //Act
            var contracts = (await ItContractV2Helper.GetItContractsAsync(token, supplierUuid: supplier.Uuid)).ToList();

            //Assert
            var retrievedContract = Assert.Single(contracts);
            AssertExpectedShallowContract(contract1, organization, retrievedContract);
        }

        [Fact]
        public async Task Can_GET_All_Contracts_With_NameContentFiltering()
        {
            //Arrange
            var content = $"CONTENT_{A<Guid>()}";
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var contract1 = await CreateItContractAsync(organization.Uuid, $"{content}ONE");
            var contract2 = await CreateItContractAsync(organization.Uuid, $"TWO{content}");
            var contract3 = await CreateItContractAsync(organization.Uuid);

            //Act
            var contracts = (await ItContractV2Helper.GetItContractsAsync(token, nameContent: content)).ToList();

            //Assert
            Assert.Equal(2, contracts.Count);
            AssertExpectedShallowContracts(contract1, organization, contracts);
            AssertExpectedShallowContracts(contract2, organization, contracts);
        }

        [Fact]
        public async Task Can_GET_Contracts_With_NameEqualsFiltering()
        {
            //Arrange
            var fullName = $"CONTENT_{A<Guid>()}";
            var (token, _, organization) = await CreatePrerequisitesAsync();
            await CreateItContractAsync(organization.Uuid, $"{fullName}ONE");
            await CreateItContractAsync(organization.Uuid, $"TWO{fullName}");
            var contract3 = await CreateItContractAsync(organization.Uuid, fullName);

            //Act
            var contracts = (await ItContractV2Helper.GetItContractsAsync(token, nameEquals: fullName)).ToList();

            //Assert
            Assert.Single(contracts);
            AssertExpectedShallowContracts(contract3, organization, contracts);
        }

        [Fact]
        public async Task Cannot_GET_Contracts_With_Empty_Organization_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractsAsync(regularUserToken.Token, organizationUuid: Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_Contracts_With_Empty_System_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractsAsync(regularUserToken.Token, systemUuid: Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_Contracts_With_Empty_SystemUsage_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractsAsync(regularUserToken.Token, systemUsageUuid: Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_Contracts_With_Empty_DPR_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractsAsync(regularUserToken.Token, dataProcessingRegistrationUuid: Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_Contracts_With_Empty_ResponsibleOrgUnit_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractsAsync(regularUserToken.Token, responsibleOrgUnitUuid: Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_Contracts_With_Empty_Supplier_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractsAsync(regularUserToken.Token, supplierUuid: Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task POST_With_Name_Alone()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var requestDto = new CreateNewContractRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
            };

            //Act
            var contractDTO = await ItContractV2Helper.PostContractAsync(token, requestDto);

            //Assert
            Assert.Equal(requestDto.Name, contractDTO.Name);
            Assert.Equal(organization.Name, contractDTO.OrganizationContext.Name);
            Assert.Equal(organization.Cvr, contractDTO.OrganizationContext.Cvr);
            Assert.Equal(organization.Uuid, contractDTO.OrganizationContext.Uuid);
        }

        [Fact]
        public async Task Cannot_POST_With_Duplicate_Name_In_Same_Org()
        {

            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var requestDto = new CreateNewContractRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName()
            };

            //Act
            await ItContractV2Helper.PostContractAsync(token, requestDto);
            using var duplicateResponse = await ItContractV2Helper.SendPostContractAsync(token, requestDto);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
        }

        [Fact]
        public async Task Can_POST_With_Parent()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var parent = await ItContractV2Helper.PostContractAsync(token, CreateNewSimpleRequest(organization.Uuid));

            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                ParentContractUuid = parent.Uuid
            };

            //Act
            var contractDTO = await ItContractV2Helper.PostContractAsync(token, requestDto);

            //Assert
            AssertCrossReference(parent, contractDTO.ParentContract);
        }

        [Fact]
        public async Task Cannot_POST_With_Parent_If_Not_Allowed_To_Read_Parent()
        {
            //Arrange
            var (token, user, organization1) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync();
            var parent = await CreateItContractAsync(organization2.Uuid);

            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization1.Uuid,
                Name = CreateName(),
                ParentContractUuid = parent.Uuid
            };

            //Act
            using var createResponse = await ItContractV2Helper.SendPostContractAsync(token, requestDto);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_With_Parent_If_Parent_In_Different_Organization()
        {
            //Arrange
            var (token, user, organization1) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization2.Uuid).DisposeAsync();
            var parent = await CreateItContractAsync(organization2.Uuid);

            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization1.Uuid,
                Name = CreateName(),
                ParentContractUuid = parent.Uuid
            };

            //Act
            using var createResponse = await ItContractV2Helper.SendPostContractAsync(token, requestDto);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_With_Unknown_Parent()
        {
            //Arrange
            var (token, user, organization1) = await CreatePrerequisitesAsync();

            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization1.Uuid,
                Name = CreateName(),
                ParentContractUuid = A<Guid>()
            };

            //Act
            using var createResponse = await ItContractV2Helper.SendPostContractAsync(token, requestDto);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, createResponse.StatusCode);
        }

        [Fact]
        public async Task Can_PATCH_With_Parent()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var parent = await ItContractV2Helper.PostContractAsync(token, CreateNewSimpleRequest(organization.Uuid));

            var contractDTO = await ItContractV2Helper.PostContractAsync(token, CreateNewSimpleRequest(organization.Uuid));

            var updateRequest1 = new UpdateContractRequestDTO()
            {
                Name = CreateName(),
                ParentContractUuid = parent.Uuid
            };

            //Act - Update from empty
            using var updatedResponse1 = await ItContractV2Helper.SendPutContractAsync(token, contractDTO.Uuid, updateRequest1);

            //Assert - Update from empty
            Assert.Equal(HttpStatusCode.OK, updatedResponse1.StatusCode);
            var updatedContractDTO1 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            AssertCrossReference(parent, updatedContractDTO1.ParentContract);

            //Act - Update from filled
            var newParent = await ItContractV2Helper.PostContractAsync(token, CreateNewSimpleRequest(organization.Uuid));
            var updateRequest2 = new UpdateContractRequestDTO()
            {
                Name = CreateName(),
                ParentContractUuid = newParent.Uuid
            };
            using var updatedResponse2 = await ItContractV2Helper.SendPutContractAsync(token, contractDTO.Uuid, updateRequest2);

            //Assert - Update from filled
            Assert.Equal(HttpStatusCode.OK, updatedResponse2.StatusCode);
            var updatedContractDTO2 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            AssertCrossReference(newParent, updatedContractDTO2.ParentContract);

            //Act - Update to empty
            var updateRequest3 = new UpdateContractRequestDTO()
            {
                Name = CreateName(),
                ParentContractUuid = null
            };
            using var updatedResponse3 = await ItContractV2Helper.SendPutContractAsync(token, contractDTO.Uuid, updateRequest3);

            //Assert - Update to empty
            Assert.Equal(HttpStatusCode.OK, updatedResponse3.StatusCode);
            var updatedContractDTO3 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            Assert.Null(updatedContractDTO3.ParentContract);
        }

        [Fact]
        public async Task Can_POST_With_Procurement()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var (procurementRequest, procurementStrategy, purchaseType) = await CreateProcurementRequestAsync(organization.Uuid);
            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                Procurement = procurementRequest
            };

            //Act
            var contractDTO = await ItContractV2Helper.PostContractAsync(token, requestDto);

            //Assert
            AssertProcurement(procurementRequest, procurementStrategy, purchaseType, contractDTO.Procurement);
        }

        [Fact]
        public async Task Can_POST_With_Procurement_If_ProcurementPlan_Is_Null()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var (procurementRequest, procurementStrategy, purchaseType) = await CreateProcurementRequestAsync(organization.Uuid);
            procurementRequest.ProcurementPlan = null;
            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                Procurement = procurementRequest
            };

            //Act
            var contractDTO = await ItContractV2Helper.PostContractAsync(token, requestDto);

            //Assert
            AssertProcurement(procurementRequest, procurementStrategy, purchaseType, contractDTO.Procurement);
        }

        [Fact]
        public async Task Cannot_POST_With_Procurement_If_Unknown_Strategy()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var (procurementRequest, procurementStrategy, purchaseType) = await CreateProcurementRequestAsync(organization.Uuid);
            procurementRequest.ProcurementStrategyUuid = A<Guid>();
            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                Procurement = procurementRequest
            };

            //Act
            using var response = await ItContractV2Helper.SendPostContractAsync(token, requestDto);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_With_Procurement_If_Unknown_PurchaseType()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var (procurementRequest, procurementStrategy, purchaseType) = await CreateProcurementRequestAsync(organization.Uuid);
            procurementRequest.PurchaseTypeUuid = A<Guid>();
            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                Procurement = procurementRequest
            };

            //Act
            using var response = await ItContractV2Helper.SendPostContractAsync(token, requestDto);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Can_PATCH_With_Procurement()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var newContract = await ItContractV2Helper.PostContractAsync(token, CreateNewSimpleRequest(organization.Uuid));

            var (procurementRequest1, procurementStrategy1, purchaseType1) = await CreateProcurementRequestAsync(organization.Uuid);

            //Act - Update from empty
            using var updatedResponse1 = await ItContractV2Helper.SendPatchProcurementAsync(token, newContract.Uuid, procurementRequest1);

            //Assert - Update from empty
            Assert.Equal(HttpStatusCode.OK, updatedResponse1.StatusCode);
            var updatedContractDTO1 = await ItContractV2Helper.GetItContractAsync(token, newContract.Uuid);
            AssertProcurement(procurementRequest1, procurementStrategy1, purchaseType1, updatedContractDTO1.Procurement);

            //Act - Update from filled
            var (procurementRequest2, procurementStrategy2, purchaseType2) = await CreateProcurementRequestAsync(organization.Uuid);

            using var updatedResponse2 = await ItContractV2Helper.SendPatchProcurementAsync(token, newContract.Uuid, procurementRequest2);

            //Assert - Update from filled
            Assert.Equal(HttpStatusCode.OK, updatedResponse2.StatusCode);
            var updatedContractDTO2 = await ItContractV2Helper.GetItContractAsync(token, newContract.Uuid);
            AssertProcurement(procurementRequest2, procurementStrategy2, purchaseType2, updatedContractDTO2.Procurement);

            //Act - Update to empty
            var procurementRequest3 = new ContractProcurementDataWriteRequestDTO();

            using var updatedResponse3 = await ItContractV2Helper.SendPatchProcurementAsync(token, newContract.Uuid, procurementRequest3);

            //Assert - Update to empty
            Assert.Equal(HttpStatusCode.OK, updatedResponse3.StatusCode);
            var updatedContractDTO3 = await ItContractV2Helper.GetItContractAsync(token, newContract.Uuid);
            Assert.Null(updatedContractDTO3.Procurement.ProcurementStrategy);
            Assert.Null(updatedContractDTO3.Procurement.PurchaseType);
            Assert.Null(updatedContractDTO3.Procurement.ProcurementPlan);
        }

        [Theory]
        [InlineData(true, true, true, true)]
        [InlineData(true, true, true, false)]
        [InlineData(true, true, false, true)]
        [InlineData(true, false, true, true)]
        [InlineData(false, true, true, true)]
        [InlineData(false, false, false, false)]
        public async Task Can_POST_With_GeneralData(bool withContractType, bool withContractTemplate, bool withAgreementElements, bool withCriticalityType)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var (contractType, contractTemplateType, agreementElements, criticalityType, generalDataWriteRequestDto) = await CreateGeneralDataRequestDTO(organization, withContractType, withContractTemplate, withAgreementElements, withCriticalityType);
            var request = new CreateNewContractRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                General = generalDataWriteRequestDto
            };

            //Act
            var dto = await ItContractV2Helper.PostContractAsync(token, request);

            //Assert
            var freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertGeneralDataSection(request.General, contractType, contractTemplateType, agreementElements, criticalityType, freshDTO);
        }

        [Fact]
        public async Task Can_PATCH_With_GeneralData()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var request = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
            };
            var dto = await ItContractV2Helper.PostContractAsync(token, request);

            //Act
            var (contractType, contractTemplateType, agreementElements, criticalityType, generalDataWriteRequestDto) = await CreateGeneralDataRequestDTO(organization, true, true, true, true);
            using var response1 = await ItContractV2Helper.SendPatchContractGeneralDataAsync(token, dto.Uuid, generalDataWriteRequestDto);
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);


            //Assert
            var freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertGeneralDataSection(generalDataWriteRequestDto, contractType, contractTemplateType, agreementElements, criticalityType, freshDTO);

            //Act - new values
            (contractType, contractTemplateType, agreementElements, criticalityType, generalDataWriteRequestDto) = await CreateGeneralDataRequestDTO(organization, false, true, false, false);
            using var response2 = await ItContractV2Helper.SendPatchContractGeneralDataAsync(token, dto.Uuid, generalDataWriteRequestDto);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

            //Assert
            freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertGeneralDataSection(generalDataWriteRequestDto, contractType, contractTemplateType, agreementElements, criticalityType, freshDTO);

            //Act - reset
            var resetRequest = new ContractGeneralDataWriteRequestDTO();
            using var response3 = await ItContractV2Helper.SendPatchContractGeneralDataAsync(token, dto.Uuid, resetRequest);
            Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

            //Assert
            freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertGeneralDataSection(resetRequest, null, null, null, null, freshDTO);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public async Task Can_POST_With_Responsible(bool withOrgUnit, bool withSignedAt, bool withSignedBy)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var contractResponsibleDataWriteRequestDto = await CreateContractResponsibleDataRequestDTO(token, organization, withOrgUnit, withSignedAt, withSignedBy);

            var request = new CreateNewContractRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                Responsible = contractResponsibleDataWriteRequestDto
            };

            //Act
            var dto = await ItContractV2Helper.PostContractAsync(token, request);

            //Assert
            var freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertResponsible(contractResponsibleDataWriteRequestDto, freshDTO);
        }

        [Fact]
        public async Task Can_PATCH_With_Responsible()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var request = new CreateNewContractRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
            };
            var dto = await ItContractV2Helper.PostContractAsync(token, request);

            //Act
            var changes = await CreateContractResponsibleDataRequestDTO(token, organization, false, false, false);
            using var response1 = await ItContractV2Helper.SendPatchContractResponsibleAsync(token, dto.Uuid, changes);
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            //Assert
            var freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertResponsible(changes, freshDTO);

            //Act - change all
            changes = await CreateContractResponsibleDataRequestDTO(token, organization, true, true, true);
            using var response2 = await ItContractV2Helper.SendPatchContractResponsibleAsync(token, dto.Uuid, changes);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

            //Assert
            freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertResponsible(changes, freshDTO);

            //Act - change all again
            changes = await CreateContractResponsibleDataRequestDTO(token, organization, true, true, true);
            using var response3 = await ItContractV2Helper.SendPatchContractResponsibleAsync(token, dto.Uuid, changes);
            Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

            //Assert
            freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertResponsible(changes, freshDTO);

            //Act - full reset
            changes = new ContractResponsibleDataWriteRequestDTO();
            using var response4 = await ItContractV2Helper.SendPatchContractResponsibleAsync(token, dto.Uuid, changes);
            Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

            //Assert
            freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertResponsible(changes, freshDTO);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public async Task Can_POST_With_Supplier(bool withOrg, bool withSignedAt, bool withSignedBy)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var input = await CreateContractSupplierDataRequestDTO(withOrg, withSignedAt, withSignedBy);

            var request = new CreateNewContractRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                Supplier = input
            };

            //Act
            var dto = await ItContractV2Helper.PostContractAsync(token, request);

            //Assert
            var freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertSupplier(input, freshDTO);
        }

        [Fact]
        public async Task Can_PATCH_With_Supplier()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            var request = new CreateNewContractRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
            };
            var dto = await ItContractV2Helper.PostContractAsync(token, request);

            //Act
            var changes = await CreateContractSupplierDataRequestDTO(false, false, false);
            using var response1 = await ItContractV2Helper.SendPatchContractSupplierAsync(token, dto.Uuid, changes);
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            //Assert
            var freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertSupplier(changes, freshDTO);

            //Act - change all
            changes = await CreateContractSupplierDataRequestDTO(true, true, true);
            using var response2 = await ItContractV2Helper.SendPatchContractSupplierAsync(token, dto.Uuid, changes);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

            //Assert
            freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertSupplier(changes, freshDTO);

            //Act - change all again
            changes = await CreateContractSupplierDataRequestDTO(true, true, true);
            using var response3 = await ItContractV2Helper.SendPatchContractSupplierAsync(token, dto.Uuid, changes);
            Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

            //Assert
            freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertSupplier(changes, freshDTO);

            //Act - full reset
            changes = new ContractSupplierDataWriteRequestDTO();
            using var response4 = await ItContractV2Helper.SendPatchContractSupplierAsync(token, dto.Uuid, changes);
            Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

            //Assert
            freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertSupplier(changes, freshDTO);
        }

        [Fact]
        public async Task Can_POST_With_ExternalReferences()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            var inputs = Many<ExternalReferenceDataWriteRequestDTO>().Transform(WithRandomMaster).ToList();

            var request = new CreateNewContractRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                ExternalReferences = inputs
            };

            //Act
            var newContract = await ItContractV2Helper.PostContractAsync(token, request);

            //Assert
            var dto = await ItContractV2Helper.GetItContractAsync(token, newContract.Uuid);
            AssertExternalReferenceResults(inputs, dto);
        }

        [Fact]
        public async Task Can_PATCH_ExternalReferences()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs

            var request = new CreateNewContractRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid
            };
            var newContract = await ItContractV2Helper.PostContractAsync(token, request);

            var inputs1 = CreateUpdateExternalReferences();

            //Act
            using var response1 = await ItContractV2Helper.SendPatchExternalReferences(token, newContract.Uuid, inputs1).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            var dto = await ItContractV2Helper.GetItContractAsync(token, newContract.Uuid);
            AssertExternalReferenceResults(inputs1, dto, true);

            //Act - reset
            var inputs2 = CreateNewExternalReferenceDataWithOldUuid(dto.ExternalReferences);
            using var response2 = await ItContractV2Helper.SendPatchExternalReferences(token, newContract.Uuid, inputs2).WithExpectedResponseCode(HttpStatusCode.OK);

            //Assert
            dto = await ItContractV2Helper.GetItContractAsync(token, newContract.Uuid);
            AssertExternalReferenceResults(inputs2, dto);
        }

        [Fact]
        public async Task Can_POST_With_SystemUsages()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var systemUsageUuids = await CreateSystemUsageUuids(token, organization);
            var request = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                SystemUsageUuids = systemUsageUuids
            };

            //Act
            var dto = await ItContractV2Helper.PostContractAsync(token, request);

            //Assert
            var freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertMultiAssignment(systemUsageUuids, freshDTO.SystemUsages);
        }

        [Fact]
        public async Task Cannot_POST_With_SystemUsages_From_Different_Org()
        {
            //Arrange
            var (token1, _, organization1) = await CreatePrerequisitesAsync();
            var (token2, _, organization2) = await CreatePrerequisitesAsync();
            var system1Usage = await CreateSystemAndTakeItIntoUsage(organization1.Uuid);
            var system2Usage = await CreateSystemAndTakeItIntoUsage(organization2.Uuid);
            var request = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization1.Uuid,
                Name = CreateName(),
                SystemUsageUuids = new[] { system1Usage.Uuid, system2Usage.Uuid }
            };
            // Using global admin as they have full access between organizations
            var globalAdminToken = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            //Act
            using var response = await ItContractV2Helper.SendPostContractAsync(globalAdminToken.Token, request);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_PATCH_With_SystemUsages()
        {
            //Arrange
            var (token, _, organization) = await CreatePrerequisitesAsync();

            var system1Usage = await CreateSystemAndTakeItIntoUsage(organization.Uuid);
            var system2Usage = await CreateSystemAndTakeItIntoUsage(organization.Uuid);
            var system3Usage = await CreateSystemAndTakeItIntoUsage(organization.Uuid);

            var request = new CreateNewContractRequestDTO { Name = CreateName(), OrganizationUuid = organization.Uuid };
            var dto = await ItContractV2Helper.PostContractAsync(token, request);

            var assignment1 = new[] { system1Usage.Uuid };
            var assignment2 = new[] { system1Usage.Uuid, system2Usage.Uuid };
            var assignment3 = new[] { system3Usage.Uuid, system2Usage.Uuid };
            var assignment4 = Array.Empty<Guid>();

            //Act
            await ItContractV2Helper.SendPatchSystemUsagesAsync(token, dto.Uuid, assignment1).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            var freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertMultiAssignment(assignment1, freshDTO.SystemUsages);

            //Act
            await ItContractV2Helper.SendPatchSystemUsagesAsync(token, dto.Uuid, assignment2).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertMultiAssignment(assignment2, freshDTO.SystemUsages);

            //Act
            await ItContractV2Helper.SendPatchSystemUsagesAsync(token, dto.Uuid, assignment3).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertMultiAssignment(assignment3, freshDTO.SystemUsages);

            //Act
            await ItContractV2Helper.SendPatchSystemUsagesAsync(token, dto.Uuid, assignment4).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertMultiAssignment(assignment4, freshDTO.SystemUsages);
        }

        [Fact]
        public async Task Can_POST_With_Roles()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var roles = await CreateRoles(organization);

            //Act
            var createdDTO = await ItContractV2Helper.PostContractAsync(token, new CreateNewContractRequestDTO()
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                Roles = roles
            });

            //Assert
            var freshReadDTO = await ItContractV2Helper.GetItContractAsync(token, createdDTO.Uuid);
            AssertRoleAssignments(roles, freshReadDTO);
        }

        [Fact]
        public async Task Can_PATCH_Modify_Roles()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var roles = await CreateRoles(organization);

            //initial roles
            var roles1 = roles.ToList();

            //Switched roles
            var roles2 = roles.ToList();
            roles2.Reverse();

            //reduced roles
            var roles3 = new List<RoleAssignmentRequestDTO>
            {
                roles[0]
            };

            //Empty roles
            var roles4 = new List<RoleAssignmentRequestDTO>();

            var createdDTO = await ItContractV2Helper.PostContractAsync(token, new CreateNewContractRequestDTO()
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
            });

            //Act
            using var put1 = await ItContractV2Helper.SendPatchRoles(token, createdDTO.Uuid, roles1);
            Assert.Equal(HttpStatusCode.OK, put1.StatusCode);

            //Assert
            var freshReadDTO = await ItContractV2Helper.GetItContractAsync(token, createdDTO.Uuid);
            AssertRoleAssignments(roles1, freshReadDTO);

            //Act - switch roles
            using var put2 = await ItContractV2Helper.SendPatchRoles(token, createdDTO.Uuid, roles2);
            Assert.Equal(HttpStatusCode.OK, put2.StatusCode);

            //Assert
            freshReadDTO = await ItContractV2Helper.GetItContractAsync(token, createdDTO.Uuid);
            AssertRoleAssignments(roles2, freshReadDTO);

            //Act - reduce roles
            using var put3 = await ItContractV2Helper.SendPatchRoles(token, createdDTO.Uuid, roles3);
            Assert.Equal(HttpStatusCode.OK, put3.StatusCode);

            //Assert
            freshReadDTO = await ItContractV2Helper.GetItContractAsync(token, createdDTO.Uuid);
            AssertRoleAssignments(roles3, freshReadDTO);

            //Act - clear
            using var put4 = await ItContractV2Helper.SendPatchRoles(token, createdDTO.Uuid, roles4);
            Assert.Equal(HttpStatusCode.OK, put4.StatusCode);

            //Assert
            freshReadDTO = await ItContractV2Helper.GetItContractAsync(token, createdDTO.Uuid);
            AssertRoleAssignments(roles4, freshReadDTO);
        }

        [Fact]
        public async Task Can_POST_With_DataProcessingRegistrations()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dataProcessingRegistrationUuids = await CreateDataProcessingRegistrationUuids(token, organization);

            var request = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                DataProcessingRegistrationUuids = dataProcessingRegistrationUuids
            };

            //Act
            var dto = await ItContractV2Helper.PostContractAsync(token, request);

            //Assert
            var freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertMultiAssignment(dataProcessingRegistrationUuids, freshDTO.DataProcessingRegistrations);
        }

        [Fact]
        public async Task Cannot_POST_With_DataProcessingRegistrations_From_Different_Org()
        {
            //Arrange
            var (token1, _, organization1) = await CreatePrerequisitesAsync();
            var (token2, _, organization2) = await CreatePrerequisitesAsync();
            var dpr1 = await DataProcessingRegistrationV2Helper.PostAsync(token1, new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization1.Uuid
            });
            var dpr2 = await DataProcessingRegistrationV2Helper.PostAsync(token2, new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization2.Uuid
            });
            var request = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization1.Uuid,
                Name = CreateName(),
                DataProcessingRegistrationUuids = new[] { dpr1.Uuid, dpr2.Uuid }
            };
            // Using global admin as they have full access between organizations
            var globalAdminToken = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            //Act
            using var response = await ItContractV2Helper.SendPostContractAsync(globalAdminToken.Token, request);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_PATCH_With_DataProcessingRegistrations()
        {
            //Arrange
            var (token, _, organization) = await CreatePrerequisitesAsync();

            var dpr1 = await DataProcessingRegistrationV2Helper.PostAsync(token, new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid
            });
            var dpr2 = await DataProcessingRegistrationV2Helper.PostAsync(token, new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid
            });
            var dpr3 = await DataProcessingRegistrationV2Helper.PostAsync(token, new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid
            });

            var request = new CreateNewContractRequestDTO { Name = CreateName(), OrganizationUuid = organization.Uuid };
            var dto = await ItContractV2Helper.PostContractAsync(token, request);

            var assignment1 = new[] { dpr1.Uuid };
            var assignment2 = new[] { dpr1.Uuid, dpr2.Uuid };
            var assignment3 = new[] { dpr3.Uuid, dpr2.Uuid };
            var assignment4 = Array.Empty<Guid>();

            //Act
            await ItContractV2Helper.SendPatchDataProcessingRegistrationsAsync(token, dto.Uuid, assignment1).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            var freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertMultiAssignment(assignment1, freshDTO.DataProcessingRegistrations);

            //Act
            await ItContractV2Helper.SendPatchDataProcessingRegistrationsAsync(token, dto.Uuid, assignment2).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertMultiAssignment(assignment2, freshDTO.DataProcessingRegistrations);

            //Act
            await ItContractV2Helper.SendPatchDataProcessingRegistrationsAsync(token, dto.Uuid, assignment3).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertMultiAssignment(assignment3, freshDTO.DataProcessingRegistrations);

            //Act
            await ItContractV2Helper.SendPatchDataProcessingRegistrationsAsync(token, dto.Uuid, assignment4).WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            //Assert
            freshDTO = await ItContractV2Helper.GetItContractAsync(token, dto.Uuid);
            AssertMultiAssignment(assignment4, freshDTO.DataProcessingRegistrations);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public async Task Can_POST_With_AgreementPeriod(bool hasExtensionOption, bool isContinuous, bool hasIrrevocableDate)
        {
            //Arrange
            var (token, _, organization) = await CreatePrerequisitesAsync();
            var input = await CreateAgreementPeriodInput(hasExtensionOption, isContinuous, hasIrrevocableDate, organization);
            var request = new CreateNewContractRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                AgreementPeriod = input
            };

            //Act
            var dto = await ItContractV2Helper.PostContractAsync(token, request);

            //Assert
            var freshDTO = (await ItContractV2Helper.GetItContractAsync(token, dto.Uuid)).AgreementPeriod;
            AssertAgreementPeriod(input, freshDTO);
        }

        [Fact]
        public async Task Can_PATCH_With_AgreementPeriod()
        {
            //Arrange
            var (token, _, organization) = await CreatePrerequisitesAsync();
            var input1 = await CreateAgreementPeriodInput(true, true, true, organization);
            var input2 = await CreateAgreementPeriodInput(true, true, true, organization);
            var input3 = new ContractAgreementPeriodDataWriteRequestDTO();

            var request = new CreateNewContractRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
            };
            var dto = await ItContractV2Helper.PostContractAsync(token, request);

            //Act
            using var response1 = await ItContractV2Helper.SendPatchAgreementPeriodAsync(token, dto.Uuid, input1);
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            //Assert
            var freshDTO = (await ItContractV2Helper.GetItContractAsync(token, dto.Uuid)).AgreementPeriod;
            AssertAgreementPeriod(input1, freshDTO);

            //Act - change all again
            using var response2 = await ItContractV2Helper.SendPatchAgreementPeriodAsync(token, dto.Uuid, input2);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

            //Assert
            freshDTO = (await ItContractV2Helper.GetItContractAsync(token, dto.Uuid)).AgreementPeriod;
            AssertAgreementPeriod(input2, freshDTO);

            //Act - reset all
            using var response3 = await ItContractV2Helper.SendPatchAgreementPeriodAsync(token, dto.Uuid, input3);
            Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

            //Assert
            freshDTO = (await ItContractV2Helper.GetItContractAsync(token, dto.Uuid)).AgreementPeriod;
            AssertAgreementPeriod(input3, freshDTO);
        }

        [Fact]
        public async Task Can_POST_With_Payments()
        {
            //Arrange
            var (token, _, organization) = await CreatePrerequisitesAsync();
            var input = await CreatePaymentsInput(token, organization, true, true);
            var request = new CreateNewContractRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                Payments = input
            };

            //Act
            var dto = await ItContractV2Helper.PostContractAsync(token, request);

            //Assert
            var freshDTO = (await ItContractV2Helper.GetItContractAsync(token, dto.Uuid)).Payments;
            AssertPayments(input, freshDTO);
        }

        [Fact]
        public async Task Can_PATCH_Payments()
        {
            //Arrange
            var (token, _, organization) = await CreatePrerequisitesAsync();
            var input1 = await CreatePaymentsInput(token, organization, true, true);
            var input2 = await CreatePaymentsInput(token, organization, true, true);
            var input3 = await CreatePaymentsInput(token, organization, true, false);
            var input4 = await CreatePaymentsInput(token, organization, false, true);
            var input5 = await CreatePaymentsInput(token, organization, false, false);
            var input6 = new ContractPaymentsDataWriteRequestDTO();

            var request = new CreateNewContractRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
            };
            var dto = await ItContractV2Helper.PostContractAsync(token, request);

            //Act
            using var response1 = await ItContractV2Helper.SendPatchPayments(token, dto.Uuid, input1);
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            //Assert
            var freshDTO = (await ItContractV2Helper.GetItContractAsync(token, dto.Uuid)).Payments;
            AssertPayments(input1, freshDTO);

            //Act
            using var response2 = await ItContractV2Helper.SendPatchPayments(token, dto.Uuid, input2);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

            //Assert
            freshDTO = (await ItContractV2Helper.GetItContractAsync(token, dto.Uuid)).Payments;
            AssertPayments(input2, freshDTO);

            //Act
            using var response3 = await ItContractV2Helper.SendPatchPayments(token, dto.Uuid, input3);
            Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

            //Assert
            freshDTO = (await ItContractV2Helper.GetItContractAsync(token, dto.Uuid)).Payments;
            AssertPayments(input3, freshDTO);

            //Act
            using var response4 = await ItContractV2Helper.SendPatchPayments(token, dto.Uuid, input4);
            Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

            //Assert
            freshDTO = (await ItContractV2Helper.GetItContractAsync(token, dto.Uuid)).Payments;
            AssertPayments(input4, freshDTO);

            //Act
            using var response5 = await ItContractV2Helper.SendPatchPayments(token, dto.Uuid, input5);
            Assert.Equal(HttpStatusCode.OK, response5.StatusCode);

            //Assert
            freshDTO = (await ItContractV2Helper.GetItContractAsync(token, dto.Uuid)).Payments;
            AssertPayments(input5, freshDTO);

            //Act
            using var response6 = await ItContractV2Helper.SendPatchPayments(token, dto.Uuid, input6);
            Assert.Equal(HttpStatusCode.OK, response6.StatusCode);

            //Assert
            freshDTO = (await ItContractV2Helper.GetItContractAsync(token, dto.Uuid)).Payments;
            AssertPayments(input6, freshDTO);
        }

        [Theory]
        [InlineData(true, true, true, true)]
        [InlineData(true, true, true, false)]
        [InlineData(true, true, false, true)]
        [InlineData(true, false, true, true)]
        [InlineData(false, false, true, true)]
        [InlineData(false, false, false, false)]
        public async Task Can_POST_With_PaymentModel(bool withPaymentFrequencyType, bool withPaymentModelType, bool withPriceRegulationType, bool withMilestones)
        {
            //Arrange
            var (token, _, organization) = await CreatePrerequisitesAsync();

            var (paymentModelRequest, paymentFrequencyType, paymentModelType, priceRegulationType) = await CreatePaymentModelRequestAsync(organization.Uuid, withPaymentFrequencyType, withPaymentModelType, withPriceRegulationType, withMilestones);
            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                PaymentModel = paymentModelRequest
            };

            //Act
            var contractDTO = await ItContractV2Helper.PostContractAsync(token, requestDto);

            //Assert
            AssertPaymentModel(paymentModelRequest, paymentFrequencyType, paymentModelType, priceRegulationType, contractDTO.PaymentModel);
        }

        [Fact]
        public async Task Can_PATCH_With_PaymentModel()
        {
            //Arrange
            var (token, _, organization) = await CreatePrerequisitesAsync();

            var (paymentModelRequest1, paymentFrequencyType1, paymentModelType1, priceRegulationType1) = await CreatePaymentModelRequestAsync(organization.Uuid, true, true, true, true);
            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName()
            };
            var contractDTO = await ItContractV2Helper.PostContractAsync(token, requestDto);

            //Act
            using var response1 = await ItContractV2Helper.SendPatchPaymentModelAsync(token, contractDTO.Uuid, paymentModelRequest1);
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            //Assert
            var contractDTO1 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            AssertPaymentModel(paymentModelRequest1, paymentFrequencyType1, paymentModelType1, priceRegulationType1, contractDTO1.PaymentModel);

            //Act
            var (paymentModelRequest2, paymentFrequencyType2, paymentModelType2, priceRegulationType2) = await CreatePaymentModelRequestAsync(organization.Uuid, true, true, true, false);
            using var response2 = await ItContractV2Helper.SendPatchPaymentModelAsync(token, contractDTO.Uuid, paymentModelRequest2);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

            //Assert
            var contractDTO2 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            AssertPaymentModel(paymentModelRequest2, paymentFrequencyType2, paymentModelType2, priceRegulationType2, contractDTO2.PaymentModel);

            //Act
            var (paymentModelRequest3, paymentFrequencyType3, paymentModelType3, priceRegulationType3) = await CreatePaymentModelRequestAsync(organization.Uuid, true, true, false, false);
            using var response3 = await ItContractV2Helper.SendPatchPaymentModelAsync(token, contractDTO.Uuid, paymentModelRequest3);
            Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

            //Assert
            var contractDTO3 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            AssertPaymentModel(paymentModelRequest3, paymentFrequencyType3, paymentModelType3, priceRegulationType3, contractDTO3.PaymentModel);

            //Act
            var (paymentModelRequest4, paymentFrequencyType4, paymentModelType4, priceRegulationType4) = await CreatePaymentModelRequestAsync(organization.Uuid, true, false, false, false);
            using var response4 = await ItContractV2Helper.SendPatchPaymentModelAsync(token, contractDTO.Uuid, paymentModelRequest4);
            Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

            //Assert
            var contractDTO4 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            AssertPaymentModel(paymentModelRequest4, paymentFrequencyType4, paymentModelType4, priceRegulationType4, contractDTO4.PaymentModel);

            //Act
            var (paymentModelRequest5, paymentFrequencyType5, paymentModelType5, priceRegulationType5) = await CreatePaymentModelRequestAsync(organization.Uuid, false, false, false, false);
            using var response5 = await ItContractV2Helper.SendPatchPaymentModelAsync(token, contractDTO.Uuid, paymentModelRequest5);
            Assert.Equal(HttpStatusCode.OK, response5.StatusCode);

            //Assert
            var contractDTO5 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            AssertPaymentModel(paymentModelRequest5, paymentFrequencyType5, paymentModelType5, priceRegulationType5, contractDTO5.PaymentModel);

            //Act
            var paymentModelRequest6 = new ContractPaymentModelDataWriteRequestDTO();
            using var response6 = await ItContractV2Helper.SendPatchPaymentModelAsync(token, contractDTO.Uuid, paymentModelRequest6);
            Assert.Equal(HttpStatusCode.OK, response6.StatusCode);

            //Assert
            var contractDTO6 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            AssertPaymentModel(paymentModelRequest6, null, null, null, contractDTO6.PaymentModel);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_POST_With_Termination(bool withNoticePeriodMonth)
        {
            //Arrange
            var (token, _, organization) = await CreatePrerequisitesAsync();
            var (terminationRequest, noticePeriodMonthsType) = await CreateTerminationRequest(organization.Uuid, withNoticePeriodMonth);
            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                Termination = terminationRequest
            };

            //Act
            var contractDTO = await ItContractV2Helper.PostContractAsync(token, requestDto);

            //Assert
            AssertTermination(terminationRequest, noticePeriodMonthsType, contractDTO.Termination);
        }

        [Fact]
        public async Task Can_PATCH_With_Termination()
        {
            //Arrange
            var (token, _, organization) = await CreatePrerequisitesAsync();
            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName()
            };
            var contractDTO = await ItContractV2Helper.PostContractAsync(token, requestDto);

            var (terminationRequest1, noticePeriodMonthsType1) = await CreateTerminationRequest(organization.Uuid, true);

            //Act
            using var response1 = await ItContractV2Helper.SendPatchTerminationAsync(token, contractDTO.Uuid, terminationRequest1);
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            //Assert
            var contractDTO1 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            AssertTermination(terminationRequest1, noticePeriodMonthsType1, contractDTO1.Termination);

            //Act
            var (terminationRequest2, noticePeriodMonthsType2) = await CreateTerminationRequest(organization.Uuid, true);
            using var response2 = await ItContractV2Helper.SendPatchTerminationAsync(token, contractDTO.Uuid, terminationRequest2);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

            //Assert
            var contractDTO2 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            AssertTermination(terminationRequest2, noticePeriodMonthsType2, contractDTO2.Termination);

            //Act
            var (terminationRequest3, noticePeriodMonthsType3) = await CreateTerminationRequest(organization.Uuid, false);
            using var response3 = await ItContractV2Helper.SendPatchTerminationAsync(token, contractDTO.Uuid, terminationRequest3);
            Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

            //Assert
            var contractDTO3 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            AssertTermination(terminationRequest3, noticePeriodMonthsType3, contractDTO3.Termination);

            //Act
            var terminationRequest4 = new ContractTerminationDataWriteRequestDTO();
            using var response4 = await ItContractV2Helper.SendPatchTerminationAsync(token, contractDTO.Uuid, terminationRequest4);
            Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

            //Assert
            var contractDTO4 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            AssertTermination(terminationRequest4, null, contractDTO4.Termination);
        }

        [Fact]
        public async Task Can_POST_Full_Contract()
        {
            //Arrange
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs

            var (token, _, organization) = await CreatePrerequisitesAsync();
            var parent = await ItContractV2Helper.PostContractAsync(token, CreateNewSimpleRequest(organization.Uuid));
            var (procurementRequest, procurementStrategy, purchaseType) = await CreateProcurementRequestAsync(organization.Uuid);
            var (contractType, contractTemplateType, agreementElements, criticalityType, generalDataWriteRequest) = await CreateGeneralDataRequestDTO(organization, true, true, true, true);
            var contractResponsibleDataWriteRequest = await CreateContractResponsibleDataRequestDTO(token, organization, true, true, true);
            var supplierRequest = await CreateContractSupplierDataRequestDTO(true, true, true);
            var externalReferences = Many<ExternalReferenceDataWriteRequestDTO>().Transform(WithRandomMaster).ToList();
            var systemUsageUuids = await CreateSystemUsageUuids(token, organization);
            var roles = await CreateRoles(organization);
            var dataProcessingRegistrationUuids = await CreateDataProcessingRegistrationUuids(token, organization);
            var agreementPeriodRequest = await CreateAgreementPeriodInput(true, true, true, organization);
            var paymentsRequest = await CreatePaymentsInput(token, organization, true, true);
            var (paymentModelRequest, paymentFrequencyType, paymentModelType, priceRegulationType) = await CreatePaymentModelRequestAsync(organization.Uuid, true, true, true, true);
            var (terminationRequest, noticePeriodMonthsType) = await CreateTerminationRequest(organization.Uuid, true);

            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                ParentContractUuid = parent.Uuid,
                Procurement = procurementRequest,
                General = generalDataWriteRequest,
                Responsible = contractResponsibleDataWriteRequest,
                Supplier = supplierRequest,
                ExternalReferences = externalReferences,
                SystemUsageUuids = systemUsageUuids,
                Roles = roles,
                DataProcessingRegistrationUuids = dataProcessingRegistrationUuids,
                AgreementPeriod = agreementPeriodRequest,
                Payments = paymentsRequest,
                PaymentModel = paymentModelRequest,
                Termination = terminationRequest
            };

            //Act
            var contractDTO = await ItContractV2Helper.PostContractAsync(token, requestDto);

            //Assert
            Assert.Equal(organization.Name, contractDTO.OrganizationContext.Name);
            Assert.Equal(organization.Cvr, contractDTO.OrganizationContext.Cvr);
            Assert.Equal(organization.Uuid, contractDTO.OrganizationContext.Uuid);

            AssertCrossReference(parent, contractDTO.ParentContract);
            AssertProcurement(procurementRequest, procurementStrategy, purchaseType, contractDTO.Procurement);
            AssertGeneralDataSection(generalDataWriteRequest, contractType, contractTemplateType, agreementElements, criticalityType, contractDTO);
            AssertResponsible(contractResponsibleDataWriteRequest, contractDTO);
            AssertSupplier(supplierRequest, contractDTO);
            AssertExternalReferenceResults(externalReferences, contractDTO);
            AssertMultiAssignment(systemUsageUuids, contractDTO.SystemUsages);
            AssertRoleAssignments(roles, contractDTO);
            AssertMultiAssignment(dataProcessingRegistrationUuids, contractDTO.DataProcessingRegistrations);
            AssertAgreementPeriod(agreementPeriodRequest, contractDTO.AgreementPeriod);
            AssertPayments(paymentsRequest, contractDTO.Payments);
            AssertPaymentModel(paymentModelRequest, paymentFrequencyType, paymentModelType, priceRegulationType, contractDTO.PaymentModel);
            AssertTermination(terminationRequest, noticePeriodMonthsType, contractDTO.Termination);
        }

        [Fact]
        public async Task Can_PUT_All()
        {
            //Arrange
            Configure(f => f.Inject(false)); //Make sure no master is added when faking the inputs
            var (token, _, organization) = await CreatePrerequisitesAsync();

            var contractDTO = await ItContractV2Helper.PostContractAsync(token, CreateNewSimpleRequest(organization.Uuid));

            var parent1 = await ItContractV2Helper.PostContractAsync(token, CreateNewSimpleRequest(organization.Uuid));
            var (procurementRequest1, procurementStrategy1, purchaseType1) = await CreateProcurementRequestAsync(organization.Uuid);
            var (contractType1, contractTemplateType1, agreementElements1, criticalityType1, generalDataWriteRequest1) = await CreateGeneralDataRequestDTO(organization, true, true, true, true);
            var contractResponsibleDataWriteRequest1 = await CreateContractResponsibleDataRequestDTO(token, organization, true, true, true);
            var supplierRequest1 = await CreateContractSupplierDataRequestDTO(true, true, true);
            var externalReferences1 = CreateUpdateExternalReferences();
            var systemUsageUuids1 = await CreateSystemUsageUuids(token, organization);
            var roles1 = await CreateRoles(organization);
            var dataProcessingRegistrationUuids1 = await CreateDataProcessingRegistrationUuids(token, organization);
            var agreementPeriodRequest1 = await CreateAgreementPeriodInput(true, true, true, organization);
            var paymentsRequest1 = await CreatePaymentsInput(token, organization, true, true);
            var (paymentModelRequest1, paymentFrequencyType1, paymentModelType1, priceRegulationType1) = await CreatePaymentModelRequestAsync(organization.Uuid, true, true, true, true);
            var (terminationRequest1, noticePeriodMonthsType1) = await CreateTerminationRequest(organization.Uuid, true);
            var requestDto1 = new UpdateContractRequestDTO
            {
                Name = CreateName(),
                ParentContractUuid = parent1.Uuid,
                Procurement = procurementRequest1,
                General = generalDataWriteRequest1,
                Responsible = contractResponsibleDataWriteRequest1,
                Supplier = supplierRequest1,
                ExternalReferences = externalReferences1,
                SystemUsageUuids = systemUsageUuids1,
                Roles = roles1,
                DataProcessingRegistrationUuids = dataProcessingRegistrationUuids1,
                AgreementPeriod = agreementPeriodRequest1,
                Payments = paymentsRequest1,
                PaymentModel = paymentModelRequest1,
                Termination = terminationRequest1
            };

            //Act - Put on empty
            using var response1 = await ItContractV2Helper.SendPutContractAsync(token, contractDTO.Uuid, requestDto1);

            //Assert - Put on empty
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
            var contractDTO1 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);

            Assert.Equal(requestDto1.Name, contractDTO1.Name);
            AssertCrossReference(parent1, contractDTO1.ParentContract);
            AssertProcurement(procurementRequest1, procurementStrategy1, purchaseType1, contractDTO1.Procurement);
            AssertGeneralDataSection(generalDataWriteRequest1, contractType1, contractTemplateType1, agreementElements1, criticalityType1, contractDTO1);
            AssertResponsible(contractResponsibleDataWriteRequest1, contractDTO1);
            AssertSupplier(supplierRequest1, contractDTO1);
            AssertExternalReferenceResults(externalReferences1, contractDTO1, true);
            AssertMultiAssignment(systemUsageUuids1, contractDTO1.SystemUsages);
            AssertRoleAssignments(roles1, contractDTO1);
            AssertMultiAssignment(dataProcessingRegistrationUuids1, contractDTO1.DataProcessingRegistrations);
            AssertAgreementPeriod(agreementPeriodRequest1, contractDTO1.AgreementPeriod);
            AssertPayments(paymentsRequest1, contractDTO1.Payments);
            AssertPaymentModel(paymentModelRequest1, paymentFrequencyType1, paymentModelType1, priceRegulationType1, contractDTO1.PaymentModel);
            AssertTermination(terminationRequest1, noticePeriodMonthsType1, contractDTO1.Termination);

            //Arrange - Put on filled
            var parent2 = await ItContractV2Helper.PostContractAsync(token, CreateNewSimpleRequest(organization.Uuid));
            var (procurementRequest2, procurementStrategy2, purchaseType2) = await CreateProcurementRequestAsync(organization.Uuid);
            var (contractType2, contractTemplateType2, agreementElements2, criticalityType2, generalDataWriteRequest2) = await CreateGeneralDataRequestDTO(organization, true, true, true, true);
            var contractResponsibleDataWriteRequest2 = await CreateContractResponsibleDataRequestDTO(token, organization, true, true, true);
            var supplierRequest2 = await CreateContractSupplierDataRequestDTO(true, true, true);
            var externalReferences2 = CreateNewExternalReferenceDataWithOldUuid(contractDTO1.ExternalReferences);
            var systemUsageUuids2 = await CreateSystemUsageUuids(token, organization);
            var roles2 = await CreateRoles(organization);
            var dataProcessingRegistrationUuids2 = await CreateDataProcessingRegistrationUuids(token, organization);
            var agreementPeriodRequest2 = await CreateAgreementPeriodInput(true, true, true, organization);
            var paymentsRequest2 = await CreatePaymentsInput(token, organization, true, true);
            var (paymentModelRequest2, paymentFrequencyType2, paymentModelType2, priceRegulationType2) = await CreatePaymentModelRequestAsync(organization.Uuid, true, true, true, true);
            var (terminationRequest2, noticePeriodMonthsType2) = await CreateTerminationRequest(organization.Uuid, true);
            var requestDto2 = new UpdateContractRequestDTO()
            {
                Name = CreateName(),
                ParentContractUuid = parent2.Uuid,
                Procurement = procurementRequest2,
                General = generalDataWriteRequest2,
                Responsible = contractResponsibleDataWriteRequest2,
                Supplier = supplierRequest2,
                ExternalReferences = externalReferences2,
                SystemUsageUuids = systemUsageUuids2,
                Roles = roles2,
                DataProcessingRegistrationUuids = dataProcessingRegistrationUuids2,
                AgreementPeriod = agreementPeriodRequest2,
                Payments = paymentsRequest2,
                PaymentModel = paymentModelRequest2,
                Termination = terminationRequest2
            };

            //Act - Put on filled
            using var response2 = await ItContractV2Helper.SendPutContractAsync(token, contractDTO.Uuid, requestDto2);

            //Assert - Put on filled
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            var contractDTO2 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);

            Assert.Equal(requestDto2.Name, contractDTO2.Name);
            AssertCrossReference(parent2, contractDTO2.ParentContract);
            AssertProcurement(procurementRequest2, procurementStrategy2, purchaseType2, contractDTO2.Procurement);
            AssertGeneralDataSection(generalDataWriteRequest2, contractType2, contractTemplateType2, agreementElements2, criticalityType2, contractDTO2);
            AssertResponsible(contractResponsibleDataWriteRequest2, contractDTO2);
            AssertSupplier(supplierRequest2, contractDTO2);
            AssertExternalReferenceResults(externalReferences2, contractDTO2);
            AssertMultiAssignment(systemUsageUuids2, contractDTO2.SystemUsages);
            AssertRoleAssignments(roles2, contractDTO2);
            AssertMultiAssignment(dataProcessingRegistrationUuids2, contractDTO2.DataProcessingRegistrations);
            AssertAgreementPeriod(agreementPeriodRequest2, contractDTO2.AgreementPeriod);
            AssertPayments(paymentsRequest2, contractDTO2.Payments);
            AssertPaymentModel(paymentModelRequest2, paymentFrequencyType2, paymentModelType2, priceRegulationType2, contractDTO2.PaymentModel);
            AssertTermination(terminationRequest2, noticePeriodMonthsType2, contractDTO2.Termination);

            //Arrange - Put to reset
            var requestDto3 = new UpdateContractRequestDTO()
            {
                Name = CreateName()
            };

            //Act - Put to reset
            using var response3 = await ItContractV2Helper.SendPutContractAsync(token, contractDTO.Uuid, requestDto3);

            //Assert - Put to reset
            Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
            var contractDTO3 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);

            Assert.Equal(requestDto3.Name, contractDTO3.Name);
            AssertCrossReference((ItContractResponseDTO)null, contractDTO3.ParentContract);
            AssertProcurement(requestDto3.Procurement, null, null, contractDTO3.Procurement);
            AssertGeneralDataSection(requestDto3.General, null, null, null, null, contractDTO3);

            var responsibleResponse = contractDTO3.Responsible;
            Assert.Null(responsibleResponse.OrganizationUnit);
            Assert.Null(responsibleResponse.SignedAt);
            Assert.Null(responsibleResponse.SignedBy);
            Assert.False(responsibleResponse.Signed);

            var supplierResponse = contractDTO3.Supplier;
            Assert.Null(supplierResponse.Organization);
            Assert.Null(supplierResponse.SignedAt);
            Assert.Null(supplierResponse.SignedBy);
            Assert.False(supplierResponse.Signed);

            Assert.Empty(contractDTO3.ExternalReferences);
            Assert.Empty(contractDTO3.SystemUsages);
            Assert.Empty(contractDTO3.Roles);
            Assert.Empty(contractDTO3.DataProcessingRegistrations);
            Assert.Empty(contractDTO3.Payments.External);
            Assert.Empty(contractDTO3.Payments.Internal);

            var agreementPeriod = contractDTO3.AgreementPeriod;
            Assert.Null(agreementPeriod.DurationMonths);
            Assert.Null(agreementPeriod.DurationYears);
            Assert.Null(agreementPeriod.ExtensionOptions);
            Assert.Null(agreementPeriod.IrrevocableUntil);
            Assert.False(agreementPeriod.IsContinuous);
            Assert.Equal(0, agreementPeriod.ExtensionOptionsUsed);

            AssertPaymentModel(requestDto3.PaymentModel, null, null, null, contractDTO3.PaymentModel);
            AssertTermination(requestDto3.Termination, null, contractDTO3.Termination);
        }

        [Fact]
        public async Task Can_DELETE_Contract()
        {
            //Arrange
            var (token, _, organization) = await CreatePrerequisitesAsync();
            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName()
            };
            var contractDTO = await ItContractV2Helper.PostContractAsync(token, requestDto);

            //Act
            using var response = await ItContractV2Helper.SendDeleteContractAsync(token, contractDTO.Uuid);
            var res = await response.Content.ReadAsStringAsync();
            //Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            using var getResponse = await ItContractV2Helper.SendGetItContractAsync(token, contractDTO.Uuid);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task Cannot_DELETE_Contract_If_Not_Allowed()
        {
            //Arrange
            var (token1, _, organization1) = await CreatePrerequisitesAsync();
            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization1.Uuid,
                Name = CreateName()
            };
            var contractDTO = await ItContractV2Helper.PostContractAsync(token1, requestDto);
            var (token2, _, _) = await CreatePrerequisitesAsync();

            //Act
            using var response = await ItContractV2Helper.SendDeleteContractAsync(token2, contractDTO.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_DELETE_Contract_If_Not_Exists()
        {
            //Arrange
            var (token, _, _) = await CreatePrerequisitesAsync();

            //Act
            using var response = await ItContractV2Helper.SendDeleteContractAsync(token, A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true, true, true)]
        [InlineData(OrganizationRole.LocalAdmin, true, true, true)]
        [InlineData(OrganizationRole.User, true, false, false)]
        public async Task Can_Get_ItContract_Permissions(OrganizationRole role, bool read, bool modify, bool delete)
        {
            //Arrange
            var org = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUserAsync(org.Uuid);

            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, role, org.Uuid).DisposeAsync();

            var contract = await CreateItContractAsync(org.Uuid);

            //Act
            var permissionsResponseDto = await ItContractV2Helper.GetPermissionsAsync(token, contract.Uuid);

            //Assert
            var expected = new ItContractPermissionsResponseDTO
            {
                Read = read,
                Modify = modify,
                Delete = delete
            };
            Assert.Equivalent(expected, permissionsResponseDto);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.User, false)]
        public async Task Can_Get_ItContract_CollectionPermissions(OrganizationRole role, bool create)
        {
            //Arrange
            var org = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUserAsync(org.Uuid);

            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, role, org.Uuid).DisposeAsync();

            //Act
            var permissionsResponseDto = await ItContractV2Helper.GetCollectionPermissionsAsync(token, org.Uuid);

            //Assert
            var expected = new ResourceCollectionPermissionsResponseDTO
            {
                Create = create
            };
            Assert.Equivalent(expected, permissionsResponseDto);
        }

        [Theory]
        [InlineData(false, false, true)]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        public async Task Can_Get_Validation_Details(bool expectDateError, bool enforceValid, bool expectValid)
        {
            //Arrange
            var (token, _, org) = await CreatePrerequisitesAsync();
            var itContractDto = await CreateItContractAsync(org.Uuid);

            await ItContractV2Helper.SendPatchContractGeneralDataAsync(await GetGlobalToken(), itContractDto.Uuid,
                new ContractGeneralDataWriteRequestDTO
                {
                    Validity = new ContractValidityWriteRequestDTO
                    {
                        ValidFrom = DateTime.Now.AddDays(expectDateError ? 1 : -1),
                        EnforcedValid = enforceValid
                    }
                });

            //Act
            var result = await ItContractV2Helper.GetItContractAsync(token, itContractDto.Uuid);

            //Assert
            Assert.Equal(expectValid, result.General.Validity.Valid);
            Assert.Equal(enforceValid, result.General.Validity.EnforcedValid);
            if (expectDateError)
            {
                var dateError = Assert.Single(result.General.Validity.ValidationErrors);
                Assert.Equal(ItContractValidationError.StartDateNotPassed.ToItContractValidationErrorChoice(), dateError);
            }
            else
            {
                Assert.Empty(result.General.Validity.ValidationErrors);
            }
        }

        [Fact]
        public async Task Can_Create_Update_And_Delete_ExternalReference()
        {
            //Arrange
            var (token, _, organization) = await CreatePrerequisitesAsync();
            var requestDto = new CreateNewContractRequestDTO
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName()
            };
            var request = new ExternalReferenceDataWriteRequestDTO
            {
                DocumentId = A<string>(),
                MasterReference = A<bool>(),
                Title = A<string>(),
                Url = A<string>()
            };
            var contract = await ItContractV2Helper.PostContractAsync(token, requestDto);

            //Act
            var createdReference = await ItContractV2Helper.AddExternalReferenceAsync(token, contract.Uuid, request);

            //Assert
            AssertExternalReference(request, createdReference);

            var afterCreate = await ItContractV2Helper.GetItContractAsync(token, contract.Uuid);

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
            var updatedReference = await ItContractV2Helper.UpdateExternalReferenceAsync(token, contract.Uuid, createdReference.Uuid, updateRequest);

            //Assert - update
            AssertExternalReference(updateRequest, updatedReference);

            var afterUpdate = await ItContractV2Helper.GetItContractAsync(token, contract.Uuid);

            var checkUpdatedExternalReference = Assert.Single(afterUpdate.ExternalReferences);
            AssertExternalReference(updateRequest, checkUpdatedExternalReference);

            //Act - delete
            await ItContractV2Helper.DeleteExternalReferenceAsync(token, contract.Uuid, createdReference.Uuid);

            //Assert - delete
            var afterDelete = await ItContractV2Helper.GetItContractAsync(token, contract.Uuid);
            Assert.Empty(afterDelete.ExternalReferences);
        }

        [Fact]
        public async Task Contract_Is_Invalid_If_Parent_Is_Invalid()
        {
            var now = DateTime.Now;
            var (token, _, organization) = await CreatePrerequisitesAsync();
            var request = new CreateNewContractRequestDTO
            {
                Name = A<string>(),
                OrganizationUuid = organization.Uuid,
                General = new ContractGeneralDataWriteRequestDTO
                { Validity = new ContractValidityWriteRequestDTO { ValidTo = now.AddDays(-1) } }
            };
            var invalidContract = await ItContractV2Helper.PostContractAsync(token, request);

            var response = await ItContractV2Helper.PostContractAsync(token, new CreateNewContractRequestDTO { Name = A<string>(), OrganizationUuid = organization.Uuid, ParentContractUuid = invalidContract.Uuid, General = new ContractGeneralDataWriteRequestDTO { Validity = new ContractValidityWriteRequestDTO { RequireValidParent = true } } });

            Assert.False(response.General.Validity.Valid);
            var error = Assert.Single(response.General.Validity.ValidationErrors);
            Assert.Equal(ItContractValidationErrorChoice.InvalidParentContract, error);
        }

        private async Task<List<Guid>> CreateDataProcessingRegistrationUuids(string token, ShallowOrganizationResponseDTO organization)
        {
            var dpr1 = await DataProcessingRegistrationV2Helper.PostAsync(token, new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid
            });
            var dpr2 = await DataProcessingRegistrationV2Helper.PostAsync(token, new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid
            });
            return new List<Guid> { dpr1.Uuid, dpr2.Uuid };
        }

        private async Task<List<RoleAssignmentRequestDTO>> CreateRoles(ShallowOrganizationResponseDTO organization)
        {
            var user1 = await CreateApiUserAsync(organization.Uuid);
            var user2 = await CreateApiUserAsync(organization.Uuid);
            var contractRoles = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItContractRoles, organization.Uuid, 10, 0)).RandomItems(2).ToList();
            var role1 = contractRoles.First();
            var role2 = contractRoles.Last();
            var roles = new List<RoleAssignmentRequestDTO>
            {
                new()
                {
                    RoleUuid = role1.Uuid,
                    UserUuid = user1.user.Uuid
                },
                new()
                {
                    RoleUuid = role2.Uuid,
                    UserUuid = user2.user.Uuid
                }
            };
            return roles;
        }

        private List<UpdateExternalReferenceDataWriteRequestDTO> CreateUpdateExternalReferences()
        {
            return Many<UpdateExternalReferenceDataWriteRequestDTO>()
                .Transform(WithRandomMaster)
                .Select(x =>
                {
                    x.Uuid = null;
                    return x;
                }).ToList();
        }

        private List<UpdateExternalReferenceDataWriteRequestDTO> CreateNewExternalReferenceDataWithOldUuid(IEnumerable<ExternalReferenceDataResponseDTO> createExternalReferences)
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

        private async Task<List<Guid>> CreateSystemUsageUuids(string token, ShallowOrganizationResponseDTO organization)
        {
            var system1 = await CreateItSystemAsync(organization.Uuid);
            var system2 = await CreateItSystemAsync(organization.Uuid);
            var system1Usage = await TakeSystemIntoUsageAsync(system1.Uuid, organization.Uuid);
            var system2Usage = await TakeSystemIntoUsageAsync(system2.Uuid, organization.Uuid);

            return new List<Guid> { system1Usage.Uuid, system2Usage.Uuid };
        }

        private static void AssertTermination(ContractTerminationDataWriteRequestDTO expected, IdentityNamePairResponseDTO noticePeriodMonthsType, ContractTerminationDataResponseDTO actual)
        {
            Assert.Equal(expected?.TerminatedAt, actual.TerminatedAt);
            Assert.Equal(expected?.Terms?.NoticePeriodExtendsCurrent, actual.Terms.NoticePeriodExtendsCurrent);
            Assert.Equal(expected?.Terms?.NoticeByEndOf, actual.Terms.NoticeByEndOf);
            AssertCrossReference(noticePeriodMonthsType, actual.Terms.NoticePeriodMonths);
        }

        private async Task<(ContractTerminationDataWriteRequestDTO terminationRequest, IdentityNamePairResponseDTO noticePeriodMonthsType)> CreateTerminationRequest(Guid organizationUuid, bool withNoticePeriodMonth)
        {
            var noticePeriodMonths = withNoticePeriodMonth
                ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItContractNoticePeriodMonthTypes,
                    organizationUuid, 10, 0)).RandomItem()
                : null;

            var terminationRequest = new ContractTerminationDataWriteRequestDTO()
            {
                TerminatedAt = A<DateTime>(),
                Terms = new ContractTerminationTermsRequestDTO()
                {
                    NoticePeriodMonthsUuid = noticePeriodMonths?.Uuid,
                    NoticePeriodExtendsCurrent = A<YearSegmentChoice>(),
                    NoticeByEndOf = A<YearSegmentChoice>()
                }
            };

            return (terminationRequest, noticePeriodMonths);
        }

        private static void AssertPaymentModel(ContractPaymentModelDataWriteRequestDTO expected, IdentityNamePairResponseDTO paymentFrequencyType, IdentityNamePairResponseDTO paymentModelType, IdentityNamePairResponseDTO priceRegulationType, ContractPaymentModelDataResponseDTO actual)
        {
            Assert.Equal(expected?.OperationsRemunerationStartedAt, actual.OperationsRemunerationStartedAt);
            AssertCrossReference(paymentFrequencyType, actual.PaymentFrequency);
            AssertCrossReference(paymentModelType, actual.PaymentModel);
            AssertCrossReference(priceRegulationType, actual.PriceRegulation);
        }

        private async Task<(
            ContractPaymentModelDataWriteRequestDTO paymentModelRequest,
            IdentityNamePairResponseDTO paymentFrequencyType,
            IdentityNamePairResponseDTO paymentModelType,
            IdentityNamePairResponseDTO priceRegulationType)>
            CreatePaymentModelRequestAsync(Guid organizationUuid, bool withPaymentFrequencyType, bool withPaymentModelType, bool withPriceRegulationType, bool withMilestones)
        {
            var paymentFrequencyType = withPaymentFrequencyType
                ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItContractPaymentFrequencyTypes,
                    organizationUuid, 10, 0)).RandomItem()
                : null;

            var paymentModelType = withPaymentModelType
                ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItContractPaymentModelTypes,
                    organizationUuid, 10, 0)).RandomItem()
                : null;

            var priceRegulationType = withPriceRegulationType
                ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItContractPriceRegulationTypes,
                    organizationUuid, 10, 0)).RandomItem()
                : null;

            var paymentModelRequest = new ContractPaymentModelDataWriteRequestDTO()
            {
                OperationsRemunerationStartedAt = A<DateTime>(),
                PaymentFrequencyUuid = paymentFrequencyType?.Uuid,
                PaymentModelUuid = paymentModelType?.Uuid,
                PriceRegulationUuid = priceRegulationType?.Uuid,
            };

            return (paymentModelRequest, paymentFrequencyType, paymentModelType, priceRegulationType);
        }

        private static void AssertMultiAssignment(IEnumerable<Guid> expected, IEnumerable<IdentityNamePairResponseDTO> actual)
        {
            var expectedUuids = (expected ?? Array.Empty<Guid>()).OrderBy(x => x).ToList();
            var actualUuids = actual.Select(x => x.Uuid).OrderBy(x => x).ToList();
            Assert.Equal(expectedUuids.Count, actualUuids.Count);
            Assert.Equal(expectedUuids, actualUuids);
        }

        private async Task<ContractSupplierDataWriteRequestDTO> CreateContractSupplierDataRequestDTO(bool withOrg, bool withSignedAt, bool withSignedBy)
        {
            var supplierOrganization = withOrg
                ? await OrganizationV2Helper.GetOrganizationAsync(await GetGlobalToken(), DefaultOrgUuid)
                : null;
            var contractResponsibleDataWriteRequestDto = new ContractSupplierDataWriteRequestDTO
            {
                Signed = A<bool>(),
                SignedAt = withSignedAt ? A<DateTime>() : null,
                SignedBy = withSignedBy ? A<string>() : null,
                OrganizationUuid = supplierOrganization?.Uuid
            };
            return contractResponsibleDataWriteRequestDto;
        }

        private async Task<(IdentityNamePairResponseDTO contractType, IdentityNamePairResponseDTO contractTemplateType, List<IdentityNamePairResponseDTO> agreementElements, IdentityNamePairResponseDTO criticalityType, ContractGeneralDataWriteRequestDTO generalDataWriteRequestDto)> CreateGeneralDataRequestDTO(ShallowOrganizationResponseDTO organization, bool withContractType, bool withContractTemplate, bool withAgreementElements, bool withCriticalityType)
        {
            var contractType = withContractType
                ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItContractContractTypes,
                    organization.Uuid, 10, 0)).RandomItem()
                : null;
            var contractTemplateType = withContractTemplate
                ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItContractContractTemplateTypes,
                    organization.Uuid, 10, 0)).RandomItem()
                : null;
            var agreementElements = withAgreementElements
                ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItContractAgreementElementTypes,
                    organization.Uuid, 10, 0)).RandomItems(2).ToList()
                : null;
            var criticalityType = withCriticalityType
                ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.CriticalityTypes,
                    organization.Uuid, 10, 0)).RandomItem()
                : null;
            var generalDataWriteRequestDto = new ContractGeneralDataWriteRequestDTO()
            {
                Notes = A<string>(),
                ContractId = A<string>(),
                ContractTypeUuid = contractType?.Uuid,
                ContractTemplateUuid = contractTemplateType?.Uuid,
                CriticalityUuid = criticalityType?.Uuid,
                AgreementElementUuids = agreementElements?.Select(x => x.Uuid).ToList(),
                Validity = new ContractValidityWriteRequestDTO()
                {
                    ValidFrom = DateTime.Now,
                    EnforcedValid = A<bool>(),
                    ValidTo = DateTime.Now.AddDays(2),
                    RequireValidParent = A<bool>()
                }
            };

            return (contractType, contractTemplateType, agreementElements, criticalityType, generalDataWriteRequestDto);
        }

        private static void AssertGeneralDataSection(
            ContractGeneralDataWriteRequestDTO request,
            IdentityNamePairResponseDTO expectedContractType,
            IdentityNamePairResponseDTO expectedContractTemplateType,
            List<IdentityNamePairResponseDTO> expectedAgreementElements,
            IdentityNamePairResponseDTO criticalityType,
            ItContractResponseDTO freshDTO)
        {
            Assert.Equal(request?.Notes, freshDTO.General.Notes);
            Assert.Equal(request?.ContractId, freshDTO.General.ContractId);
            AssertCrossReference(expectedContractType, freshDTO.General.ContractType);
            AssertCrossReference(expectedContractTemplateType, freshDTO.General.ContractTemplate);
            Assert.Equal(request?.Validity?.ValidTo?.Date, freshDTO.General.Validity?.ValidTo);
            Assert.Equal(request?.Validity?.ValidFrom?.Date, freshDTO.General.Validity?.ValidFrom);
            Assert.Equal(request?.Validity?.EnforcedValid == true, freshDTO.General.Validity?.EnforcedValid == true);
            Assert.Equal(request?.Validity?.RequireValidParent == true, freshDTO.General.Validity?.RequireValidParent == true);
            AssertCrossReference(criticalityType, freshDTO.General.Criticality);

            if (expectedAgreementElements == null)
                Assert.Empty(freshDTO.General.AgreementElements);
            else
            {
                var expectedElements = expectedAgreementElements.OrderBy(x => x.Uuid).ToList();
                var actualAgreementElements = freshDTO.General.AgreementElements.OrderBy(x => x.Uuid).ToList();
                Assert.Equal(expectedElements.Count, actualAgreementElements.Count);
                for (var i = 0; i < expectedElements.Count; i++)
                {
                    AssertCrossReference(expectedElements[i], actualAgreementElements[i]);
                }
            }
        }

        private string CreateName()
        {
            return $"{nameof(ItContractsApiV2Test)}æøå{A<string>()}";
        }

        private static void AssertCrossReference<TExpected, TActual>(TExpected expected, TActual actual) where TExpected : IHasNameExternal, IHasUuidExternal where TActual : IHasNameExternal, IHasUuidExternal
        {
            Assert.Equal(expected?.Uuid, actual?.Uuid);
            Assert.Equal(expected?.Name, actual?.Name);
        }

        private static void AssertExpectedShallowContracts(ItContractResponseDTO expectedContent, ShallowOrganizationResponseDTO expectedOrganization, IEnumerable<ItContractResponseDTO> dtos)
        {
            var dto = Assert.Single(dtos, contract => contract.Uuid == expectedContent.Uuid);
            AssertExpectedShallowContract(expectedContent, expectedOrganization, dto);
        }

        private static void AssertExpectedShallowContract(ItContractResponseDTO expectedContent, ShallowOrganizationResponseDTO expectedOrganization, ItContractResponseDTO dto)
        {
            Assert.Equal(expectedContent.Uuid, dto.Uuid);
            Assert.Equal(expectedContent.Name, dto.Name);
            Assert.Equal(expectedOrganization.Uuid, dto.OrganizationContext.Uuid);
            Assert.Equal(expectedOrganization.Name, dto.OrganizationContext.Name);
            Assert.Equal(expectedOrganization.Cvr, dto.OrganizationContext.Cvr);
        }

        private async Task<(string token, User user, ShallowOrganizationResponseDTO organization)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUserAsync(organization.Uuid);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();
            return (token, user, organization);
        }
        private async Task<(User user, string token)> CreateApiUserAsync(Guid organizationUuid)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organizationUuid, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ByUuid(userAndGetToken.userUuid));
            return (user, userAndGetToken.token);
        }
        private IEnumerable<ExternalReferenceDataWriteRequestDTO> WithRandomMaster(IEnumerable<ExternalReferenceDataWriteRequestDTO> references)
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

        private static void AssertExternalReferenceResults(IReadOnlyCollection<UpdateExternalReferenceDataWriteRequestDTO> expected, ItContractResponseDTO actual, bool ignoreUuid = false)
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

        private static void AssertExternalReferenceResults(IReadOnlyCollection<ExternalReferenceDataWriteRequestDTO> expected, ItContractResponseDTO actual)
        {
            Assert.Equal(expected.Count, actual.ExternalReferences.Count());
            expected.OrderBy(x => x.DocumentId).ToList().ToExpectedObject()
                .ShouldMatch(actual.ExternalReferences.OrderBy(x => x.DocumentId).ToList());
        }


        private static void AssertResponsible(ContractResponsibleDataWriteRequestDTO contractResponsibleDataWriteRequestDto, ItContractResponseDTO freshDTO)
        {
            Assert.Equal(contractResponsibleDataWriteRequestDto.OrganizationUnitUuid, freshDTO.Responsible.OrganizationUnit?.Uuid);
            Assert.Equal(contractResponsibleDataWriteRequestDto.Signed, freshDTO.Responsible.Signed);
            Assert.Equal(contractResponsibleDataWriteRequestDto.SignedAt?.Date, freshDTO.Responsible.SignedAt);
            Assert.Equal(contractResponsibleDataWriteRequestDto.SignedBy, freshDTO.Responsible.SignedBy);
        }

        private static void AssertSupplier(ContractSupplierDataWriteRequestDTO contractResponsibleDataWriteRequestDto, ItContractResponseDTO freshDTO)
        {
            Assert.Equal(contractResponsibleDataWriteRequestDto.OrganizationUuid, freshDTO.Supplier.Organization?.Uuid);
            Assert.Equal(contractResponsibleDataWriteRequestDto.Signed, freshDTO.Supplier.Signed);
            Assert.Equal(contractResponsibleDataWriteRequestDto.SignedAt?.Date, freshDTO.Supplier.SignedAt);
            Assert.Equal(contractResponsibleDataWriteRequestDto.SignedBy, freshDTO.Supplier.SignedBy);
        }

        private async Task<ContractResponsibleDataWriteRequestDTO> CreateContractResponsibleDataRequestDTO(string token, ShallowOrganizationResponseDTO organization, bool withOrgUnit, bool withSignedAt, bool withSignedBy)
        {
            var organizationUnit = withOrgUnit
                ? (await OrganizationUnitV2Helper.GetOrganizationUnitsAsync(token, organization.Uuid, 0, 10)).RandomItem()
                : null;
            var contractResponsibleDataWriteRequestDto = new ContractResponsibleDataWriteRequestDTO
            {
                Signed = A<bool>(),
                SignedAt = withSignedAt ? A<DateTime>() : null,
                SignedBy = withSignedBy ? A<string>() : null,
                OrganizationUnitUuid = organizationUnit?.Uuid
            };
            return contractResponsibleDataWriteRequestDto;
        }

        private static void AssertProcurement(ContractProcurementDataWriteRequestDTO expected, IdentityNamePairResponseDTO procurementStrategy, IdentityNamePairResponseDTO purchaseType, ContractProcurementDataResponseDTO actual)
        {
            AssertCrossReference(procurementStrategy, actual.ProcurementStrategy);
            AssertCrossReference(purchaseType, actual.PurchaseType);
            if (expected?.ProcurementPlan == null)
            {
                Assert.Null(actual.ProcurementPlan);
            }
            else
            {
                Assert.Equal(expected.ProcurementPlan.QuarterOfYear, actual.ProcurementPlan.QuarterOfYear);
                Assert.Equal(expected.ProcurementPlan.Year, actual.ProcurementPlan.Year);
                Assert.Equal(expected.ProcurementInitiated, actual.ProcurementInitiated);
            }
        }

        private async Task<(ContractProcurementDataWriteRequestDTO request, IdentityNamePairResponseDTO procurementStrategy, IdentityNamePairResponseDTO purchaseType)> CreateProcurementRequestAsync(Guid organizationUuid)
        {
            var procurementStrategy = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItContractProcurementStrategyTypes, organizationUuid, 10, 0)).RandomItem();
            var purchaseType = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItContractPurchaseTypes, organizationUuid, 10, 0)).RandomItem();
            var request = new ContractProcurementDataWriteRequestDTO
            {
                ProcurementStrategyUuid = procurementStrategy.Uuid,
                PurchaseTypeUuid = purchaseType.Uuid,
                ProcurementInitiated = A<YesNoUndecidedChoice>(),
                ProcurementPlan = new ProcurementPlanDTO
                {
                    QuarterOfYear = Convert.ToByte((A<int>() % 4) + 1),
                    Year = A<int>()
                }
            };
            return (request, procurementStrategy, purchaseType);
        }

        private CreateNewContractRequestDTO CreateNewSimpleRequest(Guid organizationUuid)
        {
            return new CreateNewContractRequestDTO()
            {
                Name = CreateName(),
                OrganizationUuid = organizationUuid
            };
        }

        private static void AssertPayments(ContractPaymentsDataWriteRequestDTO input, ContractPaymentsDataResponseDTO freshDto)
        {
            AssertPaymentStream(input.Internal, freshDto.Internal);
            AssertPaymentStream(input.External, freshDto.External);
        }

        private static void AssertPaymentStream(IEnumerable<PaymentRequestDTO> inputExternal, IEnumerable<PaymentResponseDTO> outputPayments)
        {
            var expectedPayments = (inputExternal?.ToList() ?? new List<PaymentRequestDTO>()).OrderBy(x => x.AccountingEntry).ToList();
            var actualPayments = (outputPayments?.ToList() ?? new List<PaymentResponseDTO>()).OrderBy(x => x.AccountingEntry).ToList();
            Assert.Equal(expectedPayments.Count, actualPayments.Count);
            for (var i = 0; i < expectedPayments.Count; i++)
            {
                var expected = expectedPayments[i];
                var actual = actualPayments[i];
                Assert.Equal(expected.AccountingEntry, actual.AccountingEntry);
                Assert.Equal(expected.Acquisition, actual.Acquisition);
                Assert.Equal(expected.Note, actual.Note);
                Assert.Equal(expected.Other, actual.Other);
                Assert.Equal(expected.AuditDate?.Date, actual.AuditDate);
                Assert.Equal(expected.AuditStatus, actual.AuditStatus);
                Assert.Equal(expected.Operation, actual.Operation);
                Assert.Equal(expected.OrganizationUnitUuid, actual.OrganizationUnit?.Uuid);
            }
        }

        private async Task<ContractPaymentsDataWriteRequestDTO> CreatePaymentsInput(string token, ShallowOrganizationResponseDTO organization, bool withExternal, bool withInternal)
        {
            List<PaymentRequestDTO> internalPayments = null;
            List<PaymentRequestDTO> externalPayments = null;
            var organizationUnits = (await OrganizationUnitV2Helper.GetOrganizationUnitsAsync(token, organization.Uuid, 0, 10)).ToList();

            if (withInternal)
            {
                internalPayments = Many<PaymentRequestDTO>(5).Select(x => WithOrganizationUnit(x, organizationUnits.RandomItem())).ToList();
                internalPayments.Skip(2).First().OrganizationUnitUuid = null;
            }
            if (withExternal)
            {
                externalPayments = Many<PaymentRequestDTO>(5).Select(x => WithOrganizationUnit(x, organizationUnits.RandomItem())).ToList();
                externalPayments.Skip(2).First().OrganizationUnitUuid = null;
            }

            var input = new ContractPaymentsDataWriteRequestDTO
            {
                Internal = internalPayments,
                External = externalPayments
            };
            return input;
        }

        private static PaymentRequestDTO WithOrganizationUnit(PaymentRequestDTO itContractPayment, OrganizationUnitResponseDTO organizationUnit)
        {
            itContractPayment.OrganizationUnitUuid = organizationUnit.Uuid;
            return itContractPayment;
        }

        private static void AssertAgreementPeriod(ContractAgreementPeriodDataWriteRequestDTO input,
            ContractAgreementPeriodDataResponseDTO freshDTO)
        {
            Assert.Equal(input.DurationMonths, freshDTO.DurationMonths);
            Assert.Equal(input.DurationYears, freshDTO.DurationYears);
            Assert.Equal(input.IsContinuous, freshDTO.IsContinuous);
            Assert.Equal(input.ExtensionOptionsUsed, freshDTO.ExtensionOptionsUsed);
            Assert.Equal(input.ExtensionOptionsUuid, freshDTO.ExtensionOptions?.Uuid);
            Assert.Equal(input.IrrevocableUntil, freshDTO.IrrevocableUntil);
        }

        private async Task<ContractAgreementPeriodDataWriteRequestDTO> CreateAgreementPeriodInput(bool hasExtensionOption, bool isContinuous, bool hasIrrevocableDate,
            ShallowOrganizationResponseDTO organization)
        {
            var extensionOption = hasExtensionOption
                ? (await OptionV2ApiHelper.GetOptionsAsync(
                    OptionV2ApiHelper.ResourceName.ItContractAgreementExtensionOptionTypes, organization.Uuid, 10, 0))
                .RandomItem()
                : null;
            var input = new ContractAgreementPeriodDataWriteRequestDTO
            {
                DurationMonths = isContinuous ? null : A<int>() % 12,
                DurationYears = isContinuous ? null : Math.Abs(A<int>()),
                IsContinuous = isContinuous,
                ExtensionOptionsUsed = Math.Abs(A<int>()),
                IrrevocableUntil = hasIrrevocableDate ? A<DateTime>() : null,
                ExtensionOptionsUuid = extensionOption?.Uuid
            };
            return input;
        }

        private static void AssertRoleAssignments(IEnumerable<RoleAssignmentRequestDTO> input, ItContractResponseDTO output)
        {
            var actualRoles = output.Roles.OrderBy(x => x.Role.Uuid).ThenBy(x => x.User.Uuid).ToList();
            var expectedRoles = input.OrderBy(x => x.RoleUuid).ThenBy(x => x.UserUuid).ToList();
            Assert.Equal(expectedRoles.Count, expectedRoles.Count);
            for (var i = 0; i < actualRoles.Count; i++)
            {
                var expected = expectedRoles[i];
                var actual = actualRoles[i];
                AssertRoleAssignment(expected, actual);
            }
        }

        private static void AssertRoleAssignment(RoleAssignmentRequestDTO expected, RoleAssignmentResponseDTO actual)
        {
            Assert.Equal(expected.RoleUuid, actual.Role?.Uuid);
            Assert.Equal(expected.UserUuid, actual.User?.Uuid);
        }

        private static void AssertExternalReference(ExternalReferenceDataWriteRequestDTO expected, ExternalReferenceDataResponseDTO actual)
        {
            Assert.Equal(expected.DocumentId, actual.DocumentId);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.Url, actual.Url);
            Assert.Equal(expected.MasterReference, actual.MasterReference);
        }
    }
}
