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
    using System.Web.Http.Description;

    [ApiExplorerSettings(IgnoreApi = true)]
    public class AttachedOptionsSensitivePersonalDataController : AttachedOptionsFunctionController<ItSystem, SensitivePersonalDataType, LocalSensitivePersonalDataType>
    {
        public AttachedOptionsSensitivePersonalDataController(IGenericRepository<AttachedOption> repository,
            IGenericRepository<LocalSensitivePersonalDataType> localSensitivePersonalDataTypeRepository, IAuthenticationService authService,
            IGenericRepository<SensitivePersonalDataType> sensitiveDataTypeRepository)
           : base(repository, authService, sensitiveDataTypeRepository,
                 localSensitivePersonalDataTypeRepository){}

        [System.Web.Http.HttpGet]
        [EnableQuery]
        [ODataRoute("GetSensitivePersonalDataByObjectID(id={id}, entitytype={entitytype})")]
        public IHttpActionResult GetOptionsByObjectID(int id, EntityType entitytype)
        {
            return base.GetOptionsByObjectIDAndType(id,entitytype, OptionType.SENSITIVEPERSONALDATA);
        }
    }
}