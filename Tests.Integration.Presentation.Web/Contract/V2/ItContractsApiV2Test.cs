using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Presentation.Web.Models.API.V2.Response.Contract;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V1;
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
            var agreementElement = Assert.Single(dto.AgreementElements);
            Assert.Equal(expectedAgreementElement.Uuid, agreementElement.Uuid);
            Assert.Equal(expectedAgreementElement.Name, agreementElement.Name);
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

        private static void AssertContractResponseDTO(ItContractDTO expected, ItContractResponseDTO actual) 
        {

            Assert.Equal(expected.Uuid, actual.Uuid);
            Assert.Equal(expected.Name, actual.Name);

            Assert.Equal(expected.Concluded, actual.ValidFrom);
            Assert.Equal(expected.ExpirationDate, actual.ValidTo);
            Assert.Equal(expected.Terminated, actual.TerminatedAt);
            Assert.Equal(expected.IsActive, actual.IsValid);

            if (expected.SupplierId.HasValue)
            {
                Assert.Equal(expected.SupplierName, actual.Supplier.Name);
                Assert.Equal(expected.SupplierUuid, actual.Supplier.Uuid);
            }
            else
            {
                Assert.Null(actual.Supplier);
            }

            if (expected.ContractTypeId.HasValue)
            {
                Assert.Equal(expected.ContractTypeName, actual.ContractType.Name);
                Assert.Equal(expected.ContractTypeUuid, actual.ContractType.Uuid);
            }
            else
            {
                Assert.Null(actual.Supplier);
            }
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
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, string.Join("", Many<int>(8).Select(x => Math.Abs(x) % 9)), A<OrganizationTypeKeys>(), AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return $"{nameof(ItContractsApiV2Test)}{A<string>()}";
        }

        private string CreateEmail()
        {
            return $"{A<string>()}@test.dk";
        }
    }
}
