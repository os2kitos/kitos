using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalTerminationDeadlineTypesController : LocalOptionBaseController<LocalTerminationDeadlineType, ItContract, TerminationDeadlineType>
    {
        public LocalTerminationDeadlineTypesController(IGenericRepository<LocalTerminationDeadlineType> repository, IGenericRepository<TerminationDeadlineType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
