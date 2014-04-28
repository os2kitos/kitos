using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class SensitiveDataTypeController : GenericOptionApiController<SensitiveDataType, ItSystemUsage, OptionDTO>
    {
        public SensitiveDataTypeController(IGenericRepository<SensitiveDataType> repository) 
            : base(repository)
        {
        }
    }
}