using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class OrganizationUnitRolesController : BaseOptionController<OrganizationUnitRole, OrganizationUnitRight>
    {
        public OrganizationUnitRolesController(IGenericRepository<OrganizationUnitRole> repository)
            : base(repository)
        {
        }
    }
}
