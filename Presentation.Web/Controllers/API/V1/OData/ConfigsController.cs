using System;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class ConfigsController : BaseEntityController<Config>
    {
        public ConfigsController(IGenericRepository<Config> repository)
            : base(repository)
        {
        }
       
        [NonAction]
        public override IHttpActionResult Delete(int key) => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Get() => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Post(int organizationId, Config entity) => throw new NotImplementedException();
    }
}
