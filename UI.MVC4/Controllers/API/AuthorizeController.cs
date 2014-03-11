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

        public AuthorizeController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // POST api/Authorize
        public HttpResponseMessage Post(LoginApiModel loginApiModel)
        {
            try
            {
                if (!Membership.ValidateUser(loginApiModel.Email, loginApiModel.Password))
                    throw new ArgumentException();

                FormsAuthentication.SetAuthCookie(loginApiModel.Email, loginApiModel.RememberMe);

                var user = _userRepository.GetByEmail(loginApiModel.Email);
                var userApiModel = AutoMapper.Mapper.Map<User, UserApiModel>(user);

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

        public HttpResponseMessage Post(bool? logout)
        {
            FormsAuthentication.SignOut();
            return CreateResponse(HttpStatusCode.OK);
        }
    }
}
