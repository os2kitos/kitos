using System.Collections.Generic;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers
{
    public class MethodController : GenericApiController<Method, int, MethodDTO>
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