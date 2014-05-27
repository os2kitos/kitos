using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class OptionExtendController : GenericOptionApiController<OptionExtend, ItContract, OptionDTO>
    {
        public OptionExtendController(IGenericRepository<OptionExtend> repository) 
            : base(repository)
        {
        }
    }
}