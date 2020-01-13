using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalHandoverTrialTypesController : LocalOptionBaseController<LocalHandoverTrialType, HandoverTrial, HandoverTrialType>
    {
        public LocalHandoverTrialTypesController(IGenericRepository<LocalHandoverTrialType> repository, IGenericRepository<HandoverTrialType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
