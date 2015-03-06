using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItSystemUsagesController : BaseController<ItSystemUsage>
    {

        public ItSystemUsagesController(IGenericRepository<ItSystemUsage> repository)
            : base(repository)
        {

        }
    }
}