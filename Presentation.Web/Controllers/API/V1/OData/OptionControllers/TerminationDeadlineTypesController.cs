﻿using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class TerminationDeadlineTypesController : BaseOptionController<TerminationDeadlineType, ItContract>
    {
        public TerminationDeadlineTypesController(IGenericRepository<TerminationDeadlineType> repository)
            : base(repository)
        {
        }
    }
}