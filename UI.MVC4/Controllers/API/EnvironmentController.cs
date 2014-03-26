using System.Collections.Generic;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class EnvironmentController : GenericOptionApiController<Environment, Technology, OptionDTO>
    {
        public EnvironmentController(IGenericRepository<Environment> repository)
            : base(repository)
        {
        }

        protected override IEnumerable<Environment> GetAllQuery()
        {
            return Repository.Get(x => x.IsActive);
        }
    }
}