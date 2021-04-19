using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Authorization;
using Core.DomainServices.Model.Options;
using Core.DomainServices.Options;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    [RoutePrefix("api/v1/itsystem-usage/options")]
    public class ItSystemUsageOptionsController : BaseApiController
    {
        private readonly IOptionsService<ItSystem, BusinessType> _businessTypeService;
        private readonly IOptionsService<ItSystemRight, ItSystemRole> _rolesService;

        public ItSystemUsageOptionsController(
            IOptionsService<ItSystem, BusinessType> businessTypeService,
            IOptionsService<ItSystemRight, ItSystemRole> rolesService)
        {
            _businessTypeService = businessTypeService;
            _rolesService = rolesService;
        }

        [HttpGet]
        [Route("overview")]
        public HttpResponseMessage Get(int organizationId)
        {
            if (GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }
            return Ok(new ItSystemUsageOptionsDTO
            {
                BusinessTypes = ToDTOs<BusinessType, ItSystem>(_businessTypeService.GetAvailableOptionsDetails(organizationId)),
                SystemRoles = _rolesService.GetAvailableOptionsDetails(organizationId).Select(ToDto).ToList()

                //TODO: Consider adding the org units as well here!
            });
        }

        private static BusinessRoleDTO ToDto(OptionDescriptor<ItSystemRole> availableRole)
        {
            return new BusinessRoleDTO(availableRole.Option.Id, availableRole.Option.Name, false, availableRole.Option.HasWriteAccess, availableRole.Description);
        }

        private static IEnumerable<NamedEntityDTO> ToDTOs<T, TOwner>(IEnumerable<OptionDescriptor<T>> options) where T : OptionEntity<TOwner>
        {
            return options.Select(ToDTO<T, TOwner>).ToList();
        }

        private static NamedEntityDTO ToDTO<T, TOwner>(OptionDescriptor<T> option) where T : OptionEntity<TOwner>
        {
            return new NamedEntityDTO(option.Option.Id, option.Option.Name);
        }
    }
}