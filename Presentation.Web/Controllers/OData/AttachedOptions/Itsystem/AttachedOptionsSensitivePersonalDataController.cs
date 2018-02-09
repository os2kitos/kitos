using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.OData;
using System.Web.OData.Routing;

namespace Presentation.Web.Controllers.OData.AttachedOptions
{
    using System.Net;

    public class AttachedOptionsSensitivePersonalDataController : AttachedOptionsFunctionController<ItSystem, SensitivePersonalDataType, LocalSensitivePersonalDataType>
    {
        public AttachedOptionsSensitivePersonalDataController(IGenericRepository<AttachedOption> repository, 
            IAuthenticationService authService, IGenericRepository<ItSystem> itSystemRepository,
            IGenericRepository<LocalSensitivePersonalDataType> localSensitivePersonalDataTypeRepository,
            IGenericRepository<SensitivePersonalDataType> sensitiveDataTypeRepository)
           : base(repository, authService, itSystemRepository, sensitiveDataTypeRepository,
                 localSensitivePersonalDataTypeRepository)
        {
            globalEntityType = EntityType.ITSYSTEM;
            globalOptionType = OptionType.SENSITIVEPERSONALDATA;
        }

        [System.Web.Http.HttpGet]
        [EnableQuery]
        [ODataRoute("GetSensitivePersonalDataByObjectID(id={id})")]
        public override IHttpActionResult GetOptionsByObjectIDAndType(int id)
        {
            return base.GetOptionsByObjectIDAndType(id);
        }
    }
}