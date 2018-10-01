using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    using Core.ApplicationServices;
    using Core.DomainModel.ItSystem;
    using Core.DomainModel.ItSystemUsage;
    using Core.DomainServices;

    [ApiExplorerSettings(IgnoreApi = true)]
    public class ArchiveTestLocationsController : BaseOptionController<ArchiveTestLocation, ItSystemUsage>
    {
        public ArchiveTestLocationsController(IGenericRepository<ArchiveTestLocation> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}