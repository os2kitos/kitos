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

            return Ok(userApiModel);
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

                return Created(userApiModel);
            }
            catch (ArgumentException)
            {
                return Unauthorized("Bad credentials");
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage PostLogout(bool? logout)
        {
            FormsAuthentication.SignOut();
            return Ok();
        }


        public HttpResponseMessage PostResetpassword(bool? resetPassword, ResetPasswordDTO dto)
        {
            try
            {
                var resetRequest = _userService.GetPasswordReset(dto.RequestId);

                _userService.ResetPassword(resetRequest, dto.NewPassword);

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
