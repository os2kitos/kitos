using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class VersionController : GenericOptionApiController<Core.DomainModel.ItSystem.Version, ItInterface, OptionDTO>
    {
        public VersionController(IGenericRepository<Core.DomainModel.ItSystem.Version> repository)
            : base(repository)
        {
        }
    }
}
