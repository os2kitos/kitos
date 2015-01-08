using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class MethodController : GenericOptionApiController<Method, ItInterface, OptionDTO>
    {
        public MethodController(IGenericRepository<Method> repository) 
            : base(repository)
        {
        }
    }
}