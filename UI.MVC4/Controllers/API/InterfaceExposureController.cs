using System.Net.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class InterfaceExposureController : GenericApiController<InterfaceExposure, InterfaceExposureDTO>
    {
        public InterfaceExposureController(IGenericRepository<InterfaceExposure> repository)
            : base(repository)
        {
        }

        public override HttpResponseMessage Delete(int id)
        {
            return NotAllowed();
        }

        public override HttpResponseMessage Post(InterfaceExposureDTO dto)
        {
            return NotAllowed();
        }

        public override HttpResponseMessage Put(int id, JObject jObject)
        {
            return NotAllowed();
        }
    }
}