using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class AdviceController : GenericApiController<Advice, AdviceDTO>
    {
        public AdviceController(IGenericRepository<Advice> repository) : base(repository)
        {
        }
    }
}
