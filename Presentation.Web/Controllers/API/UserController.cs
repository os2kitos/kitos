using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Ninject.Web.Common;
using Presentation.Web.Models;
using WebGrease.Css.Extensions;

namespace Presentation.Web.Controllers.API
{
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
            try
            {
                //do some string magic to determine parameters, and actions
                List<string> parameters = null;
                bool sendMailOnCreation = false;
                bool sendReminder = false;
                bool sendAdvis = false;

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
                    }
                }

                //check if user already exists and we are not sending a reminder or advis. If so, just return him
                var existingUser = Repository.Get(u => u.Email == dto.Email).FirstOrDefault();
                if (existingUser != null && !sendReminder && !sendAdvis)
                    return Ok(Map(existingUser));
                //if we are sending a reminder:
                if (existingUser != null && sendReminder)
                {
                    _userService.IssueAdvisMail(existingUser, true);
                    return Ok(Map(existingUser));
                }
                //if we are sending an advis:
                if (existingUser != null && sendAdvis)
                {
                    _userService.IssueAdvisMail(existingUser, false);
                    return Ok(Map(existingUser));
                }

                //otherwise we are creating a new user
                var item = Map(dto);

                item.ObjectOwner = KitosUser;
                item.LastChangedByUser = KitosUser;

                item = _userService.AddUser(item, sendMailOnCreation);

                return Created(Map(item), new Uri(Request.RequestUri + "/" + item.Id));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        //TODO REWRITE THIS, perhaps so it's passed along at login?
        public HttpResponseMessage Get(bool? organizations)
        {
            try
            {
                var user = KitosUser;

                var orgs = _organizationService.GetByUser(user);
                var dtos = AutoMapper.Mapper.Map<ICollection<Organization>, ICollection<OrganizationDTO>>(orgs);

                //if the user has selected a default org unit, use the responding organization as default organization
                var defaultOrgId = (user.DefaultOrganizationUnit == null)
                                       ? 0
                                       : user.DefaultOrganizationUnit.OrganizationId;

                var result = new UserOrganizationsDTO()
                    {
                        Organizations = dtos,
                        DefaultOrganizationId = defaultOrgId
                    };

                return Ok(result);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetBySearch(string q)
        {
            try
            {
                var users = Repository.Get(u => u.Name.Contains(q) || u.Email.Contains(q));
                return Ok(AutoMapper.Mapper.Map<IEnumerable<User>, IEnumerable<UserDTO>>(users));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetByOrganization(int orgId, bool? usePaging, [FromUri] PagingModel<User> pagingModel)
        {
            try
            {
                //Get all users inside the organization
                pagingModel.Where(u => u.CreatedInId == orgId);

                var users = Page(Repository.AsQueryable(), pagingModel);

                return Ok(Map(users));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }

}
