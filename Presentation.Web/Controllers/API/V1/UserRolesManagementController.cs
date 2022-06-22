using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Model.Users;
using Core.ApplicationServices.Rights;
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
                .GetUserRoles(userId, organizationId)
                .Select(ToRoleAssignmentsDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Delete all role assignments in the organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="userId"></param>
        /// <returns>A new <see cref="OrganizationUserRoleAssignmentsDTO"/> which represents the changes assigment state</returns>
        [Route]
        [HttpDelete]
        public HttpResponseMessage Delete(int organizationId, int userId)
        {
            //TODO: Also remove the org unit roles
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete selected org roles
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="userId"></param>
        /// <param name="assignmentsToDelete"></param>
        /// <returns>A new <see cref="OrganizationUserRoleAssignmentsDTO"/> which represents the changes assigment state</returns>
        [Route("range")]
        [HttpDelete]
        public HttpResponseMessage DeleteRange(int organizationId, int userId, [FromBody] RemoveUserRightsRequest assignmentsToDelete)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Transfers the selected roles to another user
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="userId"></param>
        /// <param name="assignmentsToDelete"></param>
        /// <returns>A new <see cref="OrganizationUserRoleAssignmentsDTO"/> which represents the changes assigment state</returns>
        [Route("range/transfer")]
        [HttpPatch]
        public HttpResponseMessage PatchTransferToAnotherUser(int organizationId, int userId, TransferRightsRequestDTO assignmentsToDelete)
        {
            throw new NotImplementedException();
        }

        private static OrganizationUserRoleAssignmentsDTO ToRoleAssignmentsDTO(UserRoleAssignments arg)
        {
            return new OrganizationUserRoleAssignmentsDTO
            {
                AdministrativeAccessRoles = arg.AdministrativeAccessRoles,
                Rights = arg
                    .ContractRoles
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
                            .ProjectRoles
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
                            .DataProcessingRegistrationRoles
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
                            .SystemRoles
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
    }
}