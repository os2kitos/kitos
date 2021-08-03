using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class PaymentFrequencyTypesController : BaseOptionController<PaymentFreqencyType, ItContract>
    {
        public PaymentFrequencyTypesController(IGenericRepository<PaymentFreqencyType> repository)
            : base(repository)
        {
        }
    }
}