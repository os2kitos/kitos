using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainModel;
using ExpectedObjects;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Xunit;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Tests.Toolkit.Extensions;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Internal.Response.Roles;
using System.Net;
using System.Net.Http;
using Presentation.Web.Models.API.V2.Response.Contract;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Organization;

namespace Tests.Integration.Presentation.Web.Contract.V2
{
    public class ItContractsInternalApiV2Test : BaseTest
    {
        [Fact]
        public async Task Can_Get_Available_DataProcessingRegistrations()
        {
            //Arrange
            var orgUuid = DefaultOrgUuid;
            var registrationName = A<string>();
            var registration1 = await CreateDPRAsync(orgUuid, registrationName + "1");
            var registration2 = await CreateDPRAsync(orgUuid, registrationName + "2");
            var contract = await CreateItContractAsync(orgUuid);

            //Act
            var dtos = (await ItContractV2Helper.GetAvailableDataProcessingRegistrationsAsync(contract.Uuid, registrationName)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            dtos.Select(x => new { x.Uuid, x.Name }).ToExpectedObject().ShouldMatch(new[] { new { registration1.Uuid, registration1.Name }, new { registration2.Uuid, registration2.Name } });
        }

        [Fact]
        public async Task Can_Get_Hierarchy()
        {
            //Arrange
            var (_, organization) = await CreateStakeHolderUserInNewOrganizationAsync();
            var (rootUuid, _, _, _, createdContracts) = CreateHierarchy(organization.Uuid);

            //Act
            var response = await ItContractV2Helper.GetHierarchyAsync(rootUuid);

            //Assert
            var hierarchy = response.ToList();
            Assert.Equal(createdContracts.Count, hierarchy.Count);

            foreach (var node in hierarchy)
            {
                var contract = Assert.Single(createdContracts, x => x.Uuid == node.Node.Uuid);
                if (contract.Uuid == rootUuid)
                {
                    Assert.Null(node.Parent);
                }
                else
                {
                    Assert.NotNull(node.Parent);
                    Assert.Equal(node.Parent.Uuid, contract.Parent.Uuid);
                }
            }
        }
        [Fact]
        public async Task Can_Get_SubHierarchy()
        {
            //Arrange
            var (_, organization) = await CreateStakeHolderUserInNewOrganizationAsync();
            var (rootUuid, childContractUuid, siblingContractUuid, grandchildContractUuid, _) = CreateHierarchy(organization.Uuid);

            //Act
            var response = await ItContractV2Helper.GetSubHierarchyAsync(childContractUuid);

            //Assert
            var hierarchy = response.ToList();
            var hierarchyUuids = hierarchy.Select(x => x.Node.Uuid).ToList();
            Assert.Contains(childContractUuid, hierarchyUuids);
            Assert.Contains(grandchildContractUuid, hierarchyUuids);
            Assert.DoesNotContain(rootUuid, hierarchyUuids);
            Assert.DoesNotContain(siblingContractUuid, hierarchyUuids);
        }

        [Fact]
        public async Task Can_GET_Roles()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUserAsync(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();

            var (roles, users) = await CreateRoles(organization);
            var createdContract = await ItContractV2Helper.PostContractAsync(token, new CreateNewContractRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                Roles = roles
            });

            //Act
            var assignedRoles = (await ItContractV2Helper.GetRoleAssignmentsInternalAsync(createdContract.Uuid)).ToList();

            //Assert
            Assert.Equal(2, assignedRoles.Count);
            Assert.Contains(assignedRoles, assignment => MatchExpectedExtendedAssignment(assignment, roles.First(), users.First()));
            Assert.Contains(assignedRoles, assignment => MatchExpectedExtendedAssignment(assignment, roles.Last(), users.Last()));
        }

        [Fact]
        public async Task Can_PATCH_Add_RoleAssignment()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUserAsync(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();
            var (roles, users) = await CreateRoles(organization);
            var createdContract = await ItContractV2Helper.PostContractAsync(token, new CreateNewContractRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid
            });

            var assignment1 = roles.First();
            var assignment2 = roles.Last();

            //Act
            using var assignmentResponse1 = await ItContractV2Helper.SendPatchAddRoleAssignment(createdContract.Uuid, assignment1);
            using var duplicateAssignment1 = await ItContractV2Helper.SendPatchAddRoleAssignment(createdContract.Uuid, assignment1);
            using var assignmentResponse2 = await ItContractV2Helper.SendPatchAddRoleAssignment(createdContract.Uuid, assignment2);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, duplicateAssignment1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, assignmentResponse1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, assignmentResponse2.StatusCode);
            var updatedDTO = await assignmentResponse2.ReadResponseBodyAsAsync<ItContractResponseDTO>();
            var rolesDTO = updatedDTO.Roles.ToList();
            Assert.Equal(2, rolesDTO.Count);
            Assert.Contains(rolesDTO, r => MatchExpectedAssignment(r, assignment1, users.First()));
            Assert.Contains(rolesDTO, r => MatchExpectedAssignment(r, assignment2, users.Last()));
        }

        [Fact]
        public async Task Can_PATCH_Remove_RoleAssignment()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUserAsync(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Uuid, OrganizationRole.LocalAdmin, organization.Uuid).DisposeAsync();
            var (roles, users) = await CreateRoles(organization);
            var createdContract = await ItContractV2Helper.PostContractAsync(token, new CreateNewContractRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                Roles = roles
            });

            var assignment1 = roles.First();
            var assignment2 = roles.Last();

            //Act
            using var assignment1Response = await ItContractV2Helper.SendPatchAddRoleAssignment(createdContract.Uuid, assignment1);
            using var assignment2Response = await ItContractV2Helper.SendPatchAddRoleAssignment(createdContract.Uuid, assignment2);
            using var removeAssignment = await ItContractV2Helper.SendPatchRemoveRoleAssignment(createdContract.Uuid, assignment1);
            using var duplicateRemoveAssignment = await ItContractV2Helper.SendPatchRemoveRoleAssignment(createdContract.Uuid, assignment1);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, duplicateRemoveAssignment.StatusCode);
            Assert.Equal(HttpStatusCode.OK, removeAssignment.StatusCode);
            var updatedDTO = await removeAssignment.ReadResponseBodyAsAsync<ItContractResponseDTO>();
            var roleAssignment = Assert.Single(updatedDTO.Roles);
            MatchExpectedAssignment(roleAssignment, assignment2, users.Last());
        }

        [Fact]
        public async Task Cannot_PATCH_Self_As_Parent()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUserAsync(organization);
            var createdContract = await ItContractV2Helper.PostContractAsync(token, new CreateNewContractRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid
            });
            var contractUuid = createdContract.Uuid;

            //Act
            using var response = await ItContractV2Helper.SendPatchParentContractAsync(token, createdContract.Uuid, contractUuid);

            //Assert
            await AssertFailedToPatchParent(response);
        }

        [Fact]
        public async Task Cannot_PATCH_Child_As_Parent()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var (user, token) = await CreateApiUserAsync(organization);
            var createdParentContract = await ItContractV2Helper.PostContractAsync(token, new CreateNewContractRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid
            });
            var createdChildContract = await ItContractV2Helper.PostContractAsync(token, new CreateNewContractRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                ParentContractUuid = createdParentContract.Uuid
            });
            var childUuid = createdChildContract.Uuid;

            //Act
            using var response = await ItContractV2Helper.SendPatchParentContractAsync(token, createdParentContract.Uuid, childUuid);

            //Assert
            await AssertFailedToPatchParent(response);

        }

        [Fact]
        public async Task Can_Delete_Contract_With_Children()
        {
            //Arrange
            var globalAdminToken = await GetGlobalToken();
            var organization = await CreateOrganizationAsync();
            var (_, token) = await CreateApiUserAsync(organization);
            var contract = await CreateItContractAsync(organization.Uuid);
            var contract2 = await CreateItContractAsync(organization.Uuid);
            using var patchParentResponse = await ItContractV2Helper.SendPatchParentContractAsync(token, contract2.Uuid, contract.Uuid);
            Assert.Equal(HttpStatusCode.OK, patchParentResponse.StatusCode);


            //Act
            await ItContractV2Helper.DeleteWithChildrenAsync(contract.Uuid);

            //Assert
            await ItContractV2Helper.SendGetItContractAsync(globalAdminToken, contract.Uuid)
                .WithExpectedResponseCode(HttpStatusCode.NotFound).DisposeAsync();

            await ItContractV2Helper.SendGetItContractAsync(globalAdminToken, contract2.Uuid)
                .WithExpectedResponseCode(HttpStatusCode.NotFound).DisposeAsync();
        }

        [Fact]
        public async Task Can_Transfer_Multiple_Contracts()
        {
            //Arrange
            var globalToken = await GetGlobalToken();
            var organizationUuid = DefaultOrgUuid;
            var contract = await CreateItContractAsync(organizationUuid);
            var contract2 = await CreateItContractAsync(organizationUuid);
            var contract3 = await CreateItContractAsync(organizationUuid);

            var request = new MultipleContractsRequestDto
            {
                ContractUuids = new List<Guid> { contract2.Uuid, contract3.Uuid },
                ParentUuid = contract.Uuid
            };

            //Act
            await ItContractV2Helper.TransferMultipleAsync(request);

            //Assert
            var contract2Response = await ItContractV2Helper.GetItContractAsync(globalToken, contract2.Uuid);
            var contract3Response = await ItContractV2Helper.GetItContractAsync(globalToken, contract3.Uuid);

            Assert.Equal(contract.Uuid, contract2Response.ParentContract.Uuid);
            Assert.Equal(contract.Uuid, contract3Response.ParentContract.Uuid);
        }

        [Fact]
        public async Task Can_Transfer_Multiple_Contracts_To_None()
        {
            //Arrange
            var globalToken = await GetGlobalToken();
            var organizationUuid = DefaultOrgUuid;
            var contract2 = await CreateItContractAsync(organizationUuid);
            var contract3 = await CreateItContractAsync(organizationUuid);

            var request = new MultipleContractsRequestDto()
            {
                ContractUuids = new List<Guid> { contract2.Uuid, contract3.Uuid }
            };

            //Act
            await ItContractV2Helper.TransferMultipleAsync(request);

            //Assert
            var contract2Response = await ItContractV2Helper.GetItContractAsync(globalToken, contract2.Uuid);
            var contract3Response = await ItContractV2Helper.GetItContractAsync(globalToken, contract3.Uuid);

            Assert.Null(contract2Response.ParentContract);
            Assert.Null(contract3Response.ParentContract);
        }

        protected async Task<(string token, ShallowOrganizationResponseDTO createdOrganization)> CreateStakeHolderUserInNewOrganizationAsync()
        {
            var organization = await CreateOrganizationAsync();

            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(),
                OrganizationRole.User, organization.Uuid, true, true);
            return (token, organization);
        }

        private (Guid rootUuid, Guid childContractUuid, Guid siblingContractUuid, Guid grandchildContractUuid, IReadOnlyList<ItContract> createdItContracts) CreateHierarchy(Guid organizationUuid)
        {
            var orgId = DatabaseAccess.GetEntityId<Organization>(organizationUuid);
            var rootContract = CreateContract(orgId);
            var childContract = CreateContract(orgId);
            var siblingContract = CreateContract(orgId);
            var grandchildContract = CreateContract(orgId);

            var createdSystems = new List<ItContract> { rootContract, childContract, siblingContract, grandchildContract };

            childContract.Children = new List<ItContract>
            {
                grandchildContract
            };
            rootContract.Children = new List<ItContract>
            {
                childContract, siblingContract
            };

            DatabaseAccess.MutateEntitySet<ItContract>(repository =>
            {
                repository.Insert(rootContract);
            });

            return (rootContract.Uuid, childContract.Uuid, siblingContract.Uuid, grandchildContract.Uuid, createdSystems);
        }

        private ItContract CreateContract(int orgId)
        {
            return new ItContract
            {
                Name = CreateName(),
                OrganizationId = orgId,
                ObjectOwnerId = TestEnvironment.DefaultUserId,
                LastChangedByUserId = TestEnvironment.DefaultUserId
            };
        }

        private async Task<(User user, string token)> CreateApiUserAsync(ShallowOrganizationResponseDTO organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.LocalAdmin, organization.Uuid, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ByUuid(userAndGetToken.userUuid));
            return (user, userAndGetToken.token);
        }

        private async Task<(List<RoleAssignmentRequestDTO>, List<User>)> CreateRoles(ShallowOrganizationResponseDTO organization)
        {
            var (user1, _) = await CreateApiUserAsync(organization);
            var (user2, _) = await CreateApiUserAsync(organization);
            var contractRoles = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.ItContractRoles, organization.Uuid, 10, 0)).RandomItems(2).ToList();
            var role1 = contractRoles.First();
            var role2 = contractRoles.Last();
            var roles = new List<RoleAssignmentRequestDTO>
            {
                new()
                {
                    RoleUuid = role1.Uuid,
                    UserUuid = user1.Uuid
                },
                new()
                {
                    RoleUuid = role2.Uuid,
                    UserUuid = user2.Uuid
                }
            };
            return (roles, new List<User> { user1, user2 });
        }

        private static bool MatchExpectedAssignment(RoleAssignmentResponseDTO assignment, RoleAssignmentRequestDTO expectedRole, User expectedUser)
        {
            return assignment.Role.Uuid == expectedRole.RoleUuid &&
                   assignment.User.Name == expectedUser.GetFullName() &&
                   assignment.User.Uuid == expectedUser.Uuid;
        }

        private static bool MatchExpectedBulkAssignment(RoleAssignmentResponseDTO actual, BulkRoleAssignmentRequestDTO expected)
        {
            return actual.Role.Uuid == expected.RoleUuid && expected.UserUuids.Contains(actual.User.Uuid);
        }

        private static bool MatchExpectedExtendedAssignment(ExtendedRoleAssignmentResponseDTO assignment, RoleAssignmentRequestDTO expectedRole, User expectedUser)
        {
            return assignment.Role.Uuid == expectedRole.RoleUuid &&
                   assignment.User.Name == expectedUser.GetFullName() &&
                   assignment.User.Uuid == expectedUser.Uuid &&
                   assignment.User.Email == expectedUser.Email;
        }

        private string CreateName()
        {
            return $"{nameof(ItContractsInternalApiV2Test)}{A<string>()}";
        }

        private static async Task AssertFailedToPatchParent(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Failed to set parent", content);
        }
    }
}
