using System.Collections.Generic;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class MethodController : GenericOptionApiController<Method, Interface, OptionDTO>
    {
        public MethodController(IGenericRepository<Method> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<Method> GetAllQuery()
        {
            return Repository.Get(x => x.IsActive);
        }
    }
}