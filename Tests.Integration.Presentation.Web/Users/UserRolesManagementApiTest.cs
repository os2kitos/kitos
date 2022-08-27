using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            var userWithRoles = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, CreateName(), CreateName(), organization.Id);
            var organizationRoles = GetAllOrganizationAdminRoles();
            var businessRoleScopes = EnumRange.All<BusinessRoleScope>().ToList();
            await AssignRoles(organization, userWithRoles.userId, organizationRoles, businessRoleScopes);

            // Act
            var result = await GetUserRolesAsync(organization, userWithRoles, globalAdminCookie);

            // Assert
            AssertGetRolesResult(result, organizationRoles, businessRoleScopes);
        }

        [Fact]
        public async Task Can_Remove_Range_Of_User_Roles()
        {
            // Arrange
            var globalAdminCookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var userWithRoles = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, CreateName(), CreateName(), organization.Id);
            var organizationRoles = GetAllOrganizationAdminRoles();
            var businessRoleScopes = EnumRange.All<BusinessRoleScope>().ToList();
            await AssignRoles(organization, userWithRoles.userId, organizationRoles, businessRoleScopes);
            var getAfterAssignResult = await GetUserRolesAsync(organization, userWithRoles, globalAdminCookie);

            // Act
            var url = TestEnvironment.CreateUrl($"api/v1/organizations/{organization.Id}/users/{userWithRoles.userId}/roles/range");
            using var deleteResult = await HttpApi.DeleteWithCookieAsync(url, globalAdminCookie,
                new RemoveUserRightsRequest
                {
                    AdminRoles = getAfterAssignResult.AdministrativeAccessRoles,
                    BusinessRights = getAfterAssignResult.Rights
                });

            // Assert
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);
            var getAfterRemoveResult = await GetUserRolesAsync(organization, userWithRoles, globalAdminCookie);
            AssertGetRolesResult(getAfterRemoveResult, Enumerable.Empty<OrganizationRole>(), Enumerable.Empty<BusinessRoleScope>());
        }

        [Fact]
        public async Task Can_Transfer_Range_Of_User_Roles()
        {
            // Arrange
            var globalAdminCookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var userWithRoles = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, CreateName(), CreateName(), organization.Id);
            var anotherUser = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, CreateName(), CreateName(), organization.Id);
            var organizationRoles = GetAllOrganizationAdminRoles();
            var businessRoleScopes = EnumRange.All<BusinessRoleScope>().ToList();
            await AssignRoles(organization, userWithRoles.userId, organizationRoles, businessRoleScopes);
            var resultAfterAssign = await GetUserRolesAsync(organization, userWithRoles, globalAdminCookie);

            // Act
            var url = TestEnvironment.CreateUrl($"api/v1/organizations/{organization.Id}/users/{userWithRoles.userId}/roles/range/transfer");
            using var deleteResult = await HttpApi.PatchWithCookieAsync(url, globalAdminCookie,
                new TransferRightsRequestDTO
                {
                    ToUserId = anotherUser.userId,
                    AdminRoles = resultAfterAssign.AdministrativeAccessRoles,
                    BusinessRights = resultAfterAssign.Rights
                });
            // Assert
            var getAfterTransferOriginalUser = await GetUserRolesAsync(organization, userWithRoles, globalAdminCookie);
            var getAfterTransferReceivingUser = await GetUserRolesAsync(organization, anotherUser, globalAdminCookie);
            AssertGetRolesResult(getAfterTransferOriginalUser, Enumerable.Empty<OrganizationRole>(), Enumerable.Empty<BusinessRoleScope>());
            AssertGetRolesResult(getAfterTransferReceivingUser, organizationRoles, businessRoleScopes);

        }

        [Fact]
        public async Task Can_Delete_User_From_Organization()
        {
            // Arrange
            var globalAdminCookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var userWithRoles = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, CreateName(), CreateName(), organization.Id);
            var organizationRoles = GetAllOrganizationAdminRoles();
            var businessRoleScopes = EnumRange.All<BusinessRoleScope>().ToList();
            await AssignRoles(organization, userWithRoles.userId, organizationRoles, businessRoleScopes);

            // Act
            var url = TestEnvironment.CreateUrl($"api/v1/organizations/{organization.Id}/users/{userWithRoles.userId}/roles");
            using var deleteResult = await HttpApi.DeleteWithCookieAsync(url, globalAdminCookie);

            // Assert that deletion went ok and that the GET now returns "not found" (user is not found in the organization)
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);
            using var getAfterRemoveResult = await SendGetUserRolesAsync(organization, userWithRoles, globalAdminCookie);
            Assert.Equal(HttpStatusCode.NotFound, getAfterRemoveResult.StatusCode);
        }

        private static async Task<OrganizationUserRoleAssignmentsDTO> GetUserRolesAsync(OrganizationDTO organization,
            (int userId, KitosCredentials credentials, Cookie loginCookie) userWithRoles, Cookie globalAdminCookie)
        {
            using var response = await SendGetUserRolesAsync(organization, userWithRoles, globalAdminCookie);
            var result = await response.ReadResponseBodyAsKitosApiResponseAsync<OrganizationUserRoleAssignmentsDTO>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return result;
        }

        private static async Task<HttpResponseMessage> SendGetUserRolesAsync(OrganizationDTO organization,
            (int userId, KitosCredentials credentials, Cookie loginCookie) userWithRoles, Cookie globalAdminCookie)
        {
            var url = TestEnvironment.CreateUrl($"api/v1/organizations/{organization.Id}/users/{userWithRoles.userId}/roles");
            var response = await HttpApi.GetWithCookieAsync(url, globalAdminCookie);
            return response;
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

        private static OrganizationRole[] GetAllOrganizationAdminRoles()
        {
            return new[]
            {
                OrganizationRole.ContractModuleAdmin,
                OrganizationRole.OrganizationModuleAdmin,
                OrganizationRole.RightsHolderAccess,
                OrganizationRole.SystemModuleAdmin,
                OrganizationRole.LocalAdmin
            };
        }
    }
}
