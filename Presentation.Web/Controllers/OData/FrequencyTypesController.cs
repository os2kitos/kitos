using System;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace Presentation.Web.Controllers.OData
{
    public class FrequencyTypesController : BaseEntityController<FrequencyType>
    {
        public FrequencyTypesController(IGenericRepository<FrequencyType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }

        [EnableQuery]
        public override IHttpActionResult Get()
        {
            return Ok(Repository.AsQueryable().Where(x => x.IsActive && !x.IsSuggestion));
        }
    }
}