using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class HandoverTrialTypesController : BaseOptionController<HandoverTrialType, HandoverTrial>
    {
        public HandoverTrialTypesController(IGenericRepository<HandoverTrialType> repository)
            : base(repository)
        {
        }
    }
}