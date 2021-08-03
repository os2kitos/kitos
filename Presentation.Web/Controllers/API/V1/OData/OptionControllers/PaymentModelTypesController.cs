using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class PaymentModelTypesController : BaseOptionController<PaymentModelType, ItContract>
    {
        public PaymentModelTypesController(IGenericRepository<PaymentModelType> repository)
            : base(repository)
        {
        }
    }
}