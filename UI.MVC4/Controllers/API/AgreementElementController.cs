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
    public class AgreementElementController : GenericOptionApiController<AgreementElement, Agreement>
    {
        public AgreementElementController(IGenericRepository<AgreementElement> repository) : base(repository)
        {
        }
    }
}
