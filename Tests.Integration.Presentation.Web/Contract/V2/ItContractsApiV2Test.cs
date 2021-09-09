using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Response.Contract;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Request.Contract;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Contract.V2
{
    public class ItContractsApiV2Test : WithAutoFixture
    {
        [Fact]
        public async Task GET_Contract_Returns_Ok()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            var (newContract, expectedAgreementElement) = await CreateContractWithAllDataSet(TestEnvironment.DefaultOrganizationId);

            //Act
            var dto = await ItContractV2Helper.GetItContractAsync(regularUserToken.Token, newContract.Uuid);

            //Assert
            AssertContractResponseDTO(newContract, dto);
        }

        [Fact]
        public async Task GET_Contract_Returns_Forbidden()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var newOrg = await CreateOrganizationAsync();
            var newContract = await ItContractHelper.CreateContract(A<string>(), newOrg.Id);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractAsync(regularUserToken.Token, newContract.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GET_Contract_Returns_NotFound()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractAsync(regularUserToken.Token, A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GET_Contract_Returns_BadRequest_For_Empty_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractAsync(regularUserToken.Token, Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GET_Contracts_Returns_Ok()
        {
            //Arrange
            var (regularUserToken, organization) = await CreateUserInNewOrganizationAsync();
            var (newContract1, _) = await CreateContractWithAllDataSet(organization.Id);
            var (newContract2, _) = await CreateContractWithAllDataSet(organization.Id);

            //Act
            var contracts = await ItContractV2Helper.GetItContractsAsync(regularUserToken, organization.Uuid, page: 0, pageSize: 100);

            //Assert
            Assert.Equal(2, contracts.Count());

            var contract1 = contracts.First(x => x.Uuid == newContract1.Uuid);
            AssertContractResponseDTO(newContract1, contract1);

            var contract2 = contracts.First(x => x.Uuid == newContract2.Uuid);
            AssertContractResponseDTO(newContract2, contract2);
        }

        [Fact]
        public async Task GET_Contracts_Returns_Ok_With_Name_Content_Filtering()
        {
            //Arrange
            var (regularUserToken, organization) = await CreateUserInNewOrganizationAsync();
            var (newContract1, _) = await CreateContractWithAllDataSet(organization.Id);
            var (newContract2, _) = await CreateContractWithAllDataSet(organization.Id);

            //Act
            var contracts = await ItContractV2Helper.GetItContractsAsync(regularUserToken, organization.Uuid, nameContent: newContract1.Name, page: 0, pageSize: 100);

            //Assert
            var contract = Assert.Single(contracts);
            AssertContractResponseDTO(newContract1, contract);
        }

        [Fact]
        public async Task GET_Contracts_Returns_Ok_With_System_Filtering()
        {
            //Arrange
            var (regularUserToken, organization) = await CreateUserInNewOrganizationAsync();
            var (newContract1, _) = await CreateContractWithAllDataSet(organization.Id);
            var (newContract2, _) = await CreateContractWithAllDataSet(organization.Id);

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organization.Id);
            await ItContractHelper.AddItSystemUsage(newContract1.Id, systemUsage.Id, organization.Id);

            //Act
            var contracts = await ItContractV2Helper.GetItContractsAsync(regularUserToken, organization.Uuid, systemUuid: system.Uuid, page: 0, pageSize: 100);
            //Assert
            var contract = Assert.Single(contracts);
            AssertContractResponseDTO(newContract1, contract);
        }

        [Fact]
        public async Task GET_Contracts_Returns_Ok_With_SystemUsage_Filtering()
        {
            //Arrange
            var (regularUserToken, organization) = await CreateUserInNewOrganizationAsync();
            var (newContract1, _) = await CreateContractWithAllDataSet(organization.Id);
            var (newContract2, _) = await CreateContractWithAllDataSet(organization.Id);

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organization.Id);
            await ItContractHelper.AddItSystemUsage(newContract1.Id, systemUsage.Id, organization.Id);

            //Act
            var contracts = await ItContractV2Helper.GetItContractsAsync(regularUserToken, organization.Uuid, systemUsageUuid: systemUsage.Uuid, nameContent: null, page: 0, pageSize: 100);

            //Assert
            var contract = Assert.Single(contracts);
            AssertContractResponseDTO(newContract1, contract);
        }

        [Fact]
        public async Task GET_Contracts_Returns_Ok_With_DataProcessingRegistration_Filtering()
        {
            //Arrange
            var (regularUserToken, organization) = await CreateUserInNewOrganizationAsync();
            var (newContract1, _) = await CreateContractWithAllDataSet(organization.Id);
            var (newContract2, _) = await CreateContractWithAllDataSet(organization.Id);

            var dpr = await DataProcessingRegistrationHelper.CreateAsync(organization.Id, CreateName());
            await ItContractHelper.SendAssignDataProcessingRegistrationAsync(newContract1.Id, dpr.Id);

            //Act
            var contracts = await ItContractV2Helper.GetItContractsAsync(regularUserToken, organization.Uuid, dataProcessingRegistrationUuid: dpr.Uuid, nameContent: null, page: 0, pageSize: 100);

            //Assert
            var contract = Assert.Single(contracts);
            AssertContractResponseDTO(newContract1, contract);
        }

        [Fact]
        public async Task GET_Contracts_Returns_BadRequest_For_Empty_Organization_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractsAsync(regularUserToken.Token, organizationUuid: Guid.Empty, page: 0, pageSize: 100);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GET_Contracts_Returns_BadRequest_For_Empty_System_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var defaultOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractsAsync(regularUserToken.Token, defaultOrgUuid, systemUuid: Guid.Empty, page: 0, pageSize: 100);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GET_Contracts_Returns_BadRequest_For_Empty_SystemUsage_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var defaultOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractsAsync(regularUserToken.Token, defaultOrgUuid, systemUsageUuid: Guid.Empty, page: 0, pageSize: 100);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GET_Contracts_Returns_BadRequest_For_Empty_DataProcessingRegistration_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var defaultOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractsAsync(regularUserToken.Token, defaultOrgUuid, dataProcessingRegistrationUuid: Guid.Empty, page: 0, pageSize: 100);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GET_Contracts_Returns_Forbidden_For_Organization_Where_User_Has_No_Roles()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                A<string>(), string.Join("", Many<int>(8).Select(x => Math.Abs(x) % 9)), A<OrganizationTypeKeys>(), AccessModifier.Public);

            //Act
            using var response = await ItContractV2Helper.SendGetItContractsAsync(regularUserToken.Token, organization.Uuid, page: 0, pageSize: 100);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
            var parent = await ItContractHelper.CreateContract(CreateName(), organization.Id);

            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName(),
                ParentContractUuid = parent.Uuid
            };

            //Act
            var contractDTO = await ItContractV2Helper.PostContractAsync(token, requestDto);

            //Assert
            Assert.Equal(parent.Name, contractDTO.ParentContract.Name);
            Assert.Equal(parent.Uuid, contractDTO.ParentContract.Uuid);
        }

        [Fact]
        public async Task Cannot_POST_With_Parent_From_Other_Organization()
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
            var parent = await ItContractHelper.CreateContract(CreateName(), organization.Id);

            var requestDto = new CreateNewContractRequestDTO()
            {
                OrganizationUuid = organization.Uuid,
                Name = CreateName()
            };

            var contractDTO = await ItContractV2Helper.PostContractAsync(token, requestDto);

            var updateRequest1 = new ContractWriteRequestDTO()
            {
                Name = CreateName(),
                ParentContractUuid = parent.Uuid
            };

            //Act - Update from empty
            var updatedResponse1 = await ItContractV2Helper.SendPutContractAsync(token, contractDTO.Uuid, updateRequest1);

            //Assert - Update from empty
            Assert.Equal(HttpStatusCode.OK, updatedResponse1.StatusCode);
            var updatedContractDTO1 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            Assert.Equal(parent.Name, updatedContractDTO1.ParentContract.Name);
            Assert.Equal(parent.Uuid, updatedContractDTO1.ParentContract.Uuid);

            //Act - Update from filled
            var newParent = await ItContractHelper.CreateContract(CreateName(), organization.Id);
            var updateRequest2 = new ContractWriteRequestDTO()
            {
                Name = CreateName(),
                ParentContractUuid = newParent.Uuid
            };
            var updatedResponse2 = await ItContractV2Helper.SendPutContractAsync(token, contractDTO.Uuid, updateRequest2);

            //Assert - Update from filled
            Assert.Equal(HttpStatusCode.OK, updatedResponse2.StatusCode);
            var updatedContractDTO2 = await ItContractV2Helper.GetItContractAsync(token, contractDTO.Uuid);
            Assert.Equal(newParent.Name, updatedContractDTO2.ParentContract.Name);
            Assert.Equal(newParent.Uuid, updatedContractDTO2.ParentContract.Uuid);

            //Act - Update to empty
            var updateRequest3 = new ContractWriteRequestDTO()
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

        private static void AssertContractResponseDTO(ItContractDTO expected, ItContractResponseDTO actual)
        {
            Assert.Equal(expected.Uuid, actual.Uuid);
            Assert.Equal(expected.Name, actual.Name);
        }

        private async Task<(ItContractDTO, AgreementElementType)> CreateContractWithAllDataSet(int orgId)
        {
            var newContract = await ItContractHelper.CreateContract(CreateName(), orgId);
            var contractType = DatabaseAccess.MapFromEntitySet<ItContractType, ItContractType>(x => x.AsQueryable().First());
            var supplier = DatabaseAccess.MapFromEntitySet<Organization, Organization>(x => x.AsQueryable().First());
            var agreementElement = DatabaseAccess.MapFromEntitySet<AgreementElementType, AgreementElementType>(x => x.AsQueryable().First());
            var patchObject = new
            {
                concluded = DateTime.Now,
                expirationDate = DateTime.Now.AddDays(1),
                terminated = DateTime.Now.AddDays(2),
                supplierId = supplier.Id,
                contractTypeId = contractType.Id
            };
            await ItContractHelper.SendAssignAgreementElementAsync(newContract.Id, orgId, agreementElement.Id).DisposeAsync();
            var updatedContract = await ItContractHelper.PatchContract(newContract.Id, orgId, patchObject);
            return (updatedContract, agreementElement);
        }

        private async Task<(string token, Organization createdOrganization)> CreateUserInNewOrganizationAsync()
        {
            var org = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "11223344", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, org.Id, true);
            return (token, org);
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

        private async Task<(string token, User user, Organization organization)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            return (token, user, organization);
        }

        private async Task<(User user, string token)> CreateApiUser(Organization organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Id, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
        }
    }
}
