using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class UserController : BaseApiController
    {
        private readonly IGenericRepository<User> _repository;
        private readonly IUserService _userService;
        private readonly IOrgService _orgService;

        public UserController(IGenericRepository<User> repository, IUserService userService, IOrgService orgService)
        {
            _repository = repository;
            _userService = userService;
            _orgService = orgService;
        }

        public HttpResponseMessage Post(UserDTO item)
        {
            try
            {
                if (!IsAuthenticated) return Unauthorized();

                var user = AutoMapper.Mapper.Map<UserDTO, User>(item);

                user = _userService.AddUser(user);

                return Created(AutoMapper.Mapper.Map<User, UserDTO>(user), new Uri(Request.RequestUri + "/" + user.Id));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Get()
        {
            try
            {
                var users = _repository.Get();
                return Ok(AutoMapper.Mapper.Map<IEnumerable<User>, List<UserDTO>>(users));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Get(int id)
        {
            try
            {
                var user = _repository.GetByKey(id);
                return Ok(AutoMapper.Mapper.Map<User, UserDTO>(user));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Get(int id, bool? organizations)
        {
            try
            {
                var orgs = _orgService.GetByUserId(id);
                return Ok(AutoMapper.Mapper.Map<IEnumerable<Organization>, IEnumerable<OrganizationDTO>>(orgs));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Get(string q)
        {
            if (string.IsNullOrEmpty(q)) return Get();

            try
            {
                var users = _repository.Get(u => u.Name.StartsWith(q) || u.Email.StartsWith(q));
                return Ok(AutoMapper.Mapper.Map<IEnumerable<User>, IEnumerable<UserDTO>>(users));
            }
            catch (Exception e)
            {
                return Error(e);
            }
            
        }
    }

}
