using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainServices.Repositories.SystemUsage;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.AttachedOptions
{
    [InternalApi]
    public class AttachedOptionsRegisterTypesController : AttachedOptionsFunctionController<ItSystemUsage, RegisterType, LocalRegisterType>
    {
        public AttachedOptionsRegisterTypesController(
                IGenericRepository<AttachedOption> repository,
                IGenericRepository<RegisterType> registerTypeRepository,
                IGenericRepository<LocalRegisterType> localRegisterTypeRepository,
                IItSystemUsageRepository usageRepository)
               : base(repository, registerTypeRepository, localRegisterTypeRepository, usageRepository)
        {

        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("GetRegisterTypesByObjectID(id={id})")]
        public IHttpActionResult GetRegisterTypesByObjectID(int id)
        {
            return GetOptionsByObjectIDAndType(id, EntityType.ITSYSTEMUSAGE, OptionType.REGISTERTYPEDATA);
        }


        [System.Web.Http.HttpPost]
        [ODataRoute("AttachedOptions")]
        public async Task<IHttpActionResult> Post([FromODataUri] int key, AttachedOption dto)
        {
            return base.Post(dto);
        }
    }
}