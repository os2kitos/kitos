using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class BusinessTypeController : GenericOptionApiController<BusinessType, ItSystem, OptionDTO>
    {
        public BusinessTypeController(IGenericRepository<BusinessType> repository) : base(repository)
        {
        }
    }
}
