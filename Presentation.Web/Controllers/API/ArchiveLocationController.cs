using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Controllers.API
{
    using Core.DomainModel.ItSystem;
    using Core.DomainModel.ItSystemUsage;
    using Core.DomainServices;

    using Presentation.Web.Models;

    public class ArchiveLocationController : GenericOptionApiController<ArchiveLocation, ItSystemUsage, OptionDTO>
    {
        public ArchiveLocationController(IGenericRepository<ArchiveLocation> repository)
            : base(repository)
        {
        }
    }
}