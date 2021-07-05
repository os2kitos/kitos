using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.RightsHolders;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Organization;
using Core.DomainServices.Queries.User;
using Infrastructure.Services.Types;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response.Organization;
using Presentation.Web.Models.External.V2.Types;
using Serilog;
using Swashbuckle.Swagger.Annotations;
using OrganizationType = Presentation.Web.Models.External.V2.Types.OrganizationType;

namespace Presentation.Web.Controllers.External.V2.Organizations
{
    [RoutePrefix("api/v2")]
    public class OrganizationV2Controller : ExternalBaseController
    {
        private readonly IRightsHolderSystemService _rightsHolderSystemService;
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationalUserContext _userContext;
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public OrganizationV2Controller(
            IRightsHolderSystemService rightsHolderSystemService,
            IOrganizationService organizationService,
            IOrganizationalUserContext userContext,
            IUserService userService,
            ILogger logger)
        {
            _rightsHolderSystemService = rightsHolderSystemService;
            _organizationService = organizationService;
            _userContext = userContext;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Returns organizations organizations from KITOS
        /// </summary>
        /// <param name="onlyWhereUserHasMembership">If set to true, only organizations where the user has role(s) will be included.</param>
        /// <param name="nameContent">Optional query for name content</param>
        /// <param name="cvrContent">Optional query on CVR number</param>
        /// <returns>A list of organizations</returns>
        [HttpGet]
        [Route("organizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<OrganizationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [DenyRightsHoldersAccess]
        public IHttpActionResult GetOrganizations(bool onlyWhereUserHasMembership = false, string nameContent = null, string cvrContent = null, [FromUri] StandardPaginationQuery pagination = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refinements = new List<IDomainQuery<Organization>>();

            if (onlyWhereUserHasMembership)
                refinements.Add(new QueryByIds<Organization>(_userContext.OrganizationIds));

            if (!string.IsNullOrWhiteSpace(nameContent))
                refinements.Add(new QueryByPartOfName<Organization>(nameContent));

            if (!string.IsNullOrWhiteSpace(cvrContent))
                refinements.Add(new QueryByCvrContent(cvrContent));

            return _organizationService
                .SearchAccessibleOrganizations(refinements.ToArray())
                .OrderBy(x => x.Id)
                .Page(pagination)
                .ToList()
                .Select(ToDTO)
                .Transform(Ok);
        }

        /// <summary>
        /// Returns organization identified by uuid
        /// </summary>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <returns>A list of organizations</returns>
        [HttpGet]
        [Route("organizations/{organizationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<OrganizationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [DenyRightsHoldersAccess]
        public IHttpActionResult GetOrganization([NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _organizationService
                .GetOrganization(organizationUuid)
                .Select(ToDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns the users of an organization if the authenticated user is a member of the organization.
        /// </summary>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <param name="nameOrEmailQuery">Query by text in name or email</param>
        /// <param name="roleQuery">Query by role assignment</param>
        /// <returns>A list og users in a specific organizational context</returns>
        [HttpGet]
        [Route("organizations/{organizationUuid}/users")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<OrganizationUserResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Organization provided does not exist")]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [DenyRightsHoldersAccess]
        public IHttpActionResult GetOrganizationUsers(
            [NonEmptyGuid] Guid organizationUuid,
            string nameOrEmailQuery = null,
            OrganizationUserRole? roleQuery = null,
            [FromUri] StandardPaginationQuery paginationQuery = null)
        {
            var queries = new List<IDomainQuery<User>>();

            if (!string.IsNullOrWhiteSpace(nameOrEmailQuery))
                queries.Add(new QueryUserByNameOrEmail(nameOrEmailQuery));

            if (roleQuery.HasValue)
                queries.Add(new QueryByRoleAssignment(ApiRoleToDomainRoleMap[roleQuery.Value]));

            return _userService
                .GetUsersInOrganization(organizationUuid, queries.ToArray())
                .Select(x => x.OrderBy(user => user.Id))
                .Select(x => x.Page(paginationQuery))
                .Select(x => x.ToList().Select(user => (organizationUuid, user)).Select(ToUserResponseDTO))
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns the a specific user within an organization
        /// </summary>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <param name="userUuid">UUID of the user entity in KITOS</param>
        /// <returns>A user in the context of a specific organization</returns>
        [HttpGet]
        [Route("organizations/{organizationUuid}/users/{userUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationUserResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [DenyRightsHoldersAccess]
        public IHttpActionResult GetOrganizationUser([NonEmptyGuid] Guid organizationUuid, [NonEmptyGuid] Guid userUuid)
        {
            return _userService
                .GetUserInOrganization(organizationUuid, userUuid)
                .Select(user => (organizationUuid, user))
                .Select(ToUserResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns the organization units of an organization if the authenticated user is a member of the organization.
        /// </summary>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <param name="nameQuery">Query by text in name or email</param>
        /// <returns>A list og organization unit representations</returns>
        [HttpGet]
        [Route("organizations/{organizationUuid}/organization-units")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<OrganizationUnitResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Organization provided does not exist")]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [DenyRightsHoldersAccess]
        public IHttpActionResult GetOrganizationUnits(
            [NonEmptyGuid] Guid organizationUuid,
            string nameQuery = null,
            [FromUri] StandardPaginationQuery paginationQuery = null)
        {
            var queries = new List<IDomainQuery<User>>();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the a specific user within an organization
        /// </summary>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <param name="userUuid">UUID of the user entity in KITOS</param>
        /// <returns>A user in the context of a specific organization</returns>
        //[HttpGet]
        //[Route("organizations/{organizationUuid}/users/{userUuid}")]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationUserResponseDTO))]
        //[SwaggerResponse(HttpStatusCode.BadRequest)]
        //[SwaggerResponse(HttpStatusCode.NotFound)]
        //[SwaggerResponse(HttpStatusCode.Forbidden)]
        //[SwaggerResponse(HttpStatusCode.Unauthorized)]
        //[DenyRightsHoldersAccess]
        //public IHttpActionResult GetOrganizationUser([NonEmptyGuid] Guid organizationUuid, [NonEmptyGuid] Guid userUuid)
        //{
        //    return _userService
        //        .GetUserInOrganization(organizationUuid, userUuid)
        //        .Select(user => (organizationUuid, user))
        //        .Select(ToUserResponseDTO)
        //        .Match(Ok, FromOperationError);
        //}

        /// <summary>
        /// Returns organizations in which the current user has "RightsHolderAccess" permission
        /// </summary>
        /// <returns>A list of organizations formatted as uuid, cvr and name pairs</returns>
        [HttpGet]
        [Route("rightsholder/organizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ShallowOrganizationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetOrganizationsAsRightsHolder([FromUri] StandardPaginationQuery pagination = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _rightsHolderSystemService
                .ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess()
                .OrderBy(x => x.Id)
                .Page(pagination)
                .Transform(ToShallowDTOs)
                .Transform(Ok);
        }

        private OrganizationUserResponseDTO ToUserResponseDTO((Guid organizationUuid, User user) context)
        {
            return new()
            {
                Uuid = context.user.Uuid,
                Name = context.user.GetFullName(),
                Email = context.user.Email,
                PhoneNumber = context.user.PhoneNumber,
                FirstName = context.user.Name,
                LastName = context.user.LastName,
                ApiAccess = context.user.HasApiAccess.GetValueOrDefault(false),
                Roles = MapRoles(context)
            };
        }

        /// <summary>
        /// NOTE: Global admin is intentionally left out. It's a KITOS-only term and should not be exported.
        /// </summary>
        private static readonly IReadOnlyDictionary<OrganizationRole, OrganizationUserRole> DomainRoleToApiRoleMap = new Dictionary<OrganizationRole, OrganizationUserRole>
                {
                    {OrganizationRole.User, OrganizationUserRole.User},
                    {OrganizationRole.LocalAdmin, OrganizationUserRole.LocalAdmin},
                    {OrganizationRole.OrganizationModuleAdmin, OrganizationUserRole.OrganizationModuleAdmin},
                    {OrganizationRole.ProjectModuleAdmin, OrganizationUserRole.ProjectModuleAdmin},
                    {OrganizationRole.SystemModuleAdmin, OrganizationUserRole.SystemModuleAdmin},
                    {OrganizationRole.ContractModuleAdmin, OrganizationUserRole.ContractModuleAdmin},
                    {OrganizationRole.ReportModuleAdmin, OrganizationUserRole.ReportModuleAdmin},
                    {OrganizationRole.RightsHolderAccess, OrganizationUserRole.RightsHolderAccess}
                }.AsReadOnly();

        /// <summary>
        /// NOTE: Global admin is intentionally left out. It's a KITOS-only term and should not be exported.
        /// </summary>
        private static readonly IReadOnlyDictionary<OrganizationUserRole, OrganizationRole> ApiRoleToDomainRoleMap =
            DomainRoleToApiRoleMap.ToDictionary(x => x.Value, x => x.Key).AsReadOnly();

        private IEnumerable<OrganizationUserRole> MapRoles((Guid organizationUuid, User user) context)
        {
            return from organizationRole in context.user.GetRolesInOrganization(context.organizationUuid)
                   select MapRole(organizationRole)
                into role
                   where role.HasValue
                   select role.Value;
        }

        private Maybe<OrganizationUserRole> MapRole(OrganizationRole role)
        {
            if (!Ignore(role))
            {
                if (DomainRoleToApiRoleMap.TryGetValue(role, out var exportedRole))
                {
                    return exportedRole;
                }

                _logger.Error("ERROR: Failed mapping org role:{role} to an exported role", role.ToString("G"));
            }
            return Maybe<OrganizationUserRole>.None;
        }

        private static bool Ignore(OrganizationRole organizationRole)
        {
            return organizationRole == OrganizationRole.GlobalAdmin;
        }

        private static IEnumerable<ShallowOrganizationResponseDTO> ToShallowDTOs(IQueryable<Organization> organizations)
        {
            return organizations.ToList().Select(x => x.MapShallowOrganizationResponseDTO()).ToList();
        }

        private static OrganizationResponseDTO ToDTO(Organization organization)
        {
            return new(organization.Uuid, organization.Name, organization.GetActiveCvr(), MapOrganizationType(organization));
        }

        private static OrganizationType MapOrganizationType(Organization organization)
        {
            return organization.Type.Id switch
            {
                (int)OrganizationTypeKeys.Virksomhed => OrganizationType.Company,
                (int)OrganizationTypeKeys.Kommune => OrganizationType.Municipality,
                (int)OrganizationTypeKeys.AndenOffentligMyndighed => OrganizationType.OtherPublicAuthority,
                (int)OrganizationTypeKeys.Interessefællesskab => OrganizationType.CommunityOfInterest,
                _ => throw new ArgumentOutOfRangeException(nameof(organization.Type.Id), "Unknown organization type key")
            };
        }
    }
}