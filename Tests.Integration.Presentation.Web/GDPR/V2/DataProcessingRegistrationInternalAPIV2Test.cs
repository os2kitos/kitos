using System;
using Core.DomainModel.Organization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using System.Net;
using Xunit;
using Presentation.Web.Models.API.V2.Response.DataProcessing;
using Presentation.Web.Models.API.V2.Response.Organization;
using Tests.Integration.Presentation.Web.Tools.Internal.DPR;

namespace Tests.Integration.Presentation.Web.GDPR.V2
{
    public class DataProcessingRegistrationInternalAPIV2Test : BaseTest
    {

        [Fact]
        public async Task Can_GET_All_DPRs()
        {
            //Arrange
            var (cookie, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Uuid);
            var dpr2 = await CreateDPRAsync(organization.Uuid);

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
            var dpr1 = await CreateDPRAsync(organization.Uuid);
            var dpr2 = await CreateDPRAsync(organization.Uuid);
            var dpr3 = await CreateDPRAsync(organization.Uuid);

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
            var dpr1 = await CreateDPRAsync(organization.Uuid);
            await CreateDPRAsync(organization.Uuid);
            await CreateDPRAsync(organization.Uuid);
            
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
            var dpr1 = await CreateDPRAsync(organization1.Uuid);
            var organization2 = await CreateOrganizationAsync();
            var dpr2 = await CreateDPRAsync(organization2.Uuid);

            //Act
            var dprs = await DataProcessingRegistrationInternalV2Helper.GetDPRsAsync(token, organization1.Uuid, 0, 250);

            //Assert
            var retrievedDPR = Assert.Single(dprs);
            AssertExpectedShallowDPR(dpr1, organization1, retrievedDPR);
        }

        private static void AssertExpectedShallowDPRs(DataProcessingRegistrationResponseDTO expectedContent, ShallowOrganizationResponseDTO expectedOrganization, IEnumerable<DataProcessingRegistrationResponseDTO> dtos)
        {
            var dto = Assert.Single(dtos, dpr => dpr.Uuid == expectedContent.Uuid);
            AssertExpectedShallowDPR(expectedContent, expectedOrganization, dto);
        }

        private static void AssertExpectedShallowDPR(DataProcessingRegistrationResponseDTO expectedContent, ShallowOrganizationResponseDTO expectedOrganization, DataProcessingRegistrationResponseDTO dto)
        {
            Assert.Equal(expectedContent.Uuid, dto.Uuid);
            Assert.Equal(expectedContent.Name, dto.Name);
            Assert.Equal(expectedOrganization.Uuid, dto.OrganizationContext.Uuid);
            Assert.Equal(expectedOrganization.Name, dto.OrganizationContext.Name);
            Assert.Equal(expectedOrganization.Cvr, dto.OrganizationContext.Cvr);
        }

        private async Task<(Cookie cookie, ShallowOrganizationResponseDTO organization)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync();
            var (userId, cookie) = await CreateApiUserAsync(organization.Uuid);
            await HttpApi.SendAssignRoleToUserAsync(userId, OrganizationRole.GlobalAdmin, organization.Uuid).DisposeAsync();
            return (cookie, organization);
        }
        private async Task<(Guid userUuid, Cookie cookie)> CreateApiUserAsync(Guid organizationUuid)
        {
            var userAndGetCookie = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.GlobalAdmin, organizationUuid);
            return (userAndGetCookie.userUuid, userAndGetCookie.loginCookie);
        }
    }
}
