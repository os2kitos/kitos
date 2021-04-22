using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
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
        private readonly IGenericRepository<OrganizationUnit> _orgUnitsRepository;

        public ItSystemUsageOptionsController(
            IOptionsService<ItSystem, BusinessType> businessTypeService,
            IOptionsService<ItSystemRight, ItSystemRole> rolesService,
            IGenericRepository<OrganizationUnit> orgUnitsRepository)
        {
            _businessTypeService = businessTypeService;
            _rolesService = rolesService;
            _orgUnitsRepository = orgUnitsRepository;
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
                BusinessTypes = _businessTypeService.GetAvailableOptionsDetails(organizationId).Select(ToDTO<BusinessType, ItSystem>).ToList(),
                SystemRoles = _rolesService.GetAvailableOptionsDetails(organizationId).Select(ToDto).ToList(),
                OrganizationUnits = _orgUnitsRepository
                    .AsQueryable()
                    .ByOrganizationId(organizationId)
                    .AsEnumerable()
                    .Select(orgUnit => new HierachyNodeDTO(orgUnit.Id, orgUnit.Name, orgUnit.ParentId))
                    .ToList()
            });
        }

        private static BusinessRoleDTO ToDto(OptionDescriptor<ItSystemRole> availableRole)
        {
            return new BusinessRoleDTO(availableRole.Option.Id, availableRole.Option.Name, false, availableRole.Option.HasWriteAccess, availableRole.Description);
        }

        private static NamedEntityDTO ToDTO<T, TOwner>(OptionDescriptor<T> option) where T : OptionEntity<TOwner>
        {
            return new NamedEntityDTO(option.Option.Id, option.Option.Name);
        }
    }
}