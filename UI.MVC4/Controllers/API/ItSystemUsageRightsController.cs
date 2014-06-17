using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemUsageRightsController : GenericRightsController<ItSystemUsage, ItSystemRight, ItSystemRole>
    {
        public ItSystemUsageRightsController(IGenericRepository<ItSystemRight> rightRepository, IGenericRepository<ItSystemUsage> objectRepository) : base(rightRepository, objectRepository)
        {
        }
    }
}
