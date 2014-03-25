using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class PaymentModelController : GenericOptionApiController<PaymentModel, ItContract, OptionDTO>
    {
        public PaymentModelController(IGenericRepository<PaymentModel> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<PaymentModel> GetAllQuery()
        {
            return Repository.Get(x => x.IsActive);
        }
    }
}