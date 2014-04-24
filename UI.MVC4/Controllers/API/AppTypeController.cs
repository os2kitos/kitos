using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class AppTypeController : GenericOptionApiController<AppType, ItSystem, OptionDTO>
    {
        public AppTypeController(IGenericRepository<AppType> repository) 
            : base(repository)
        {
        }
    }
}