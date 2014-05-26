using System.Net.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class InterfaceExposureController : GenericApiController<InterfaceExposure, int, InterfaceExposureDTO>
    {
        private readonly IGenericRepository<ItSystem> _systemRepository;
        private readonly IItSystemUsageService _itSystemUsageService;

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

        public override HttpResponseMessage Put(int id, InterfaceExposureDTO dto)
        {
            return NotAllowed();
        }
    }
}