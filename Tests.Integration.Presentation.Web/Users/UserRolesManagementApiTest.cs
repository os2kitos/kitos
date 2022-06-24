using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.Users;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External.Rights;
using Tests.Integration.Presentation.Web.Tools.Model;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Users
{
    public class UserRolesManagementApiTest : WithAutoFixture
    {
        private string CreateName() => $"{nameof(UserRolesManagementApiTest)}{A<Guid>():N}";
        private string CreateEmail() => $"{A<Guid>():N}@kitos.dk";

        [Fact]
        public async Task Can_Get_User_Roles()
        {
            // Arrange
            var globalAdminCookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var userWithRoles = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.LocalAdmin, CreateName(), CreateName(), organization.Id);
            var organizationRoles = new[]
            {
                OrganizationRole.ContractModuleAdmin,
                OrganizationRole.OrganizationModuleAdmin,
                OrganizationRole.ProjectModuleAdmin,
                OrganizationRole.RightsHolderAccess,
                OrganizationRole.SystemModuleAdmin
            };
            var businessRoleScopes = EnumRange.All<BusinessRoleScope>().ToList();
            await AssignRoles(organization, userWithRoles.userId, organizationRoles, businessRoleScopes);

            // Act
            var result = await GetUserRolesAsync(organization, userWithRoles, globalAdminCookie);

            // Assert
            AssertGetRolesResult(result, organizationRoles.Append(OrganizationRole.LocalAdmin)/*Local admin was initial role*/, businessRoleScopes);
        }

        [Fact]
        public async Task Can_Remove_Range_Of_User_Roles()
        {
            // Arrange

            // Act

            // Assert

        }

        [Fact]
        public async Task Can_Transfer_Range_Of_User_Roles()
        {
            // Arrange

            // Act

            // Assert

        }

        [Fact]
        public async Task Can_Delete_User_From_Organization()
        {
            // Arrange

            // Act

            // Assert

        }

        private static async Task<OrganizationUserRoleAssignmentsDTO> GetUserRolesAsync(OrganizationDTO organization,
            (int userId, KitosCredentials credentials, Cookie loginCookie) userWithRoles, Cookie globalAdminCookie)
        {
            var url = TestEnvironment.CreateUrl($"api/v1/organizations/{organization.Id}/users/{userWithRoles.userId}/roles");
            using var response = await HttpApi.GetWithCookieAsync(url, globalAdminCookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.ReadResponseBodyAsKitosApiResponseAsync<OrganizationUserRoleAssignmentsDTO>();
            return result;
        }

        private async Task AssignRoles(OrganizationDTO organization, int userId, IEnumerable<OrganizationRole> orgRoles, IEnumerable<BusinessRoleScope> businessRole)
        {

            foreach (var businessRoleScope in businessRole)
            {
                switch (businessRoleScope)
                {
                    case BusinessRoleScope.ItSystemUsage:
                        var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Public);
                        var systemUsageDto = await ItSystemHelper.TakeIntoUseAsync(system.Id, organization.Id);
                        await RightsHelper.AddUserRole(userId, organization.Id, RightsType.ItSystemRights, objectId: systemUsageDto.Id);
                        break;
                    case BusinessRoleScope.ItContract:
                        var contractDto = await ItContractHelper.CreateContract(CreateName(), organization.Id);
                        await RightsHelper.AddUserRole(userId, organization.Id, RightsType.ItContractRights, objectId: contractDto.Id);
                        break;
                    case BusinessRoleScope.DataProcessingRegistration:
                        var dprDto = await DataProcessingRegistrationHelper.CreateAsync(organization.Id, CreateName());
                        await RightsHelper.AddDprRoleToUser(userId, organization.Id, objectId: dprDto.Id);
                        break;
                    case BusinessRoleScope.ItProject:
                        var projectDto = await ItProjectHelper.CreateProject(CreateName(), organization.Id);
                        await RightsHelper.AddUserRole(userId, organization.Id, RightsType.ItProjectRights, objectId: projectDto.Id);
                        break;
                    case BusinessRoleScope.OrganizationUnit:
                        await RightsHelper.AddUserRole(userId, organization.Id, RightsType.OrganizationUnitRights);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            //Administrative roles
            foreach (var organizationRole in orgRoles)
            {
                await HttpApi.SendAssignRoleToUserAsync(userId, organizationRole, organization.Id).WithExpectedResponseCode(HttpStatusCode.Created).DisposeAsync();
            }
        }

        private static void AssertGetRolesResult(OrganizationUserRoleAssignmentsDTO result, IEnumerable<OrganizationRole> orgRoles, IEnumerable<BusinessRoleScope> businessRoles)
        {
            var expectedOrgRoles = orgRoles.OrderBy(x => x).ToList();
            var expectedBusinessRoles = businessRoles.OrderBy(x => x).ToList();

            var actualBusinessRoles = result.Rights.Select(x => x.Scope).OrderBy(x => x).ToList();
            var actualOrgRoles = result.AdministrativeAccessRoles.OrderBy(x => x).ToList();

            Assert.Equal(expectedBusinessRoles.Count, actualBusinessRoles.Count);
            Assert.Equal(expectedOrgRoles.Count, actualOrgRoles.Count);

            foreach (var (expected, actual) in expectedBusinessRoles.Zip(actualBusinessRoles, (expected, actual) => (expected, actual)))
            {
                Assert.Equal(expected, actual);
            }

            foreach (var (expected, actual) in expectedOrgRoles.Zip(actualOrgRoles, (expected, actual) => (expected, actual)))
            {
                Assert.Equal(expected, actual);
            }
        }
    }
}
