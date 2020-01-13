using Core.DomainModel.LocalOptions;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalOrganizationUnitRolesController : LocalOptionBaseController<LocalOrganizationUnitRole, OrganizationUnitRight, OrganizationUnitRole>
    {
        public LocalOrganizationUnitRolesController(IGenericRepository<LocalOrganizationUnitRole> repository, IGenericRepository<OrganizationUnitRole> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}