using Core.DomainModel;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class OrganizationsController : BaseController<Organization>
    {
        public OrganizationsController(IGenericRepository<Organization> repository)
            : base(repository)
        {
        }
    }
}
