using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class BusinessTypeController : GenericOptionApiController<BusinessType, ItSystem, OptionDTO>
    {
        public BusinessTypeController(IGenericRepository<BusinessType> repository) : base(repository)
        {
        }
    }
}
