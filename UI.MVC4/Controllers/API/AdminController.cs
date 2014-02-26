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
    public class AdminController : ApiController
    { 
        private readonly IGenericRepository<User> _repository;

        public AdminController(IGenericRepository<User> repository)
        {
            _repository = repository;
        }

        [Authorize(Roles = "GlobalAdmin")]
        public HttpResponseMessage Post(AdminApiModel item)
        {
            try
            {
                var user = AutoMapper.Mapper.Map<AdminApiModel, User>(item);

                //todo: fix obvious security leak here
                //(probably just create random default password)
                user.Password = "default password";

                _repository.Insert(user);
                _repository.Save();

                var msg = Request.CreateResponse(HttpStatusCode.Created, AutoMapper.Mapper.Map<User,AdminApiModel>(user));
                msg.Headers.Location = new Uri(Request.RequestUri + "/" + item.Id.ToString());
                return msg;
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            }
        }

        /* TODO: SHOULD THESE BE AVAILABLE?? 
        [Authorize]
        public IEnumerable<UserApiModel> Get()
        {
            return AutoMapper.Mapper.Map<IEnumerable<User>, List<UserApiModel>>(_repository.Get());
        }
        
        [Authorize]
        public UserApiModel Get(int id)
        {
            return AutoMapper.Mapper.Map<User,UserApiModel>(_repository.GetById(id));
        }
        */
    }
}
