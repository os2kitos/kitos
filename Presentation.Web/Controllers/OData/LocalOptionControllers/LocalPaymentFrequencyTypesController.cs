using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalPaymentFrequencyTypesController : LocalOptionBaseController<LocalPaymentFreqencyType, ItContract, PaymentFreqencyType>
    {
        public LocalPaymentFrequencyTypesController(IGenericRepository<LocalPaymentFreqencyType> repository, IGenericRepository<PaymentFreqencyType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
