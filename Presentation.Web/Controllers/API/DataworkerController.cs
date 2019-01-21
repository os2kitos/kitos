using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Presentation.Web.Controllers.API
{
    public class DataworkerController : GenericApiController<ItSystemDataWorkerRelation, ItSystemDataWorkerRelationDTO>
    {
        public DataworkerController(IGenericRepository<ItSystemDataWorkerRelation> repository)
            : base(repository)
        {
        }
    }
}