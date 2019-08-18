using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.AttachedOptions
{
    [InternalApi]
    public class AttachedOptionsRegisterTypesController : AttachedOptionsFunctionController<ItSystemUsage, RegisterType, LocalRegisterType>
    {
        public AttachedOptionsRegisterTypesController(
                IGenericRepository<AttachedOption> repository,
                IAuthenticationService authService,
                IGenericRepository<RegisterType> registerTypeRepository,
                IGenericRepository<LocalRegisterType> localRegisterTypeRepository)
               : base(repository, authService, registerTypeRepository, localRegisterTypeRepository)
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
            //try
            //{
            //    var Entities = _AttachedOptionRepository.Get();

            //    if (Entities.Count() > 0)
            //    {
            //        entity.Priority = _AttachedOptionRepository.Get().Max(e => e.Priority) + 1;
            //    }
            //    else
            //    {
            //        entity.Priority = 1;
            //    }
            //}
            //catch (Exception e)
            //{
            //    var message = e.Message;
            //    return InternalServerError(e);
            //}

            return base.Post(dto);
        }
    }
}