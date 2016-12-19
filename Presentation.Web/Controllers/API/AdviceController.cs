using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;
using Core.DomainModel.Advice;
namespace Presentation.Web.Controllers.API
{
    public class AdviceOldController : GenericContextAwareApiController<Advice, AdviceDTO>
    {
        public AdviceOldController(IGenericRepository<Advice> repository) : base(repository)
        {
        }

    }
}
