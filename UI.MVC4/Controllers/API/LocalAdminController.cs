using System;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class LocalAdminController : ApiController
    {
        private readonly IUserService _userService;
        private readonly IGenericRepository<User> _repository;

        public LocalAdminController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "GlobalAdmin")]
        public HttpResponseMessage Post(UserApiModel item)
        {
            try
            {
                var user = AutoMapper.Mapper.Map<UserApiModel, User>(item);

                //TODO: Not hardcoding this
                user.Role_Id = 2;

                user = _userService.AddUser(user);

                var msg = Request.CreateResponse(HttpStatusCode.Created, AutoMapper.Mapper.Map<User,UserApiModel>(user));
                msg.Headers.Location = new Uri(Request.RequestUri + "/" + user.Id);
                return msg;
            }
            catch (SmtpException)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            }
        }
    }
}
