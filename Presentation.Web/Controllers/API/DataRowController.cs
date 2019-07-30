using System;
using System.Net.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class DataRowController : GenericContextAwareApiController<DataRow, DataRowDTO>
    {
        public DataRowController(IGenericRepository<DataRow> repository)
            : base(repository)
        {
        }

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
