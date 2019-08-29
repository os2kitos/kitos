﻿using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class PriceRegulationTypesController : BaseOptionController<PriceRegulationType, ItContract>
    {
        public PriceRegulationTypesController(IGenericRepository<PriceRegulationType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}