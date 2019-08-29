﻿using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalRegisterTypesController : LocalOptionBaseController<LocalRegisterType, ItSystemUsage, RegisterType>
    {
        public LocalRegisterTypesController(IGenericRepository<LocalRegisterType> repository, IAuthenticationService authService, IGenericRepository<RegisterType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}