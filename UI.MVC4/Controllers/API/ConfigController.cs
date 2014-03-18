using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ConfigController : GenericApiController<Config, int, ConfigDTO>
    {
        public ConfigController(IGenericRepository<Config> repository) 
            : base(repository)
        {
        }

        // GET api/T/default
        public HttpResponseMessage GetDefault(bool? @default)
        {
            var item = Repository.GetByKey(1); // global municipality has id 1

            if (item == null)
                return NoContent();

            return Ok(Map<Config, ConfigDTO>(item));
        }

        //[Authorize(Roles = "LocalAdmin")]
        public override HttpResponseMessage Patch(int id, Newtonsoft.Json.Linq.JObject obj)
        {
            return base.Patch(id, obj);
        }

        //[Authorize(Roles = "LocalAdmin")]
        protected override Config PatchQuery(Config item)
        {
            return base.PatchQuery(item);
        }
    }
}