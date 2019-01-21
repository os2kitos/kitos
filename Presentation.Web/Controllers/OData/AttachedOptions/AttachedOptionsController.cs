using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.OData;
using System.Web.OData.Routing;

namespace Presentation.Web.Controllers.OData.AttachedOptions
{
    public class AttachedOptionsController : BaseEntityController<AttachedOption>
    {
        public AttachedOptionsController(IGenericRepository<AttachedOption> repository, IAuthenticationService authService)
               : base(repository, authService)
        {
        }
    }
}