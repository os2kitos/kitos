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
            await AssignRoles(
                organization,
                userWithRoles.userId,
                new[]
                {
                    OrganizationRole.ContractModuleAdmin,
                    OrganizationRole.OrganizationModuleAdmin,
                    OrganizationRole.ProjectModuleAdmin,
                    OrganizationRole.RightsHolderAccess,
                    OrganizationRole.SystemModuleAdmin
                },
                EnumRange.All<BusinessRoleScope>()
            );

            var url = TestEnvironment.CreateUrl($"api/v1/organizations/{organization.Id}/users/{userWithRoles.userId}/roles");

            // Act
            using var response = await HttpApi.GetWithCookieAsync(url, globalAdminCookie);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.ReadResponseBodyAsKitosApiResponseAsync<OrganizationUserRoleAssignmentsDTO>();
            var roleScopes = result.Rights.Select(x => x.Scope).ToList();
            Assert.Equal(6, result.AdministrativeAccessRoles.Count());
            Assert.Contains(OrganizationRole.LocalAdmin, result.AdministrativeAccessRoles);
            Assert.Contains(OrganizationRole.ContractModuleAdmin, result.AdministrativeAccessRoles);
            Assert.Contains(OrganizationRole.ProjectModuleAdmin, result.AdministrativeAccessRoles);
            Assert.Contains(OrganizationRole.OrganizationModuleAdmin, result.AdministrativeAccessRoles);
            Assert.Contains(OrganizationRole.RightsHolderAccess, result.AdministrativeAccessRoles);
            Assert.Contains(OrganizationRole.SystemModuleAdmin, result.AdministrativeAccessRoles);
            Assert.Equal(5, result.Rights.Count());
            Assert.Contains(BusinessRoleScope.DataProcessingRegistration, roleScopes);
            Assert.Contains(BusinessRoleScope.ItContract, roleScopes);
            Assert.Contains(BusinessRoleScope.ItProject, roleScopes);
            Assert.Contains(BusinessRoleScope.ItSystemUsage, roleScopes);
            Assert.Contains(BusinessRoleScope.OrganizationUnit, roleScopes);
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
    }
}
