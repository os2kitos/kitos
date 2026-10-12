using Core.ApplicationServices.Users.Write;
using Presentation.Web.Infrastructure.Attributes;
using System.Net;
using System;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainServices.Queries.User;
using Core.DomainServices.Queries;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Types.Shared;
using System.Collections.Generic;
using System.Linq;
using Presentation.Web.Models.API.V2.Internal.Response.User;
using Core.ApplicationServices;
using Presentation.Web.Extensions;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Response.Organization;
using Core.DomainModel.Organization;
using Core.ApplicationServices.Rights;
using Microsoft.AspNetCore.Mvc;
using Core.ApplicationServices.Model.RightsHolder;
using Microsoft.EntityFrameworkCore;


namespace Presentation.Web.Controllers.API.V2.Internal.Users
{
    /// <summary>
    /// Internal API for managing users in all of KITOS
    /// </summary>
    [Route("api/v2/internal/users")]
    public class GlobalUserInternalV2Controller : InternalApiV2Controller
    {
        private readonly IUserWriteService _userWriteService;
        private readonly IUserService _userService;
        private readonly IOrganizationResponseMapper _organizationResponseMapper;
        private readonly IUserRightsService _userRightsService;

        public GlobalUserInternalV2Controller(IUserWriteService userWriteService, 
            IUserService userService, 
            IOrganizationResponseMapper organizationResponseMapper,
            IUserRightsService userRightsService)
        {
            _userWriteService = userWriteService;
            _userService = userService;
            _organizationResponseMapper = organizationResponseMapper;
            _userRightsService = userRightsService;
        }

        [Route("{userUuid}")]
        [HttpDelete]
        [ApiResponse(HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult DeleteUser([NonEmptyGuid] Guid userUuid)
        {
            return _userWriteService.DeleteUser(userUuid, Maybe<Guid>.None)
                .Match(FromOperationError, Ok);
        }

        [Route("search")]
        [HttpGet]
        [ApiResponse(typeof(IEnumerable<UserReferenceResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetUsers(
            string? nameOrEmailQuery = null,
            string? emailQuery = null,
            CommonOrderByProperty? orderByProperty = null,
            [FromQuery] BoundedPaginationQuery? paginationQuery = null)
        {
            var queries = new List<IDomainQuery<User>>();

            if (!string.IsNullOrWhiteSpace(nameOrEmailQuery))
                queries.Add(new QueryUserByNameOrEmail(nameOrEmailQuery));

            if (!string.IsNullOrWhiteSpace(emailQuery))
                queries.Add(new QueryUserByEmail(emailQuery));

            var result = _userService
                .GetUsers(queries.ToArray());
            result = result.OrderUserApiResults(orderByProperty);
            result = result.Page(paginationQuery);
            return Ok(result.ToList().Select(InternalDtoModelV2MappingExtensions.MapUserReferenceResponseDTO));
        }

        [Route("global-admins")]
        [HttpGet]
        [ApiResponse(typeof(IEnumerable<UserReferenceResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetGlobalAdmins()
        {
            var query = new List<IDomainQuery<User>> { new QueryByGlobalAdmin() };
            var globalAdmins = _userService.GetUsers(query.ToArray())
                .Select(InternalDtoModelV2MappingExtensions.MapUserReferenceResponseDTO)
                .ToList();
            return Ok(globalAdmins);
        }

        [Route("global-admins/{userUuid}")]
        [HttpPost]
        [ApiResponse(typeof(UserReferenceResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult AddGlobalAdmin([FromRoute][NonEmptyGuid] Guid userUuid)
        {
            return _userWriteService.AddGlobalAdmin(userUuid)
                        .Select(InternalDtoModelV2MappingExtensions.MapUserReferenceResponseDTO)
                        .Match(Ok, FromOperationError);
        }

        [Route("global-admins/{userUuid}")]
        [HttpDelete]
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult RemoveGlobalAdmin([FromRoute][NonEmptyGuid] Guid userUuid)
        {
            return _userWriteService.RemoveGlobalAdmin(userUuid)
                        .Match(FromOperationError, NoContent);
        }
        [Route("{userUuid}/organizations")]
        [HttpGet]
        [ApiResponse(typeof(IEnumerable<OrganizationResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetOrganizationsByUserUuid(Guid userUuid)
        {
            
            return _userService
                .GetUserOrganizations(userUuid)
                .Select(x => x.Select(_organizationResponseMapper.ToOrganizationDTO).ToList())
                .Match(Ok, FromOperationError);

        }

        [Route("local-admins")]
        [HttpGet]
        [ApiResponse(typeof(IEnumerable<UserReferenceWithOrganizationResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetAllLocalAdmins()
        {
            return _userService.GetUsersWithRoleAssignedInAnyOrganization(Core.DomainModel.Organization.OrganizationRole.LocalAdmin)
                    .Select(users => users
                        .Include(user => user.OrganizationRights)
                        .ThenInclude(right => right.Organization)
                        .ToList()
                        .SelectMany(InternalDtoModelV2MappingExtensions.MapUserToMultipleLocalAdminResponse)
                        .ToList())
                    .Match(Ok, FromOperationError);
        }

        [Route("{organizationUuid}/local-admins/{userUuid}")]
        [HttpPost]
        [ApiResponse(typeof(UserReferenceWithOrganizationResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult AddLocalAdmin([NonEmptyGuid][FromRoute] Guid organizationUuid, [NonEmptyGuid][FromRoute] Guid userUuid)
        {
            return _userWriteService.AddLocalAdmin(organizationUuid, userUuid)
                    .Select(user => user.MapUserToSingleLocalAdminResponse(organizationUuid))
                    .Match(Ok, FromOperationError);
        }

        [Route("{organizationUuid}/local-admins/{userUuid}")]
        [HttpDelete]
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult RemoveLocalAdmin([NonEmptyGuid][FromRoute] Guid organizationUuid, [NonEmptyGuid][FromRoute] Guid userUuid)
        {
            return _userWriteService.RemoveLocalAdmin(organizationUuid, userUuid)
                    .Match(FromOperationError, NoContent);
        }
        
        [HttpGet]
        [Route("with-rightsholder-access")]
        [ApiResponse(typeof(IEnumerable<UserWithOrganizationResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetUsersWithRightsholderAccess()
        {
            return _userRightsService
                .GetUsersWithRoleAssignment(OrganizationRole.RightsHolderAccess)
                .Select(relations => relations.OrderBy(relation => relation.User.Id))
                .Select(relations => relations.ToList())
                .Select(ToUserWithOrgDTOs)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("with-cross-organization-permissions")]
        [ApiResponse(typeof(IEnumerable<UserWithCrossOrganizationalRightsResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetUsersWithCrossAccess()
        {
            return _userService
                .GetUsersWithCrossOrganizationPermissions()
                .Select(users => users.OrderBy(user => user.Id))
                .Select(users => users.ToList())
                .Select(ToUserWithCrossRightsDTOs)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("system-integrators")]
        [ApiResponse(typeof(IEnumerable<UserReferenceResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetSystemIntegrators()
        {
            var results = _userService.GetUsers(new QueryBySystemIntegrator());
            var mappedUsers = results.AsEnumerable()
                .Select(InternalDtoModelV2MappingExtensions.MapUserReferenceResponseDTO).ToList();
            return Ok(mappedUsers);
        }

        [HttpPatch]
        [Route("system-integrators/{userUuid}")]
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult UpdateSystemIntegrator([NonEmptyGuid] [FromRoute] Guid userUuid, [FromQuery] bool requestedValue)
        {
            return _userWriteService.UpdateSystemIntegrator(userUuid, requestedValue)
                    .Match(NoContent, FromOperationError);
        }

        private static IEnumerable<UserWithOrganizationResponseDTO> ToUserWithOrgDTOs(List<UserRoleAssociationDTO> dtos)
        {
            return dtos.Select(ToUserWithOrgDTO).ToList();
        }

        private static UserWithOrganizationResponseDTO ToUserWithOrgDTO(UserRoleAssociationDTO dto)
        {
            return new UserWithOrganizationResponseDTO(dto.User.Uuid, dto.User.GetFullName(), dto.User.Email, dto.User.HasApiAccess.GetValueOrDefault(false), dto.Organization.Name);
        }

        private static IEnumerable<UserWithCrossOrganizationalRightsResponseDTO> ToUserWithCrossRightsDTOs(IEnumerable<User> users)
        {
            return users.Select(ToUserWithCrossRightsDTO).ToList();
        }

        private static UserWithCrossOrganizationalRightsResponseDTO ToUserWithCrossRightsDTO(User user)
        {
            return new UserWithCrossOrganizationalRightsResponseDTO(user.Uuid, user.GetFullName(), user.Email, user.HasApiAccess.GetValueOrDefault(false), user.HasStakeHolderAccess, GetOrganizationNames(user));
        }

        private static IEnumerable<string> GetOrganizationNames(User user)
        {
            return user.GetOrganizations()
                .GroupBy(x => (x.Id, x.Name))
                .Distinct()
                .Select(x => x.Key.Name)
                .OrderBy(x => x)
                .ToList();
        }
    }
}

