using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class WishController : GenericContextAwareApiController<Wish, WishDTO>
    {
        public WishController(IGenericRepository<Wish> repository)
            : base(repository)
        {
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<WishDTO>>))]
        public HttpResponseMessage GetWishes([FromUri] int userId, [FromUri] int usageId)
        {
            var wishes = Repository.Get(x => x.ItSystemUsageId == usageId && (x.IsPublic || x.UserId == userId));
            return Ok(Map(wishes));
        }
    }
}
