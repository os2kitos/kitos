using Core.ApplicationServices.Users.Write;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System;
using System.Web.Http;
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
using Core.ApplicationServices.Model.RightsHolder;


namespace Presentation.Web.Controllers.API.V2.Internal.Users
{
    /// <summary>
    /// Internal API for managing users in all of KITOS
    /// </summary>
    [RoutePrefix("api/v2/internal/users")]
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
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult DeleteUser([NonEmptyGuid] Guid userUuid)
        {
            return _userWriteService.DeleteUser(userUuid, Maybe<Guid>.None)
                .Match(FromOperationError, Ok);
        }

        [Route("search")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<UserReferenceResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetUsers(
            string nameOrEmailQuery = null,
            string emailQuery = null,
            CommonOrderByProperty? orderByProperty = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<UserReferenceResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetGlobalAdmins()
        {
            var query = new List<IDomainQuery<User>> { new QueryByGlobalAdmin() };
            var globalAdmins = _userService.GetUsers(query.ToArray())
                .Select(InternalDtoModelV2MappingExtensions.MapUserReferenceResponseDTO)
                .ToList();
            return Ok(globalAdmins);
        }

        [Route("global-admins/{userUuid}")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UserReferenceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult AddGlobalAdmin([FromUri][NonEmptyGuid] Guid userUuid)
        {
            return _userWriteService.AddGlobalAdmin(userUuid)
                        .Select(InternalDtoModelV2MappingExtensions.MapUserReferenceResponseDTO)
                        .Match(Ok, FromOperationError);
        }

        [Route("global-admins/{userUuid}")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult RemoveGlobalAdmin([FromUri][NonEmptyGuid] Guid userUuid)
        {
            return _userWriteService.RemoveGlobalAdmin(userUuid)
                        .Match(FromOperationError, NoContent);
        }
        [Route("{userUuid}/organizations")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<OrganizationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetOrganizationsByUserUuid(Guid userUuid)
        {
            
            return _userService
                .GetUserOrganizations(userUuid)
                .Select(x => x.Select(_organizationResponseMapper.ToOrganizationDTO).ToList())
                .Match(Ok, FromOperationError);

        }

        [Route("local-admins")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<UserReferenceWithOrganizationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetAllLocalAdmins()
        {
            return _userService.GetUsersWithRoleAssignedInAnyOrganization(Core.DomainModel.Organization.OrganizationRole.LocalAdmin)
                    .Select(users => users.SelectMany(InternalDtoModelV2MappingExtensions.MapUserToMultipleLocalAdminResponse).ToList())
                    .Match(Ok, FromOperationError);
        }

        [Route("{organizationUuid}/local-admins/{userUuid}")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UserReferenceWithOrganizationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult AddLocalAdmin([NonEmptyGuid][FromUri] Guid organizationUuid, [NonEmptyGuid][FromUri] Guid userUuid)
        {
            return _userWriteService.AddLocalAdmin(organizationUuid, userUuid)
                    .Select(user => user.MapUserToSingleLocalAdminResponse(organizationUuid))
                    .Match(Ok, FromOperationError);
        }

        [Route("{organizationUuid}/local-admins/{userUuid}")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult RemoveLocalAdmin([NonEmptyGuid][FromUri] Guid organizationUuid, [NonEmptyGuid][FromUri] Guid userUuid)
        {
            return _userWriteService.RemoveLocalAdmin(organizationUuid, userUuid)
                    .Match(FromOperationError, NoContent);
        }
        
        [HttpGet]
        [Route("with-rightsholder-access")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<UserWithOrganizationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetUsersWithRightsholderAccess()
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<UserWithCrossOrganizationalRightsResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetUsersWithCrossAccess()
        {
            return _userService
                .GetUsersWithCrossOrganizationPermissions()
                .Select(users => users.OrderBy(user => user.Id))
                .Select(users => users.ToList())
                .Select(ToUserWithCrossRightsDTOs)
                .Match(Ok, FromOperationError);
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