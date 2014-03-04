using System.Collections.Generic;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers
{
    public class InterfaceTypeController : GenericApiController<InterfaceType, int, InterfaceTypeDTO>
    {
        public InterfaceTypeController(IGenericRepository<InterfaceType> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<InterfaceType> GetAllQuery()
        {
            return Repository.Get(x => x.IsActive);
        }
    }
}