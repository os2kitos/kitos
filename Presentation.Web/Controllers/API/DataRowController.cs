using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class DataRowController : GenericContextAwareApiController<DataRow, DataRowDTO>
    {
        public DataRowController(IGenericRepository<DataRow> repository)
            : base(repository)
        {
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<DataRowDTO>>))]
        public virtual HttpResponseMessage GetByInterface(int interfaceId)
        {
            try
            {
                var item = Repository.Get(x => x.ItInterfaceId == interfaceId);
                if (item == null) return NotFound();

                var dto = Map(item);
                return Ok(dto);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
