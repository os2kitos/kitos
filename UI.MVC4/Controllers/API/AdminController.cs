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
    public class AdminController : ApiController
    {
        private readonly IUserService _userService;
        private readonly IGenericRepository<User> _repository;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "GlobalAdmin")]
        public HttpResponseMessage Post(AdminApiModel item)
        {
            try
            {
                var user = AutoMapper.Mapper.Map<AdminApiModel, User>(item);

                user = _userService.AddUser(user);

                var msg = Request.CreateResponse(HttpStatusCode.Created, AutoMapper.Mapper.Map<User,AdminApiModel>(user));
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
