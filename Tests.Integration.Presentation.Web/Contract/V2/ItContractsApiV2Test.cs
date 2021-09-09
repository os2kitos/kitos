using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Response.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.SharedProperties;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Contract.V2
{
    public class ItContractsApiV2Test : WithAutoFixture
    {
        [Fact]
        public async Task Can_GET_Specific_Contract()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var newContract = await CreateContractAsync(organization.Id);

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
            var newContract = await CreateContractAsync(organization2.Id);

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
            var contract1 = await CreateContractAsync(organization.Id);
            var contract2 = await CreateContractAsync(organization.Id);

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
            var contract1 = await CreateContractAsync(organization.Id);
            var contract2 = await CreateContractAsync(organization.Id);
            var contract3 = await CreateContractAsync(organization.Id);

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
        public async Task Can_GET_All_Contracts_With_User_OrganizationFiltering_Implicit()
        {
            //Arrange
            var (token, user, organization1) = await CreatePrerequisitesAsync();
            var contract1 = await CreateContractAsync(organization1.Id);
            var organization2 = await CreateOrganizationAsync();
            var contract2 = await CreateContractAsync(organization2.Id);

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
            var contract1 = await CreateContractAsync(organization1.Id);
            var organization2 = await CreateOrganizationAsync();
            var contract2 = await CreateContractAsync(organization2.Id);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

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
            var newSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Local);
            var newSystemUsage = await ItSystemHelper.TakeIntoUseAsync(newSystem.Id, organization.Id);
            var contract1 = await CreateContractAsync(organization.Id);
            var contract2 = await CreateContractAsync(organization.Id);
            await ItContractHelper.AddItSystemUsage(contract1.Id, newSystemUsage.Id, organization.Id);

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
            var newSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Local);
            var newSystemUsage = await ItSystemHelper.TakeIntoUseAsync(newSystem.Id, organization.Id);
            var contract1 = await CreateContractAsync(organization.Id);
            var contract2 = await CreateContractAsync(organization.Id);
            await ItContractHelper.AddItSystemUsage(contract1.Id, newSystemUsage.Id, organization.Id);

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
            var dpr = await DataProcessingRegistrationHelper.CreateAsync(organization.Id, CreateName());
            var contract1 = await CreateContractAsync(organization.Id);
            var contract2 = await CreateContractAsync(organization.Id);
            using var dprAssignmentResponse = await ItContractHelper.SendAssignDataProcessingRegistrationAsync(contract1.Id, dpr.Id);
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
            var orgUnit = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, CreateName());
            var contract1 = await CreateContractAsync(organization.Id);
            var contract2 = await CreateContractAsync(organization.Id);
            using var responsibleOrgUnitAssignmentResponse = await ItContractHelper.SendAssignResponsibleOrgUnitAsync(contract1.Id, orgUnit.Id, organization.Id);
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
            var contract1 = await CreateContractAsync(organization.Id);
            var contract2 = await CreateContractAsync(organization.Id);
            using var supplierAssignmentResponse = await ItContractHelper.SendAssignSupplierAsync(contract1.Id, supplier.Id, organization.Id);
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
            var contract1 = await CreateContractAsync(organization.Id, $"{content}ONE");
            var contract2 = await CreateContractAsync(organization.Id, $"TWO{content}");
            var contract3 = await CreateContractAsync(organization.Id);

            //Act
            var contracts = (await ItContractV2Helper.GetItContractsAsync(token, nameContent: content)).ToList();

            //Assert
            Assert.Equal(2, contracts.Count);
            AssertExpectedShallowContracts(contract1, organization, contracts);
            AssertExpectedShallowContracts(contract2, organization, contracts);
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
            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName()
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
            var parent = await ItContractHelper.CreateContract(CreateName(), organization2.Id);

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
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();
            var parent = await ItContractHelper.CreateContract(CreateName(), organization2.Id);

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
        public async Task Can_PUT_With_Parent()
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
            var updatedResponse1 = await ItContractV2Helper.SendPutContractAsync(token, contractDTO.Uuid, updateRequest1);

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
            var updatedResponse2 = await ItContractV2Helper.SendPutContractAsync(token, contractDTO.Uuid, updateRequest2);

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
            var updatedResponse3 = await ItContractV2Helper.SendPutContractAsync(token, contractDTO.Uuid, updateRequest3);

            //Assert - Update to empty
            Assert.Equal(HttpStatusCode.OK, updatedResponse3.StatusCode);
            var updatedContractDTO3 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            Assert.Null(updatedContractDTO3.ParentContract);
        }

        private CreateNewContractRequestDTO CreateNewSimpleRequest(Guid organizationUuid)
        {
            return new CreateNewContractRequestDTO()
            {
                Name = CreateName(),
                OrganizationUuid = organizationUuid
            };
        }

        private async Task<Organization> CreateOrganizationAsync()
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, organizationName, "13370000", A<OrganizationTypeKeys>(), AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return $"{nameof(ItContractsApiV2Test)}{A<string>()}";
        }

        private string CreateEmail()
        {
            return $"{nameof(ItContractsApiV2Test)}{A<string>()}@test.dk";
        }

        private static void AssertCrossReference<T>(T expected, IdentityNamePairResponseDTO actual) where T : IHasNameExternal, IHasUuidExternal
        {
            Assert.Equal(expected?.Uuid, actual?.Uuid);
            Assert.Equal(expected?.Name, actual?.Name);
        }

        private static void AssertExpectedShallowContracts(ItContractDTO expectedContent, Organization expectedOrganization, IEnumerable<ItContractResponseDTO> dtos)
        {
            var dto = Assert.Single(dtos, contract => contract.Uuid == expectedContent.Uuid);
            AssertExpectedShallowContract(expectedContent, expectedOrganization, dto);
        }

        private static void AssertExpectedShallowContract(ItContractDTO expectedContent, Organization expectedOrganization, ItContractResponseDTO dto)
        {
            Assert.Equal(expectedContent.Uuid, dto.Uuid);
            Assert.Equal(expectedContent.Name, dto.Name);
            Assert.Equal(expectedOrganization.Uuid, dto.OrganizationContext.Uuid);
            Assert.Equal(expectedOrganization.Name, dto.OrganizationContext.Name);
            Assert.Equal(expectedOrganization.Cvr, dto.OrganizationContext.Cvr);
        }

        private async Task<ItContractDTO> CreateContractAsync(int orgId, string name = null)
        {
            if(name == null)
                return await ItContractHelper.CreateContract(CreateName(), orgId);

            return await ItContractHelper.CreateContract(name, orgId);
        }

        private async Task<(string token, User user, Organization organization)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUserAsync(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            return (token, user, organization);
        }
        private async Task<(User user, string token)> CreateApiUserAsync(Organization organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Id, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
        }
    }
}
