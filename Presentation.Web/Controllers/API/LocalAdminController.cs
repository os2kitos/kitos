using System;
using System.Net.Http;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
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
                var user = UserRepository.GetByKey(item.UserId);

                var organization = _organizationRepository.GetByKey(item.OrganizationId);

                _adminService.MakeLocalAdmin(user, organization, KitosUser);

                return Created(AutoMapper.Mapper.Map<User, UserDTO>(user), new Uri(Request.RequestUri + "/" + user.Id));

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
