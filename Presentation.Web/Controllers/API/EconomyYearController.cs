using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class EconomyYearController : GenericContextAwareApiController<EconomyYear, EconomyYearDTO>
    {
        public EconomyYearController(IGenericRepository<EconomyYear> repository) : base(repository)
        {
        }
    }
}
