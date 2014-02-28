﻿using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers
{
    public class PurchaseFormController : GenericApiController<PurchaseForm, int, PurchaseFormDTO>
    {
        public PurchaseFormController(IGenericRepository<PurchaseForm> repository) 
            : base(repository)
        {
        }
    }
}