using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    [ControllerEvaluationCompleted]
    public class UserController : GenericApiController<User, UserDTO>
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;

        public UserController(IGenericRepository<User> repository, IUserService userService, IOrganizationService organizationService)
            : base(repository)
        {
            _userService = userService;
            _organizationService = organizationService;
        }

        public override HttpResponseMessage Post(UserDTO dto)
        {
            //TODO: Hit when sending advis from organisation/brugere.
            try
            {
                // do some string magic to determine parameters, and actions
                List<string> parameters = null;
                var sendMailOnCreation = false;
                var sendReminder = false;
                var sendAdvis = false;
                int? orgId = null;

                if (!string.IsNullOrWhiteSpace(Request.RequestUri.Query))
                    parameters = new List<string>(Request.RequestUri.Query.Replace("?", string.Empty).Split('&'));
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        if (parameter.StartsWith("sendMailOnCreation"))
                        {
                            sendMailOnCreation = bool.Parse(parameter.Replace("sendMailOnCreation=", string.Empty));
                        }
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
                    return Error("Organization id is missing!");
                }

                // only global admin is allowed to set others to global admin
                if (dto.IsGlobalAdmin && !KitosUser.IsGlobalAdmin)
                {
                    return Forbidden();
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

                // otherwise we are creating a new user
                var item = Map(dto);

                item.ObjectOwner = KitosUser;
                item.LastChangedByUser = KitosUser;

                item = _userService.AddUser(item, sendMailOnCreation, orgId.Value);

                return Created(Map(item), new Uri(Request.RequestUri + "/" + item.Id));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            // get name of mapped property
            var map = Mapper.FindTypeMapFor<UserDTO, User>().GetPropertyMaps();
            var nonNullMaps = map.Where(x => x.SourceMember != null).ToList();

            foreach (var valuePair in obj)
            {
                var mapMember = nonNullMaps.SingleOrDefault(x => x.SourceMember.Name.Equals(valuePair.Key, StringComparison.InvariantCultureIgnoreCase));
                if (mapMember == null)
                    continue; // abort if no map found

                var destName = mapMember.DestinationProperty.Name;

                if (destName == "IsGlobalAdmin")
                    if (valuePair.Value.Value<bool>()) // check if value is being set to true
                        if (!KitosUser.IsGlobalAdmin)
                            return Forbidden(); // don't allow users to elevate to global admin unless done by a global admin
            }

            return base.Patch(id, organizationId, obj);
        }

        [DeprecatedApi]
        public HttpResponseMessage GetBySearch(string q)
        {
            try
            {
                var users = Repository.Get(u => u.Name.Contains(q) || u.Email.Contains(q));
                return Ok(Mapper.Map<IEnumerable<User>, IEnumerable<UserDTO>>(users));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [DeprecatedApi]
        public HttpResponseMessage GetByOrganization(int orgId, bool? usePaging, [FromUri] PagingModel<User> pagingModel, [FromUri] string q)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(q))
                    pagingModel.Where(u =>
                        u.Name.Contains(q)
                        || u.Email.Contains(q));

                // Get all users inside the organization
                pagingModel.Where(u => u.OrganizationRights.Count(r => r.Role == OrganizationRole.User && r.OrganizationId == orgId) > 0);

                var users = Page(Repository.AsQueryable(), pagingModel).ToList();

                return Ok(Map(users));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [DeprecatedApi]
        public HttpResponseMessage GetOverview(bool? overview, int orgId, [FromUri] PagingModel<User> pagingModel, [FromUri] string q)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(q))
                    pagingModel.Where(u =>
                        u.Name.Contains(q)
                        || u.Email.Contains(q));

                // Get all users inside the organization
                pagingModel.Where(u => u.OrganizationRights.Count(r => r.OrganizationId == orgId) > 0);

                var users = Page(Repository.AsQueryable(), pagingModel).ToList();
                var dtos = new List<UserOverviewDTO>();

                foreach (var user in users)
                {
                    var newDTO = Map<User, UserOverviewDTO>(user);

                    newDTO.CanBeEdited = HasWriteAccess(user, KitosUser, orgId);
                    dtos.Add(newDTO);
                }

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [DeprecatedApi]
        public HttpResponseMessage GetNameIsAvailable(string checkname, int orgId)
        {
            try
            {
                return IsAvailable(checkname, orgId) ? Ok() : Conflict("Name is already taken!");
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        private bool IsAvailable(string email, int orgId)
        {
            var users = Repository.Get(u => u.Email == email);

            return !users.Any();
        }

        [DeprecatedApi]
        public HttpResponseMessage GetUserExistsWithRole(string email, int orgId, bool? userExistsWithRole)
        {
            var users = Repository.Get(u => u.Email == email && u.OrganizationRights.Count(r => r.Role == OrganizationRole.User && r.OrganizationId == orgId) > 0);

            if (users.Any()) return Ok();

            return NotFound();
        }

        public HttpResponseMessage PostDefaultOrgUnit(bool? updateDefaultOrgUnit, UpdateDefaultOrgUnitDto dto)
        {
            try
            {
                _organizationService.SetDefaultOrgUnit(KitosUser, dto.OrgId, dto.OrgUnitId);

                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage PostDefaultOrganization(bool? updateDefaultOrganization, int organizationId)
        {
            var userId = int.Parse(User.Identity.Name);
            var user = Repository.Get(x => x.Id == userId).First();
            user.DefaultOrganizationId = organizationId;
            Repository.Save();
            return Ok();
        }

        protected override bool HasWriteAccess(User obj, User user, int organizationId)
        {
            //if user is readonly
            if (user.IsReadOnly && !user.IsGlobalAdmin)
                return false;

            return base.HasWriteAccess(obj, user, organizationId);
        }

        /// <summary>
        /// Deletes user from the system
        /// </summary>
        /// <param name="id">The id of the user to be deleted</param>
        /// <param name="organizationId">Not used in this case. Should remain empty</param>
        /// <returns></returns>
        public override HttpResponseMessage Delete(int id, int organizationId = 0)
        {
            if (!KitosUser.OrganizationRights.Any(x => x.Role == OrganizationRole.GlobalAdmin || x.Role == OrganizationRole.LocalAdmin || x.Role == OrganizationRole.OrganizationModuleAdmin))
            {
                return Forbidden();
            }

            return base.Delete(id, organizationId);
        }
    }
}
