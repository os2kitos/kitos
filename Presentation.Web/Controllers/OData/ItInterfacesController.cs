using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
﻿using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItInterfacesController : BaseController<ItInterface>
    {
        public ItInterfacesController(IGenericRepository<ItInterface> repository)
            : base(repository)
        {
        }

        [EnableQuery]
        [ODataRoute("ItInterfaces")]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }

        // GET /Organizations(1)/ItInterfaces
        [EnableQuery]
        [ODataRoute("Organizations({key})/ItInterfaces")]
        public IHttpActionResult GetItInterfaces(int key)
        {
            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key || m.AccessModifier == AccessModifier.Public);
            return Ok(result);
        }
    }
}
