﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Newtonsoft.Json.Linq;
using Presentation.Web.Models.API.V2.Request.Organization;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Extensions;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.Internal.Organizations;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationInternalApiV2Test: OrganizationApiV2TestBase
    {
        private const int CvrMaxLength = 10;

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true, true, true)]
        [InlineData(OrganizationRole.LocalAdmin, true, false, false)]
        [InlineData(OrganizationRole.User, true, false, false)]
        public async Task Can_Get_Specific_Organization_Permissions(OrganizationRole userRole, bool expectRead, bool expectModify, bool expectDelete)
        {
            //Arrange
            var cookie = await HttpApi.GetCookieAsync(userRole);
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());


            //Act
            var permissionsResponseDto = await OrganizationV2Helper.GetPermissionsAsync(cookie, organization.Uuid);

            //Assert - exhaustive content assertions are done in the read-after-write assertion tests (POST/PUT)
            var expected = new ResourcePermissionsResponseDTO()
            {
                Read = expectRead,
                Modify = expectModify,
                Delete = expectDelete
            };
            Assert.Equivalent(expected, permissionsResponseDto);
        }

        [Fact]
        public async Task CanPatchOrganizationMasterDataWithValues()
        {
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var patchDto = new OrganizationMasterDataRequestDTO
            {
                Address = A<string>(),
                Cvr = A<string>().Truncate(CvrMaxLength),
                Email = A<string>(),
                Phone = A<string>()
            };

            var organizations = await OrganizationV2Helper.GetOrganizationsAsync(regularUserToken.Token, 0, 250);
            var organizationToPatch = organizations.First();
            Assert.NotNull(organizationToPatch);

            var response =
                await OrganizationInternalV2Helper.PatchOrganizationMasterData(organizationToPatch.Uuid, patchDto);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(organizationToPatch.Uuid.ToString(), content);
            Assert.Contains(patchDto.Cvr, content);
        }

        [Fact]
        public async Task CanPatchOrganizationMasterDataWithNull()
        {
            var patchDto = new OrganizationMasterDataRequestDTO();

            var organizationToPatch = await GetOrganization();

            var response =
                await OrganizationInternalV2Helper.PatchOrganizationMasterData(organizationToPatch.Uuid, patchDto);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(organizationToPatch.Uuid.ToString(), content);
            Assert.Contains("null", content);
        }
        
        [Fact]
        public async Task CanGetOrganizationMasterDataRoles()
        {
            var organization = await GetOrganization();
            
            var response = await OrganizationInternalV2Helper.GetOrganizationMasterDataRoles(organization.Uuid);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private static async Task<OrganizationResponseDTO> GetOrganization()
        {
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var organizations = await OrganizationV2Helper.GetOrganizationsAsync(regularUserToken.Token, 0, 250);
            var organization = organizations.First();
            Assert.NotNull(organization);
            return organization;
        }
    }
}
