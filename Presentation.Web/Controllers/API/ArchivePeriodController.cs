using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ArchivePeriodController : GenericApiController<ArchivePeriod, ArchivePeriodDTO>
    {
        // GET: ArchivePeriod

        public ArchivePeriodController(IGenericRepository<ArchivePeriod> repository)
                    : base(repository)
        {
        }

        public HttpResponseMessage GetSingle(int id, [FromUri] bool system)
        {
            var item = Repository.Get(x => x.ItSystemUsageId == id);

            if (item == null)
                return NotFound();

            return Ok(Map(item));
        }
    }
}