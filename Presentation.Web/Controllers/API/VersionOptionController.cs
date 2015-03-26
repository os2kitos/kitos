using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class VersionOptionController : GenericOptionApiController<Core.DomainModel.ItSystem.VersionOption, ItInterface, OptionDTO>
    {
        public VersionOptionController(IGenericRepository<Core.DomainModel.ItSystem.VersionOption> repository)
            : base(repository)
        {
        }
    }
}
