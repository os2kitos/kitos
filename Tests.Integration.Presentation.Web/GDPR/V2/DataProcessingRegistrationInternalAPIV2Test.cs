using Core.DomainModel.Organization;
using Core.DomainModel;
using Presentation.Web.Models.API.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using System.Net;
using Xunit;
using Presentation.Web.Models.API.V1.GDPR;
using Presentation.Web.Models.API.V2.Response.DataProcessing;
using Tests.Integration.Presentation.Web.Tools.Internal.DPR;

namespace Tests.Integration.Presentation.Web.GDPR.V2
{
    public class DataProcessingRegistrationInternalAPIV2Test : WithAutoFixture
    {

        [Fact]
        public async Task Can_GET_All_DPRs()
        {
            //Arrange
            var (cookie, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);

            //Act
            var dprs = await DataProcessingRegistrationInternalV2Helper.GetDPRsAsync(cookie, organization.Uuid, 0, 250);

            //Assert
            Assert.Equal(2, dprs.Count());
            AssertExpectedShallowDPRs(dpr1, organization, dprs);
            AssertExpectedShallowDPRs(dpr2, organization, dprs);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_Paging()
        {
            //Arrange
            var (token, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            var dpr3 = await CreateDPRAsync(organization.Id);

            //Act
            var page1Dprs = (await DataProcessingRegistrationInternalV2Helper.GetDPRsAsync(token, organization.Uuid, 0, 2)).ToList();
            var page2Dprs = (await DataProcessingRegistrationInternalV2Helper.GetDPRsAsync(token, organization.Uuid, 1, 2)).ToList();

            //Assert
            Assert.Equal(2, page1Dprs.Count);
            AssertExpectedShallowDPRs(dpr1, organization, page1Dprs);
            AssertExpectedShallowDPRs(dpr2, organization, page1Dprs);

            var page2Dpr = Assert.Single(page2Dprs);
            AssertExpectedShallowDPR(dpr3, organization, page2Dpr);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_NameContains_Filtering()
        {
            //Arrange
            var (token, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Id);
            await CreateDPRAsync(organization.Id);
            await CreateDPRAsync(organization.Id);
            
            //Act
            var dtos = await DataProcessingRegistrationInternalV2Helper.GetDPRsAsync(token, organization.Uuid, page: 0, pageSize: 10, nameContains: dpr1.Name);

            var response = Assert.Single(dtos);
            Assert.Equal(response.Uuid, dpr1.Uuid);

        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_OrganizationFiltering()
        {
            //Arrange
            var (token, organization1) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization1.Id);
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var dpr2 = await CreateDPRAsync(organization2.Id);

            //Act
            var dprs = await DataProcessingRegistrationInternalV2Helper.GetDPRsAsync(token, organization1.Uuid, 0, 250);

            //Assert
            var retrievedDPR = Assert.Single(dprs);
            AssertExpectedShallowDPR(dpr1, organization1, retrievedDPR);
        }
        private async Task<DataProcessingRegistrationDTO> CreateDPRAsync(int orgId)
        {
            return await DataProcessingRegistrationHelper.CreateAsync(orgId, CreateName());
        }

        private static void AssertExpectedShallowDPRs(DataProcessingRegistrationDTO expectedContent, OrganizationDTO expectedOrganization, IEnumerable<DataProcessingRegistrationResponseDTO> dtos)
        {
            var dto = Assert.Single(dtos, dpr => dpr.Uuid == expectedContent.Uuid);
            AssertExpectedShallowDPR(expectedContent, expectedOrganization, dto);
        }

        private static void AssertExpectedShallowDPR(DataProcessingRegistrationDTO expectedContent, OrganizationDTO expectedOrganization, DataProcessingRegistrationResponseDTO dto)
        {
            Assert.Equal(expectedContent.Uuid, dto.Uuid);
            Assert.Equal(expectedContent.Name, dto.Name);
            Assert.Equal(expectedOrganization.Uuid, dto.OrganizationContext.Uuid);
            Assert.Equal(expectedOrganization.Name, dto.OrganizationContext.Name);
            Assert.Equal(expectedOrganization.Cvr, dto.OrganizationContext.Cvr);
        }

        private async Task<(Cookie cookie, OrganizationDTO organization)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (userId, cookie) = await CreateApiUserAsync(organization);
            await HttpApi.SendAssignRoleToUserAsync(userId, OrganizationRole.GlobalAdmin, organization.Id).DisposeAsync();
            return (cookie, organization);
        }
        private async Task<(int userId, Cookie cookie)> CreateApiUserAsync(OrganizationDTO organization)
        {
            var userAndGetCookie = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.GlobalAdmin, organization.Id);
            return (userAndGetCookie.userId, userAndGetCookie.loginCookie);
        }

        private async Task<OrganizationDTO> CreateOrganizationAsync(OrganizationTypeKeys orgType)
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, string.Join("", Many<int>(8).Select(x => Math.Abs(x) % 9)), orgType, AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return $"{nameof(DataProcessingRegistrationApiV2Test)}æøå{A<string>()}";
        }

        private string CreateEmail()
        {
            return $"{A<string>()}{DateTime.Now.Ticks}@kitos.dk";
        }
    }
}
