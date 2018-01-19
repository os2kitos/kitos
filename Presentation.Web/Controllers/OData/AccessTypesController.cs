using Core.ApplicationServices;
using Core.DomainServices;
using Core.DomainModel;
using Core.DomainModel.ItSystem;

namespace Presentation.Web.Controllers.OData
{
    public class AccessTypesController : BaseEntityController<AccessType>
    {
        public AccessTypesController(IGenericRepository<AccessType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }


    }
}