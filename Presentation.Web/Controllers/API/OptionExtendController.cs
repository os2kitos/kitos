using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class OptionExtendController : GenericOptionApiController<OptionExtend, ItContract, OptionDTO>
    {
        public OptionExtendController(IGenericRepository<OptionExtend> repository) 
            : base(repository)
        {
        }
    }
}
