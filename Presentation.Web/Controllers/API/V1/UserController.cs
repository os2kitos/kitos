using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Model.RightsHolder;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Rights;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.User;
using Newtonsoft.Json.Linq;
using Presentation.Web.Controllers.API.V1.Mapping;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.Generic.Queries;
using Presentation.Web.Models.API.V1.Users;
namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    public class UserController : GenericApiController<User, UserDTO>
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IUserRightsService _userRightsService;
        private readonly IOrganizationalUserContext _userContext;

        public UserController(
            IGenericRepository<User> repository,
            IUserService userService,
            IOrganizationService organizationService,
            IUserRightsService userRightsService,
            IOrganizationalUserContext userContext)
            : base(repository)
        {
            _userService = userService;
            _organizationService = organizationService;
            _userRightsService = userRightsService;
            _userContext = userContext;
        }

        [NonAction]
        public override HttpResponseMessage Post(int organizationId, UserDTO dto) => throw new NotSupportedException();

        /// <summary>
        /// Sends advice to user
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public HttpResponseMessage Post(UserDTO dto)
        {
            try
            {
                // do some string magic to determine parameters, and actions
                List<string> parameters = null;
                var sendReminder = false;
                var sendAdvis = false;
                int? orgId = null;

                if (!string.IsNullOrWhiteSpace(Request.RequestUri.Query))
                    parameters = new List<string>(Request.RequestUri.Query.Replace("?", string.Empty).Split('&'));
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        if (parameter.StartsWith("sendReminder"))
                        {
                            sendReminder = bool.Parse(parameter.Replace("sendReminder=", string.Empty));
                        }
                        if (parameter.StartsWith("sendAdvis"))
                        {
                            sendAdvis = bool.Parse(parameter.Replace("sendAdvis=", string.Empty));
                        }
                        if (parameter.StartsWith("organizationId="))
                        {
                            orgId = int.Parse(parameter.Replace("organizationId=", string.Empty));
                        }
                    }
                }

                // check if orgId is set, if not return error as we cannot continue without it
                if (!orgId.HasValue)
                {
                    return BadRequest("Organization id is missing!");
                }

                // check if user already exists and we are not sending a reminder or advis. If so, just return him
                var existingUser = Repository.Get(u => u.Email == dto.Email).FirstOrDefault();
                if (existingUser != null && !sendReminder && !sendAdvis)
                    return Ok(Map(existingUser));
                // if we are sending a reminder:
                if (existingUser != null && sendReminder)
                {
                    _userService.IssueAdvisMail(existingUser, true, orgId.Value, false);
                    return Ok(Map(existingUser));
                }
                // if we are sending an advis:
                if (existingUser != null && sendAdvis)
                {
                    _userService.IssueAdvisMail(existingUser, false, orgId.Value, false);
                    return Ok(Map(existingUser));
                }

                return BadRequest("Unkown command");
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            var existingUser = Repository.AsQueryable().ById(id);
            if (existingUser == null)
                return NotFound();

            // get name of mapped property
            var map = Mapper.ConfigurationProvider.FindTypeMapFor<UserDTO, User>().PropertyMaps;

            var nonNullMaps = map.Where(x => x.SourceMember != null).ToList();

            foreach (var valuePair in obj)
            {
                var mapMember = nonNullMaps.SingleOrDefault(x => x.SourceMember.Name.Equals(valuePair.Key, StringComparison.InvariantCultureIgnoreCase));
                if (mapMember == null)
                    continue; // abort if no map found

                var destName = mapMember.DestinationName;

                if (destName == nameof(Core.DomainModel.User.Uuid))
                    if (valuePair.Value?.Value<Guid>() != existingUser.Uuid)
                        return BadRequest($"{nameof(Core.DomainModel.User.Uuid)}cannot be updated");

                if (destName == nameof(Core.DomainModel.User.IsGlobalAdmin))
                    if ((valuePair.Value?.Value<bool>()).GetValueOrDefault()) // check if value is being set to true
                        if (!AuthorizationContext.HasPermission(new AdministerGlobalPermission(GlobalPermission.GlobalAdmin)))
                            return Forbidden();

                if (destName == nameof(Core.DomainModel.User.HasStakeHolderAccess))
                {
                    if (existingUser.HasStakeHolderAccess != (valuePair.Value?.Value<bool>()).GetValueOrDefault())
                    {
                        if (!AuthorizationContext.HasPermission(new AdministerGlobalPermission(GlobalPermission.StakeHolderAccess)))
                            return Forbidden();
                    }
                }
            }

            return base.Patch(id, organizationId, obj);
        }

        public HttpResponseMessage PostDefaultOrgUnit(bool? updateDefaultOrgUnit, UpdateDefaultOrgUnitDto dto)
        {
            try
            {
                var user = Repository.GetByKey(_userContext.UserId);
                if (user == null)
                    return NotFound();

                if (!AllowModify(user))
                    return Forbidden();

                _organizationService.SetDefaultOrgUnit(user, dto.OrgId, dto.OrgUnitId);

                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [NonAction]
        public override HttpResponseMessage Delete(int id, int organizationId = 0)
        {
            throw new NotSupportedException();
        }
        

        [HttpGet]
        [Route("api/user/with-rightsholder-access")]
        public HttpResponseMessage GetUsersWithRightsholderAccess()
        {
            return _userRightsService
                .GetUsersWithRoleAssignment(OrganizationRole.RightsHolderAccess)
                .Select(relations => relations.OrderBy(relation => relation.User.Id))
                .Select(relations => relations.ToList())
                .Select(ToUserWithOrgDTOs)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("api/user/with-cross-organization-permissions")]
        public HttpResponseMessage GetUsersWithCrossAccess()
        {
            return _userService
                .GetUsersWithCrossOrganizationPermissions()
                .Select(users => users.OrderBy(user => user.Id))
                .Select(users => users.ToList())
                .Select(ToUserWithCrossRightsDTOs)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns the users with matching name or email
        /// </summary>
        /// <param name="nameOrEmailQuery">Name or email of the user</param>
        /// <returns>A list of users</returns>
        [HttpGet]
        [Route("api/users/search")]
        public HttpResponseMessage SearchUsers(string query, [FromUri] V1BoundedPaginationQuery paginationQuery = null)
        {
            if (string.IsNullOrEmpty(query))
                return BadRequest("Query needs a value");

            var decodedQuery = HttpUtility.UrlDecode(query);

            var queries = new List<IDomainQuery<User>> { new QueryUserByNameOrEmail(decodedQuery) };

            return _userService
                .SearchAllKitosUsers(queries.ToArray())
                .Select(x => x.OrderBy(user => user.Id))
                .Select(x => x.Page(paginationQuery))
                .Select(x => x.ToList())
                .Select(ToUserWithEmailDtos)
                .Match(Ok, FromOperationError);
        }

        private static IEnumerable<UserWithOrganizationDTO> ToUserWithOrgDTOs(List<UserRoleAssociationDTO> dtos)
        {
            return dtos.Select(ToUserWithOrgDTO).ToList();
        }

        private static UserWithOrganizationDTO ToUserWithOrgDTO(UserRoleAssociationDTO dto)
        {
            return new(dto.User.Id, dto.User.GetFullName(), dto.User.Email, dto.Organization.Name, dto.User.HasApiAccess.GetValueOrDefault(false));
        }

        private static IEnumerable<UserWithEmailDTO> ToUserWithEmailDtos(List<User> users)
        {
            return users.Select(user => user.MapToUserWithEmailDTO()).ToList();
        }

        private static IEnumerable<UserWithCrossOrganizationalRightsDTO> ToUserWithCrossRightsDTOs(IEnumerable<User> users)
        {
            return users.Select(ToUserWithCrossRightsDTO).ToList();
        }

        private static UserWithCrossOrganizationalRightsDTO ToUserWithCrossRightsDTO(User user)
        {
            return new(user.Id, user.GetFullName(), user.Email, user.HasApiAccess.GetValueOrDefault(false), user.HasStakeHolderAccess, GetOrganizationNames(user));
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
