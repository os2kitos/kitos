using System;
using System.Net.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class AppTypeController : GenericOptionApiController<AppType, ItSystem, OptionDTO>
    {
        private readonly IItSystemService _itSystemService;

        public AppTypeController(IGenericRepository<AppType> repository, IItSystemService itSystemService) 
            : base(repository)
        {
            _itSystemService = itSystemService;
        }

        public HttpResponseMessage GetInterfaceAppType(bool? interfaceAppType)
        {
            try
            {
                return Ok(Map(_itSystemService.InterfaceAppType));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}