using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ItContractRoleController : GenericOptionApiController<ItContractRole, ItContractRight>
    {
        public ItContractRoleController(IGenericRepository<ItContractRole> repository) : base(repository)
        {
        }
    }
}
