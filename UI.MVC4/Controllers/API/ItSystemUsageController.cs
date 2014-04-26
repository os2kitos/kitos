using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemUsageController : GenericApiController<ItSystemUsage, int, ItSystemUsageDTO> 
    {
        public ItSystemUsageController(IGenericRepository<ItSystemUsage> repository) 
            : base(repository)
        {
        }

        public HttpResponseMessage GetByItSystemAndOrganization(int systemId, int organizationId)
        {
            try
            {
                var usage = Repository.Get(u => u.ItSystemId == systemId && u.OrganizationId == organizationId).FirstOrDefault();

                return usage == null ? NotFound() : Ok(Map(usage));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
