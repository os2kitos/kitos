using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class SystemTypeController : GenericOptionApiController<SystemType, ItSystem, OptionDTO>
    {
        public SystemTypeController(IGenericRepository<SystemType> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<SystemType> GetAllQuery()
        {
            return Repository.Get(x => x.IsActive);
        }
    }
}