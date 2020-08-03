using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Repositories.SystemUsage;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.AttachedOptions.ItsystemUsage
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

        [HttpPost]
        [ODataRoute("AttachedOptions")]
        public override IHttpActionResult Post([FromUri]int organizationId, AttachedOption entity)
        {
            return base.Post(organizationId, entity);
        }
    }
}