using System;
using System.Net.Http;
using System.Web.Security;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    //TODO refactor this mess
    public class AuthorizeController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;

        public AuthorizeController(IUserRepository userRepository, IUserService userService)
        {
            _userRepository = userRepository;
            _userService = userService;
        }

        public HttpResponseMessage GetLogin()
        { 
            try
            {
                var userApiModel = AutoMapper.Mapper.Map<User, UserDTO>(KitosUser);

                return Ok(userApiModel);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        // POST api/Authorize
        public HttpResponseMessage PostLogin(LoginDTO loginDto)
        {
            try
            {
                if (!Membership.ValidateUser(loginDto.Email, loginDto.Password))
                    throw new ArgumentException();

                var user = _userRepository.GetByEmail(loginDto.Email);

                FormsAuthentication.SetAuthCookie(user.Id.ToString(), loginDto.RememberMe);

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
            try
            {
                FormsAuthentication.SignOut();
                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
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
