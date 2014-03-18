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
    public class PasswordResetRequestController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;

        public PasswordResetRequestController(IUserService userService, IUserRepository userRepository)
        {
            _userService = userService;
            _userRepository = userRepository;
        }

        // POST api/PasswordResetRequest
        public HttpResponseMessage Post([FromBody] UserDTO input)
        {
            try
            {
                var user = _userRepository.GetByEmail(input.Email);
                var request = _userService.IssuePasswordReset(user);

                var dto = AutoMapper.Mapper.Map<PasswordResetRequest, PasswordResetRequestDTO>(request);

                var msg = CreateResponse(HttpStatusCode.OK);
                msg.Headers.Location = new Uri(Request.RequestUri.ToString());
                return msg;
            }
            catch (Exception e)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        // GET api/PasswordResetRequest
        public HttpResponseMessage Get(string requestId)
        {
            try
            {
                var request = _userService.GetPasswordReset(requestId);
                if(request == null) throw new Exception("Request not found");
                var dto = AutoMapper.Mapper.Map<PasswordResetRequest, PasswordResetRequestDTO>(request);

                var msg = CreateResponse(HttpStatusCode.OK, dto);
                return msg;
            }
            catch (Exception e)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}
