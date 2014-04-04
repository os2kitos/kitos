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
    public class GlobalAdminController : BaseApiController
    {
        private readonly IUserService _userService;

        public GlobalAdminController(IUserService userService)
        {
            _userService = userService;
        }

        public HttpResponseMessage Post(CreateGlobalAdminDTO item)
        {
            try
            {
                var user = UserRepository.GetByKey(item.UserId);

                user.IsGlobalAdmin = true;

                UserRepository.Save();

                return Created(AutoMapper.Mapper.Map<User, UserDTO>(user), new Uri(Request.RequestUri + "/" + user.Id));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
