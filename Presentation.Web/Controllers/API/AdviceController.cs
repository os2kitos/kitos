using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;
using Core.DomainModel.Advice;
namespace Presentation.Web.Controllers.API
{
    public class AdviceController : GenericContextAwareApiController<Advice, AdviceDTO>
    {
        public AdviceController(IGenericRepository<Advice> repository) : base(repository)
        {
        }

    }
}
