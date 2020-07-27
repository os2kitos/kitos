using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainServices.Repositories.SystemUsage;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.AttachedOptions
{
    [InternalApi]
    public class AttachedOptionsSensitivePersonalDataController : AttachedOptionsFunctionController<ItSystem, SensitivePersonalDataType, LocalSensitivePersonalDataType>
    {
        public AttachedOptionsSensitivePersonalDataController(
            IGenericRepository<AttachedOption> repository,
            IGenericRepository<LocalSensitivePersonalDataType> localSensitivePersonalDataTypeRepository,
            IGenericRepository<SensitivePersonalDataType> sensitiveDataTypeRepository,
            IItSystemUsageRepository usageRepository)
           : base(repository, sensitiveDataTypeRepository, localSensitivePersonalDataTypeRepository, usageRepository)
        {
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("GetSensitivePersonalDataByUsageId(id={id})")]
        public IHttpActionResult GetSensitivePersonalDataByUsageId(int id)
        {
            return GetOptionsByObjectIDAndType(id, EntityType.ITSYSTEMUSAGE, OptionType.SENSITIVEPERSONALDATA);
        }
    }
}