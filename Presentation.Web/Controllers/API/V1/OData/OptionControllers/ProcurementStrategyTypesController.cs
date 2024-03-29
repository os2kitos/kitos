﻿using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class ProcurementStrategyTypesController : BaseOptionController<ProcurementStrategyType, ItContract>
    {
        public ProcurementStrategyTypesController(IGenericRepository<ProcurementStrategyType> repository)
            : base(repository)
        {
        }
    }
}