using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools.Model;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External.Rights
{
    public class RightsHelper
    {
        public static async Task AddUserRole(int userId, int orgId, RightsType rightsType, string name = "", int? objectId = null, int? idOfRoleToUse = null,Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var roleId = idOfRoleToUse ?? await GetDefaultRoleIdForRight(rightsType, cookie);
            var roleDto = new RightDTO
            {
                UserId = userId,
                RoleId = roleId
            };

            var url = TestEnvironment.CreateUrl(await PrepareUrl(orgId, name, rightsType, objectId));
            using var response = await HttpApi.PostWithCookieAsync(url, cookie, roleDto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        public static async Task AddDprRoleToUser(int userId, int orgId, string name = "", int? objectId = null)
        {
            if (objectId == null)
            {
                var dpr = await DataProcessingRegistrationHelper.CreateAsync(orgId, name);
                objectId = dpr.Id;
            }

            var roles = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(objectId.Value);
            var roleDtos = roles.ToList();
            Assert.True(roleDtos.Any());

            var singleRole = roleDtos.FirstOrDefault();
            Assert.NotNull(singleRole);

            using var response = await DataProcessingRegistrationHelper.SendAssignRoleRequestAsync(objectId.Value, singleRole.Id, userId);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private static async Task<string> PrepareUrl(int orgId, string name, RightsType rightsType, int? objectId = null)
        {
            switch (rightsType)
            {
                case RightsType.ItContractRights:
                    if (objectId == null)
                    {
                        var contract = await ItContractHelper.CreateContract(name, orgId);
                        objectId = contract.Id;
                    }
                    return $"api/itcontractright/{objectId.Value}?organizationId={orgId}";
                case RightsType.ItSystemRights:
                    if (objectId == null)
                    {
                        var itSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(name, orgId, AccessModifier.Local);
                        var itSystemUsage = await ItSystemHelper.TakeIntoUseAsync(itSystem.Id, orgId);
                        objectId = itSystemUsage.Id;
                    }
                    return $"api/itSystemUsageRights/{objectId.Value}?organizationId={orgId}";
                case RightsType.OrganizationUnitRights:
                    if (objectId == null)
                    {
                        var orgUnit = await OrganizationUnitHelper.GetOrganizationUnitsAsync(orgId);
                        objectId = orgUnit.Id;
                    }
                    return $"api/organizationunitright/{objectId.Value}?organizationId={orgId}";
                default: throw new Exception("Incorrect Rights Type");
            }
        }

        private static async Task<int> GetDefaultRoleIdForRight(RightsType rightsType, Cookie cookie)
        {
            switch (rightsType)
            {
                case RightsType.ItContractRights:
                    var contractRoles = await ItContractHelper.GetRolesAsync(cookie);
                    Assert.NotEmpty(contractRoles);

                    var singleContractRole = contractRoles.Where(x=>x.IsEnabled && x.IsObligatory).RandomItem();
                    Assert.NotNull(singleContractRole);

                    return singleContractRole.Id;
                case RightsType.ItSystemRights:
                    var systemRoles = await ItSystemHelper.GetRolesAsync(cookie);
                    Assert.NotEmpty(systemRoles);

                    var singleSystemRole = systemRoles.Where(x => x.IsEnabled && x.IsObligatory).RandomItem();
                    Assert.NotNull(singleSystemRole);

                    return singleSystemRole.Id;
                case RightsType.OrganizationUnitRights:
                    var organizationUnitRoles = await OrganizationUnitHelper.GetOrganizationUnitRolesAsync(cookie);
                    Assert.NotEmpty(organizationUnitRoles);

                    var singleOrganizationUnit = organizationUnitRoles.Where(x=>x.IsEnabled && x.IsObligatory).RandomItem();
                    Assert.NotNull(singleOrganizationUnit);

                    return singleOrganizationUnit.Id;
                default: throw new Exception("Incorrect Rights Type");
            }
        }
    }
}
