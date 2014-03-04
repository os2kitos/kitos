using System.Collections.Generic;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers
{
    public class DatabaseTypeController : GenericApiController<DatabaseType, int, DatabaseTypeDTO>
    {
        public DatabaseTypeController(IGenericRepository<DatabaseType> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<DatabaseType> GetAllQuery()
        {
            return Repository.Get(x => x.IsActive);
        }
    }
}