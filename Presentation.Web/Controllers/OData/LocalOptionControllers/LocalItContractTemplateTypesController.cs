﻿using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalItContractTemplateTypesController : LocalOptionBaseController<LocalItContractTemplateType, ItContract, ItContractTemplateType>
    {
        public LocalItContractTemplateTypesController(IGenericRepository<LocalItContractTemplateType> repository, IAuthenticationService authService, IGenericRepository<ItContractTemplateType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
