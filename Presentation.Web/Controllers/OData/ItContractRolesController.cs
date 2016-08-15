using System.Linq;
using System.Web.Http;
using System.Web.OData;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItContractRolesController : BaseEntityController<ItContractRole>
    {
        public ItContractRolesController(IGenericRepository<ItContractRole> repository)
            : base(repository)
        {
        }

        [EnableQuery]
        public override IHttpActionResult Get()
        {
            return Ok(Repository.AsQueryable().Where(x => x.IsActive && !x.IsSuggestion));
        }
    }
}
