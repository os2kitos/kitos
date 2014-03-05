using System.Net;
using System.Net.Http;
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
            var item = Repository.GetById(1); // global municipality has id 1

            if (item == null)
                return Request.CreateResponse(HttpStatusCode.NoContent);

            return Request.CreateResponse(HttpStatusCode.OK, Map<Config, ConfigDTO>(item));
        }
    }
}