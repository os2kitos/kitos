using System;
using System.Net.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class DataRowController : GenericApiController<DataRow, DataRowDTO>
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
                return Error(e);
            }
        }
    }
}