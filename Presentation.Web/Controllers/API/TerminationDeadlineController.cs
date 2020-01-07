using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [DeprecatedApi]
    public class TerminationDeadlineController : GenericOptionApiController<TerminationDeadlineType, ItContract, OptionDTO>
    {
        public TerminationDeadlineController(IGenericRepository<TerminationDeadlineType> repository)
            : base(repository)
        {
        }
    }
}
