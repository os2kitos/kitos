using System;
using System.Net.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class DataTypeController : GenericOptionApiController<DataType, DataRow, OptionDTO>
    {

        public DataTypeController(IGenericRepository<DataType> repository) 
            : base(repository)
        {
        }
    }
}