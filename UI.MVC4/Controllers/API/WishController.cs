using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class WishController : GenericApiController<Wish, int, WishDTO>
    {
        public WishController(IGenericRepository<Wish> repository) 
            : base(repository)
        {
        }

        public HttpResponseMessage GetWishes([FromUri] int userId, [FromUri] int usageId)
        {          
            var wishes = Repository.Get(x => x.UserId == userId && x.ItSystemUsageId == usageId);
            return Ok(Map(wishes));
        }
    }
}