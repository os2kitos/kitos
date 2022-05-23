using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.SSO;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External.Rights
{
    public class RightsHelper
    {
        public static async Task AddUserRole(int userId, int orgId, RightsType rightsType, string name, int projectId = 0, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            
            var roleDto = new RightDTO
            {
                UserId = userId,
                RoleId = (int)OrganizationRole.LocalAdmin
            };

            var url = TestEnvironment.CreateUrl(await PrepareUrl(orgId, name, rightsType, projectId));
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

        public static async Task AddSsoIdentityToUser(int userId)
        {
            var user = await UserHelper.GetUserByIdAsync(userId);
            var ssoIdentity = new SsoUserIdentity
            {
                ExternalUuid = user.Uuid
            };

            DatabaseAccess.MutateDatabase(x =>
            {
                x.SsoUserIdentities.Add(ssoIdentity);
                x.SaveChanges();
            });
        }

        private static async Task<string> PrepareUrl(int orgId, string name, RightsType rightsType, int projectId = 0)
        {
            switch (rightsType)
            {
                case RightsType.ItContractRights:
                    var contract = await ItContractHelper.CreateContract(name, orgId);
                    return $"api/itcontractright/{contract.Id}?organizationId={orgId}";
                case RightsType.ItProjectRights:
                    return $"api/itprojectright/{projectId}?organizationId={orgId}";
                case RightsType.ItSystemRights:
                    var itSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(name, orgId, AccessModifier.Local);
                    var itSystemUsage = await ItSystemHelper.TakeIntoUseAsync(itSystem.Id, orgId);
                    return $"api/itSystemUsageRights/{itSystemUsage.Id}?organizationId={orgId}";
                case RightsType.OrganizationUnitRights:
                    var orgUnit = OrganizationUnitHelper.GetOrganizationUnits(orgId);
                    return $"api/organizationunitright/{orgUnit.Result.Id}?organizationId={orgId}";
                default: throw new Exception("Incorrect Rights Type");
            }
        }
    }
}
