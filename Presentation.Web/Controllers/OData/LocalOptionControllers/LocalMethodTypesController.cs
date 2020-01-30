﻿using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalMethodTypesController : LocalOptionBaseController<LocalMethodType, ItInterface, MethodType>
    {
        public LocalMethodTypesController(IGenericRepository<LocalMethodType> repository, IGenericRepository<MethodType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
