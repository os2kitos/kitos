using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.AttachedOptions
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class AttachedOptionsSensitivePersonalDataController : AttachedOptionsFunctionController<ItSystem, SensitivePersonalDataType, LocalSensitivePersonalDataType>
    {
        public AttachedOptionsSensitivePersonalDataController(
            IGenericRepository<AttachedOption> repository,
            IGenericRepository<LocalSensitivePersonalDataType> localSensitivePersonalDataTypeRepository,
            IGenericRepository<SensitivePersonalDataType> sensitiveDataTypeRepository)
           : base(repository, sensitiveDataTypeRepository, localSensitivePersonalDataTypeRepository)
        {
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("GetSensitivePersonalDataByUsageId(id={id})")]
        public IHttpActionResult GetSensitivePersonalDataByUsageId(int id)
        {
            return GetOptionsByObjectIDAndType(id, EntityType.ITSYSTEMUSAGE, OptionType.SENSITIVEPERSONALDATA);
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("GetSensitivePersonalDataBySystemId(id={id})")]
        public IHttpActionResult GetSensitivePersonalDataBySystemId(int id)
        {
            return GetOptionsByObjectIDAndType(id, EntityType.ITSYSTEM, OptionType.SENSITIVEPERSONALDATA);
        }
    }
}