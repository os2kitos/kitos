using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class InterfaceUsageController : GenericApiController<InterfaceUsage, int, InterfaceUsageDTO>
    {
        private readonly IGenericRepository<ItSystem> _systemRepository;
        private readonly IItSystemUsageService _itSystemUsageService;

        public InterfaceUsageController(IGenericRepository<InterfaceUsage> repository, IGenericRepository<ItSystem> systemRepository, IItSystemUsageService itSystemUsageService) : base(repository)
        {
            _systemRepository = systemRepository;
            _itSystemUsageService = itSystemUsageService;
        }

        protected override InterfaceUsage PostQuery(InterfaceUsage item)
        {
            /* adding data row usages */
            var theInterface = _systemRepository.GetByKey(item.InterfaceId);
            
            return _itSystemUsageService.AddInterfaceUsage(item.ItSystemUsageId, theInterface);
        }
    }
}
