using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
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
       public class AttachedOptionsRegisterTypesController : AttachedOptionsFunctionController<ItSystemUsage,RegisterType, LocalRegisterType>
    {
            public AttachedOptionsRegisterTypesController(IGenericRepository<AttachedOption> repository,
                IAuthenticationService authService,
                IGenericRepository<RegisterType> registerTypeRepository,
                IGenericRepository<LocalRegisterType> localRegisterTypeRepository,
                IGenericRepository<ItSystemUsage> usageRepository)
               : base(repository, authService, usageRepository, registerTypeRepository, localRegisterTypeRepository)
            {
            globalEntityType = EntityType.ITSYSTEMUSAGE;
            globalOptionType = OptionType.REGISTERTYPEDATA;
        }

            [System.Web.Http.HttpGet]
            [EnableQuery]
            [ODataRoute("GetRegisterTypesByObjectID(id={id})")]
            public override IHttpActionResult GetOptionsByObjectIDAndType(int id)
            {
            return base.GetOptionsByObjectIDAndType(id);
            }
    }
}