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
    public class AttachedOptionsRegularPersonalDataController : AttachedOptionsFunctionController<ItSystem, RegularPersonalDataType, LocalRegularPersonalDataType>
    {
        public AttachedOptionsRegularPersonalDataController(
            IGenericRepository<AttachedOption> repository,
            IGenericRepository<RegularPersonalDataType> regularPersonalDataTypeRepository,
            IGenericRepository<LocalRegularPersonalDataType> localregularPersonalDataTypeRepository)
           : base(repository, regularPersonalDataTypeRepository, localregularPersonalDataTypeRepository)
        {

        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("GetRegularPersonalDataBySystemId(id={id})")]
        public IHttpActionResult GetRegularPersonalDataBySystemId(int id)
        {
            return GetOptionsByObjectIDAndType(id, EntityType.ITSYSTEM, OptionType.REGULARPERSONALDATA);
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("GetRegularPersonalDataByUsageId(id={id})")]
        public IHttpActionResult GetRegularPersonalDataByUsageId(int id)
        {
            return GetOptionsByObjectIDAndType(id, EntityType.ITSYSTEMUSAGE, OptionType.REGULARPERSONALDATA);
        }
    }
}