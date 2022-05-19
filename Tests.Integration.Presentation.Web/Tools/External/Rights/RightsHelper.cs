using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools.Model;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External.Rights
{
    public class RightsHelper
    {
        public static async Task AddUserRight(int userId, int orgId, RightsType rightsType, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var roleDto = new RightDTO
            {
                UserId = userId,
                RoleId = (int)OrganizationRole.LocalAdmin //role.ToString("G")
            };

            var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl(PrepareUrl(1, orgId, rightsType)), cookie, roleDto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private static string PrepareUrl(int id, int orgId, RightsType rightsType)
        {
            switch (rightsType)
            {
                case RightsType.ItContractRights:
                    return $"api/itcontractright/{id}?organizationId={orgId}";
                case RightsType.ItProjectRights:
                    return $"api/itcontractright/{id}?organizationId={orgId}";
                case RightsType.DprRights:
                    return $"api/itcontractright/{id}?organizationId={orgId}";
                case RightsType.ItSystemRights:
                    return $"api/itsystemusageright/{id}?organizationId={orgId}";
                case RightsType.OrganizationRights:
                    return $"api/itcontractright/{id}?organizationId={orgId}";
                case RightsType.OrganizationUnitRights:
                    return $"api/organizationunitright/{id}?organizationId={orgId}";
                default: throw new Exception();
            }
        }
    }
}
