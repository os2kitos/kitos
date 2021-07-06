using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Presentation.Web.Models.External.V2.Response.Contract;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
            var newContract = await ItContractHelper.CreateContract(A<string>(), TestEnvironment.DefaultOrganizationId);
            var contractType = DatabaseAccess.MapFromEntitySet<ItContractType, ItContractType>(x => x.AsQueryable().First());
            var supplier = DatabaseAccess.MapFromEntitySet<Organization, Organization>(x => x.AsQueryable().First());
            var inOperationAgreementElement = DatabaseAccess.MapFromEntitySet<AgreementElementType, AgreementElementType>(x => x.AsQueryable().First(x => x.Name == ItContract.InOperationAgreementElementName));
            var patchObject = new
            {
                concluded = DateTime.Now,
                expirationDate = DateTime.Now.AddDays(1),
                terminated = DateTime.Now.AddDays(2),
                supplierId = supplier.Id,
                contractTypeId = contractType.Id
            };
            await ItContractHelper.PatchContract(newContract.Id, TestEnvironment.DefaultOrganizationId, patchObject);
            await ItContractHelper.SendAssignAgreementElementAsync(newContract.Id, TestEnvironment.DefaultOrganizationId, inOperationAgreementElement.Id);
            var updatedContract = await ItContractHelper.GetItContract(newContract.Id);

            //Act
            var dto = await ItContractV2Helper.GetItContractAsync(regularUserToken.Token, newContract.Uuid);

            //Assert
            AssertContractResponseDTO(updatedContract, dto);
            Assert.True(dto.InOperation);
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
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var defaultOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            // Used to compare specific contract related data
            var expectedContractId = DatabaseAccess.MapFromEntitySet<ItContract, int>(x => x
                .AsQueryable()
                .First(x => x.OrganizationId == TestEnvironment.DefaultOrganizationId)
                .Id);
            var expectedContract = await ItContractHelper.GetItContract(expectedContractId);

            //Act
            var contracts = await ItContractV2Helper.GetItContractsAsync(regularUserToken.Token, defaultOrgUuid, null, null, 0, 100);

            //Assert
            Assert.NotEmpty(contracts);

            // Checking contract data
            var contract = contracts.First(x => x.Uuid == expectedContract.Uuid);
            AssertContractResponseDTO(expectedContract, contract);
        }

        [Fact]
        public async Task GET_Contracts_Returns_Ok_With_Name_Content_Filtering()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var newContract = await ItContractHelper.CreateContract(A<string>(), TestEnvironment.DefaultOrganizationId);
            var defaultOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            //Act
            var contracts = await ItContractV2Helper.GetItContractsAsync(regularUserToken.Token, defaultOrgUuid, null, newContract.Name, 0, 100);

            //Assert
            var contract = Assert.Single(contracts);
            Assert.Equal(newContract.Uuid, contract.Uuid);
        }

        [Fact]
        public async Task GET_Contracts_Returns_Ok_With_System_Filtering()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, TestEnvironment.DefaultOrganizationId);
            var newContract = await ItContractHelper.CreateContract(A<string>(), TestEnvironment.DefaultOrganizationId);
            await ItContractHelper.AddItSystemUsage(newContract.Id, systemUsage.Id, TestEnvironment.DefaultOrganizationId);
            var defaultOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            //Act
            var contracts = await ItContractV2Helper.GetItContractsAsync(regularUserToken.Token, defaultOrgUuid, system.Uuid, null, 0, 100);

            //Assert
            var contract = Assert.Single(contracts);
            Assert.Equal(newContract.Uuid, contract.Uuid);
        }

        [Fact]
        public async Task GET_Contracts_Returns_BadRequest_For_Empty_Organization_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            var response = await ItContractV2Helper.SendGetItContractsAsync(regularUserToken.Token, Guid.Empty, null, null, 0, 100);

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
            var response = await ItContractV2Helper.SendGetItContractsAsync(regularUserToken.Token, defaultOrgUuid, Guid.Empty, null, 0, 100);

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
            var response = await ItContractV2Helper.SendGetItContractsAsync(regularUserToken.Token, organization.Uuid, null, null, 0, 100);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private static void AssertContractResponseDTO(ItContractDTO expected, ItContractResponseDTO actual) 
        {

            Assert.Equal(expected.Uuid, actual.Uuid);
            Assert.Equal(expected.Name, actual.Name);

            Assert.Equal(expected.Concluded, actual.ValidFrom);
            Assert.Equal(expected.ExpirationDate, actual.ExpiresAt);
            Assert.Equal(expected.Terminated, actual.TerminatedAt);

            if (expected.SupplierId.HasValue)
            {
                Assert.Equal(expected.SupplierName, actual.Supplier.Name);
            }
            else
            {
                Assert.Null(actual.Supplier);
            }

            if (expected.ContractTypeId.HasValue)
            {
                Assert.Equal(expected.ContractTypeName, actual.ContractType.Name);
            }
            else
            {
                Assert.Null(actual.Supplier);
            }
        }
    }
}
