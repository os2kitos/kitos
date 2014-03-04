using System.Collections.Generic;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers
{
    public class ProtocolTypeController : GenericApiController<ProtocolType, int, ProtocolTypeDTO>
    {
        public ProtocolTypeController(IGenericRepository<ProtocolType> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<ProtocolType> GetAllQuery()
        {
            return Repository.Get(x => x.IsActive);
        }
    }
}