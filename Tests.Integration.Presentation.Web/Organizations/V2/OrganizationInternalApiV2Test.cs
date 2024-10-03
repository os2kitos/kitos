using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Newtonsoft.Json;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Presentation.Web.Models.API.V2.Response.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Extensions;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.Internal.Organizations;
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
        public async Task Can_Get_Master_Data()
        {
            var organization = await CreateTestOrganization();

            var response =
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
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var patchDto = new OrganizationMasterDataRequestDTO
            {
                Address = A<string>(),
                Cvr = GetCvr(),
                Email = A<string>(),
                Phone = A<string>()
            };

            var organizationToPatch = await CreateTestOrganization();

            var response =
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

            var response =
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

            var response = await OrganizationInternalV2Helper.GetOrganizationMasterDataRoles(organization.Uuid);

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

            var response = await OrganizationInternalV2Helper.GetOrganizationMasterDataRoles(organization.Uuid);

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

            var response =
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

            var response =
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

            var response =
                await OrganizationInternalV2Helper.PatchOrganizationMasterDataRoles(organization.Uuid, requestDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<OrganizationMasterDataRolesResponseDTO>(content);
            Assert.Equal(organization.Uuid, responseDto.OrganizationUuid);
            await GetMasterDataRolesAndAssertNotNull(organization.Uuid);
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
    }
}
