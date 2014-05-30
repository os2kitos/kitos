using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class InterfaceUsageController : GenericApiController<InterfaceUsage, InterfaceUsageDTO>
    {
        private readonly IGenericRepository<ItSystem> _systemRepository;
        private readonly IItSystemUsageService _itSystemUsageService;

        public InterfaceUsageController(IGenericRepository<InterfaceUsage> repository, IGenericRepository<ItSystem> systemRepository, IItSystemUsageService itSystemUsageService) : base(repository)
        {
            _systemRepository = systemRepository;
            _itSystemUsageService = itSystemUsageService;
        }

        public override System.Net.Http.HttpResponseMessage Delete(int id)
        {
            return NotAllowed();
        }

        public override System.Net.Http.HttpResponseMessage Post(InterfaceUsageDTO dto)
        {
            return NotAllowed();
        }

    }
}
