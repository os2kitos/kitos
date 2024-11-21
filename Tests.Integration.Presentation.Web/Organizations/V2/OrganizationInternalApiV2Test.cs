using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Newtonsoft.Json;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Response.Contract;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Extensions;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.Internal.Organizations;
using Tests.Integration.Presentation.Web.Tools.Internal.UI_Configuration;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationInternalApiV2Test: OrganizationApiV2TestBase
    {
        private const int CvrMaxLength = 10;

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true, true, true, true)]
        [InlineData(OrganizationRole.LocalAdmin, true, false, false, false)]
        [InlineData(OrganizationRole.User, true, false, false, false)]
        public async Task Can_Get_Specific_Organization_Permissions(OrganizationRole userRole, bool expectRead, bool expectModify, bool expectDelete, bool expectModifyCvr)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(userRole);
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());


            //Act
            var permissionsResponseDto = await OrganizationV2Helper.GetPermissionsAsync(cookie, organization.Uuid);

            //Assert - exhaustive content assertions are done in the read-after-write assertion tests (POST/PUT)
            var expected = new OrganizationPermissionsResponseDTO()
            {
                Read = expectRead,
                Modify = expectModify,
                Delete = expectDelete,
                ModifyCvr = expectModifyCvr
            };
            Assert.Equivalent(expected, permissionsResponseDto);
        }

        [Fact]
        public async Task Can_Get_Default_UI_Root_Config_For_New_Org()
        {
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());

            var response = await OrganizationInternalV2Helper.GetOrganizationUIRootConfig(organization.Uuid);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDto = await response.ReadResponseBodyAsAsync<UIRootConfigResponseDTO>();
            Assert.True(responseDto.ShowItContractModule);
            Assert.True(responseDto.ShowDataProcessing);
            Assert.True(responseDto.ShowItSystemModule);
        }

        [Fact]
        public async Task Can_Patch_UI_Root_Config()
        {
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var dto = new UIRootConfigUpdateRequestDTO()
            {
                ShowDataProcessing = A<bool>(),
                ShowItContractModule = A<bool>(),
                ShowItSystemModule = A<bool>()
            };

            var response = await OrganizationInternalV2Helper.PatchOrganizationUIRootConfig(organization.Uuid, dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseDto = await response.ReadResponseBodyAsAsync<UIRootConfigResponseDTO>();
            Assert.Equal(dto.ShowItContractModule, responseDto.ShowItContractModule);
            Assert.Equal(dto.ShowDataProcessing, responseDto.ShowDataProcessing);
            Assert.Equal(dto.ShowItSystemModule, responseDto.ShowItSystemModule);
        }

        [Fact]
        public async Task Get_UI_Customization_Returns_New_Empty_Customization_If_None_Exists()
        {
            var moduleName = "ItSystemUsages";
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());

            var response = await OrganizationInternalV2Helper.GetUIModuleCustomization(organization.Uuid, moduleName);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var dto = await response.ReadResponseBodyAsAsync<UIModuleCustomizationResponseDTO>();
            Assert.NotNull(dto);
            Assert.Empty(dto.Nodes);
            Assert.Equal(moduleName, dto.Module);
        }

        [Fact]
        public async Task Can_Get_UI_Customization()
        {
            var moduleName = "ItSystemUsages";
            var (cookie, organization) = await CreateUiCustomizationPrerequisitesAsync();
            var dto = new UIModuleCustomizationRequestDTO()
            {
                Nodes = GetNodeDTOs(5),
            };
            var putResponse = await OrganizationInternalV2Helper.PutUIModuleCustomization(organization.Uuid, moduleName, dto, cookie);
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

            var response = await OrganizationInternalV2Helper.GetUIModuleCustomization(organization.Uuid, moduleName);

            await AssertUICustomizationResponse(dto, response);

        }

        [Fact]
        public async Task Can_Put_UI_Customization()
        {
            var moduleName = "ItSystemUsages";
            var (cookie, organization) = await CreateUiCustomizationPrerequisitesAsync();

            var dto = new UIModuleCustomizationRequestDTO()
            {
                Nodes = GetNodeDTOs(5),
            };

            var response = await OrganizationInternalV2Helper.PutUIModuleCustomization(organization.Uuid, moduleName, dto, cookie);

            await AssertUICustomizationResponse(dto, response);
        }

        [Fact]
        public async Task Can_Get_Master_Data()
        {
            var organization = await CreateTestOrganization();

            using var response =
                await OrganizationInternalV2Helper.GetOrganizationMasterData(organization.Uuid);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<OrganizationMasterDataResponseDTO>(content);
            Assert.Equal(organization.Uuid, responseDto.Uuid);
            Assert.Equal(organization.Cvr, responseDto.Cvr);
            Assert.Equal(organization.Phone, responseDto.Phone);
        }

        [Fact]
        public async Task Can_Patch_Organization_Master_Data_With_Values()
        {
            var patchDto = new OrganizationMasterDataRequestDTO
            {
                Address = A<string>(),
                Cvr = GetCvr(),
                Email = A<string>(),
                Phone = A<string>()
            };

            var organizationToPatch = await CreateTestOrganization();

            using var response =
                await OrganizationInternalV2Helper.PatchOrganizationMasterData(organizationToPatch.Uuid, patchDto);
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<OrganizationMasterDataResponseDTO>(content);
            Assert.Equal(patchDto.Cvr, responseDto.Cvr);
            Assert.Equal(organizationToPatch.Uuid, responseDto.Uuid);
        }

        [Fact]
        public async Task Can_Patch_Organization_Master_Data_With_Null()
        {
            var patchDto = new OrganizationMasterDataRequestDTO();

            var organizationToPatch = await CreateTestOrganization();

            using var response =
                await OrganizationInternalV2Helper.PatchOrganizationMasterData(organizationToPatch.Uuid, patchDto);
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<OrganizationMasterDataResponseDTO>(content);
            Assert.Equal(patchDto.Cvr, responseDto.Cvr);
            Assert.Equal(organizationToPatch.Uuid, responseDto.Uuid);
        }
        
        [Fact]
        public async Task Can_Get_Organization_Master_Data_Roles()
        {
            var organization = await CreateTestOrganization();
            var contactPersonDto = new ContactPersonRequestDTO()
            {
                Email = A<string>(),
                LastName = A<string>(),
                Name = A<string>(),
                PhoneNumber = A<string>()
            };
            var request = new OrganizationMasterDataRolesRequestDTO()
            {
                ContactPerson = contactPersonDto
            };
            var upsertResponse =
                await OrganizationInternalV2Helper.PatchOrganizationMasterDataRoles(organization.Uuid, request);
            Assert.Equal(HttpStatusCode.OK, upsertResponse.StatusCode);
            var upsertContent = await upsertResponse.Content.ReadAsStringAsync();
            var upsertResponseDto = JsonConvert.DeserializeObject<OrganizationMasterDataRolesResponseDTO>(upsertContent);
            Assert.Equal(contactPersonDto.Name, upsertResponseDto.ContactPerson.Name);
            Assert.Equal(organization.Uuid, upsertResponseDto.OrganizationUuid);

            using var response = await OrganizationInternalV2Helper.GetOrganizationMasterDataRoles(organization.Uuid);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<OrganizationMasterDataRolesResponseDTO>(content);
            Assert.Equal(contactPersonDto.Name, responseDto.ContactPerson.Name);
            Assert.Equal(organization.Uuid, responseDto.OrganizationUuid);
        }

        [Fact]
        public async Task Can_Get_Organization_Master_Data_Roles_If_None_Persisted()
        {
            var organization = await CreateTestOrganization();

            using var response = await OrganizationInternalV2Helper.GetOrganizationMasterDataRoles(organization.Uuid);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<OrganizationMasterDataRolesResponseDTO>(content);
            Assert.Equal(organization.Uuid, responseDto.OrganizationUuid);
            var cpDto = responseDto.ContactPerson;
            var drDto = responseDto.DataResponsible;
            var dpaDto = responseDto.DataProtectionAdvisor;
            Assert.Null(cpDto.Name);
            Assert.Null(drDto.Cvr);
            Assert.Null(dpaDto.Phone);
            var nonNullProps = new List<string>() { "OrganizationId", "Id" };
            AssertPropertiesNull(cpDto, nonNullProps);
            AssertPropertiesNull(drDto, nonNullProps);
            AssertPropertiesNull(dpaDto, nonNullProps);
        }

        private void AssertPropertiesNull(object o, ICollection<string> noNullProps)
        {
            var type = o.GetType();
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (!noNullProps.Contains(property.Name)) Assert.Null(property.GetValue(o));
            }
        }

        [Fact]
        public async Task Can_Upsert_All_Organization_Master_Data_Roles()
        {
            var organization = await CreateTestOrganization();
            var (contactPersonDto, dataResponsibleDto, dataProtectionAdvisorDto) = GetRequestDtos();

            var requestDto = new OrganizationMasterDataRolesRequestDTO()
            {
                   ContactPerson = contactPersonDto,
                   DataResponsible = dataResponsibleDto,
                   DataProtectionAdvisor = dataProtectionAdvisorDto
            };

            using var response =
                await OrganizationInternalV2Helper.PatchOrganizationMasterDataRoles(organization.Uuid, requestDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<OrganizationMasterDataRolesResponseDTO>(content);
            Assert.Equal(contactPersonDto.Name, responseDto.ContactPerson.Name);
            Assert.Equal(organization.Uuid, responseDto.OrganizationUuid);
            Assert.Equal(dataResponsibleDto.Name, responseDto.DataResponsible.Name);
            Assert.Equal(dataProtectionAdvisorDto.Name, responseDto.DataProtectionAdvisor.Name);
        }

        public enum RoleType
        {
            ContactPerson,
            DataResponsible,
            DataProtectionAdvisor
        }

        [Theory]
        [InlineData(RoleType.ContactPerson)]
        [InlineData(RoleType.DataResponsible)]
        [InlineData(RoleType.DataProtectionAdvisor)]
        public async Task Can_Upsert_Single_Organization_Master_Data_Role(RoleType roleType)
        {
            var organization = await CreateTestOrganization();
            var resetRolesResponse =
                 await OrganizationInternalV2Helper.PatchOrganizationMasterDataRoles(organization.Uuid, 
                     new OrganizationMasterDataRolesRequestDTO());
             Assert.Equal(HttpStatusCode.OK, resetRolesResponse.StatusCode);
             var (contactPersonDto, dataResponsibleDto, dataProtectionAdvisorDto) = GetRequestDtos();
             var request = new OrganizationMasterDataRolesRequestDTO();
            switch (roleType)
            {
                case RoleType.ContactPerson: request.ContactPerson = contactPersonDto;
                    break;
                case RoleType.DataResponsible: request.DataResponsible = dataResponsibleDto;
                    break;
                case RoleType.DataProtectionAdvisor: request.DataProtectionAdvisor = dataProtectionAdvisorDto; 
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(roleType), roleType, null);
            }

            using var response =
                await OrganizationInternalV2Helper.PatchOrganizationMasterDataRoles(organization.Uuid, request);
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<OrganizationMasterDataRolesResponseDTO>(content);
            Assert.Equal(organization.Uuid, responseDto.OrganizationUuid);
            switch (roleType)
            {
                case RoleType.ContactPerson:
                    Assert.Equal(contactPersonDto.LastName, responseDto.ContactPerson.LastName);
                    break;
                case RoleType.DataResponsible:
                    Assert.Equal(dataResponsibleDto.Email, responseDto.DataResponsible.Email);
                    break;
                case RoleType.DataProtectionAdvisor:
                    Assert.Equal(dataProtectionAdvisorDto.Cvr, responseDto.DataProtectionAdvisor.Cvr);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(roleType), roleType, null);
            }
        }

        [Fact]
        public async Task Upsert_Can_Create_Organization_Master_Data_Roles_If_Null()
        {
            var organization = await CreateTestOrganization();
            var requestDto = new OrganizationMasterDataRolesRequestDTO();

            using var response =
                await OrganizationInternalV2Helper.PatchOrganizationMasterDataRoles(organization.Uuid, requestDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<OrganizationMasterDataRolesResponseDTO>(content);
            Assert.Equal(organization.Uuid, responseDto.OrganizationUuid);
            await GetMasterDataRolesAndAssertNotNull(organization.Uuid);
        }

        [Fact]
        public async Task Can_Update_Organization()
        {
            var organization = await CreateTestOrganization();
            var requestDto = UpdateRequestDtoWithoutCountryCode();

            using var response = await OrganizationInternalV2Helper.PatchOrganization(organization.Uuid, requestDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var updatedOrganization = await OrganizationHelper.GetOrganizationAsync(organization.Id, cookie);
            Assert.Equal(requestDto.Cvr, updatedOrganization.Cvr);
            Assert.Equal(requestDto.Name, updatedOrganization.Name);
            Assert.Equal((int)requestDto.Type, updatedOrganization.TypeId);
        }

        [Fact]
        public async Task Can_Create_Organization()
        {
            var requestDto = CreateRequestDtoWithoutCountryCode();

            using var response = await OrganizationInternalV2Helper.CreateOrganization(requestDto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var organization = await response.ReadResponseBodyAsAsync<IdentityNamePairResponseDTO>();
           
            Assert.Equal(requestDto.Name, organization.Name);
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Can_Only_Create_Organization_As_Global_Admin(OrganizationRole role)
        {
            var cookie = await HttpApi.GetCookieAsync(role);
            var requestDto = CreateRequestDtoWithoutCountryCode();

            using var response = await OrganizationInternalV2Helper.CreateOrganization(requestDto, cookie);

            var wasAllowed = response.StatusCode == HttpStatusCode.Created;
            var isGlobalAdmin = role == OrganizationRole.GlobalAdmin;
            Assert.Equal(isGlobalAdmin, wasAllowed);
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.GlobalAdmin)]
        public async Task Can_Only_Delete_Organization_As_Global_Admin(OrganizationRole role)
        {
            var orgToDelete = await CreateTestOrganization();

            using var response = await OrganizationInternalV2Helper.DeleteOrganization(orgToDelete.Uuid, true, role);

            var wasAllowed = response.StatusCode == HttpStatusCode.NoContent;
            var isGlobalAdmin = role == OrganizationRole.GlobalAdmin;
            Assert.Equal(isGlobalAdmin, wasAllowed);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Can_Only_Delete_Organization_With_Conflicts_When_Enforcing(bool enforceDeletion)
        {
            var orgToDelete = await CreateTestOrganization();
            await CreateConflictsForOrg(orgToDelete.Uuid);

            using var response = await OrganizationInternalV2Helper.DeleteOrganization(orgToDelete.Uuid, enforceDeletion);

            var wasAllowed = response.StatusCode == HttpStatusCode.NoContent;
            Assert.Equal(enforceDeletion, wasAllowed);
        }

        [Fact]
        public async Task Can_Get_Conflicts()
        {
            var org = await CreateTestOrganization();
            await CreateConflictsForOrg(org.Uuid);

            using var response = await OrganizationInternalV2Helper.GetRemovalConflicts(org.Uuid);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.ReadResponseBodyAsAsync<OrganizationRemovalConflictsResponseDTO>();
            Assert.NotNull(body);
            Assert.Single(body.ContractsInOtherOrganizationsWhereOrgIsSupplier);
        }

        [Fact]
        public async Task Update_Organization_Returns_Bad_Request_If_Invalid_Uuid()
        {
            var invalidUuid = new Guid();
            var requestDto = new OrganizationUpdateRequestDTO()
            {
                Cvr = GetCvr(),
                Name = A<string>()
            };

            using var response = await OrganizationInternalV2Helper.PatchOrganization(invalidUuid, requestDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private async Task CreateConflictsForOrg(Guid organizationUuid)
        {
            var otherOrganization = await CreateTestOrganization();
            var token = (await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin)).Token;

            //Create contract in another organization
            var contractRequest = new CreateNewContractRequestDTO { OrganizationUuid = otherOrganization.Uuid, Name = A<string>() };
            var contractResponse = await ItContractV2Helper.SendPostContractAsync(token, contractRequest);
            Assert.Equal(HttpStatusCode.Created, contractResponse.StatusCode);
            var contract = await contractResponse.ReadResponseBodyAsAsync<ItContractResponseDTO>();

            //Set the original organization to be deleted as the supplier for the contract
            var patchRequest =  new ContractSupplierDataWriteRequestDTO { OrganizationUuid = organizationUuid};
            var patchResponse = await ItContractV2Helper.SendPatchContractSupplierAsync(token, contract.Uuid, patchRequest);
            Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);

        }

        private async Task GetMasterDataRolesAndAssertNotNull(Guid orgUuid)
        {
            var getResponse = await OrganizationInternalV2Helper.GetOrganizationMasterDataRoles(orgUuid);
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var getResponseDto = JsonConvert.DeserializeObject<OrganizationMasterDataRolesResponseDTO>(getContent);
            Assert.NotNull(getResponseDto.ContactPerson);
            Assert.NotNull(getResponseDto.DataResponsible);
            Assert.NotNull(getResponseDto.DataProtectionAdvisor);
        }

        private (ContactPersonRequestDTO, DataResponsibleRequestDTO, DataProtectionAdvisorRequestDTO) GetRequestDtos()
        {
            var contactPersonDto = new ContactPersonRequestDTO()
            {
                Email = A<string>(),
                LastName = A<string>(),
                Name = A<string>(),
                PhoneNumber = A<string>()
            };

            var dataResponsibleDto = new DataResponsibleRequestDTO()
            {
                Address = A<string>(),
                Cvr = GetCvr(),
                Email = A<string>(),
                Name = A<string>(),
                Phone = A<string>()
            };

            var dataProtectionAdvisorDto = new DataProtectionAdvisorRequestDTO()
            {
                Address = A<string>(),
                Cvr = GetCvr(),
                Email = A<string>(),
                Name = A<string>(),
                Phone = A<string>()
            };
            return (contactPersonDto, dataResponsibleDto, dataProtectionAdvisorDto);
        }

        private async Task<OrganizationDTO> CreateTestOrganization()
        {
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, A<string>(),
                "11223344", OrganizationTypeKeys.Kommune, AccessModifier.Local);
            Assert.NotNull(organization);
            return organization;
        }

        private string GetCvr()
        {
            return A<string>().Truncate(CvrMaxLength);
        }

        private async Task<(Cookie loginCookie, OrganizationDTO organization)> CreateUiCustomizationPrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (_, _, loginCookie) =
                await HttpApi.CreateUserAndLogin(UIConfigurationHelper.CreateEmail(), OrganizationRole.LocalAdmin, organization.Id);
            return (loginCookie, organization);
        }

        private async Task AssertUICustomizationResponse(UIModuleCustomizationRequestDTO expected, HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<UIModuleCustomizationResponseDTO>(content);
            var expectedNodes = expected.Nodes.ToList();
            var actualNodes = responseDto.Nodes.ToList();
            Assert.Equal(expectedNodes.Count, actualNodes.Count);
            foreach (var expectedNode in expectedNodes)
            {
                var actual = actualNodes.FirstOrDefault(nodeDto => nodeDto.Key == expectedNode.Key);

                Assert.NotNull(actual);
                Assert.Equal(expectedNode.Enabled, actual.Enabled);
            }
        }

        private OrganizationUpdateRequestDTO UpdateRequestDtoWithoutCountryCode()
        {
            var requestDto = A<OrganizationUpdateRequestDTO>();
            requestDto.ForeignCountryCodeUuid = null;
            return requestDto;
        }

        private OrganizationCreateRequestDTO CreateRequestDtoWithoutCountryCode()
        {
            var requestDto = A<OrganizationCreateRequestDTO>();
            requestDto.ForeignCountryCodeUuid = null;
            return requestDto;
        }

        private IList<CustomizedUINodeRequestDTO> GetNodeDTOs(int numberOfNodes)
        {
            var nodes = new List<CustomizedUINodeRequestDTO>();
            for (var i = 0; i < numberOfNodes; i++)
            {
                nodes.Add(new CustomizedUINodeRequestDTO()
                {
                    Key = GenerateKey(),
                    Enabled = A<bool>(),
                });
            }
            return nodes;
        }

        private string GenerateKey()
        {
            return Regex.Replace(A<string>(), "[0-9-]", "a");
        }
    }
}
