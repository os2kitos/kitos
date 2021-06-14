using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class UserController : GenericApiController<User, UserDTO>
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationalUserContext _userContext;

        public UserController(
            IGenericRepository<User> repository,
            IUserService userService,
            IOrganizationService organizationService,
            IOrganizationalUserContext userContext)
            : base(repository)
        {
            _userService = userService;
            _organizationService = organizationService;
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
                    _userService.IssueAdvisMail(existingUser, true, orgId.Value);
                    return Ok(Map(existingUser));
                }
                // if we are sending an advis:
                if (existingUser != null && sendAdvis)
                {
                    _userService.IssueAdvisMail(existingUser, false, orgId.Value);
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
                var user = Repository.GetByKey(UserId);
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

        /// <summary>
        /// Deletes user from the system
        /// </summary>
        /// <param name="id">The id of the user to be deleted</param>
        /// <param name="organizationId">Not used in this case. Should remain empty</param>
        /// <returns></returns>
        public override HttpResponseMessage Delete(int id, int organizationId = 0)
        {
            // NOTE: Only exists to apply optional param for org id
            return base.Delete(id, organizationId);
        }
    }
}
