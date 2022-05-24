using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainModel.SSO;
using Core.DomainServices.Extensions;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External.Rights
{
    public class RightsHelper
    {
        public static async Task AddUserRole(int userId, int orgId, RightsType rightsType, string name, 
            int objectId = 0, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var roleId = await GetDefaultRoleIdForRight(rightsType, cookie, objectId);
            var roleDto = new RightDTO
            {
                UserId = userId,
                RoleId = roleId
            };

            var url = TestEnvironment.CreateUrl(await PrepareUrl(orgId, name, rightsType, objectId));
            using var response = await HttpApi.PostWithCookieAsync(url, cookie, roleDto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        public static async Task AddDprRoleToUser(int userId, int orgId, string name)
        {
            var dpr = await DataProcessingRegistrationHelper.CreateAsync(orgId, name);

            var roles = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(dpr.Id);
            var roleDtos = roles.ToList();
            Assert.True(roleDtos.Any());

            var singleRole = roleDtos.FirstOrDefault();
            Assert.NotNull(singleRole);

            using var response = await DataProcessingRegistrationHelper.SendAssignRoleRequestAsync(dpr.Id, singleRole.Id, userId);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private static async Task<string> PrepareUrl(int orgId, string name, RightsType rightsType, int objectId = 0)
        {
            switch (rightsType)
            {
                case RightsType.ItContractRights:
                    var contract = await ItContractHelper.CreateContract(name, orgId);
                    return $"api/itcontractright/{contract.Id}?organizationId={orgId}";
                case RightsType.ItProjectRights:
                    return $"api/itprojectright/{objectId}?organizationId={orgId}";
                case RightsType.ItSystemRights:
                    var itSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(name, orgId, AccessModifier.Local);
                    var itSystemUsage = await ItSystemHelper.TakeIntoUseAsync(itSystem.Id, orgId);
                    return $"api/itSystemUsageRights/{itSystemUsage.Id}?organizationId={orgId}";
                case RightsType.OrganizationUnitRights:
                    var orgUnit = await OrganizationUnitHelper.GetOrganizationUnitsAsync(orgId);
                    return $"api/organizationunitright/{orgUnit.Id}?organizationId={orgId}";
                default: throw new Exception("Incorrect Rights Type");
            }
        }

        private static async Task<int> GetDefaultRoleIdForRight(RightsType rightsType, Cookie cookie, int objectId = 0)
        {
            switch (rightsType)
            {
                case RightsType.ItContractRights:
                    var contractRoles = await ItContractHelper.GetRolesAsync(cookie);
                    Assert.NotEmpty(contractRoles);

                    var singleContractRole = contractRoles.FirstOrDefault();
                    Assert.NotNull(singleContractRole);

                    return singleContractRole.Id;
                case RightsType.ItProjectRights:
                    var projectRoles = await ItProjectHelper.GetRolesAsync(cookie);
                    Assert.NotEmpty(projectRoles);

                    var singleProjectRole = projectRoles.FirstOrDefault();
                    Assert.NotNull(singleProjectRole);

                    return singleProjectRole.Id;
                case RightsType.ItSystemRights:
                    var systemRoles = await ItSystemHelper.GetRolesAsync(cookie);
                    Assert.NotEmpty(systemRoles);

                    var singleSystemRole = systemRoles.FirstOrDefault();
                    Assert.NotNull(singleSystemRole);

                    return singleSystemRole.Id;
                case RightsType.OrganizationUnitRights:
                    var organizationUnitRoles = await OrganizationUnitHelper.GetOrganizationUnitRolesAsync(cookie);
                    Assert.NotEmpty(organizationUnitRoles);

                    var singleOrganizationUnit = organizationUnitRoles.FirstOrDefault();
                    Assert.NotNull(singleOrganizationUnit);

                    return singleOrganizationUnit.Id;
                default: throw new Exception("Incorrect Rights Type");
            }
        }
    }
}
