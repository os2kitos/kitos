using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalPaymentModelTypesController : LocalOptionBaseController<LocalPaymentModelType, ItContract, PaymentModelType>
    {
        public LocalPaymentModelTypesController(IGenericRepository<LocalPaymentModelType> repository, IAuthenticationService authService, IGenericRepository<PaymentModelType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
