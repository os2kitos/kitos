using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace Presentation.Web.Controllers.OData
{
    public class ItSystemTypesController : BaseEntityController<ItSystemType>
    {
        public ItSystemTypesController(IGenericRepository<ItSystemType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}