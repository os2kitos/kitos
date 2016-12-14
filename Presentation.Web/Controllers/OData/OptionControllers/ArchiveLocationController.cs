using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    using Core.ApplicationServices;
    using Core.DomainModel.ItSystemUsage;
    using Core.DomainServices;

    public class ArchiveLocationController : BaseOptionController<ArchiveLocation, ItSystemUsage>
    {
        public ArchiveLocationController(IGenericRepository<ArchiveLocation> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}