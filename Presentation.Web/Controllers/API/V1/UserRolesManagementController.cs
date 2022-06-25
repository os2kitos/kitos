using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Model.Users;
using Core.ApplicationServices.Rights;
using Core.DomainModel.Organization;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1.Users;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    [RoutePrefix("api/v1/organizations/{organizationId}/users/{userId}/roles")]

    public class UserRolesManagementController : BaseApiController
    {
        private readonly IUserRightsService _rightsService;

        public UserRolesManagementController(IUserRightsService rightsService)
        {
            _rightsService = rightsService;
        }

        /// <summary>
        /// Get all user roles in the organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route]
        public HttpResponseMessage Get(int organizationId, int userId)
        {
            return _rightsService
                .GetUserRights(userId, organizationId)
                .Select(ToRoleAssignmentsDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Delete all role assignments in the organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="userId"></param>
        /// <returns>A new <see cref="OrganizationUserRoleAssignmentsDTO"/> which represents the changes assignment state</returns>
        [Route]
        [HttpDelete]
        public HttpResponseMessage Delete(int organizationId, int userId)
        {
            return _rightsService
                .RemoveAllRights(userId, organizationId)
                .Match(FromOperationError, Ok);
        }

        /// <summary>
        /// Delete selected org roles
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="userId"></param>
        /// <param name="assignmentsToDelete"></param>
        /// <returns>A new <see cref="OrganizationUserRoleAssignmentsDTO"/> which represents the changes assignment state</returns>
        [Route("range")]
        [HttpDelete]
        public HttpResponseMessage DeleteRange(int organizationId, int userId, [FromBody] RemoveUserRightsRequest assignmentsToDelete)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var changeParameters = CreateChangeParameters(assignmentsToDelete.AdminRoles, assignmentsToDelete.BusinessRights);

            return _rightsService
                .RemoveRights(userId, organizationId, changeParameters)
                .Match(FromOperationError, Ok);
        }

        /// <summary>
        /// Transfers the selected roles to another user
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="userId"></param>
        /// <param name="assignmentsToTransfer"></param>
        /// <returns>A new <see cref="OrganizationUserRoleAssignmentsDTO"/> which represents the changes assignment state</returns>
        [Route("range/transfer")]
        [HttpPatch]
        public HttpResponseMessage PatchTransferToAnotherUser(int organizationId, int userId, TransferRightsRequestDTO assignmentsToTransfer)
        {
            var changeParameters = CreateChangeParameters(assignmentsToTransfer.AdminRoles, assignmentsToTransfer.BusinessRights);

            return _rightsService
                .TransferRights(userId, assignmentsToTransfer.ToUserId, organizationId, changeParameters)
                .Match(FromOperationError, Ok);
        }

        private static OrganizationUserRoleAssignmentsDTO ToRoleAssignmentsDTO(UserRightsAssignments arg)
        {
            return new OrganizationUserRoleAssignmentsDTO
            {
                AdministrativeAccessRoles = arg.LocalAdministrativeAccessRoles,
                Rights = arg
                    .ContractRights
                    .Select(x => new AssignedRightDTO
                    {
                        RoleName = x.Role.Name,
                        BusinessObjectName = x.Object.Name,
                        RightId = x.Id,
                        Scope = BusinessRoleScope.ItContract

                    })
                    .Concat
                    (
                        arg
                            .ProjectRights
                            .Select(x => new AssignedRightDTO
                            {
                                RoleName = x.Role.Name,
                                BusinessObjectName = x.Object.Name,
                                RightId = x.Id,
                                Scope = BusinessRoleScope.ItProject

                            })
                    )
                    .Concat
                    (
                        arg
                            .DataProcessingRegistrationRights
                            .Select(x => new AssignedRightDTO
                            {
                                RoleName = x.Role.Name,
                                BusinessObjectName = x.Object.Name,
                                RightId = x.Id,
                                Scope = BusinessRoleScope.DataProcessingRegistration

                            })
                    )
                    .Concat
                    (
                        arg
                            .SystemRights
                            .Select(x => new AssignedRightDTO
                            {
                                RoleName = x.Role.Name,
                                BusinessObjectName = x.Object.ItSystem.Name,
                                RightId = x.Id,
                                Scope = BusinessRoleScope.ItSystemUsage

                            })
                    )
                    .Concat
                    (
                        arg
                            .OrganizationUnitRights
                            .Select(x => new AssignedRightDTO
                            {
                                RoleName = x.Role.Name,
                                BusinessObjectName = x.Object.Name,
                                RightId = x.Id,
                                Scope = BusinessRoleScope.OrganizationUnit

                            })
                    )
                    .ToList()
            };
        }

        private static UserRightsChangeParameters CreateChangeParameters(IEnumerable<OrganizationRole> adminRoles, IEnumerable<AssignedRightDTO> rightsToTransfer)
        {
            var rightIds = rightsToTransfer
                .GroupBy(rights => rights.Scope)
                .ToDictionary
                (
                    grp => grp.Key,
                    grp => grp.Select(x => x.RightId).ToList()
                );

            return new UserRightsChangeParameters(
                adminRoles,
                GetIdsByScope(rightIds, BusinessRoleScope.DataProcessingRegistration),
                GetIdsByScope(rightIds, BusinessRoleScope.ItSystemUsage),
                GetIdsByScope(rightIds, BusinessRoleScope.ItContract),
                GetIdsByScope(rightIds, BusinessRoleScope.ItProject),
                GetIdsByScope(rightIds, BusinessRoleScope.OrganizationUnit)
            );
        }

        private static IEnumerable<int> GetIdsByScope(IReadOnlyDictionary<BusinessRoleScope, List<int>> rightIds, BusinessRoleScope scope)
        {
            return rightIds.TryGetValue(scope, out var result) ? result : new List<int>();
        }
    }
}