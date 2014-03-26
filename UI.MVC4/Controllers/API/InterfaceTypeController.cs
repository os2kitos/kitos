using System.Collections.Generic;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class InterfaceTypeController : GenericOptionApiController<InterfaceType, ItSystem, OptionDTO>
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