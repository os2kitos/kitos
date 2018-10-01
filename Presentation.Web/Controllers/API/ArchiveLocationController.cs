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
        /// <summary>
        /// Archive location(arkiveringssted) for it system usage pane archiving 
        /// </summary>
        /// <param name="repository"></param>
        public ArchiveLocationController(IGenericRepository<ArchiveLocation> repository)
            : base(repository)
        {
        }
    }
}