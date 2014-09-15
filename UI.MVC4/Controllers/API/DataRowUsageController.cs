using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class DataRowUsageController : GenericApiController<DataRowUsage, DataRowUsageDTO>
    {
        public DataRowUsageController(IGenericRepository<DataRowUsage> repository) : base(repository)
        {
        }
    }
}
