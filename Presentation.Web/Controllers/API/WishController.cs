using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class WishController : GenericContextAwareApiController<Wish, WishDTO>
    {
        public WishController(IGenericRepository<Wish> repository)
            : base(repository)
        {
        }

        public HttpResponseMessage GetWishes([FromUri] int userId, [FromUri] int usageId)
        {
            var wishes = Repository.Get(x => x.ItSystemUsageId == usageId && (x.IsPublic || x.UserId == userId));
            return Ok(Map(wishes));
        }
    }
}
