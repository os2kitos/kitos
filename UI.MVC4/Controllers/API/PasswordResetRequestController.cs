using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using System.Web.Security;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class PasswordResetRequestController : ApiController
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;

        public PasswordResetRequestController(IUserService userService, IUserRepository userRepository)
        {
            _userService = userService;
            _userRepository = userRepository;
        }

        // POST api/PasswordResetRequest
        public HttpResponseMessage Post(UserApiModel userApiModel)
        {
            try
            {
                var user = _userRepository.GetByEmail(userApiModel.Email);

                var request = _userService.IssuePasswordReset(user);

                var msg = new HttpResponseMessage(HttpStatusCode.Created);
                msg.Headers.Location = new Uri(Request.RequestUri + request.Id);
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
