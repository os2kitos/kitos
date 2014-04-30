using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class FrequencyController : GenericOptionApiController<Frequency, DataRowUsage, OptionDTO>
    {
        public FrequencyController(IGenericRepository<Frequency> repository) : base(repository)
        {
        }
    }
}
