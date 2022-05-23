﻿using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class OrganizationUnitHelper
    {
        public static async Task<OrganizationUnit> CreateOrganizationUnit(int orgId, string name, Cookie optionalLogin = null)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitDto = new OrgUnitDTO()
            {
                Name = name,
                OrganizationId = orgId
            };
            var orgUnitUrl = TestEnvironment.CreateUrl($"odata/OrganizationUnit");

            var response = await HttpApi.PostWithCookieAsync(orgUnitUrl, cookie, orgUnitDto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<OrganizationUnit>();
        }

        public static async Task<OrgUnitDTO> GetOrganizationUnits(int orgId, Cookie optionalLogin = null)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/OrganizationUnit?organization={orgId}");

            var response = await HttpApi.GetWithCookieAsync(orgUnitUrl, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsKitosApiResponseAsync<OrgUnitDTO>();
        }
    }
}
