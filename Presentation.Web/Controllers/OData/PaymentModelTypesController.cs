using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace Presentation.Web.Controllers.OData
{
    public class PaymentModelTypesController : BaseEntityController<PaymentModelType>
    {
        public PaymentModelTypesController(IGenericRepository<PaymentModelType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}