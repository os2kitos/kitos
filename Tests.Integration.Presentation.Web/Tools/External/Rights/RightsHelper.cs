using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External.Rights
{
    public class RightsHelper
    {
        public static async Task AddUserRole(int userId, int orgId, RightsType rightsType, string name, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            
            var roleDto = new RightDTO
            {
                UserId = userId,
                RoleId = (int)OrganizationRole.LocalAdmin
            };

            var url = TestEnvironment.CreateUrl(await PrepareUrl(orgId, name, rightsType));
            var response = await HttpApi.PostWithCookieAsync(url, cookie, roleDto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        public static async Task AddOrganizationRoleToUser(int userId, int orgId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            
            var roleDto = new OrgRightDTO
            {
                UserId = userId,
                Role = OrganizationRole.LocalAdmin.ToString("G")
            };

            var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"odata/organizations({orgId})/Rights"), cookie, roleDto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        public static async Task AddDprRoleToUser(int userId, int orgId, string name)
        {
            var dpr= await DataProcessingRegistrationHelper.CreateAsync(orgId, name);

            var roles = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(dpr.Id);
            var roleDtos = roles.ToList();
            Assert.True(roleDtos.Any());

            var singleRole = roleDtos.FirstOrDefault();
            Assert.NotNull(singleRole);

            var response = await DataProcessingRegistrationHelper.SendAssignRoleRequestAsync(dpr.Id, singleRole.Id, userId);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private static async Task<string> PrepareUrl(int orgId, string name, RightsType rightsType)
        {
            switch (rightsType)
            {
                case RightsType.ItContractRights:
                    var contract = await ItContractHelper.CreateContract(name, orgId);
                    return $"api/itcontractright/{contract.Id}?organizationId={orgId}";
                case RightsType.ItProjectRights:
                    var project= await ItProjectHelper.CreateProject(name, orgId);
                    return $"api/itprojectright/{project.Id}?organizationId={orgId}";
                case RightsType.ItSystemRights:
                    var itSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(name, orgId, AccessModifier.Local);
                    var itSystemUsage = await ItSystemUsageHelper.CreateItSystemUsage(orgId, itSystem.Id);
                    return $"api/itSystemUsageRights/{itSystemUsage.Id}?organizationId={orgId}";
                case RightsType.OrganizationUnitRights:
                    var orgUnit = OrganizationUnitHelper.GetOrganizationUnits(orgId);
                    return $"api/organizationunitright/{orgUnit.Result.Id}?organizationId={orgId}";
                default: throw new Exception("Incorrect Rights Type");
            }
        }
    }
}
