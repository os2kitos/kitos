﻿using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalBusinessTypesController : LocalOptionBaseController<LocalBusinessType, ItSystem, BusinessType>
    {
        public LocalBusinessTypesController(IGenericRepository<LocalBusinessType> repository, IAuthenticationService authService, IGenericRepository<BusinessType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
