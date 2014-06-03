using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Core.DomainModel;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class UserController : GenericApiController<User, UserDTO>
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;

        public UserController(IGenericRepository<User> repository, IUserService userService, IOrganizationService organizationService) : base(repository)
        {
            _userService = userService;
            _organizationService = organizationService;
        }

        protected override User PostQuery(User item)
        {
            return _userService.AddUser(item);
        }

        //TODO REWRITE THIS, perhaps so it's passes along at login?
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
                var users = Repository.Get(u => u.Name.StartsWith(q) || u.Email.StartsWith(q));
                return Ok(AutoMapper.Mapper.Map<IEnumerable<User>, IEnumerable<UserDTO>>(users));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }

}
