using System;
using System.Net.Http;
using System.Web.Http.Description;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalAdminController : BaseApiController
    {
        private readonly IOrganizationRoleService _organizationRoleService;
        private readonly IGenericRepository<Organization> _organizationRepository;

        public LocalAdminController(IOrganizationRoleService organizationRoleService, IGenericRepository<Organization> organizationRepository)
        {
            _organizationRoleService = organizationRoleService;
            _organizationRepository = organizationRepository;
        }

        public HttpResponseMessage Post(CreateLocalAdminDTO item)
        {
            try
            {
                var user = UserRepository.GetByKey(item.UserId);
                var organization = _organizationRepository.GetByKey(item.OrganizationId);
                _organizationRoleService.MakeLocalAdmin(user, organization, KitosUser);
                return Created(AutoMapper.Mapper.Map<User, UserDTO>(user), new Uri(Request.RequestUri + "/" + user.Id));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
