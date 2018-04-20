using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ArchivePeriodsController : BaseEntityController<ArchivePeriod>
    {
        // GET: ArchivePeriode
        public ArchivePeriodsController(IGenericRepository<ArchivePeriod> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}