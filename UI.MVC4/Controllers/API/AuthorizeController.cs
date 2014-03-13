using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using Core.DomainServices;
using Core.DomainModel;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class AuthorizeController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;

        public AuthorizeController(IUserRepository userRepository, IUserService userService)
        {
            _userRepository = userRepository;
            _userService = userService;
        }

        [Authorize]
        public HttpResponseMessage GetLogin()
        {
            var user = _userRepository.GetByEmail(User.Identity.Name);
            var userApiModel = AutoMapper.Mapper.Map<User, UserDTO>(user);

            return CreateResponse(HttpStatusCode.Created, userApiModel);
        }

        // POST api/Authorize
        public HttpResponseMessage PostLogin(LoginDTO loginDto)
        {
            try
            {
                if (!Membership.ValidateUser(loginDto.Email, loginDto.Password))
                    throw new ArgumentException();

                FormsAuthentication.SetAuthCookie(loginDto.Email, loginDto.RememberMe);

                var user = _userRepository.GetByEmail(loginDto.Email);
                var userApiModel = AutoMapper.Mapper.Map<User, UserDTO>(user);

                return CreateResponse(HttpStatusCode.Created, userApiModel);
            }
            catch (ArgumentException)
            {
                return CreateResponse(HttpStatusCode.Unauthorized, "Bad credentials");
            }
            catch (Exception e)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        public HttpResponseMessage PostLogout(bool? logout)
        {
            FormsAuthentication.SignOut();
            return CreateResponse(HttpStatusCode.OK);
        }


        public HttpResponseMessage PostResetpassword(bool? resetPassword, ResetPasswordDTO dto)
        {
            try
            {
                var resetRequest = _userService.GetPasswordReset(dto.RequestId);

                _userService.ResetPassword(resetRequest, dto.NewPassword);
                
                return CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}
