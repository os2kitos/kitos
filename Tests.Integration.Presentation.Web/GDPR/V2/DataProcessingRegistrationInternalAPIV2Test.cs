using Core.DomainModel.Organization;
using Core.DomainModel;
using Presentation.Web.Models.API.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DomainServices.Extensions;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using System.Net;
using Tests.Integration.Presentation.Web.Tools.External;
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
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);

            //Act
            var dprs = await DataProcessingRegistrationInternalV2Helper.GetDPRsAsync(token, 0, 250);

            //Assert
            Assert.Equal(2, dprs.Count());
            AssertExpectedShallowDPRs(dpr1, organization, dprs);
            AssertExpectedShallowDPRs(dpr2, organization, dprs);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_Paging()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            var dpr3 = await CreateDPRAsync(organization.Id);

            //Act
            var page1Dprs = (await DataProcessingRegistrationInternalV2Helper.GetDPRsAsync(token, 0, 2)).ToList();
            var page2Dprs = (await DataProcessingRegistrationInternalV2Helper.GetDPRsAsync(token, 1, 2)).ToList();

            //Assert
            Assert.Equal(2, page1Dprs.Count());
            AssertExpectedShallowDPRs(dpr1, organization, page1Dprs);
            AssertExpectedShallowDPRs(dpr2, organization, page1Dprs);

            var page2Dpr = Assert.Single(page2Dprs);
            AssertExpectedShallowDPR(dpr3, organization, page2Dpr);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_LastModified_Filtering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            var dpr3 = await CreateDPRAsync(organization.Id);

            foreach (var dto in new[] { dpr2, dpr3, dpr1 })
            {
                using var patchResponse = await DataProcessingRegistrationV2Helper.SendPatchName(token, dto.Uuid, CreateName());
                Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
            }

            var referenceChange = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, dpr3.Uuid);

            //Act
            var dtos = await DataProcessingRegistrationInternalV2Helper.GetDPRsAsync(token, page: 0, pageSize: 10, changedSinceGtEq: referenceChange.LastModified);

            //Assert that the right items are returned in the correct order
            Assert.Equal(new[] { dpr3.Uuid, dpr1.Uuid }, dtos.Select(x => x.Uuid));

        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_NameContains_Filtering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            var dpr3 = await CreateDPRAsync(organization.Id);

            foreach (var dto in new[] { dpr2, dpr3, dpr1 })
            {
                using var patchResponse = await DataProcessingRegistrationV2Helper.SendPatchName(token, dto.Uuid, CreateName());
                Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
            }

            //Act
            var dtos = await DataProcessingRegistrationInternalV2Helper.GetDPRsAsync(token, page: 0, pageSize: 10, nameContains: dpr1.Name);

            var response = Assert.Single(dtos);
            Assert.Equal(response.Uuid, dpr1.Uuid);

        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_OrganizationFiltering()
        {
            //Arrange
            var (token, user, organization1) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization1.Id);
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var dpr2 = await CreateDPRAsync(organization2.Id);

            //Act
            var dprs = await DataProcessingRegistrationInternalV2Helper.GetDPRsAsync(token, 0, 250);

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

        private async Task<(string token, User user, OrganizationDTO organization)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUserAsync(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            return (token, user, organization);
        }
        private async Task<(User user, string token)> CreateApiUserAsync(OrganizationDTO organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Id, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
        }

        private async Task<User> CreateUser(OrganizationDTO organization)
        {
            var userId = await HttpApi.CreateOdataUserAsync(ObjectCreateHelper.MakeSimpleApiUserDto(CreateEmail(), false), OrganizationRole.User, organization.Id);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userId));
            return user;
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
