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
    public class AttachedOptionsRegularPersonalDataController : AttachedOptionsFunctionController<ItSystem, RegularPersonalDataType, LocalRegularPersonalDataType>
    {
        public AttachedOptionsRegularPersonalDataController(IGenericRepository<AttachedOption> repository, IAuthenticationService authService,
            IGenericRepository<RegularPersonalDataType> regularPersonalDataTypeRepository,
            IGenericRepository<LocalRegularPersonalDataType> localregularPersonalDataTypeRepository)
           : base(repository, authService, regularPersonalDataTypeRepository,
                 localregularPersonalDataTypeRepository){}

        [System.Web.Http.HttpGet]
        [EnableQuery]
        [ODataRoute("GetRegularPersonalDataBySystemId(id={id})")]
        public IHttpActionResult GetRegularPersonalDataBySystemId(int id)
        {
            return base.GetOptionsByObjectIDAndType(id, EntityType.ITSYSTEM, OptionType.REGULARPERSONALDATA);
        }

        [System.Web.Http.HttpGet]
        [EnableQuery]
        [ODataRoute("GetRegularPersonalDataByUsageId(id={id})")]
        public IHttpActionResult GetRegularPersonalDataByUsageId(int id)
        {
            return base.GetOptionsByObjectIDAndType(id, EntityType.ITSYSTEMUSAGE, OptionType.REGULARPERSONALDATA);
        }
    }
}