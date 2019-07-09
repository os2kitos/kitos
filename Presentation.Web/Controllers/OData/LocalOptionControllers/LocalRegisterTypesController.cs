﻿using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalRegisterTypesController : LocalOptionBaseController<LocalRegisterType, ItSystemUsage, RegisterType>
    {
        public LocalRegisterTypesController(IGenericRepository<LocalRegisterType> repository, IAuthenticationService authService, IGenericRepository<RegisterType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}