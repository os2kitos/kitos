﻿using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers
{
    public class PaymentModelController : GenericApiController<PaymentModel, int, PaymentModelDTO>
    {
        public PaymentModelController(IGenericRepository<PaymentModel> repository) 
            : base(repository)
        {
        }
    }
}