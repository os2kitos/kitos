using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class DatabaseTypeController : GenericApiController<DatabaseType, int>
    {
        public DatabaseTypeController(IGenericRepository<DatabaseType> repository) 
            : base(repository)
        {
        }
    }
}