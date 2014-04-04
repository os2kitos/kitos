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
    public class LocalAdminController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IAdminService _adminService;
        private readonly IGenericRepository<Organization> _organizationRepository;

        public LocalAdminController(IUserService userService, IAdminService adminService, IGenericRepository<Organization> organizationRepository)
        {
            _userService = userService;
            _adminService = adminService;
            _organizationRepository = organizationRepository;
        }

        public HttpResponseMessage Post(CreateLocalAdminDTO item)
        {
            try
            {
                var user = UserRepository.GetByKey(item.User_Id);

                var organization = _organizationRepository.GetByKey(item.Organization_Id);

                _adminService.MakeLocalAdmin(user, organization);

                return Created(AutoMapper.Mapper.Map<User, UserDTO>(user), new Uri(Request.RequestUri + "/" + user.Id));

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
