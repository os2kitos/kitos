using System.Collections.Generic;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class MethodController : GenericOptionApiController<Method, ItSystem, OptionDTO>
    {
        public MethodController(IGenericRepository<Method> repository) 
            : base(repository)
        {
        }
    }
}