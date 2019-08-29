﻿using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class HandoverTrialTypesController : BaseOptionController<HandoverTrialType, HandoverTrial>
    {
        public HandoverTrialTypesController(IGenericRepository<HandoverTrialType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}