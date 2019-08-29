﻿using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    using Core.ApplicationServices;
    using Core.DomainModel.ItSystem;
    using Core.DomainModel.ItSystemUsage;
    using Core.DomainModel.LocalOptions;
    using Core.DomainServices;

    [InternalApi]
    public class LocalArchiveTestLocationsController : LocalOptionBaseController<LocalArchiveTestLocation, ItSystemUsage, ArchiveTestLocation>
    {
        public LocalArchiveTestLocationsController(IGenericRepository<LocalArchiveTestLocation> repository, IAuthenticationService authService, IGenericRepository<ArchiveTestLocation> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}